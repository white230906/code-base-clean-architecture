using FluentValidation;

namespace CodeBase.Application.Services.Sample;

public sealed class Service : IService
{
    private readonly IValidator<Request.EchoRequest> _validator;

    public Service(IValidator<Request.EchoRequest> validator)
    {
        _validator = validator;
    }

    public async Task<Response.EchoResponse> EchoAsync(
        Request.EchoRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        return new Response.EchoResponse(
            request.Message.Trim(),
            DateTimeOffset.UtcNow);
    }
}
