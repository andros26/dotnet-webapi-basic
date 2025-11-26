namespace TODO.WebApi.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string? CoverImage { get; set; }

        // Clave Foránea: OwnerId (referencia a User_LD)
        public int OwnerId { get; set; }
        public User? Owner { get; set; } = default!; // <--- Referencia a User_LD

        // Relación 1:M a Reseñas
        public ICollection<Review>? Reviews { get; set; } = new List<Review>();
    }
}
