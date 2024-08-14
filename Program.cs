using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace WorkerServiceExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient(); // Registrar o HttpClient para injeção de dependência

                    services.AddHostedService<Worker>(); // Registrar o Worker como um serviço hospedado
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.ClearProviders(); // Limpar os provedores de logging padrão
                    configLogging.AddConsole(); // Adicionar o provedor de logging para console
                });
    }
}
