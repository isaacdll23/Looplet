using Infisical.Sdk;

namespace Looplet.API.Infrastructure.Infisical;

public class InfisicalService
{
    public string _environment;
    private string _projectId;
    private InfisicalClient _infisicalClient;

    public InfisicalService(InfisicalOptions options)
    {
        _environment = options.Environment;
        _projectId = options.ProjectId;

        var clientSettings = new ClientSettings
        {
            Auth = new AuthenticationOptions
            {
                UniversalAuth = new UniversalAuthMethod
                {
                    ClientId = options.ClientId,
                    ClientSecret = options.ClientSecret
                }
            },
            SiteUrl = options.SiteUrl
        };

        _infisicalClient = new InfisicalClient(clientSettings);
    }

    public SecretElement[] ListSecrets()
    {
        var options = new ListSecretsOptions
        {
            ProjectId = _projectId,
            Environment = _environment,
        };

        return _infisicalClient.ListSecrets(options);
    }    
}