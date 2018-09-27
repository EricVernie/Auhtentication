using Authentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationUserFx
{
    class Program
    {
        static string LoginUrl = ConfigurationManager.AppSettings["LoginUrl"];
        static string Tenant = ConfigurationManager.AppSettings["Tenant"];
        static string Authority = $"{LoginUrl}/{Tenant}";
        static string ClientId = ConfigurationManager.AppSettings["ClientId"];
        static string Resource = ConfigurationManager.AppSettings["Resource"];
        static Uri RedirectUrl = new Uri(ConfigurationManager.AppSettings["RedirectUrl"]);

        static void Main(string[] args) => MainAsync(args).Wait();
        
        static async Task MainAsync(string[] args)
        {
            AuthenticationContext ctx = new AuthenticationContext(Authority, true,new CustomTokenCache("sct.dat"));
            AuthenticationResult result = null;
            try
            {
                result = await ctx.AcquireTokenSilentAsync(Resource, ClientId);
            }
            catch(AdalException)
            {
                result = await ctx.AcquireTokenAsync(Resource, ClientId, RedirectUrl, new PlatformParameters(PromptBehavior.Always));
            }

            Console.WriteLine(result.AccessToken);
            Console.Read();
            
        }
    }
}
