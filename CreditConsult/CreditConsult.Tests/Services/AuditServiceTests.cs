using CreditConsult.DTOs;
using CreditConsult.Services;
using CreditConsult.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CreditConsult.Tests.Services;

public class AuditServiceTests
{
    private readonly Mock<IAuditPublisherService> _publisherMock;
    private readonly Mock<ILogger<AuditService>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly AuditService _service;

    public AuditServiceTests()
    {
        _publisherMock = new Mock<IAuditPublisherService>();
        _loggerMock = new Mock<ILogger<AuditService>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        
        _service = new AuditService(
            _publisherMock.Object,
            _loggerMock.Object,
            _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task LogConsultationAsync_ShouldPublishEvent_WhenPublisherIsAvailable()
    {
        // Arrange
        _publisherMock.Setup(x => x.IsAvailable).Returns(true);
        _publisherMock
            .Setup(x => x.PublishAuditEventAsync(It.IsAny<AuditEventDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/creditos/12345";
        httpContext.Request.Method = "GET";
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        await _service.LogConsultationAsync(
            "GetByNumeroNfse",
            new { NumeroNfse = "12345" },
            new { Count = 1 });

        // Assert
        _publisherMock.Verify(
            x => x.PublishAuditEventAsync(
                It.Is<AuditEventDto>(e => 
                    e.EventType == "ConsultationRequest" &&
                    e.EntityType == "CreditConsult" &&
                    e.Operation == "GetByNumeroNfse" &&
                    e.IpAddress == "127.0.0.1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogConsultationAsync_ShouldNotPublish_WhenPublisherIsNotAvailable()
    {
        // Arrange
        _publisherMock.Setup(x => x.IsAvailable).Returns(false);

        // Act
        await _service.LogConsultationAsync(
            "GetByNumeroNfse",
            new { NumeroNfse = "12345" },
            new { Count = 1 });

        // Assert
        _publisherMock.Verify(
            x => x.PublishAuditEventAsync(
                It.IsAny<AuditEventDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LogConsultationAsync_ShouldCaptureUserId_WhenUserIsAuthenticated()
    {
        // Arrange
        _publisherMock.Setup(x => x.IsAvailable).Returns(true);
        _publisherMock
            .Setup(x => x.PublishAuditEventAsync(It.IsAny<AuditEventDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "testuser")
        }, "test"));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        await _service.LogConsultationAsync(
            "GetByNumeroCredito",
            new { NumeroCredito = "12345" },
            null);

        // Assert
        _publisherMock.Verify(
            x => x.PublishAuditEventAsync(
                It.Is<AuditEventDto>(e => e.UserId == "testuser"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LogConsultationAsync_ShouldNotThrow_WhenPublishFails()
    {
        // Arrange
        _publisherMock.Setup(x => x.IsAvailable).Returns(true);
        _publisherMock
            .Setup(x => x.PublishAuditEventAsync(It.IsAny<AuditEventDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Publish failed"));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

        // Act & Assert
        await _service.LogConsultationAsync(
            "GetByNumeroNfse",
            new { NumeroNfse = "12345" },
            null);

        // Should not throw
        _publisherMock.Verify(
            x => x.PublishAuditEventAsync(
                It.IsAny<AuditEventDto>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

