using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WorkerServiceExample
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            // Configuração do tempo limite
            _httpClientFactory.CreateClient().Timeout = TimeSpan.FromMinutes(2);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    // Chamar o método para consumir a API e processar os dados
                    await ProcessStudents();

                    _logger.LogInformation("Processing completed at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing students.");
                }

                await Task.Delay(6000, stoppingToken); // Delay de 1 minuto entre as execuções
            }
        }

        private async Task ProcessStudents()
        {
            try
            {
                var apiUrl = "https://forlearn.ispm.ao/pt/api/get-information-emissor/$2y$10$19zAZeIPVkBECPo95nn8AufYrG1vSDIhaWRgeDkqx8TrCjxclOo5m,$2y$10$t2VxsPmPPD8.F318aItD9u7HtAfUQIELy4TSICenOS9GhtVaSYg6,1235,191020031,p.seulo@forlearn.ao";

                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(apiUrl);

                response.EnsureSuccessStatusCode(); // Lança uma exceção em caso de falha HTTP

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Permitir que as propriedades do JSON sejam desserializadas independentemente do case
                };

                var apiResponse = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream, options);

                // Verifica se o JSON possui a estrutura esperada
                if (apiResponse.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var outerArrayElement in dataElement.EnumerateArray())
                    {
                        if (outerArrayElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var studentElement in outerArrayElement.EnumerateArray())
                            {
                                // Processar cada aluno diretamente a partir do JsonElement
                                var name = studentElement.GetProperty("name").GetString();
                                var fullName = studentElement.GetProperty("full_name").GetString();
                                var matriculation = studentElement.GetProperty("matriculation").GetString();
                                var email = studentElement.GetProperty("email").GetString();
                                var idUser = studentElement.GetProperty("id_user").GetInt32();
                                var course = studentElement.GetProperty("course").GetString();
                                var bloqueado = studentElement.GetProperty("bloqueado").GetInt32();

                                // Simulação de processamento de aluno
                                await ExecuteStoredProcedure(name, fullName, matriculation, email, idUser, course, bloqueado);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("API response does not contain valid data.");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while processing students from the API.");
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error while processing students from the API. Please check if the JSON structure matches the expected format.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing students from the API.");
                throw; // Rethrow the exception for the caller (optional depending on desired error handling)
            }
        }

        private async Task ExecuteStoredProcedure(string name, string fullName, string matriculation, string email, int idUser, string course, int bloqueado)
        {
            try
            {
                // Configuração da conexão com o banco de dados
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Configuração do comando SQL para chamar o procedimento armazenado
                    using (SqlCommand cmd = new SqlCommand("sp_VerificarAlunoCartao", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Parâmetros do procedimento armazenado
                        // Remove caracteres não numéricos de matriculation
                        string numAluno = new string(matriculation.Where(char.IsDigit).ToArray());
                        cmd.Parameters.AddWithValue("@NUMALUNO", idUser);
                        cmd.Parameters.AddWithValue("@NOME", fullName);
                        cmd.Parameters.AddWithValue("@Bloqueado", bloqueado);

                        // Executa o comando
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                _logger.LogInformation($"Student {name} {matriculation} processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while processing student: {fullName} {matriculation}");
                throw; // Rethrow a exceção para o caller (opcional dependendo do tratamento de erro desejado)
            }
        }
    }
}
