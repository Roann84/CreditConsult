using CreditConsult.DTOs;

namespace CreditConsult.Services.Interfaces;

public interface IAuditPublisherService
{
    Task PublishAuditEventAsync(AuditEventDto auditEvent, CancellationToken cancellationToken = default);
    bool IsAvailable { get; }
}

