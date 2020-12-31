using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using FenixAlliance.ABM.Data.Access.Interfaces.StorageAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace FenixAlliance.Data.Access.DataAccess
{
    public class BlobStorageDataAccessClient : IBlobStorageDataAccessClient
    {
        public async Task<BlobContainerClient> GetBlobContainerClientAsync(string containerName)
        {
            var Configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var connectionString = Configuration["ConnectionStrings:AzureStorageConnectionString"];
            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            // If "ContainerGUID" doesn't exist, create it.
            await container.CreateIfNotExistsAsync();

            return container;
        }


        // Get Blob Container or create if not exists<
        public async Task<CloudBlobContainer> GetCloudBlobContainerAsync(string ContainerName)
        {
            var Configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:AzureStorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            // If "ContainerGUID" doesn't exist, create it.
            await container.CreateIfNotExistsAsync();
            return container;
        }
    }
}
