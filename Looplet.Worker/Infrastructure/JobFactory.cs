using Looplet.Abstractions.Interfaces;

namespace Looplet.Worker.Infrastructure;

public class JobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    public JobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IJob Create(string jobType)
    {
        var jobTypeInstance = Type.GetType(jobType);
        if (jobTypeInstance == null)
        {
            throw new ArgumentException($"Job type '{jobType}' not found.");
        }

        return (IJob)ActivatorUtilities.CreateInstance(_serviceProvider, jobTypeInstance);
    }
}
