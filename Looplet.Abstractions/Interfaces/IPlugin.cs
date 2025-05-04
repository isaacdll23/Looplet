using Microsoft.Extensions.DependencyInjection;

namespace Looplet.Abstractions.Interfaces;

public interface IPlugin
{
    void ConfigureServices(IServiceCollection services);
}
