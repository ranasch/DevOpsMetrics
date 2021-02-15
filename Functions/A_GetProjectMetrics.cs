namespace Metrics.Functions
{
    using Flurl;
    using Flurl.Http;
    using Metrics.Model;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class A_GetProjectMetrics
    {
        private static ILogger _metricLog = null;
        private static Appsettings _config;
        private readonly string _organization;
        private readonly string _pat;

        public A_GetProjectMetrics(Appsettings appSettings)
        {
            _config = appSettings;
            _organization = _config.VSTSOrganization;
            _pat = _config.PAT;
        }

        [FunctionName(Constants.GetProjectMetrics)]
        public async Task<DevOpsProjectContext> GetProjectMetricsAsync([ActivityTrigger] DevOpsProjectContext context, ILogger metriclog)
        {
            _metricLog = metriclog;

            var gitRepos = 0.0;


            var reposRequests = await $"https://dev.azure.com/{_organization}/{context.CurrentProject}"
                .AppendPathSegment("_apis/git/repositories")
                .SetQueryParam("api-version", "6.0-preview.1")
                .WithBasicAuth(string.Empty, _pat)
                .AllowAnyHttpStatus()
                .GetJsonAsync();

            if (reposRequests != null)
            {
                try
                {
                    gitRepos = (double)((dynamic)reposRequests)?.count;
                }
                catch (Exception ex)
                {
                    _metricLog.LogError(ex, $"*** Failed to log Repos for {context.CurrentProject} ***");
                    gitRepos = 0.0;
                }

                context.TotalRepositories += gitRepos;
            }
            return context;
        }
    }
}
