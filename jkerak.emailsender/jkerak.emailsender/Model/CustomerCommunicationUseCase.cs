using System.Collections.Generic;

namespace jkerak.emailsender.Model
{
    public class CustomerCommunicationUseCase
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public bool IsEnabled { get; set; }
        public string PersonIdPath { get; set; }
        public string TriggerTopic { get; set; }
        public List<KeyValuePair<string,string>> Parameters { get; set; }
        public Filter Filter { get; set; }
        public Recurrence Recurrence { get; set; }
        public List<KeyValuePair<string,string>> TemplateMessageParameters { get; set; }
        public bool CanUnsubscribe { get; set; }
    }
    
    public class Filter
    {
        public string Predicate { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }

    public class Recurrence
    {
        public string Period { get; set; }
        public int TotalMaximumDeliveries { get; set; }
    }


}