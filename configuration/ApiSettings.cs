namespace StudentCreateOrUpdate.Configuration
{
    public class AppSettings
    {
        public ApiSettings ApiSettings { get; set; }
        // Adicione outras configurações se necessário
    }

    public class ApiSettings
    {
        public string BaseUrl { get; set; }
        // Adicione outras configurações da API se necessário
    }
}
