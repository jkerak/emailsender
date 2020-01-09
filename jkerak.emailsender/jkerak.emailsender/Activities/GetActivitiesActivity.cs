using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using jkerak.emailsender.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace jkerak.emailsender.Activities
{
    public class GetActivitiesActivity
    {
        [FunctionName("Activities-GetActivities")]
        public static async Task<List<CustomerCommunicationUseCase>> GetActivities(
            [ActivityTrigger] IDurableActivityContext context)
        {
            var fileName = context.GetInput<string>();
            string response;
            
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                // Connect to the storage account's blob endpoint 
                CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);
                CloudBlobClient client = account.CreateCloudBlobClient();
                // Create the blob storage container 
                CloudBlobContainer container = client.GetContainerReference("config");

                // Create the blob in the container 
                var blob = container.GetBlockBlobReference(fileName);
                response = await blob.DownloadTextAsync();
            

            return JsonConvert.DeserializeObject<List<CustomerCommunicationUseCase>>(response);
        }
    }
}