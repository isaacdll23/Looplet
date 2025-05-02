namespace Looplet.Abstractions.Interfaces;

public interface IJobFactory
{
    IJob Create(string jobType);
}
