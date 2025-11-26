using Microsoft.EntityFrameworkCore;

namespace TODO.WebApi.Models
{
    public class TODOAppDbContext : DbContext
    {
        public TODOAppDbContext(DbContextOptions<TODOAppDbContext> options) : base(options) { }

        // DbSets (Colecciones de la Librería Digital)
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ----------------------------------------------------------------------
            // Configuración de la Entidad USER (Ajustar nombres de tabla si es necesario)
            // Ya que Users no tiene una entidad antigua, la mantenemos simple:
            // modelBuilder.Entity<User>().ToTable("Users");

            // ----------------------------------------------------------------------
            // 1. Relación User (Dueño) a Book (1:M)
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Owner)
                .WithMany(u => u.Books)
                .HasForeignKey(b => b.OwnerId)
                // Usamos RESTRICT para proteger al User. Esto previene el error 1785
                // y la lógica académica de no permitir eliminar usuarios con contenido.
                .OnDelete(DeleteBehavior.Restrict);

            // ----------------------------------------------------------------------
            // 2. Relación Book a Review (1:M)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookId)
                // Usamos CASCADE. Si eliminas un Libro, sus Reseñas deben irse.
                .OnDelete(DeleteBehavior.Cascade);

            // ----------------------------------------------------------------------
            // 3. Relación User (Reseñador) a Review (1:M)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                // Usamos RESTRICT para proteger al User. No se puede eliminar si tiene reseñas.
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}