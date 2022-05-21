using Microsoft.AspNetCore.Components.Authorization;

using SocialCookies.Bwasm.Modelos;

using System.Security.Claims;

namespace SocialCookies.Bwasm.Providers
{
    /// <summary>
    /// AuthStateProvider personalizado. Customised AuthStateProvider
    /// </summary>
    public class PersonalAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return new AuthenticationState(claimsPrincipal);
        }

        /// <summary>
        /// Establecer informacion de autenticacio porque se o usuario marcha da paxina perdese a cache dos datos, anque ao volver sigue autenticado
        /// </summary>
        public void SetAuthenticationInfo(UsuarioPerfilModel perfilUsuario)
        {
            //Damoslle nome CookieAutorizacion para reflexar no authentication state
            var identidade = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.Email, perfilUsuario.Email),
            new Claim(ClaimTypes.Name, $"{perfilUsuario.Nome} {perfilUsuario.Apelidos}"),
            new Claim("userid", perfilUsuario.UsuarioId.ToString())
        }, "CookieAutorizacion");
            //aqui cambiamos os datos dos claims
            //facendo que o usuario se autentique, engadimos datos personalizados
            //algunha info do usuario, como claims
            claimsPrincipal = new ClaimsPrincipal(identidade);

            //notificamos os cambios
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
