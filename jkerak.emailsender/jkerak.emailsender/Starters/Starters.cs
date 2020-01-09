using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace jkerak.emailsender.Starters
{
    public class Starters
    {
        [FunctionName("ManagerStarter")]
        public async Task<IActionResult> Start(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter
        )
        {
            int batchSize = Convert.ToInt32(Environment.GetEnvironmentVariable("USER_BATCH_SIZE"));
            int taskBatchSize = Convert.ToInt32(Environment.GetEnvironmentVariable("TASK_BATCH_SIZE"));

            string instanceId = await starter.StartNewAsync("Orchestrators-ScheduledCommunication", new {batchSize, taskBatchSize});
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("TimerStarter")]
        public async Task TimerStarter(
            [TimerTrigger("0 0 10 * * *")] TimerInfo timerInfo,
            [DurableClient] IDurableOrchestrationClient starter
        )
        {
            int batchSize = Convert.ToInt32(Environment.GetEnvironmentVariable("USER_BATCH_SIZE"));
            int taskBatchSize = Convert.ToInt32(Environment.GetEnvironmentVariable("TASK_BATCH_SIZE"));

            string instanceId = await starter.StartNewAsync("Orchestrators-ScheduledCommunication", new {batchSize, taskBatchSize});
            //TODO Log this to slack? return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("ServiceBusStarter")]
        public async Task SendCommunication(
            [ServiceBusTrigger("send-communication", "Partner", Connection = "ServiceBusConnectionString")]
            Message message, [DurableClient] IDurableOrchestrationClient starter)
        {
            string instanceId = await starter.StartNewAsync("Orchestrators-TriggeredCommunication",
                message);
        }
    }
}