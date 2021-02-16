using Microsoft.Azure.Functions.Extensions.DependencyInjection;
[assembly: FunctionsStartup(typeof(Metrics.Startup))]

namespace Metrics
{
    using Microsoft.Azure.Storage;
    using Microsoft.Azure.Storage.Queue;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class Startup: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()
               .AddUserSecrets<Startup>(true, true)
               .Build();

            var appSettings = new Appsettings()
            {
                PAT = config["PAT"],
                VSTSApiVersion = config["VSTSApiVersion"],
                VSTSOrganization = config["VSTSOrganization"]
            };
            //var appSettings = config.GetSection("AppSettings").Get<Appsettings>();
            builder.Services.AddSingleton(appSettings);

            // Create queue if not exists
            var storage = config.GetValue<string>("AzureWebJobsStorage");
            var storageAccount = CloudStorageAccount.Parse(storage);
            var qc = storageAccount.CreateCloudQueueClient();
            var queue = qc.GetQueueReference(Constants.StorageQueueName);
            queue.CreateIfNotExistsAsync(null, null).Wait();
        }
    }
}
