using CodeBase.Application.Common.Models;

namespace CodeBase.Application.Abstractions.Media;

public interface IFileStorage
{
    Task<string> UploadAsync(FileUpload file, CancellationToken cancellationToken = default);
}
