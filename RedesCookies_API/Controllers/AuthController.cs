using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Twitter;
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

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync();
        return Ok("Success");
    }

    /// <summary>
    /// Metodo para conectarse a autenticacion Facebook.
    /// Recibe un string returnUrl porque unha vez nos autenticamos en facebook,
    /// volvemos a aplicacion Blazor WebAssembly.
    /// Method to connect to facebook authentication.
    /// It takes a returnUrl string because once we are authenticated in facebook,
    /// we must redirect back to our Blazor Web Assembly app.
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("login-facebook")]
    public IActionResult LoginFacebook(string returnUrl)
    {
        //Challenge usase para invocar os external login providers
        //Challenge is used for invoking the external login providers
        return Challenge(
            new AuthenticationProperties
            {
                //chamamos Action method no noso AuthController
                //we call an Action method in our AuthController
                RedirectUri = Url.Action(nameof(LoginFacebookCallback), new { returnUrl })
            },
            FacebookDefaults.AuthenticationScheme
            );
    }

    /// <summary>
    /// Crea cookie de login de facebook, a cal non podemos controlar, e ademais actualiza
    /// o Http Context coa informacion do usuario autenticado, creando nova Identity na BD.
    /// Creates a facebook login cookie, which we cannot control, and also updates the Http Context with the informacion of the authenticated user, adding a new Identity in the DB
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("login-facebook-callback")]
    public async Task<IActionResult> LoginFacebookCallback(string returnUrl)
    {
        var resultadoAutenticacion = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);
        if (resultadoAutenticacion.Succeeded)
        {
            //se usuario esta autenticado agregamolo a base de datos, por primeira vez
            //podemos usar a info para crear a cookie
            //if user is authenticated in facebook we add him/her to the database for the first time
            //we can use that info to create our cookie
            string email = HttpContext.User.Claims
                .Where(_ => _.Type == ClaimTypes.Email)
                .Select(_ => _.Value).FirstOrDefault();

            string nome = HttpContext.User.Claims
                .Where(_ => _.Type == ClaimTypes.Name)
                .Select(_ => _.Value).FirstOrDefault();

            string apelidos = HttpContext.User.Claims
                .Where(_ => _.Type == ClaimTypes.Surname)
                .Select(_ => _.Value).FirstOrDefault();

            var usuario = await XestionarExternalLoginUsuario(email, nome, apelidos, "facebook");

            await RefrescarExternalLogin(usuario);

            //redireccionamos, e como todo foi ben engadimos query parameter con un true.
            //We redirect the page and given the fact that everything worked properly we add a query parameter set to true
            return Redirect($"{returnUrl}?externalauth=true");
        }

        //se autenticacion de facebook falla redireccionamos aqui con query parameter a false
        //if facebook authentication fails we redirect here with query parameter set to false
        return Redirect($"{returnUrl}?externalauth=false");
    }

    [HttpGet]
    [Route("login-twitter")]
    public IActionResult LoginTwitter(string returnUrl)
    {
        return Challenge(
            new AuthenticationProperties
            {
                //chamamos Action method no noso AuthController
                //we call an Action method in our AuthController
                RedirectUri = Url.Action(nameof(LoginTwitterCallback), new { returnUrl })
            },
            TwitterDefaults.AuthenticationScheme
        );
    }

    [HttpGet]
    [Route("login-twitter-callback")]
    public async Task<ActionResult> LoginTwitterCallback(string returnUrl)
    {
        var resultadoAutenticacion = await HttpContext.AuthenticateAsync(TwitterDefaults.AuthenticationScheme);
        if (resultadoAutenticacion.Succeeded)
        {
            string email = HttpContext.User.Claims
                .Where(_ => _.Type == ClaimTypes.Email)
                .Select(_ => _.Value).FirstOrDefault();

            string nome = HttpContext.User.Claims
                .Where(_ => _.Type == ClaimTypes.GivenName)
                .Select(_ => _.Value).FirstOrDefault();

            string apelidos = HttpContext.User.Claims
                .Where(_ => _.Type == ClaimTypes.Surname)
                .Select(_ => _.Value).FirstOrDefault();

            var usuario = await XestionarExternalLoginUsuario(email, nome, apelidos, "Twitter");

            await RefrescarExternalLogin(usuario);

            return Redirect($"{returnUrl}?externalauth=true");
        }

        return Redirect($"{returnUrl}?externalauth=false");
    }

    /// <summary>
    /// Metodo comun para manexar o login en redes sociais de facebook, microsoft, google e twitter.
    /// Common method to manage the user login on social networks: facebook, microsoft, google and twitter.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="nome"></param>
    /// <param name="apelidos"></param>
    /// <param name="externalLoginNome"></param>
    /// <returns></returns>
    private async Task<Usuario> XestionarExternalLoginUsuario(
        string email,
        string nome,
        string apelidos,
        string externalLoginNome
        )
    {
        var usuario = await _socialAuthDbContext
            .Usuarios.Where(_ => _.Email.ToLower() == email.ToLower() && _.ExternalLoginNome == externalLoginNome)
            .FirstOrDefaultAsync();

        //se usuario existe, devolvemos o usuario
        //if the user exists, we return the user    
        if (usuario != null)
        {
            return usuario;
        }

        //se usuario non existe, engadimos usuario novo a BD
        //If the user doesnt exist, we add the new user to the DB
        var novoUsuario = new Usuario
        {
            Email = email,
            ExternalLoginNome = externalLoginNome,
            Nome = nome,
            Apelidos = apelidos
        };

        _socialAuthDbContext.Usuarios.Add(novoUsuario);
        await _socialAuthDbContext.SaveChangesAsync();
        return novoUsuario;
    }

    //metodo tamen comun usado polos nosos External Login Providers das redes sociais
    private async Task RefrescarExternalLogin(Usuario usuario)
    {
        var claims = new List<Claim>()
        {
            new Claim("userid", usuario.Id.ToString()),
            new Claim(ClaimTypes.Email,usuario.Email)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authPropiedades = new AuthenticationProperties();
        //ata aqui mesmo codigo que en metodo LoginAsync(). Till now this is the same code as in LoginAsync() method

        //agora cambia o codigo. Preparamos un novo Claims Identity coa informacion de usuario da BD
        //now the code changes. We prepare a new Claims Identity with the user info from the DB
        //facemos override a informacion proporcionada polo cookie de facebook, microsoft, google, twitter etc
        //we override the information obtained from the cookie from facebook, microsoft, google, twitter etc
        HttpContext.User.AddIdentity(claimsIdentity);

        //quitamos o user login co cookie actual
        //we remove the user login with the current cookie
        await HttpContext.SignOutAsync();

        //agora creamos unha nova cookie cos datos de usuario gardados na BD
        //now we create a new cookie with the user data stored in the DB
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authPropiedades
        );
    }
}