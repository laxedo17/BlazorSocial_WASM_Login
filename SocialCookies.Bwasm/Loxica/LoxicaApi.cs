using SocialCookies.Bwasm.Modelos;

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SocialCookies.Bwasm.Loxica;

public class LoxicaApi : ILoxicaApi
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoxicaApi(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> LoginAsync(LoginModel loginModel)
    {
        //usamos o mesmo nome da configuracion de Program.cs
        //we use the same name, API, from the configuration in the Program.cs file
        var factoriaCliente = _httpClientFactory.CreateClient("API");

        //payload: headers
        string cargaUtil = JsonSerializer.Serialize(loginModel);
        var contido = new StringContent(cargaUtil, Encoding.UTF8, "application/json");
        //usamos endpoint login, como en metodo HttpPost do Auth controller da API
        //we use the endpoint login, like the HttpPost method from the Aucontroller in the API
        var resposta = await factoriaCliente.PostAsync("/auth/login", contido);
        if (resposta.IsSuccessStatusCode)
        {
            return "Success";
        }
        else
        {
            return "Error";
        }
    }

    public async Task<(string Mensaxe, UsuarioPerfilModel UsuarioPerfil)> UsuarioPerfilAsync()
    {
        var factoriaCliente = _httpClientFactory.CreateClient("API");
        //usamos perfil-usuario, mismo endpoint que en AuthController
        //we use AuthController, same endpoint as in AuthController
        var resposta = await factoriaCliente.GetAsync("/auth/usuario-perfil");
        if (resposta.IsSuccessStatusCode)
        {
            return ("Success", await resposta.Content.ReadFromJsonAsync<UsuarioPerfilModel>());
        }
        else
        {
            if (resposta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return ("Unauthorized", null);
            }
            else
            {
                return ("Error", null);
            }
        }
    }


    public async Task<string> LogoutAsync()
    {
        var factoriaCliente = _httpClientFactory.CreateClient("API");
        var resposta = await factoriaCliente.PostAsync("/auth/logout", null);

        if (resposta.IsSuccessStatusCode)
        {
            return "Success";
        }
        return "Error";
    }
}