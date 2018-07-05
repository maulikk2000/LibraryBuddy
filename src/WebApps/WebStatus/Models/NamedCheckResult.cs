using Microsoft.Extensions.HealthChecks;

namespace WebStatus.Models
{
    public class NamedCheckResult
    {
        public string Name { get; }
        public IHealthCheckResult Result { get; }

        public NamedCheckResult(string name, IHealthCheckResult result)
        {
            Name = name;
            Result = result;
        }
    }
}