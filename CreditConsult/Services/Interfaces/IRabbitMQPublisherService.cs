using CreditConsult.DTOs;

namespace CreditConsult.Services.Interfaces;

public interface IRabbitMQPublisherService
{
    Task PublishMessageAsync(CreditConsultRequestDto message, CancellationToken cancellationToken = default);
    Task PublishMessagesAsync(IEnumerable<CreditConsultRequestDto> messages, CancellationToken cancellationToken = default);
}

