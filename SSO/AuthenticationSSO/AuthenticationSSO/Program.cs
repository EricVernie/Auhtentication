using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using AzureRM;
using AdalAuthentication;

namespace AuthenticationSSO
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).Wait();

       
        static async Task MainAsync(string[] args)
        {
            
            
            try
            {
                Func<Task<AuthenticationResult>> _func = Helper.AcquireTokenWithSSOAsync;
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
