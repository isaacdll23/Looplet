namespace Looplet.Hub.Infrastructure.Infisical;

public class InfisicalConfigurationSource : IConfigurationSource
{
    private readonly InfisicalOptions _options;
    public InfisicalConfigurationSource(Action<InfisicalOptions> configureOptions)
    {
        _options = new InfisicalOptions();
        configureOptions(_options);
    }
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new InfisicalConfigurationProvider(_options);
}
