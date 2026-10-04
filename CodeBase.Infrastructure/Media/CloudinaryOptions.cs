using System.ComponentModel.DataAnnotations;

namespace CodeBase.Infrastructure.Media;

public sealed class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    [Required]
    public string CloudName { get; init; } = string.Empty;

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Required]
    public string ApiSecret { get; init; } = string.Empty;

    [Range(1, 100_000_000)]
    public long MaxFileSizeBytes { get; init; } = 10_000_000;
}
