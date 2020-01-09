using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace jkerak.emailsender.Entities
{
    
    public class CommsTrackingEntity : ICommsTrackingEntity
    {
        [JsonProperty] public List<CommsTrackingModel> CommsList { get; set; }
        [JsonProperty] public UserPreferences UserPreferences { get; set; }
        

        public void Track(string useCase)
        {
            if (CommsList.Any())
            {
                var index = CommsList.FindIndex(c => c.UseCase.ToLower() == useCase.ToLower());
                if (index != -1)
                {
                    CommsList[index].LastSent = (DateTime.UtcNow);
                    CommsList[index].DeliveryCount++;
                }
                else
                {
                    CommsList.Add(
                        new CommsTrackingModel()
                        {
                            UseCase = useCase,
                            LastSent = DateTime.UtcNow,
                            DeliveryCount = 1
                        }
                    );
                }
            }
            else
            {
                CommsList.Add(
                    new CommsTrackingModel()
                    {
                        UseCase = useCase,
                        LastSent = DateTime.UtcNow,
                        DeliveryCount = 1
                    }
                );
            }
        }
        public void UnsubscribeAll()
        {
            UserPreferences.HasUnsubscribedAll = true;
        }

        public void ResubscribeAll()
        {
            UserPreferences.HasUnsubscribedAll = false;
        }


        [FunctionName(nameof(CommsTrackingEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            if (!context.HasState)
            {
                context.SetState(new CommsTrackingEntity()
                {
                    CommsList = new List<CommsTrackingModel>(),
                    UserPreferences = new UserPreferences()
                });
            }
            return context.DispatchAsync<CommsTrackingEntity>();
        }
    }
}