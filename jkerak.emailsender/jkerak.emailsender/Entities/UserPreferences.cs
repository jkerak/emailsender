using Newtonsoft.Json;

namespace jkerak.emailsender.Entities
{
    public class UserPreferences
    {
        [JsonProperty]
        public bool HasUnsubscribedAll { get; set; }
    }
}