using CreditConsult.Data.Repositories.Interfaces;
using CreditConsult.DTOs;
using CreditConsult.Models;
using CreditConsult.Services.Interfaces;

namespace CreditConsult.Services;

public class CreditConsultService : ICreditConsultService
{
    private readonly ICreditConsultRepository _repository;
    private readonly ILogger<CreditConsultService> _logger;
    private readonly AuditService? _auditService;

    public CreditConsultService(
        ICreditConsultRepository repository,
        ILogger<CreditConsultService> logger,
        AuditService? auditService = null)
    {
        _repository = repository;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<CreditConsultResponseDto> CreateAsync(CreditConsultRequestDto requestDto)
    {
        try
        {
            var entity = new CreditConsultModel
            {
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

            var created = await _repository.AddAsync(entity);
            _logger.LogInformation("Credit consult created: {Id}", created.Id);

            return MapToDto(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating credit consult");
            throw;
        }
    }

    public async Task<CreditConsultResponseDto?> GetByIdAsync(long id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return null;

            return MapToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit consult by id: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CreditConsultResponseDto>> GetAllAsync()
    {
        try
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all credit consults");
            throw;
        }
    }

    public async Task<IEnumerable<CreditConsultResponseDto>> GetByNumeroCreditoAsync(string numeroCredito)
    {
        try
        {
            var entities = await _repository.GetByNumeroCreditoAsync(numeroCredito);
            var result = entities.Select(MapToDto).ToList();

            // Publicar evento de auditoria
            if (_auditService != null)
            {
                await _auditService.LogConsultationAsync(
                    "GetByNumeroCredito",
                    new { NumeroCredito = numeroCredito },
                    new { Count = result.Count },
                    CancellationToken.None);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit consults by numero credito: {NumeroCredito}", numeroCredito);
            throw;
        }
    }

    public async Task<IEnumerable<CreditConsultResponseDto>> GetByNumeroNfseAsync(string numeroNfse)
    {
        try
        {
            var entities = await _repository.GetByNumeroNfseAsync(numeroNfse);
            var result = entities.Select(MapToDto).ToList();

            // Publicar evento de auditoria
            if (_auditService != null)
            {
                await _auditService.LogConsultationAsync(
                    "GetByNumeroNfse",
                    new { NumeroNfse = numeroNfse },
                    new { Count = result.Count },
                    CancellationToken.None);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit consults by numero nfse: {NumeroNfse}", numeroNfse);
            throw;
        }
    }

    public async Task<IEnumerable<CreditConsultResponseDto>> GetByTipoCreditoAsync(string tipoCredito)
    {
        try
        {
            var entities = await _repository.GetByTipoCreditoAsync(tipoCredito);
            return entities.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit consults by tipo credito: {TipoCredito}", tipoCredito);
            throw;
        }
    }

    public async Task<CreditConsultResponseDto> UpdateAsync(long id, CreditConsultRequestDto requestDto)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Credit consult with id {id} not found");

            entity.NumeroCredito = requestDto.NumeroCredito;
            entity.NumeroNfse = requestDto.NumeroNfse;
            entity.DataConstituicao = requestDto.DataConstituicao;
            entity.ValorIssqn = requestDto.ValorIssqn;
            entity.TipoCredito = requestDto.TipoCredito;
            entity.SimplesNacional = requestDto.SimplesNacional;
            entity.Aliquota = requestDto.Aliquota;
            entity.ValorFaturado = requestDto.ValorFaturado;
            entity.ValorDeducao = requestDto.ValorDeducao;
            entity.BaseCalculo = requestDto.BaseCalculo;

            await _repository.UpdateAsync(entity);
            _logger.LogInformation("Credit consult updated: {Id}", id);

            return MapToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating credit consult: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return false;

            await _repository.DeleteAsync(entity);
            _logger.LogInformation("Credit consult deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting credit consult: {Id}", id);
            throw;
        }
    }

    private static CreditConsultResponseDto MapToDto(CreditConsultModel entity)
    {
        return new CreditConsultResponseDto
        {
            Id = entity.Id,
            NumeroCredito = entity.NumeroCredito,
            NumeroNfse = entity.NumeroNfse,
            DataConstituicao = entity.DataConstituicao,
            ValorIssqn = entity.ValorIssqn,
            TipoCredito = entity.TipoCredito,
            SimplesNacionalBool = entity.SimplesNacional, // Usa a propriedade bool interna
            Aliquota = entity.Aliquota,
            ValorFaturado = entity.ValorFaturado,
            ValorDeducao = entity.ValorDeducao,
            BaseCalculo = entity.BaseCalculo
        };
    }
}
