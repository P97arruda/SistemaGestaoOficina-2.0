namespace SistemaGestaoOficina.Data.Entities
{
    public class User
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
