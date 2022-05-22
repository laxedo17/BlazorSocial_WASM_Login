using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace SocialCookies.Bwasm.Xestores
{
    public class CookieXestor : DelegatingHandler
    {
        //override de metodo da clase base DelegatingHandler (usa Go to Definition)
        //method override of the base class DelegatinHandler (check Go to Definition)
        //cando se chame a Invoke, este delegate handler rexistrase
        //para o domain da API, antes de ir ao server
        //fara bypass aqui o cal significa que este metodo
        //se executa coa loxica que agreguemos que afectara
        //a API request
        //When we call Invoke, this delegate handler will register
        //for the API domain, before going to the server
        //it will bypass here, which means that this method
        //is going to execute with the logic that we add
        //and will affect the API request
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //SetBrowserRequestCredential para cookies autenticados no domain
            //SetBrowserRequestCredential for authenticated cookies in the domain
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            //informamos ao request para que agregue as credentials do navegador para este domain, se hai algunha no browser
            //faremos attach das cookies no login
            //we inform the requesta to add the credentials of the browser for this domain, if there is anything in the browser
            //cookies will be attached when we login
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
