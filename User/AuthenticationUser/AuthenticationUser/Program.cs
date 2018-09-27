using AdalAuthentication;
using AzureRM;
using System;
using System.Threading.Tasks;

namespace AuthenticationUser
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).Wait();

        static async Task MainAsync(string[] args)
        {

            try
            {

                var result = await Helper.AcquireTokenWithUserInteraction();
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
