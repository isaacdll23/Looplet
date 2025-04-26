using Looplet.Shared.Interfaces;
using System.Reflection;

namespace Looplet.API.Infrastructure;

public interface IJobTypeCatalog
{
    IReadOnlyList<string> GetAll();
}
public class JobTypeCatalog : IJobTypeCatalog
{
    private readonly List<string> _jobTypes = [];

    public JobTypeCatalog()
    {
        var asm = Assembly.Load("Looplet.Jobs");
        var jobTypes = asm.GetTypes()
            .Where(t => typeof(IJob).IsAssignableFrom(t)
            && !t.IsAbstract
            && !t.IsInterface)
            .Select(t => $"{t.FullName}, {asm.GetName().Name}")
            .ToList();

        _jobTypes.AddRange(jobTypes);
    }

    public IReadOnlyList<string> GetAll() => _jobTypes;
}
