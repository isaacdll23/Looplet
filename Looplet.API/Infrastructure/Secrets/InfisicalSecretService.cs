using Infisical.Sdk;
using Looplet.API.Infrastructure.Secrets;

namespace Looplet.API.Infrastructure.Services;

public class InfisicalSercretService: ISecretService
{
    public string _environment;
    private string _projectId;
    private InfisicalClient _infisicalClient;

    public InfisicalSercretService()
    {
        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        _environment = !string.IsNullOrEmpty(environment) && environment == "Development" ? "Dev" : "Prod";

        _projectId = Environment.GetEnvironmentVariable("INFISICAL_LOOPLET_PROJECT_ID") ?? throw new ArgumentException("Unable to retrive environment variable: INFISICAL_LOOPLET_PROJECT_ID");

        var clientId = Environment.GetEnvironmentVariable("INFISICAL_UNIVERSAL_AUTH_CLIENT_ID") ?? throw new ArgumentException("Unable to retrive environment variable: INFISICAL_UNIVERSAL_AUTH_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("INFISICAL_UNIVERSAL_AUTH_CLIENT_SECRET") ?? throw new ArgumentException("Unable to retrive environment variable: INFISICAL_UNIVERSAL_AUTH_CLIENT_SECRET");

        var clientSettings = new ClientSettings
        {
            Auth = new AuthenticationOptions
            {
                UniversalAuth = new UniversalAuthMethod
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            }
        };

        _infisicalClient = new InfisicalClient(clientSettings);
    }

    public string? GetSecret(string secretName)
    {
        var options = new GetSecretOptions
        {
            SecretName = secretName,
            ProjectId = _projectId,
            Environment = _environment
        };

         var secret = _infisicalClient.GetSecret(options);
         return secret.ToString();
    }
}