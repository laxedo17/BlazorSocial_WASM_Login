using System.Text;
using System.Text.Json;
using SocialCookies.Bwasm.Modelos;

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
        //we use the same name, API, from the configuration in the Program.cs
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
}