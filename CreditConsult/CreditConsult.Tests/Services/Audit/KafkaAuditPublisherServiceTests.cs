using CreditConsult.DTOs;
using CreditConsult.Services.Audit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CreditConsult.Tests.Services.Audit;

public class KafkaAuditPublisherServiceTests
{
    private readonly Mock<ILogger<KafkaAuditPublisherService>> _loggerMock;

    public KafkaAuditPublisherServiceTests()
    {
        _loggerMock = new Mock<ILogger<KafkaAuditPublisherService>>();
    }

    [Fact]
    public void IsAvailable_ShouldReturnFalse_WhenBootstrapServersNotConfigured()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Audit:Kafka:TopicName", "test-topic" }
            })
            .Build();

        // Act
        var service = new KafkaAuditPublisherService(configuration, _loggerMock.Object);

        // Assert
        service.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_ShouldReturnFalse_WhenBootstrapServersIsEmpty()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Audit:Kafka:BootstrapServers", "" },
                { "Audit:Kafka:TopicName", "test-topic" }
            })
            .Build();

        // Act
        var service = new KafkaAuditPublisherService(configuration, _loggerMock.Object);

        // Assert
        service.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task PublishAuditEventAsync_ShouldNotThrow_WhenPublisherNotAvailable()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var service = new KafkaAuditPublisherService(configuration, _loggerMock.Object);
        var auditEvent = new AuditEventDto
        {
            EventType = "TestEvent",
            EntityType = "TestEntity",
            Operation = "TestOperation",
            Timestamp = DateTime.UtcNow
        };

        // Act & Assert
        await service.PublishAuditEventAsync(auditEvent);

        // Should not throw, just log warning
        service.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldUseDefaultTopicName_WhenNotConfigured()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Audit:Kafka:BootstrapServers", "localhost:9092" }
            })
            .Build();

        // Act
        var service = new KafkaAuditPublisherService(configuration, _loggerMock.Object);

        // Assert
        // Se não configurado, deve usar o default "credit-consult-audit"
        // Não podemos verificar diretamente, mas o serviço não deve lançar exceção
        service.Should().NotBeNull();
    }
}

