using System;
using System.Threading.Tasks;
using AdalAuthentication;
using AzureRM;

namespace AuthenticateDeviceCode
{
    class Program
    {
        //Don't put identity information in code

        static void Main(string[] args) => MainAsync(args).Wait();
           
        static async Task MainAsync(string[] args)
        {
        

            try
            {
                var result = await Helper.AcquireTokenWithDeviceCodeAsync();
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

