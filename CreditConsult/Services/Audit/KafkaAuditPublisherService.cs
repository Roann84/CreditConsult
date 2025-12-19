using Confluent.Kafka;
using CreditConsult.DTOs;
using CreditConsult.Services.Interfaces;
using System.Text.Json;

namespace CreditConsult.Services.Audit;

public class KafkaAuditPublisherService : IAuditPublisherService, IDisposable
{
    private readonly IProducer<string, string>? _producer;
    private readonly ILogger<KafkaAuditPublisherService> _logger;
    private readonly string _topicName;
    private readonly bool _isConfigured;

    public KafkaAuditPublisherService(
        IConfiguration configuration,
        ILogger<KafkaAuditPublisherService> logger)
    {
        _logger = logger;
        _topicName = configuration["Audit:Kafka:TopicName"] ?? "credit-consult-audit";

        var bootstrapServers = configuration["Audit:Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogWarning("Kafka BootstrapServers não configurado. Auditoria desabilitada.");
            _isConfigured = false;
            return;
        }

        try
        {
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                ClientId = "CreditConsult-Audit-Publisher",
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 100
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _isConfigured = true;

            _logger.LogInformation("Kafka Audit Publisher inicializado. Topic: {TopicName}, Servers: {BootstrapServers}", 
                _topicName, bootstrapServers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar Kafka Audit Publisher");
            _isConfigured = false;
        }
    }

    public bool IsAvailable => _isConfigured && _producer != null;

    public async Task PublishAuditEventAsync(AuditEventDto auditEvent, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            _logger.LogWarning("Kafka Audit Publisher não está disponível. Evento não será publicado.");
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(auditEvent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var message = new Message<string, string>
            {
                Key = auditEvent.EventType,
                Value = json,
                Headers = new Headers
                {
                    { "timestamp", System.Text.Encoding.UTF8.GetBytes(auditEvent.Timestamp.ToString("O")) },
                    { "entity-type", System.Text.Encoding.UTF8.GetBytes(auditEvent.EntityType) },
                    { "operation", System.Text.Encoding.UTF8.GetBytes(auditEvent.Operation) }
                }
            };

            var result = await _producer!.ProduceAsync(_topicName, message, cancellationToken);
            
            _logger.LogDebug(
                "Evento de auditoria publicado no Kafka. Topic: {TopicName}, Offset: {Offset}, Partition: {Partition}",
                _topicName, result.Offset, result.Partition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento de auditoria no Kafka. EventType: {EventType}", 
                auditEvent.EventType);
        }
    }

    public void Dispose()
    {
        try
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fechar conexão Kafka");
        }
    }
}

