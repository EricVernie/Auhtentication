using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

    namespace Authentication
    {
        // This is a simple persistent cache implementation for an ADAL V3 desktop application
        public class CustomTokenCache : TokenCache
        {



            public string CacheFilePath { get; }
            private static readonly object FileLock = new object();

            // Initializes the cache against a local file.
            // If the file is already present, it loads its content in the ADAL cache
            public CustomTokenCache(string filePath)
            {
                CacheFilePath = filePath;
                this.AfterAccess = AfterAccessNotification;
                this.BeforeAccess = BeforeAccessNotification;
                lock (FileLock)
                {
                    this.Deserialize(ReadFromFileIfExists(CacheFilePath));
                }
            }

            // Empties the persistent store.
            public override void Clear()
            {
                base.Clear();
                File.Delete(CacheFilePath);
            }

            // Triggered right before ADAL needs to access the cache.
            // Reload the cache from the persistent store in case it changed since the last access.
            void BeforeAccessNotification(TokenCacheNotificationArgs args)
            {
                lock (FileLock)
                {
                    this.Deserialize(ReadFromFileIfExists(CacheFilePath));
                }
            }

            // Triggered right after ADAL accessed the cache.
            void AfterAccessNotification(TokenCacheNotificationArgs args)
            {
                // if the access operation resulted in a cache update
                if (this.HasStateChanged)
                {
                    lock (FileLock)
                    {
                        // reflect changes in the persistent store
                        WriteToFileIfNotNull(CacheFilePath, this.Serialize());
                        // once the write operation took place, restore the HasStateChanged bit to false
                        this.HasStateChanged = false;
                    }
                }
            }

            /// <summary>
            /// Read the content of a file if it exists
            /// </summary>
            /// <param name="path">File path</param>
            /// <returns>Content of the file (in bytes)</returns>
            private byte[] ReadFromFileIfExists(string path)
            {
                byte[] blob = (!string.IsNullOrEmpty(path) && File.Exists(path))
                    ? File.ReadAllBytes(path) : null;

                return blob;
            }

            /// <summary>
            /// Writes a blob of bytes to a file. If the blob is <c>null</c>, deletes the file
            /// </summary>
            /// <param name="path">path to the file to write</param>
            /// <param name="blob">Blob of bytes to write</param>
            private static void WriteToFileIfNotNull(string path, byte[] blob)
            {
            //WARNING this is only for demo purpose
            //YOU NEED TO PROTECT THE TOKEN WITH A CRYPTO API LIKE 
            //https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata?view=netframework-4.7.2
            //This API does not exist (yet?) with .NET Core.
            //Need to develop Core API with the Windows API
            //https://msdn.microsoft.com/en-us/library/ms995355.aspx
            if (blob != null)
                {
                    File.WriteAllBytes(path, blob);
                }
                else
                {
                    File.Delete(path);
                }
            }
            public static void WriteData(string path,string upn)
            {
                File.WriteAllText(path, upn);
            }
            public static string ReadData(string path)
            {
                string upn = (!string.IsNullOrEmpty(path) && File.Exists(path))
                    ? File.ReadAllText(path) : null;
                return upn;
            }
        }

    }
