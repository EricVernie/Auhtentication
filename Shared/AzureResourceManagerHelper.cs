using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Management.Fluent;
using static Microsoft.Azure.Management.Fluent.Azure;
using System.Threading.Tasks;

namespace AzureRM
{
    public static class AzureResourceManagerHelper
    {
    static AzureResourceManagerHelper()
        {
         
        }
       public static IAuthenticated Authenticate(string token,string tenantId)
        {
            TokenCredentials tokenCredentials = new TokenCredentials(token);
            
            var azureCredentials = new AzureCredentials(
                    tokenCredentials,
                    tokenCredentials,
                    tenantId,
                    AzureEnvironment.AzureGlobalCloud);

            return Azure.Authenticate(azureCredentials);
        }
        public static async Task ListSubscriptionAsync(this IAuthenticated azure)
        {
            Console.WriteLine("List Subscriptions... ");
            var subs = await azure.Subscriptions.ListAsync();
            foreach (var sub in subs)
            {
                Console.WriteLine(sub.SubscriptionId);
            }
        }
    }
}
