using CreditConsult.Data.Repositories.Interfaces;
using CreditConsult.DTOs;
using CreditConsult.Models;
using CreditConsult.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace CreditConsult.Tests.Services;

public class CreditConsultServiceTests
{
    private readonly Mock<ICreditConsultRepository> _repositoryMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<CreditConsultService>> _loggerMock;
    private readonly Mock<AuditService> _auditServiceMock;
    private readonly CreditConsultService _service;

    public CreditConsultServiceTests()
    {
        _repositoryMock = new Mock<ICreditConsultRepository>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<CreditConsultService>>();
        _auditServiceMock = new Mock<AuditService>(
            Mock.Of<CreditConsult.Services.Interfaces.IAuditPublisherService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<AuditService>>(),
            null);
        
        _service = new CreditConsultService(
            _repositoryMock.Object,
            _loggerMock.Object,
            _auditServiceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedDto_WhenEntityIsCreated()
    {
        // Arrange
        var requestDto = new CreditConsultRequestDto
        {
            NumeroCredito = "12345",
            NumeroNfse = "NFSE001",
            DataConstituicao = DateTime.Now,
            ValorIssqn = 1000.00m,
            TipoCredito = "ISSQN",
            SimplesNacional = true,
            Aliquota = 5.0m,
            ValorFaturado = 10000.00m,
            ValorDeducao = 1000.00m,
            BaseCalculo = 9000.00m
        };

        var entity = new CreditConsultModel
        {
            Id = 1,
            NumeroCredito = requestDto.NumeroCredito,
            NumeroNfse = requestDto.NumeroNfse,
            DataConstituicao = requestDto.DataConstituicao,
            ValorIssqn = requestDto.ValorIssqn,
            TipoCredito = requestDto.TipoCredito,
            SimplesNacional = requestDto.SimplesNacional,
            Aliquota = requestDto.Aliquota,
            ValorFaturado = requestDto.ValorFaturado,
            ValorDeducao = requestDto.ValorDeducao,
            BaseCalculo = requestDto.BaseCalculo
        };

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CreditConsultModel>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _service.CreateAsync(requestDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.NumeroCredito.Should().Be("12345");
        result.NumeroNfse.Should().Be("NFSE001");
        result.ValorIssqn.Should().Be(1000.00m);
        
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<CreditConsultModel>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDto_WhenEntityExists()
    {
        // Arrange
        var entity = new CreditConsultModel
        {
            Id = 1,
            NumeroCredito = "12345",
            NumeroNfse = "NFSE001",
            DataConstituicao = DateTime.Now,
            ValorIssqn = 1000.00m,
            TipoCredito = "ISSQN",
            SimplesNacional = true,
            Aliquota = 5.0m,
            ValorFaturado = 10000.00m,
            ValorDeducao = 1000.00m,
            BaseCalculo = 9000.00m
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(entity);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.NumeroCredito.Should().Be("12345");
        
        _repositoryMock.Verify(x => x.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((CreditConsultModel?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(x => x.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task GetByNumeroCreditoAsync_ShouldReturnList_AndPublishAuditEvent()
    {
        // Arrange
        var entities = new List<CreditConsultModel>
        {
            new CreditConsultModel
            {
                Id = 1,
                NumeroCredito = "12345",
                NumeroNfse = "NFSE001",
                DataConstituicao = DateTime.Now,
                ValorIssqn = 1000.00m,
                TipoCredito = "ISSQN",
                SimplesNacional = true,
                Aliquota = 5.0m,
                ValorFaturado = 10000.00m,
                ValorDeducao = 1000.00m,
                BaseCalculo = 9000.00m
            }
        };

        _repositoryMock
            .Setup(x => x.GetByNumeroCreditoAsync("12345"))
            .ReturnsAsync(entities);

        _auditServiceMock
            .Setup(x => x.LogConsultationAsync(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetByNumeroCreditoAsync("12345");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().NumeroCredito.Should().Be("12345");
        
        _repositoryMock.Verify(x => x.GetByNumeroCreditoAsync("12345"), Times.Once);
        _auditServiceMock.Verify(
            x => x.LogConsultationAsync(
                "GetByNumeroCredito",
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByNumeroNfseAsync_ShouldReturnList_AndPublishAuditEvent()
    {
        // Arrange
        var entities = new List<CreditConsultModel>
        {
            new CreditConsultModel
            {
                Id = 1,
                NumeroCredito = "12345",
                NumeroNfse = "NFSE001",
                DataConstituicao = DateTime.Now,
                ValorIssqn = 1000.00m,
                TipoCredito = "ISSQN",
                SimplesNacional = true,
                Aliquota = 5.0m,
                ValorFaturado = 10000.00m,
                ValorDeducao = 1000.00m,
                BaseCalculo = 9000.00m
            }
        };

        _repositoryMock
            .Setup(x => x.GetByNumeroNfseAsync("NFSE001"))
            .ReturnsAsync(entities);

        _auditServiceMock
            .Setup(x => x.LogConsultationAsync(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetByNumeroNfseAsync("NFSE001");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().NumeroNfse.Should().Be("NFSE001");
        
        _repositoryMock.Verify(x => x.GetByNumeroNfseAsync("NFSE001"), Times.Once);
        _auditServiceMock.Verify(
            x => x.LogConsultationAsync(
                "GetByNumeroNfse",
                It.IsAny<object?>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedDto_WhenEntityExists()
    {
        // Arrange
        var existingEntity = new CreditConsultModel
        {
            Id = 1,
            NumeroCredito = "12345",
            NumeroNfse = "NFSE001",
            DataConstituicao = DateTime.Now,
            ValorIssqn = 1000.00m,
            TipoCredito = "ISSQN",
            SimplesNacional = true,
            Aliquota = 5.0m,
            ValorFaturado = 10000.00m,
            ValorDeducao = 1000.00m,
            BaseCalculo = 9000.00m
        };

        var updateDto = new CreditConsultRequestDto
        {
            NumeroCredito = "99999",
            NumeroNfse = "NFSE002",
            DataConstituicao = DateTime.Now,
            ValorIssqn = 2000.00m,
            TipoCredito = "Outros",
            SimplesNacional = false,
            Aliquota = 10.0m,
            ValorFaturado = 20000.00m,
            ValorDeducao = 2000.00m,
            BaseCalculo = 18000.00m
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingEntity);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<CreditConsultModel>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.NumeroCredito.Should().Be("99999");
        result.NumeroNfse.Should().Be("NFSE002");
        result.ValorIssqn.Should().Be(2000.00m);
        
        _repositoryMock.Verify(x => x.GetByIdAsync(1), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<CreditConsultModel>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenEntityDoesNotExist()
    {
        // Arrange
        var updateDto = new CreditConsultRequestDto
        {
            NumeroCredito = "99999",
            NumeroNfse = "NFSE002",
            DataConstituicao = DateTime.Now,
            ValorIssqn = 2000.00m,
            TipoCredito = "Outros",
            SimplesNacional = false,
            Aliquota = 10.0m,
            ValorFaturado = 20000.00m,
            ValorDeducao = 2000.00m,
            BaseCalculo = 18000.00m
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((CreditConsultModel?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(999, updateDto));
        
        _repositoryMock.Verify(x => x.GetByIdAsync(999), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<CreditConsultModel>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenEntityExists()
    {
        // Arrange
        var entity = new CreditConsultModel
        {
            Id = 1,
            NumeroCredito = "12345",
            NumeroNfse = "NFSE001",
            DataConstituicao = DateTime.Now,
            ValorIssqn = 1000.00m,
            TipoCredito = "ISSQN",
            SimplesNacional = true,
            Aliquota = 5.0m,
            ValorFaturado = 10000.00m,
            ValorDeducao = 1000.00m,
            BaseCalculo = 9000.00m
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(entity);

        _repositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<CreditConsultModel>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(x => x.GetByIdAsync(1), Times.Once);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<CreditConsultModel>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((CreditConsultModel?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
        _repositoryMock.Verify(x => x.GetByIdAsync(999), Times.Once);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<CreditConsultModel>()), Times.Never);
    }
}

