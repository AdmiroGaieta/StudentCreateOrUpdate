namespace StudentCreateOrUpdate.Models
{
    public class Aluno
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Matriculation { get; set; }
        public string Email { get; set; }
        public int IdUser { get; set; }
        public string Course { get; set; }
        public int Bloqueado { get; set; }
    }
}
