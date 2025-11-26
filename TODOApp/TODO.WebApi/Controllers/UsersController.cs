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
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TODOAppDbContext _context;

        public UsersController(TODOAppDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------------------------------------
        // GET: api/Users (Se mantiene)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // ----------------------------------------------------------------------------------
        // GET: api/Users/5 (Se mantiene, añadiendo limpieza de password para la respuesta)
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Omitir el password al devolver la entidad
            user.Password = string.Empty;
            return user;
        }

        // ----------------------------------------------------------------------------------
        // POST: api/Users/register (NUEVO: Endpoint de registro de la Librería Digital)
        [HttpPost("register")]
        public async Task<ActionResult<User>> RegisterUser(User user) // Recibe la Entidad User directamente
        {
            // **IMPORTANTE:** Este método recibe la entidad User. En producción,
            // aquí se debería hacer un DTO mapping y limpiar colecciones como Books/Reviews.

            // Validación de email duplicado (Librería Digital)
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return Conflict("El email ya está registrado.");
            }

            // Seguridad (limpiar ID y colecciones para prevenir Over-posting, aunque arriesgado sin DTO)
            user.Id = 0; // Obligar a la base de datos a generar un nuevo ID
            user.Books = null;
            user.Reviews = null;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new { user.Id, user.Name, user.Email });
        }

        // ----------------------------------------------------------------------------------
        // PUT: api/Users/5 (ADAPTADO: Usa la Entidad User directamente, manteniendo la ruta original)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user) // Recibe la Entidad User directamente
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            // Validación de email duplicado (Librería Digital)
            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != id))
            {
                return Conflict("El email ya está en uso por otro usuario.");
            }

            // Seguridad: Asegurar que las colecciones no se modifiquen accidentalmente por el input
            user.Books = null;
            user.Reviews = null;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // ----------------------------------------------------------------------------------
        // POST: api/Users (SE ELIMINA/REEMPLAZA, ya que /register es el endpoint de creación de la Librería Digital)
        /*
         * El método original era:
            [HttpPost]
            public async Task<ActionResult<User>> PostUser(User user)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetUser", new { id = user.Id }, user);
            }
         * * Ahora la creación se hace a través de POST api/Users/register. Si necesitas mantener
         * la ruta POST api/Users funcionando, simplemente renombra el método RegisterUser 
         * a PostUser y quita el atributo de ruta 'register'.
         * Por simplicidad, mantendremos RegisterUser para distinguirlo del antiguo CRUD.
        */

        // ----------------------------------------------------------------------------------
        // DELETE: api/Users/5 (ADAPTADO: Añade validación de integridad referencial)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Captura el error si el usuario tiene Books o Reviews (Behavior.Restrict)
                return BadRequest("No se puede eliminar el usuario. Debe eliminar primero todos sus libros y reseñas.");
            }

            return NoContent();
        }

        // ----------------------------------------------------------------------------------
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}