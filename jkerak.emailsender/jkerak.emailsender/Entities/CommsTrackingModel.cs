using System;
using Newtonsoft.Json;

namespace jkerak.emailsender.Entities
{
    public class CommsTrackingModel
    {
        [JsonProperty]
        public string UseCase { get; set; }
        
        [JsonProperty]
        public int DeliveryCount { get; set; }
        
        [JsonProperty]
        public DateTime LastSent { get; set; }
    }
}