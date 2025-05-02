namespace Looplet.API.Infrastructure.Infisical;

public class InfisicalConfigurationProvider(InfisicalOptions _options) : ConfigurationProvider
{
    public override void Load()
    {
        var infisicalService = new InfisicalService(_options);

        var secrets = infisicalService.ListSecrets();

        foreach (var secret in secrets)
        {
            Data[secret.SecretKey] = secret.SecretValue;
        }
    }
}

