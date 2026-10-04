using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CodeBase.Application.Abstractions.Media;
using CodeBase.Application.Common.Exceptions;
using CodeBase.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace CodeBase.Infrastructure.Media;

public sealed class CloudinaryFileStorage : IFileStorage
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _options;

    public CloudinaryFileStorage(IOptions<CloudinaryOptions> options)
    {
        _options = options.Value;
        _cloudinary = new Cloudinary(new Account(
            _options.CloudName,
            _options.ApiKey,
            _options.ApiSecret));
    }

    public async Task<string> UploadAsync(
        FileUpload file,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName);
        var isImage = file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

        if (file.Length <= 0 || file.Length > _options.MaxFileSizeBytes ||
            !isImage || !AllowedExtensions.Contains(extension))
        {
            throw new AppException(
                ErrorType.Validation,
                "INVALID_IMAGE",
                "The uploaded image is empty, too large, or has an unsupported format.");
        }

        var result = await _cloudinary.UploadAsync(
            new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.Content)
            },
            cancellationToken);

        if (result.Error is not null)
        {
            throw new AppException(
                ErrorType.Unexpected,
                "MEDIA_UPLOAD_FAILED",
                result.Error.Message);
        }

        return result.SecureUrl.ToString();
    }
}
