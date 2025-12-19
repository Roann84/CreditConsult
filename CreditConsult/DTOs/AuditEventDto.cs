namespace CreditConsult.DTOs;

public class AuditEventDto
{
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public object? Data { get; set; }
}

