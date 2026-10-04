using CodeBase.Application.Common.Models;

namespace CodeBase.Application.Abstractions.Communication;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
