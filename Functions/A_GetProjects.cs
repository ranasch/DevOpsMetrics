namespace Metrics
{
    using Metrics.Model;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using Flurl;
    using Flurl.Http;
    using System.Collections.Generic;
    using System.Linq;

    public class A_GetProjects
    {
        private static ILogger _metricLog = null;
        private static Appsettings _config;
        private readonly string _organization;
        private readonly string _pat;

        public A_GetProjects(Appsettings appSettings)
        {
            _config = appSettings;
            _organization = _config.VSTSOrganization;
            _pat = _config.PAT;
        }

        [FunctionName(Constants.GetProjects)]
        public async Task<DevOpsProjectContext> GetProjectsAsync([ActivityTrigger] DevOpsProjectContext projectContext, ILogger metriclog)
        {
            _metricLog= metriclog;
            List<string> projectList = null;

            var result = await $"https://dev.azure.com/{_organization}"
                .AppendPathSegment("_apis/projects")
                .SetQueryParam("api-version", _config.VSTSApiVersion)
                .WithBasicAuth(string.Empty, _pat)
                .AllowAnyHttpStatus()
                .GetJsonAsync();

            if(result!=null)
            {
                projectList = new List<string>();
                foreach (var project in ((dynamic)result.value))
                    projectList.Add(project.name);
            }
            projectContext.DevOpsProjects = projectList.ToArray<string>();

            return projectContext;
        }
    }
}