namespace Looplet.Shared.Interfaces;

public interface IJobFactory
{
    IJob Create(string jobType);
}
