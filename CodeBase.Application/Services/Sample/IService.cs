namespace CodeBase.Application.Services.Sample;

public interface IService
{
    Task<Response.EchoResponse> EchoAsync(
        Request.EchoRequest request,
        CancellationToken cancellationToken = default);
}
