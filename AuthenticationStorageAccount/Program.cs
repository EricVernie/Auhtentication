using AdalAuthentication;
using AzureRM;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;

namespace AuthenticationStorageAccount
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).Wait();

        static async Task MainAsync(string[] args)
        {
            try
            {
                string clientId = ConfigurationManager.AppSettings["Modern.Authentication.Storage"];
                string resourceId = ConfigurationManager.AppSettings["ResourceStorage"];
                string storageEndPoint = ConfigurationManager.AppSettings["StorageEndPoint"];


                var result = await Helper.AcquireTokenWithSSOAsync();
                // Use the access token to create the storage credentials.
                TokenCredential tokenCredential = new TokenCredential(result.AccessToken);
                StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);
               
                CloudBlobContainer blobContainer = new CloudBlobContainer(new StorageUri(new Uri(storageEndPoint)), storageCredentials);
                
                var blobFile = blobContainer.GetBlockBlobReference("upn.dat");
                await blobFile.UploadFromFileAsync("upn.dat");

            
                Console.WriteLine("Upload succeeded !!");
            }
            catch (Exception exc)
            {
                Console.WriteLine("Something went wrong.");
                Console.WriteLine("Message: " + exc.Message + "\n");
            }
        }
    }
}
