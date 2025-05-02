namespace Looplet.API.Infrastructure.Secrets;

public interface ISecretService {
    string? GetSecret(string secretName);
}