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

    public class A_GetUserMetrics
    {
        private static ILogger _metricLog = null;
        private static Appsettings _config;
        private readonly string _organization;
        private readonly string _pat;

        public A_GetUserMetrics(Appsettings appSettings)
        {
            _config = appSettings;
            _organization = _config.VSTSOrganization;
            _pat = _config.PAT;
        }

        [FunctionName(Constants.GetUserMetrics)]
        public async Task<DevOpsProjectContext> GetUserMetrics([ActivityTrigger] DevOpsProjectContext context, ILogger metriclog)
        {
            _metricLog = metriclog;

            var userEntitlementsUri = new string($"https://vsaex.dev.azure.com/{_organization}/_apis/userentitlements?top=10000&api-version=5.1-preview.2");

            try
            {
                dynamic userList = await userEntitlementsUri
                        .WithBasicAuth(string.Empty, _pat)
                        .AllowAnyHttpStatus()
                        .GetJsonAsync()
                        .ConfigureAwait(false);

                context.RegisteredUser = (double)((dynamic)userList).totalCount;                

                double activeUsers = 0;
                double basicUsers = 0;
                double usersLoginLast4weeks = 0;
                double usersLoginLast3month = 0;
                double msdnLicenses = 0;

                foreach (var user in ((dynamic)userList).items)
                {
                    try
                    {
                        DateTime lastAccess = ((dynamic)user).lastAccessedDate;
                        var license = ((dynamic)user).accessLevel.licenseDisplayName;
                        var status = ((dynamic)user).accessLevel.status;
                        var msdn = ((dynamic)user).accessLevel.msdnLicenseType;

                        if (lastAccess >= DateTime.Now.AddDays(-30.0)) ++usersLoginLast4weeks;
                        if (lastAccess >= DateTime.Now.AddDays(-90.0)) ++usersLoginLast3month;
                        if (license.ToLower() == "basic") ++basicUsers;
                        if (status.ToLower() == "active") ++activeUsers;
                        if (msdn.ToLower() != "none") ++msdnLicenses;
                    }
                    catch (Exception ex)
                    {
                        _metricLog.LogError(ex, $"*** Could not parse user {user} ***");
                    }
                }
                _metricLog.LogMetric("DevOps Active Users", activeUsers);
                _metricLog.LogMetric("DevOps Basic Users", basicUsers);
                _metricLog.LogMetric("DevOps MSDN Users", msdnLicenses);
                _metricLog.LogMetric("DevOps Access last 30 days", usersLoginLast4weeks);
                _metricLog.LogMetric("DevOps Access last 90 days", usersLoginLast3month);
            }
            catch (Exception ex)
            {
                _metricLog.LogError(ex, $"*** Failed to load users {userEntitlementsUri} ***");
            }

            _metricLog.LogDebug("*** Active User Metric complete ***");

            return context;
        }
    }
}
