
namespace Metrics.Model
{
    using System.Collections.Generic;

    public class DevOpsProjectContext
    {
        public IEnumerable<string> DevOpsProjects { get; set; }
        public string CurrentProject { get; set; }
        public double TotalRepositories { get; set; }
        public double RegisteredUser { get; set; }
    }
}
