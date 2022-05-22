using SocialCookies.Bwasm.Modelos;

namespace SocialCookies.Bwasm.Loxica;

public interface ILoxicaApi
{
    Task<string> LoginAsync(LoginModel loginModel);
    Task<(string Mensaxe, UsuarioPerfilModel UsuarioPerfil)> UsuarioPerfilAsync();
}