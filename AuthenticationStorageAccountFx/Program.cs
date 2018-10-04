using Authentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationStorageAccountFx
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).Wait();
        
        static async Task MainAsync(string[] args)
        {
            try
            {                
                string storageEndPoint = ConfigurationManager.AppSettings["StorageEndPoint"];

                Console.WriteLine("Login...");
                var result = await AcquireTokenWithUserInteraction();
                // Use the access token to create the storage credentials.
                TokenCredential tokenCredential = new TokenCredential(result.AccessToken);
                StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);
                
                CloudBlobContainer blobContainer = new CloudBlobContainer(new StorageUri(new Uri(storageEndPoint)),storageCredentials);
               

                var symKey = await GetCustomKey(createKey: true);

                Console.WriteLine("Upload File");
                var blobFileCrypt=blobContainer.GetBlockBlobReference("AuthenticationStorageAccountFx.exe.config.cryp");
                BlobEncryptionPolicy policy = new BlobEncryptionPolicy(symKey, null);
                
                BlobRequestOptions options = new BlobRequestOptions { EncryptionPolicy = policy,RequireEncryption = true, StoreBlobContentMD5 = true };
                OperationContext context = new OperationContext();
                context.LogLevel = Microsoft.WindowsAzure.Storage.LogLevel.Verbose;
                await blobFileCrypt.UploadFromFileAsync("AuthenticationStorageAccountFx.exe.config",null,options,context);

                var blobFile = blobContainer.GetBlockBlobReference("AuthenticationStorageAccountFx.exe.config");
                await blobFile.UploadFromFileAsync("AuthenticationStorageAccountFx.exe.config", null, null, context);

                var blobs = blobContainer.ListBlobs();
                Console.WriteLine("List Blobs");
                foreach (var b in blobs)
                {
                    Console.WriteLine(b.Uri.ToString());
                }

                Console.WriteLine("Upload succeeded !!");
            }
            catch (Exception exc)
            {
                Console.WriteLine("Something went wrong.");
                Console.WriteLine("Message: " + exc.Message + "\n");
            }
        }
        public static async Task<IKey> GetCustomKey(bool createKey)
        {

            RandomNumberGenerator randomNumberGenerator;

            byte[] keyRnd = new byte[64]; //512 bits


            if (createKey)
            {
                //Génére la clé
                randomNumberGenerator = RandomNumberGenerator.Create();
                randomNumberGenerator.GetBytes(keyRnd);

                var protectData = ProtectedData.Protect(keyRnd, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes("dtkey.bin", protectData);

            }
            else
            {
                var protectData = File.ReadAllBytes("dtkey.bin");
                keyRnd = ProtectedData.Unprotect(protectData, null, DataProtectionScope.CurrentUser);

            }
            SymmetricKey symKey = new SymmetricKey("private:key1", keyRnd);
            return symKey;
        }

        static string LoginUrl = ConfigurationManager.AppSettings["LoginUrl"];
        static string Tenant = ConfigurationManager.AppSettings["Tenant"];
        static string Authority = $"{LoginUrl}/{Tenant}";
        static string ClientId = ConfigurationManager.AppSettings["Modern.Authentication.Storage"];
        static string Resource = ConfigurationManager.AppSettings["ResourceStorage"];
        static Uri RedirectUrl = new Uri(ConfigurationManager.AppSettings["RedirectUrl"]);
        static AuthenticationContext ctx = new AuthenticationContext(Authority, true, new CustomTokenCache("scp.dat"));
        public static async Task<AuthenticationResult> AcquireTokenWithUserInteraction(string clientId = null, string redirectUrl = null)
        {
            PlatformParameters platformParameters = new PlatformParameters(PromptBehavior.Always);
            AuthenticationResult result = null;
            if (string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(redirectUrl))
            {
                try
                {
                    result= await ctx.AcquireTokenSilentAsync(Resource, ClientId);
                }
                catch(AdalException)
                {
                    result= await ctx.AcquireTokenAsync(Resource, ClientId, RedirectUrl, platformParameters).ConfigureAwait(false);
                }
                
            }
            return result;
            //return await ctx.AcquireTokenAsync(Resource, clientId, new Uri(redirectUrl), platformParameters).ConfigureAwait(false);

        }
    }
}
