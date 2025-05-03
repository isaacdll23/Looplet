using System.ComponentModel.DataAnnotations;

public class RegisterWorkerDto
{
    [Required]
    public string Alias { get; set; } = default!;
    [Required]
    public string BaseUrl { get; set; } = default!;
}
