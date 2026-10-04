namespace CodeBase.Application.Common.Models;

public sealed record FileUpload(
    string FileName,
    string ContentType,
    long Length,
    Stream Content);
