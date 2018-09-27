
using Authentication;
using AzureRM;
using KeyVaultHelper;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Threading.Tasks;

namespace AdalAuthentication
{
    static class Helper
    {
        static string LoginUrl = ConfigurationManager.AppSettings["LoginUrl"];
        static string Tenant = ConfigurationManager.AppSettings["Tenant"];
        static string Authority = $"{LoginUrl}/{Tenant}";
        static string ClientId = ConfigurationManager.AppSettings["ClientId"];
        static string Resource= ConfigurationManager.AppSettings["Resource"];
        static Uri RedirectUrl = new Uri(ConfigurationManager.AppSettings["RedirectUrl"]);
        static AuthenticationContext ctx = new AuthenticationContext(Authority, true, new CustomTokenCache("scp.dat"));
        static Helper()
        {
            Console.WriteLine("Login...");
        }
        public static async Task<AuthenticationResult> AcquireTokenWithUserInteraction()
        {
            PlatformParameters platformParameters = new PlatformParameters();            
            return await ctx.AcquireTokenAsync(Resource, ClientId, RedirectUrl,platformParameters).ConfigureAwait(false);
        }

       
        public static async Task<AuthenticationResult> Run(Func<Task<AuthenticationResult>> func)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var result = await func();
            watch.Start();
            Console.WriteLine($"Elapsed time: {watch.ElapsedMilliseconds}");
            return result;
        }
        public static async Task<AuthenticationResult> AcquireTokenWithSSOAsync()
        {

            AuthenticationResult result = null;
            
            //Get the local upn from connected user
            //cache upn

            string upn = CustomTokenCache.ReadData("upn.dat");
            if (string.IsNullOrEmpty(upn))
            {
                upn = UserPrincipal.Current.UserPrincipalName;
                CustomTokenCache.WriteData("upn.dat", upn);
            }
                                               
            try
            {                
                result = await ctx.AcquireTokenSilentAsync(Resource, ClientId).ConfigureAwait(false);
            }
            catch (AdalException)
            {
                result = await ctx.AcquireTokenAsync(Resource, ClientId, new UserCredential(upn)).ConfigureAwait(false);
            }
            

          
            return result;
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<AuthenticationResult> AcquireTokenWithDeviceCodeAsync()
        {
            
            DeviceCodeResult codeResult = await ctx.AcquireDeviceCodeAsync(Resource, ClientId);
            Console.WriteLine("You need to sign in.");
            Console.WriteLine("Message: " + codeResult.Message + "\n");
            return await ctx.AcquireTokenByDeviceCodeAsync(codeResult).ConfigureAwait(false);
        }

        public static async Task<AuthenticationResult> AcquireTokenWithClientCredentialAsync()
        {
            AuthenticationResult result = null;
            ClientCredential cred = null;
            string ClientId = null;
            string ClientSecret = null;
            if (ctx.TokenCache.Count>0)
            {
                ClientId = CustomTokenCache.ReadData("Clientid.dat");
                ClientSecret = CustomTokenCache.ReadData("Sec.dat");

            }
            else
            {
                string keyvaultUri = ConfigurationManager.AppSettings["ServicePrincipalContextUri"];

                KeyVaultSecret spContext = await KeyVaultHelper.KeyVaultHelper.GetSecretFromMsiAsync(keyvaultUri);
                CustomTokenCache.WriteData("clientid.dat", spContext.ServicePrincipal.ClientId);
                CustomTokenCache.WriteData("Sec.dat", spContext.ServicePrincipal.ClientSecret);
                ClientId = spContext.ServicePrincipal.ClientId;
                ClientSecret = spContext.ServicePrincipal.ClientSecret;
            }
            cred = new ClientCredential(ClientId, ClientSecret);
            result = await ctx.AcquireTokenAsync(Resource, cred);



            return result;


        }

    }
}
