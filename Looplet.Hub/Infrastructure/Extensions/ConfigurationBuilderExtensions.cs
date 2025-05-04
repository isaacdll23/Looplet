using Looplet.Hub.Infrastructure.Infisical;

namespace Looplet.Hub.Extensions;

public static class InfisicalConfigurationExtensions
{
    public static IConfigurationBuilder AddInfisical(this IConfigurationBuilder builder, Action<InfisicalOptions> configure)
    {
        builder.Sources.Add(new InfisicalConfigurationSource(configure));
        return builder;
    }
}
