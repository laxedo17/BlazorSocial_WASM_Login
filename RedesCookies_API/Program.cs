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
).AddCookie()
//rexistramos nosa facebook id e secret/We register our facebook id and secret
.AddFacebook(fb =>
{
    fb.AppId = builder.Configuration.GetSection("FacebookSettings").GetValue<string>("AppId");
    fb.AppSecret = builder.Configuration.GetSection("FacebookSettings").GetValue<string>("AppSecret");
})
.AddTwitter(t =>
{
    t.ConsumerKey = builder.Configuration.GetSection("Twitter").GetValue<string>("ApiKey");
    t.ConsumerSecret = builder.Configuration.GetSection("Twitter").GetValue<string>("ApiSecret");
    t.RetrieveUserDetails = true;

})
.AddMicrosoftAccount(mi =>
{
    mi.ClientId = builder.Configuration.GetSection("Microsoft").GetValue<string>("ClientId");
    mi.ClientSecret = builder.Configuration.GetSection("Microsoft").GetValue<string>("ClientSecret"); ;
});

//para permitir CORS e que non haxa conflicto entre o porto que usa a API e o da app Blazor WASM
//to allow CORS so that there is no conflict between the port that uses the API and the port belonging to the Blazor WASM app
builder.Services.AddCors(options =>
{
    options.AddPolicy("corsDetalles",
    builder =>
    {
        builder.AllowAnyHeader()
        .AllowAnyMethod()
        // .AllowAnyOrigin() non podemos usar AllowAnyOrigin cando usamos AllowCredentials, co cal...usamos
        .SetIsOriginAllowed(options => true)
        .AllowCredentials();//para evitar exception de usar o delegate handler ao xenerar as cookies
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
