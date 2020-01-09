using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jkerak.emailsender.Interfaces.Services;
using jkerak.emailsender.Model;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;

namespace jkerak.emailsender.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly EventHubClient _client;

        public LoggingService()
        {
            string EventHubConnectionString = Environment.GetEnvironmentVariable("LOGGING_EVENT_HUB");
            string EventHubName = Environment.GetEnvironmentVariable("EVENT_HUB_NAME");

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString) { EntityPath = EventHubName };

            _client = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }


        public async Task LogCommunication(List<CommunicationSendRequest> sendRequests)
        {
            var tasks = sendRequests.Select(
                    r => _client.SendAsync(FormatMessage(r)));

                await Task.WhenAll(tasks);
        }


        public async Task LogUnsubscribe(string personId, string useCase)
        {
            await _client.SendAsync(FormatMessage(new UnsubscribeInfo(personId, useCase,DateTime.UtcNow)));
        }

        private EventData FormatMessage(UnsubscribeInfo request)
        {
            string message = JsonConvert.SerializeObject(request);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            return new EventData(messageBytes);

        }



        private EventData FormatMessage(CommunicationSendRequest request)
        {
            var logEntry = new
            {
                request.PersonId,
                request.UserContactInfo.EmailAddress,
                request.UseCase.Id,
                request.UseCase.Type,
                Channel = "Email",
                TimeStamp = DateTime.UtcNow
            };
            string message = JsonConvert.SerializeObject(logEntry);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            return new EventData(messageBytes);

        }
    }
}