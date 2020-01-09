using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace jkerak.emailsender.Entities
{
    public interface ICommsTrackingEntity
    {
        [Deterministic]
        void Track(string useCase);

        [Deterministic]
        void UnsubscribeAll();

        [Deterministic]
        void ResubscribeAll();
    }
}