using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jkerak.emailsender.Entities;
using jkerak.emailsender.Interfaces.Services;
using jkerak.emailsender.Model;
using jkerak.emailsender.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace jkerak.emailsender.Activities
{
    public class SendEmailActivity
    {
        private readonly MailgunService _mailgunService;
        private readonly ILoggingService _loggingService;

        public SendEmailActivity(MailgunService mailgunService, ILoggingService loggingService)
        {
            _mailgunService = mailgunService;
            _loggingService = loggingService;
        }

        [FunctionName("Activities-SendEmail")]
        public async Task<List<CommunicationSendRequest>> SendEmail([ActivityTrigger] IDurableActivityContext context, [DurableClient] IDurableEntityClient client)
        {
            var sendRequests = await FilterList(context.GetInput<List<CommunicationSendRequest>>(), client);
            await _mailgunService.SendEmails(sendRequests);
            await _loggingService.LogCommunication(sendRequests);

            return sendRequests;
        }

        public async Task<List<CommunicationSendRequest>> FilterList(List<CommunicationSendRequest> unfilteredRequests, IDurableEntityClient client)
        {
            var filteredRequests = new List<CommunicationSendRequest>();
            foreach (var request in unfilteredRequests)
            {
                var key = new EntityId(nameof(CommsTrackingEntity), request.PersonId.ToString());
                var entityState = await client.ReadEntityStateAsync<CommsTrackingEntity>(key);
                
                
                // If there's no entity state yet, we haven't sent them any emails so they couldn't have unsubscribed
                if (!entityState.EntityExists)
                {
                    filteredRequests.Add(request);
                    continue;
                }
                
                // Check if user has unsubscribed, don't send if so
                if (request.UseCase.CanUnsubscribe && entityState.EntityExists &&
                    entityState.EntityState.UserPreferences.HasUnsubscribedAll)
                {
                    continue;
                }

                // Check if the use case has recurrence rules, if not, send the email
                if (request.UseCase.Recurrence is null)
                {
                    filteredRequests.Add(request);
                    continue;
                }
                
                // have we sent the requested use case to this person yet?
                var comm = entityState.EntityState?.CommsList?.FirstOrDefault(c => c.UseCase == request.UseCase.Id);
                if (comm is null)
                {
                    filteredRequests.Add(request);
                }
                else
                {
                    if (PassesRecurrenceCheck(comm, request.UseCase.Recurrence))
                    {
                        filteredRequests.Add(request);
                    }
                }
            }

            return filteredRequests;
        }

        public bool PassesRecurrenceCheck(CommsTrackingModel comm, Recurrence recurrence)
        {
            DateTime cutoffDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(recurrence.Period))
            {
                switch (recurrence.Period.ToLower())
                {
                    case "second":
                        cutoffDate = cutoffDate.AddSeconds(-1);
                        break;
                    case "minute":
                        cutoffDate = cutoffDate.AddMinutes(-1);
                        break;
                    case "hour":
                        cutoffDate = cutoffDate.AddHours(-1);
                        break;
                    case "day":
                        cutoffDate = cutoffDate.AddDays(-1);
                        break;
                    case "week":
                        cutoffDate = cutoffDate.AddDays(-7);
                        break;
                    case "month":
                        cutoffDate = cutoffDate.AddMonths(-1);
                        break;
                    case "quarter":
                        cutoffDate = cutoffDate.AddMonths(-3);
                        break;
                    case "year":
                        cutoffDate = cutoffDate.AddYears(-1);
                        break;
                }
            }

            return comm.LastSent < cutoffDate
                   && (recurrence.TotalMaximumDeliveries == 0 ||
                       comm.DeliveryCount < recurrence.TotalMaximumDeliveries);
        }
    }
}