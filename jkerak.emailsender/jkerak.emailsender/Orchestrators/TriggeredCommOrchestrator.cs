using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jkerak.emailsender.Services;
using jkerak.emailsender.Model;
using jkerak.emailsender.Utilities;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json.Linq;

namespace jkerak.emailsender.Orchestrators
{
    public class TriggeredCommOrchestrator
    {

 [FunctionName("Orchestrators-TriggeredCommunication")]
        public async Task TriggeredCommunicationOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var message = context.GetInput<Message>();

            string deserializedBody = Helpers.DeserializeServiceBusMessage(message);
            
            if (message.UserProperties.TryGetValue("topicSource", out var topicSource))
            {
                // TODO: store config json as durable entity so we don't have to read the blob every time
                var sendRequests =
                    (await context.CallActivityAsync<List<CustomerCommunicationUseCase>>("Activities-GetActivities",
                        "Config.json"))
                    .Where(a => a.IsEnabled)
                    .Where(a => a.Type.ToLower() == "triggered")
                    .Where(a => a.TriggerTopic == (string) topicSource)
                    .Where(a => ApplyFilter(a, deserializedBody))
                    .Select(a => GetSendRequest(a, deserializedBody)).ToList();

                if (sendRequests.Any())
                {
                    var contactInfo = await context.CallActivityAsync<List<CommunicationSendRequest>>(
                        "Activities-GetUserContactInfo",
                        sendRequests);

                    await context.CallActivityAsync<List<CommunicationSendRequest>>("Activities-SendEmail",
                        contactInfo);
                }
            }
        }

        [Deterministic]
        private static bool ApplyFilter(CustomerCommunicationUseCase useCase, string message)
        {
            var filter = useCase.Filter;

            var jmessage = JObject.Parse(message);

            var predicate = jmessage.SelectToken(filter.Predicate).ToString();

            return predicate.ToLower() == filter.Value.ToLower();
        }

        [Deterministic]
        private static CommunicationSendRequest GetSendRequest(CustomerCommunicationUseCase useCase, string message)
        {
            var sendRequest = new CommunicationSendRequest()
            {
                UseCase = useCase,
                PersonId = JObject.Parse(message).SelectToken(useCase.PersonIdPath).ToObject<int>(),
                Parameters = new List<KeyValuePair<string, string>>()
            };

            var templateParams = useCase.TemplateMessageParameters;

            var jmessage = JObject.Parse(message);

            if (templateParams != null)
            {
                foreach (var templateParamsKey in templateParams)
                {
                    var value = jmessage.SelectToken(templateParamsKey.Value).ToString();
                    if (templateParamsKey.Key.ToLower().Contains("amount")) { value = value.FormatCurrency(); }
                        sendRequest.Parameters.Add(new KeyValuePair<string, string>(templateParamsKey.Key, value));
                }
            }

            return sendRequest;
        }


    }
}