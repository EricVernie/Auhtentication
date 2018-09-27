using AdalAuthentication;
using AzureRM;
using KeyVaultHelper;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace AuthenticationClientCredential
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).Wait();

        static async Task MainAsync(string[] args)
        {

            try
            {
                
                Func<Task<AuthenticationResult>> _func = Helper.AcquireTokenWithClientCredentialAsync;
                var result = await Helper.Run(_func);

                await AzureRM.AzureResourceManagerHelper.Authenticate(result.AccessToken, result.TenantId)
                    .ListSubscriptionAsync();
            }
            catch (Exception exc)
            {
                Console.WriteLine("Something went wrong.");
                Console.WriteLine("Message: " + exc.Message + "\n");
            }


        }
    }
}
