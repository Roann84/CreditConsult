using CreditConsult.Controllers;
using CreditConsult.DTOs;
using CreditConsult.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CreditConsult.Tests.Controllers;

public class CreditConsultControllerTests
{
    private readonly Mock<ICreditConsultService> _serviceMock;
    private readonly Mock<Services.Interfaces.IRabbitMQPublisherService> _publisherMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<CreditConsultController>> _loggerMock;
    private readonly CreditConsultController _controller;

    public CreditConsultControllerTests()
    {
        _serviceMock = new Mock<ICreditConsultService>();
        _publisherMock = new Mock<Services.Interfaces.IRabbitMQPublisherService>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<CreditConsultController>>();
        
        _controller = new CreditConsultController(
            _serviceMock.Object,
            _publisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByNumeroNfse_ShouldReturnOk_WhenEntitiesExist()
    {
        // Arrange
        var dtos = new List<CreditConsultResponseDto>
        {
            new CreditConsultResponseDto
            {
                Id = 1,
                NumeroCredito = "12345",
                NumeroNfse = "NFSE001",
                DataConstituicao = DateTime.Now,
                ValorIssqn = 1000.00m,
                TipoCredito = "ISSQN",
                SimplesNacional = "Sim",
                Aliquota = 5.0m,
                ValorFaturado = 10000.00m,
                ValorDeducao = 1000.00m,
                BaseCalculo = 9000.00m
            }
        };

        _serviceMock
            .Setup(x => x.GetByNumeroNfseAsync("NFSE001"))
            .ReturnsAsync(dtos);

        // Act
        var result = await _controller.GetByNumeroNfse("NFSE001");

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDtos = okResult.Value.Should().BeOfType<List<CreditConsultResponseDto>>().Subject;
        returnedDtos.Should().HaveCount(1);
        returnedDtos.First().NumeroNfse.Should().Be("NFSE001");
    }

    [Fact]
    public async Task GetByNumeroNfse_ShouldReturnNotFound_WhenNoEntitiesExist()
    {
        // Arrange
        _serviceMock
            .Setup(x => x.GetByNumeroNfseAsync("NFSE999"))
            .ReturnsAsync(new List<CreditConsultResponseDto>());

        // Act
        var result = await _controller.GetByNumeroNfse("NFSE999");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByNumeroNfse_ShouldReturnBadRequest_WhenNumeroNfseIsEmpty()
    {
        // Act
        var result = await _controller.GetByNumeroNfse("");

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _serviceMock.Verify(x => x.GetByNumeroNfseAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetByNumeroCredito_ShouldReturnOk_WhenEntityExists()
    {
        // Arrange
        var dtos = new List<CreditConsultResponseDto>
        {
            new CreditConsultResponseDto
            {
                Id = 1,
                NumeroCredito = "12345",
                NumeroNfse = "NFSE001",
                DataConstituicao = DateTime.Now,
                ValorIssqn = 1000.00m,
                TipoCredito = "ISSQN",
                SimplesNacional = "Sim",
                Aliquota = 5.0m,
                ValorFaturado = 10000.00m,
                ValorDeducao = 1000.00m,
                BaseCalculo = 9000.00m
            }
        };

        _serviceMock
            .Setup(x => x.GetByNumeroCreditoAsync("12345"))
            .ReturnsAsync(dtos);

        // Act
        var result = await _controller.GetByNumeroCredito("12345");

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedDto = okResult.Value.Should().BeOfType<CreditConsultResponseDto>().Subject;
        returnedDto.NumeroCredito.Should().Be("12345");
    }

    [Fact]
    public async Task GetByNumeroCredito_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        // Arrange
        _serviceMock
            .Setup(x => x.GetByNumeroCreditoAsync("99999"))
            .ReturnsAsync(new List<CreditConsultResponseDto>());

        // Act
        var result = await _controller.GetByNumeroCredito("99999");

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByNumeroCredito_ShouldReturnBadRequest_WhenNumeroCreditoIsEmpty()
    {
        // Act
        var result = await _controller.GetByNumeroCredito("");

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _serviceMock.Verify(x => x.GetByNumeroCreditoAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task IntegrarCreditoConstituido_ShouldReturnAccepted_WhenRequestIsValid()
    {
        // Arrange
        var creditos = new List<CreditConsultIntegrarDto>
        {
            new CreditConsultIntegrarDto
            {
                NumeroCredito = "12345",
                NumeroNfse = "NFSE001",
                DataConstituicao = DateTime.Now,
                ValorIssqn = 1000.00m,
                TipoCredito = "ISSQN",
                SimplesNacional = "Sim",
                Aliquota = 5.0m,
                ValorFaturado = 10000.00m,
                ValorDeducao = 1000.00m,
                BaseCalculo = 9000.00m
            }
        };

        _publisherMock
            .Setup(x => x.PublishMessagesAsync(It.IsAny<IEnumerable<CreditConsultRequestDto>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.IntegrarCreditoConstituido(creditos);

        // Assert
        result.Should().NotBeNull();
        var acceptedResult = result.Result.Should().BeOfType<AcceptedResult>().Subject;
        var response = acceptedResult.Value.Should().BeOfType<IntegrarCreditoResponseDto>().Subject;
        response.Success.Should().BeTrue();
        
        _publisherMock.Verify(
            x => x.PublishMessagesAsync(
                It.IsAny<IEnumerable<CreditConsultRequestDto>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IntegrarCreditoConstituido_ShouldReturnBadRequest_WhenListIsEmpty()
    {
        // Act
        var result = await _controller.IntegrarCreditoConstituido(new List<CreditConsultIntegrarDto>());

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _publisherMock.Verify(
            x => x.PublishMessagesAsync(
                It.IsAny<IEnumerable<CreditConsultRequestDto>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IntegrarCreditoConstituido_ShouldReturnBadRequest_WhenListIsNull()
    {
        // Act
        var result = await _controller.IntegrarCreditoConstituido(null!);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _publisherMock.Verify(
            x => x.PublishMessagesAsync(
                It.IsAny<IEnumerable<CreditConsultRequestDto>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

