using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RedesCookies_API.Db.Entity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//we register our database context here
//rexistramos o contexto da base de datos aqui
builder.Services.AddDbContext<SocialAuthDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SocialConnection"));
});

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme
).AddCookie();

//para permitir CORS e que non haxa conflicto entre o porto que usa a API e o da app Blazor WASM
//to allow CORS so that there is no conflict between the port that uses the API and the port belonging to the Blazor WASM app
builder.Services.AddCors(options =>
{
    options.AddPolicy("corsDetalles",
    builder =>
    {
        builder.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("corsDetalles");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
