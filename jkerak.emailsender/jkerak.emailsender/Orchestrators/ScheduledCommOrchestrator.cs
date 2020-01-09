using System.Dynamic;
using System.Threading.Tasks;
using jkerak.emailsender.Services;
using jkerak.emailsender.Interfaces.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace jkerak.emailsender.Orchestrators
{
    public class ScheduledCommOrchestrator
    {
        private readonly IActivityService _activityService;

        public ScheduledCommOrchestrator(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [FunctionName("Orchestrators-ScheduledCommunication")]
        public async Task ScheduledCommunicationOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {

            dynamic progress = new ExpandoObject();
            progress.StartTime = context.CurrentUtcDateTime;
            context.SetCustomStatus(progress);

            // Get Activities
            var useCases = await _activityService.GetScheduledActivities(context);
            progress.RetrievedActivities = context.CurrentUtcDateTime;
            context.SetCustomStatus(progress);

            // Get Comms Tasks
            var taskBatches = await _activityService.GetUsersForBatchUseCase(context, useCases);
            progress.RetrievedUsers = context.CurrentUtcDateTime;
            progress.TotalBatchCount = taskBatches.Count;
            progress.CompletedBatchCount = 0;
            context.SetCustomStatus(progress);

            foreach (var batch in taskBatches)
            {
                await _activityService.SendComms(context, batch);
                progress.CompletedBatchCount++;
                context.SetCustomStatus(progress);
            }

            progress.EndTime = context.CurrentUtcDateTime;
            progress.TotalDuration = (progress.EndTime - progress.StartTime).TotalSeconds;
            context.SetCustomStatus(progress);
        }
    }
}