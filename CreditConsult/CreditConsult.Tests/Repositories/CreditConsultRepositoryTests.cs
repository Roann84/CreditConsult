using CreditConsult.Data.Context;
using CreditConsult.Data.Repositories;
using CreditConsult.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CreditConsult.Tests.Repositories;

public class CreditConsultRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CreditConsultRepository _repository;

    public CreditConsultRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CreditConsultRepository(_context);
    }

    [Fact]
    public async Task GetByNumeroCreditoAsync_ShouldReturnEntities_WhenExists()
    {
        // Arrange
        var entity1 = new CreditConsultModel
        {
            NumeroCredito = "12345",
            NumeroNfse = "NFSE001",
            DataConstituicao = DateTime.Now.AddDays(-10),
            ValorIssqn = 1000.00m,
            TipoCredito = "ISSQN",
            SimplesNacional = true,
            Aliquota = 5.0m,
            ValorFaturado = 10000.00m,
            ValorDeducao = 1000.00m,
            BaseCalculo = 9000.00m
        };

        var entity2 = new CreditConsultModel
        {
            NumeroCredito = "12345",
            NumeroNfse = "NFSE002",
            DataConstituicao = DateTime.Now,
            ValorIssqn = 2000.00m,
            TipoCredito = "ISSQN",
            SimplesNacional = true,
            Aliquota = 5.0m,
            ValorFaturado = 20000.00m,
            ValorDeducao = 2000.00m,
            BaseCalculo = 18000.00m
        };

        await _context.CreditConsults.AddRangeAsync(entity1, entity2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNumeroCreditoAsync("12345");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(x => x.DataConstituicao);
        result.All(x => x.NumeroCredito == "12345").Should().BeTrue();
    }

    [Fact]
    public async Task GetByNumeroNfseAsync_ShouldReturnEntities_WhenExists()
    {
        // Arrange
        var entity = new CreditConsultModel
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

        await _context.CreditConsults.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNumeroNfseAsync("NFSE001");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().NumeroNfse.Should().Be("NFSE001");
    }

    [Fact]
    public async Task GetByTipoCreditoAsync_ShouldReturnEntities_WhenExists()
    {
        // Arrange
        var entity = new CreditConsultModel
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

        await _context.CreditConsults.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTipoCreditoAsync("ISSQN");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().TipoCredito.Should().Be("ISSQN");
    }

    [Fact]
    public async Task GetByDataConstituicaoAsync_ShouldReturnEntities_WhenExists()
    {
        // Arrange
        var targetDate = new DateTime(2024, 1, 15);
        var entity = new CreditConsultModel
        {
            NumeroCredito = "12345",
            NumeroNfse = "NFSE001",
            DataConstituicao = targetDate,
            ValorIssqn = 1000.00m,
            TipoCredito = "ISSQN",
            SimplesNacional = true,
            Aliquota = 5.0m,
            ValorFaturado = 10000.00m,
            ValorDeducao = 1000.00m,
            BaseCalculo = 9000.00m
        };

        await _context.CreditConsults.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDataConstituicaoAsync(targetDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().DataConstituicao.Date.Should().Be(targetDate.Date);
    }

    [Fact]
    public async Task GetBySimplesNacionalAsync_ShouldReturnEntities_WhenExists()
    {
        // Arrange
        var entity = new CreditConsultModel
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

        await _context.CreditConsults.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySimplesNacionalAsync(true);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().SimplesNacional.Should().BeTrue();
    }

    [Fact]
    public async Task GetByNumeroCreditoAsync_ShouldReturnEmpty_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByNumeroCreditoAsync("99999");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

