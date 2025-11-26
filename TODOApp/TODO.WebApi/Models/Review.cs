namespace TODO.WebApi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; } // 1 a 5
        public string? Comment { get; set; }

        // Clave Foránea al Libro
        public int BookId { get; set; }
        public Book? Book { get; set; } = default!;

        // Clave Foránea al Usuario que reseña (referencia a User_LD)
        public int UserId { get; set; }
        public User? User { get; set; } = default!; // <--- Referencia a User_LD
    }
}
