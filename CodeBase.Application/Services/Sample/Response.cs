namespace CodeBase.Application.Services.Sample;

public static class Response
{
    public sealed record EchoResponse(string Message, DateTimeOffset CreatedAt);
}
