using Microsoft.Extensions.DependencyInjection;

namespace Looplet.Abstractions.Interfaces;

public interface IJobModule
{
    void ConfigureServices(IServiceCollection services);
}
