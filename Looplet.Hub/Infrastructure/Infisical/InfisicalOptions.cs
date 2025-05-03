namespace Looplet.Hub.Infrastructure.Infisical;

public class InfisicalOptions
{
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string ProjectId { get; set; } = default!;
    public string Environment { get; set; } = "Prod";
    public string SiteUrl { get; set; } = default!;
}
