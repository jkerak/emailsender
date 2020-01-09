using System.Collections.Generic;
using System.Threading.Tasks;
using jkerak.emailsender.Model;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace jkerak.emailsender.Interfaces.Services
{
    public interface IActivityService
    {
        [Deterministic] Task<List<CustomerCommunicationUseCase>> GetScheduledActivities(IDurableOrchestrationContext context);
        [Deterministic] Task<List<List<List<CommunicationSendRequest>>>> GetUsersForBatchUseCase(IDurableOrchestrationContext context, List<CustomerCommunicationUseCase> useCases);
        [Deterministic] Task SendComms(IDurableOrchestrationContext context, List<List<CommunicationSendRequest>> batch);
    }
}