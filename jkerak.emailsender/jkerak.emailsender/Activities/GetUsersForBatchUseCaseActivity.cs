using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using jkerak.emailsender.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace jkerak.emailsender.Activities
{
    public class GetUsersForBatchUseCaseActivity
    {
        [FunctionName("Activities-GetUsersForBatchUseCase")]
        public static async Task<List<int>> GetUsersForBatchUseCase(
            [ActivityTrigger] IDurableActivityContext context)
        {
            var activity = context.GetInput<CustomerCommunicationUseCase>();

            string queryFilename = activity.Parameters.First(p => p.Key == "SQL").Value;
            string query;
            
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            // Connect to the storage account's blob endpoint 
            CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient client = account.CreateCloudBlobClient();
            // Create the blob storage container 
            CloudBlobContainer container = client.GetContainerReference("queries");

            // Create the blob in the container 
            var blob = container.GetBlockBlobReference(queryFilename);
            query = await blob.DownloadTextAsync();
            

            var connectionString = Environment.GetEnvironmentVariable("HUB_CONNECTION_STRING");

            using (var connection = new SqlConnection(connectionString))
            {
                return (List<int>) await connection.QueryAsync<int>(query);
            }
        }
    }
}