using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jkerak.emailsender.Entities;
using jkerak.emailsender.Interfaces.Services;
using jkerak.emailsender.Model;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace jkerak.emailsender.Services
{
    public class ActivityService : IActivityService
    {
        [Deterministic]
        public async Task<List<CustomerCommunicationUseCase>> GetScheduledActivities(
            IDurableOrchestrationContext context)
        {
            var useCases =
                (await context.CallActivityAsync<List<CustomerCommunicationUseCase>>("Activities-GetActivities",
                    "Config.json"))
                .Where(a => a.IsEnabled)
                .Where(a => a.Type.ToLower() == "scheduled")
                .ToList();

            return useCases;
        }

        [Deterministic]
        public async Task<List<List<List<CommunicationSendRequest>>>> GetUsersForBatchUseCase(
            IDurableOrchestrationContext context,
            List<CustomerCommunicationUseCase> useCases)
        {

            dynamic input = context.GetInput<object>();

            int batchSize = input.batchSize;
            int taskBatchSize = input.taskBatchSize;

            var requests = new List<CommunicationSendRequest>();

            foreach (var useCase in useCases)
            {
                var sendRequests =
                    (await context.CallActivityAsync<List<int>>("Activities-GetUsersForBatchUseCase", useCase)).Select(
                        p =>
                            new CommunicationSendRequest { PersonId = p, UseCase = useCase, Parameters = new List<KeyValuePair<string, string>>() }).ToList();
                requests.AddRange(sendRequests);
            }

            // Split into batches 
            var batches = new List<List<CommunicationSendRequest>>();

            for (int i = 0; i < requests.Count; i += batchSize)
            {
                batches.Add(requests.GetRange(i, Math.Min(batchSize, requests.Count - i)));
            }

            var batchOfBatches = new List<List<List<CommunicationSendRequest>>>();

            for (int i = 0; i < batches.Count; i += taskBatchSize)
            {
                batchOfBatches.Add(batches.GetRange(i, Math.Min(taskBatchSize, batches.Count - i)));
            }

            return batchOfBatches;
        }

        [Deterministic]
        public async Task SendComms(IDurableOrchestrationContext context, List<List<CommunicationSendRequest>> batch)
        {
            var taskBatch = batch
                .Select(b =>
                    context.CallActivityAsync<List<CommunicationSendRequest>>("Activities-GetUserContactInfo", b))
                .ToList();

            while (taskBatch.Any())
            {
                var task = await Task.WhenAny(taskBatch);
                taskBatch.Remove(task);
                var result = await task;
                var emailsSent =
                    await context.CallActivityAsync<List<CommunicationSendRequest>>("Activities-SendEmail", result);

                foreach (var emailSent in emailsSent)
                {
                    LogEmailSent(context, emailSent);
                }
            }
        }

        [Deterministic]
        private void LogEmailSent(IDurableOrchestrationContext client, CommunicationSendRequest request)
        {
            var key = new EntityId(nameof(CommsTrackingEntity), request.PersonId.ToString());

            var proxy = client.CreateEntityProxy<ICommsTrackingEntity>(key);

            proxy.Track(request.UseCase.Id);
        }
    }
}