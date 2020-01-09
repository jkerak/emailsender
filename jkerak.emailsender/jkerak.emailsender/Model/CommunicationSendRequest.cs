using System.Collections.Generic;

namespace jkerak.emailsender.Model
{
    public class CommunicationSendRequest
    {
        public int PersonId { get; set; }
        public CustomerCommunicationUseCase UseCase { get; set; }
        public UserContactInfo UserContactInfo { get; set; }
        public string FromEmail { get; set; } = "";
        public List<KeyValuePair<string,string>> Parameters { get; set; }
        public string MailgunResponse { get; set; }
    }
}