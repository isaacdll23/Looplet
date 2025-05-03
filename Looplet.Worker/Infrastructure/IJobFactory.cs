using Looplet.Abstractions.Interfaces;

namespace Looplet.Worker.Infrastructure;

public interface IJobFactory
{
    IJob Create(string jobType);
}
