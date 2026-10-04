using System.ComponentModel.DataAnnotations;

namespace CodeBase.Infrastructure.Communication;

public sealed class MailOptions
{
    public const string SectionName = "Mail";

    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    public bool UseSsl { get; init; } = true;

    [Required, EmailAddress]
    public string FromAddress { get; init; } = string.Empty;

    [Required]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    public string UserName { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
