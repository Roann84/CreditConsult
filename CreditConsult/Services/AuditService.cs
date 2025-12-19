using CreditConsult.DTOs;
using CreditConsult.Services.Interfaces;

namespace CreditConsult.Services;

public class AuditService
{
    private readonly IAuditPublisherService _publisher;
    private readonly ILogger<AuditService> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AuditService(
        IAuditPublisherService publisher,
        ILogger<AuditService> logger,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogConsultationAsync(
        string operation,
        object? queryData,
        object? resultData = null,
        CancellationToken cancellationToken = default)
    {
        if (!_publisher.IsAvailable)
        {
            _logger.LogDebug("Audit Publisher não está disponível. Pulando publicação de evento.");
            return;
        }

        try
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var userId = httpContext?.User?.Identity?.Name;

            var auditEvent = new AuditEventDto
            {
                EventType = "ConsultationRequest",
                EntityType = "CreditConsult",
                Operation = operation,
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                IpAddress = ipAddress,
                Data = new
                {
                    Query = queryData,
                    Result = resultData
                },
                Metadata = new Dictionary<string, object>
                {
                    { "RequestPath", httpContext?.Request?.Path.ToString() ?? string.Empty },
                    { "RequestMethod", httpContext?.Request?.Method ?? string.Empty }
                }
            };

            await _publisher.PublishAuditEventAsync(auditEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar evento de auditoria. Operation: {Operation}", operation);
        }
    }
}

