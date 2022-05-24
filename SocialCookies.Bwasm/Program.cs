using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using SocialCookies.Bwasm;
using SocialCookies.Bwasm.Loxica;
using SocialCookies.Bwasm.Providers;
using SocialCookies.Bwasm.Xestores;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
//Usamos a conexion que temos en Properties/launchsetting.json do proxecto da API
//We use the connection we have set at Properties/launchsettings.json from the API project
//agora podemos crear unha instancia http da httpFactory usando o nome API
//que se declarara coa configuracion indicada nas linhas de abaixo
//now we can create a http instance from httpFactory using the name API
//which will be declared with the configuration show in the lines below
//builder.Services.AddHttpClient("API", options =>
//{
//    options.BaseAddress = new Uri("https://localhost:7061");
//});
//cambiamos sentencia anterior para engadir o DelegateHandler
//we modify the previous statement to add the DelegateHandler
//calquera API request deste domain sera interceptada por este delegate handler
//any api request of this domain will be automatically intercepted by this domain handler
builder.Services.AddHttpClient("API", options =>
{
    options.BaseAddress = new Uri("https://localhost:7061");
}).AddHttpMessageHandler<CookieXestor>();

builder.Services.AddScoped<ILoxicaApi, LoxicaApi>();

//para autenticacion -- for authentication purposes
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

//rexistramos o noso AuthStateProvider personalizado
//we register our customised AuthStateProvider
builder.Services.AddScoped<AuthenticationStateProvider, PersonalAuthStateProvider>();
builder.Services.AddScoped<CookieXestor>();
builder.Services.AddBlazoredLocalStorage();


await builder.Build().RunAsync();
