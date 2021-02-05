using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace blob_quickstart
{
    class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Azure Blob Storage - .NET quickstart sample\n");

            await ProcessAsync();

            Console.WriteLine("Press any key to exit the sample application.");
            Console.ReadLine();
        }

        private static async Task ProcessAsync()
        {

            /**
             * 1.验证客户端
             */
            // Retrieve the connection string for use with the application. The storage 
            // connection string is stored in an environment variable on the machine 
            // running the application called AZURE_STORAGE_CONNECTION_STRING. If the 
            // environment variable is created after the application is launched in a 
            // console or with Visual Studio, the shell or application needs to be closed
            // and reloaded to take the environment variable into account.
            //要现在电脑下配置环境,打开cmd -》 setx AZURE_STORAGE_CONNECTION_STRING "<yourconnectionstring>"
            string storageConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

            // Check whether the connection string can be parsed.
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                // If the connection string is valid, proceed with operations against Blob
                // storage here.
                // ADD OTHER OPERATIONS HERE
            }
            else
            {
                // Otherwise, let the user know that they need to define the environment variable.
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add an environment variable named 'AZURE_STORAGE_CONNECTION_STRING' with your storage " +
                    "connection string as a value.");
                Console.WriteLine("Press any key to exit the application.");
                Console.ReadLine();
            }

            /**
             * 2.创建一个容器
             */
            // Create the CloudBlobClient that represents the 
            // Blob storage endpoint for the storage account.
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container called 'quickstartblobs' and 
            // append a GUID value to it to make the name unique.
            CloudBlobContainer cloudBlobContainer =
                cloudBlobClient.GetContainerReference("quickstartblobs" +
                    Guid.NewGuid().ToString());
            await cloudBlobContainer.CreateAsync();

            /**
             * 3.在容器上设置权限
             */
            // Set the permissions so the blobs are public.
            BlobContainerPermissions permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            await cloudBlobContainer.SetPermissionsAsync(permissions);

            /**
             * 4.将Blob上传到容器
             */
            // Create a file in your local MyDocuments folder to upload to a blob.
            string localPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string localFileName = "QuickStart_" + Guid.NewGuid().ToString() + ".txt";
            string sourceFile = Path.Combine(localPath, localFileName);
            // Write text to the file.
            File.WriteAllText(sourceFile, "Hello, World!");

            Console.WriteLine("Temp file = {0}", sourceFile);
            Console.WriteLine("Uploading to Blob storage as blob '{0}'", localFileName);

            // Get a reference to the blob address, then upload the file to the blob.
            // Use the value of localFileName for the blob name.
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFileName);
            await cloudBlockBlob.UploadFromFileAsync(sourceFile);


            /**
             * 5.列出容器中的斑点
             */
            // List the blobs in the container.
            Console.WriteLine("List blobs in container.");
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                foreach (IListBlobItem item in results.Results)
                {
                    Console.WriteLine(item.Uri);
                }
            } while (blobContinuationToken != null);

            // Loop while the continuation token is not null.

            //6.下载Blob
            // Download the blob to a local file, using the reference created earlier.
            // Append the string "_DOWNLOADED" before the .txt extension so that you 
            // can see both files in MyDocuments.
            string destinationFile = sourceFile.Replace(".txt", "_DOWNLOADED.txt");
            Console.WriteLine("Downloading blob to {0}", destinationFile);
            await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);


            //7.删除容器
            Console.WriteLine("Press the 'Enter' key to delete the example files, " +
                "example container, and exit the application.");
            Console.ReadLine();
            // Clean up resources. This includes the container and the two temp files.
            Console.WriteLine("Deleting the container");
            if (cloudBlobContainer != null)
            {
                await cloudBlobContainer.DeleteIfExistsAsync();
            }
            Console.WriteLine("Deleting the source, and downloaded files");
            File.Delete(sourceFile);
            File.Delete(destinationFile);




        }
    }
}
