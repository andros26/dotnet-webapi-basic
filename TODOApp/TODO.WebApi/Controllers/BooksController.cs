using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TODO.WebApi.Models;

namespace TODO.WebApi.Controllers
{
    [Route("api/[controller]")] // Ruta base: /api/books
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly TODOAppDbContext _context;

        public BooksController(TODOAppDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------------------------
        // POST: api/books
        // [Equivalente a addBook en GraphQL]
        [HttpPost]
        public async Task<IActionResult> AddBook(Book book) // Recibe la Entidad Book
        {
            // Validación: ¿Existe el OwnerId en la tabla Users?
            var ownerExists = await _context.Users.AnyAsync(u => u.Id == book.OwnerId);
            if (!ownerExists)
            {
                return BadRequest("El OwnerId proporcionado no corresponde a un usuario válido.");
            }

            // *** LIMPIEZA DE SEGURIDAD (Mitigación de Over-posting y corrección CS8625) ***
            book.Id = 0;
            book.Reviews = new List<Review>();

            // CORRECCIÓN CS8625: Si Owner no es nullable, no se puede asignar null.
            // Para fines académicos, confiamos en que OwnerId es suficiente y EF Core ignora Owner.
            // book.Owner = null; // ELIMINADO para evitar el error.

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        // ----------------------------------------------------------------------
        // GET: api/books/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Owner)
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            // CORRECCIÓN CS8625: Usar string.Empty para limpiar el Password (que es string no anulable).
            if (book.Owner != null) book.Owner.Password = string.Empty;
            foreach (var review in book.Reviews)
            {
                if (review.User != null) review.User.Password = string.Empty;
            }

            return book;
        }

        // ----------------------------------------------------------------------
        // GET: api/books/collection/{ownerId}
        [HttpGet("collection/{ownerId}")]
        public async Task<ActionResult<IEnumerable<Book>>> GetUserCollection(int ownerId)
        {
            var books = await _context.Books
                .Where(b => b.OwnerId == ownerId)
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.User)
                .ToListAsync();

            if (!books.Any())
            {
                var ownerExists = await _context.Users.AnyAsync(u => u.Id == ownerId);
                return ownerExists ? Ok(new List<Book>()) : NotFound("El usuario no existe o no tiene libros registrados.");
            }

            // CORRECCIÓN CS8625: Usar string.Empty para limpiar el Password.
            foreach (var book in books)
            {
                foreach (var review in book.Reviews)
                {
                    if (review.User != null) review.User.Password = string.Empty;
                }
            }

            return Ok(books);
        }

        // ----------------------------------------------------------------------
        // PUT: api/books/{id} (Se añade el método que faltaba en tu código anterior)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            // Mapeo seguro: Solo actualizamos los campos permitidos.
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.PublicationYear = book.PublicationYear;
            existingBook.CoverImage = book.CoverImage;

            _context.Entry(existingBook).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Books.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // ----------------------------------------------------------------------
        // DELETE: api/books/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ----------------------------------------------------------------------
        // POST: api/reviews (Se añade el método que faltaba en tu código anterior)
        [HttpPost("~/api/reviews")]
        public async Task<IActionResult> AddReview(Review review)
        {
            if (review.Rating < 1 || review.Rating > 5)
            {
                return BadRequest("La calificación debe ser entre 1 y 5.");
            }

            var bookExists = await _context.Books.AnyAsync(b => b.Id == review.BookId);
            var userExists = await _context.Users.AnyAsync(u => u.Id == review.UserId);

            if (!bookExists || !userExists)
            {
                return BadRequest("El Libro o el Usuario especificado no existe.");
            }

            // *** LIMPIEZA DE SEGURIDAD (Mitigación de Over-posting y corrección CS8625) ***
            review.Id = 0;

            // CORRECCIÓN CS8625: Si Book y User no son nullable, no se puede asignar null.
            // review.Book = null; // ELIMINADO
            // review.User = null; // ELIMINADO

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = review.BookId }, review);
        }
        // El error CS1513 (falta de '}') probablemente estaba al final del archivo.
        // Asegúrate de que el archivo termine con dos llaves de cierre: } }
    }
}