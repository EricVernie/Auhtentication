using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;

namespace KeyVaultHelper
{


    public class KeyVaultSecret
    {
        public Serviceprincipal ServicePrincipal { get; set; }
    }

    public class Serviceprincipal
    {
        public string SubscriptionId { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class KeyVaultHelper
    {
        static KeyVaultHelper()
        {
            Console.WriteLine("Get Secret from Keyvault");
        }
        public static async Task<KeyVaultSecret> GetSecretFromMsiAsync(string uri)
        {
            AzureServiceTokenProvider azk = new AzureServiceTokenProvider();
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azk.KeyVaultTokenCallback));
            var secret = await kv.GetSecretAsync(uri);

            return JsonConvert.DeserializeObject<KeyVaultSecret>(secret.Value);
        }
        public static async Task<string> GetSecretAsync(string uri)
        {
            AzureServiceTokenProvider azk = new AzureServiceTokenProvider();
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azk.KeyVaultTokenCallback));
            var secret = await kv.GetSecretAsync(uri);
            return secret.Value;
        }
    }
}
