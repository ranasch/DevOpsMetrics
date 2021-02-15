using System.Threading.Tasks;
using Metrics.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace Metrics
{
    public class O_DevOpsMetricsCollector
    {
        private static Appsettings _config;
        private static ILogger _metricLog = null;

        public O_DevOpsMetricsCollector(Appsettings appSettings)
        {
            _config = appSettings;
        }

        /// <summary>
        /// Queuetrigger
        /// </summary>
        /// <param name="myQueueItem"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName(Constants.OrchestratorTrigger)]
        public async Task StartMetricCollection(
            [QueueTrigger(Constants.StorageQueueName)] string myQueueItem, 
            [DurableClient] IDurableOrchestrationClient starter, 
            ILogger log)
        {
            _metricLog = log;

            string instanceId = await starter.StartNewAsync("O_CollectMetrics");

            log.LogDebug($"*** Started orchestration with ID = '{instanceId}'. ***");

            var status = starter.GetStatusAsync(instanceId);

            log.LogDebug("*** Orchestration {0} status: {@DurableOrchestrationStatus} ***", instanceId, status);
        }

        [FunctionName(Constants.OrchstratorDurableFunction)]
        public async Task RunOrchestratorAsync([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger metriclog)
        {
            _metricLog = metriclog;
            _metricLog.LogInformation("*** Start metric crawling ***");
            var startTime = context.CurrentUtcDateTime;
            var projectContext = new DevOpsProjectContext();

            if (!context.IsReplaying)
            {
                metriclog.LogInformation($"*** Start tracking Azure DevOps project metrics {startTime} ***");
            }

            // get all projects for orga
            projectContext= await context.CallActivityAsync<DevOpsProjectContext>(Constants.GetProjects, projectContext);

            // iterate projects for collecting project KPIs
            foreach (var currentProject in projectContext?.DevOpsProjects)
            {
                _metricLog.LogDebug($"*** Crawling KPIs for project {currentProject} ***");
                projectContext.CurrentProject = currentProject;
                projectContext= await context.CallActivityAsync<DevOpsProjectContext>(Constants.GetProjectMetrics, projectContext);
            }

            // collect user KPIs
            projectContext= await context.CallActivityAsync<DevOpsProjectContext>(Constants.GetUserMetrics, projectContext);

            _metricLog.LogMetric("DevOps Repositories", projectContext.TotalRepositories);
            _metricLog.LogMetric("DevOps Registered Users", projectContext.RegisteredUser);

            metriclog.LogInformation("*** Project Metrics complete ***");
        }
    }
}
