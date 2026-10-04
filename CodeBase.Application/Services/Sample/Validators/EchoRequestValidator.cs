using FluentValidation;

namespace CodeBase.Application.Services.Sample.Validators;

public sealed class EchoRequestValidator : AbstractValidator<Request.EchoRequest>
{
    public EchoRequestValidator()
    {
        RuleFor(request => request.Message)
            .NotEmpty()
            .MaximumLength(500);
    }
}
