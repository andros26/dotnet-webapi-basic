namespace TODO.WebApi.Models
{
    public class User
    {
        public int Id { get; set; } // PK
        public string Name { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Propiedades de Navegación (Relaciones 1:M)
        public ICollection<Book>? Books { get; set; } = new List<Book>();
        public ICollection<Review>? Reviews { get; set; } = new List<Review>();

    }
}
