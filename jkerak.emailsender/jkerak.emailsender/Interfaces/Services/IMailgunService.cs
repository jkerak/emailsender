using System.Collections.Generic;
using System.Threading.Tasks;
using jkerak.emailsender.Model;

namespace jkerak.emailsender.Interfaces.Services
{
    public interface IMailgunService
    {
        Task RefreshTemplates(string templateString, string fileName);
        Task SendEmails(List<CommunicationSendRequest> sendRequests);
    }
}