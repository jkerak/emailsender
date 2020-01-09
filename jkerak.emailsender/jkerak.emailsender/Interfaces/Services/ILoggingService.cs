using System.Collections.Generic;
using System.Threading.Tasks;
using jkerak.emailsender.Model;

namespace jkerak.emailsender.Interfaces.Services
{
    public interface ILoggingService
    {
        Task LogCommunication(List<CommunicationSendRequest> sendRequests);
        Task LogUnsubscribe(string personId, string useCase);
    }
}