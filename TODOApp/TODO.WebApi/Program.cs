using Microsoft.EntityFrameworkCore;
using TODO.WebApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TODOAppDbContext>(options =>
{
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// DESHABILITAR REDIRECCIÓN HTTPS EN DESARROLLO (DENTRO DE DOCKER)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.Run();



