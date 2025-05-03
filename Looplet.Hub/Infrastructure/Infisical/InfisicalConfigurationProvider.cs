using Infisical.Sdk;

namespace Looplet.Hub.Infrastructure.Infisical;

public class InfisicalConfigurationProvider(InfisicalOptions _options) : ConfigurationProvider
{
    public override void Load()
    {
        var infisicalService = new InfisicalService(_options);

        SecretElement[] secrets = infisicalService.ListSecrets();

        foreach (SecretElement secret in secrets)
        {
            Data[secret.SecretKey] = secret.SecretValue;
        }
    }
}

