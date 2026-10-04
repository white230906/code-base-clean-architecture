namespace CodeBase.Application.Common.Exceptions;

public sealed class AppException : Exception
{
    public AppException(
        ErrorType errorType,
        string messageCode,
        string detail,
        object? errors = null,
        Exception? innerException = null)
        : base(detail, innerException)
    {
        ErrorType = errorType;
        MessageCode = messageCode;
        Errors = errors;
    }

    public ErrorType ErrorType { get; }
    public string MessageCode { get; }
    public object? Errors { get; }
}
