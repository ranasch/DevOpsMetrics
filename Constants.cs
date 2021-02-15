namespace Metrics
{
    internal class Constants
    {
        public const string StorageQueueName = "metric-crawler-trigger";
        public const string OrchestratorTrigger = "O_CollectMetrics_Start";
        public const string OrchstratorDurableFunction = "O_CollectMetrics";
        public const string GetProjects = "A_GetProjects";
        public const string GetProjectMetrics = "A_GetProjectMetrics";
        public const string GetUserMetrics = "A_GetUserMetrics";
    }
}
