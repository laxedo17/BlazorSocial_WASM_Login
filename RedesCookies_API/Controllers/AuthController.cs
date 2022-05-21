using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RedesCookies_API.Db.Entity;
using RedesCookies_API.Dto;

using System.Security.Claims;

namespace RedesCookies_API.Controllers;

//login endpoint para usuario
//login endpoint for the user
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly SocialAuthDbContext _socialAuthDbContext;

    public AuthController(SocialAuthDbContext socialAuthDbContext)
    {
        _socialAuthDbContext = socialAuthDbContext;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync(LoginDto login)
    {
        //Non necesitamos considerar o ExternalLoginNome para obter o usuario
        //We dont need to consider the external login name to get the user
        var usuario = await _socialAuthDbContext
            .Usuarios.Where(_ => _.Email.ToLower() == login.Email.ToLower() && _.Password == login.Password && _.ExternalLoginNome == null)
            .FirstOrDefaultAsync();

        if (usuario == null)
        {
            return BadRequest("Credenciais non validas");
        }

        var claims = new List<Claim>()
        {
            new Claim("userid", usuario.Id.ToString()),
            new Claim(ClaimTypes.Email,usuario.Email)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authPropiedades = new AuthenticationProperties();

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authPropiedades
        );

        return Ok("success");
    }

    [HttpGet]
    [Route("usuario-perfil")]
    [Authorize]
    public async Task<IActionResult> UsuarioPerfilAsync()
    {
        int usuarioId = HttpContext.User.Claims
            .Where(_ => _.Type == "userid")
            .Select(_ => Convert.ToInt32(_.Value))
            .First();

        //Unha vez obtemos a usuarioId facemos query a base de datos
        //Once we got the usuarioId we perform a query into the database
        var perfilUsuario = await _socialAuthDbContext
            .Usuarios.Where(_ => _.Id == usuarioId)
            .Select(_ => new UsuarioPerfilDto
            {
                UsuarioId = _.Id,
                Apelidos = _.Apelidos,
                Nome = _.Nome,
                Email = _.Email
            }).FirstOrDefaultAsync();

        return Ok(perfilUsuario);
    }
}