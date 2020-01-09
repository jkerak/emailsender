using System;

namespace jkerak.emailsender.Model
{
    public class UnsubscribeInfo
    {
        public string PersonId { get; set; }
        public string UseCaseId { get; set; }
        public DateTime UnsubscribeDateTime { get; set; }
        
        public UnsubscribeInfo(string personId, string useCaseId, DateTime unsubscribeDateTime){
            UseCaseId = useCaseId;
            UnsubscribeDateTime = unsubscribeDateTime;
            PersonId = personId;
        }
    }
}