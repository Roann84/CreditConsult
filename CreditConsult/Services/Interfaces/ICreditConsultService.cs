using CreditConsult.DTOs;

namespace CreditConsult.Services.Interfaces;

public interface ICreditConsultService
{
    Task<CreditConsultResponseDto> CreateAsync(CreditConsultRequestDto requestDto);
    Task<CreditConsultResponseDto?> GetByIdAsync(long id);
    Task<IEnumerable<CreditConsultResponseDto>> GetAllAsync();
    Task<IEnumerable<CreditConsultResponseDto>> GetByNumeroCreditoAsync(string numeroCredito);
    Task<IEnumerable<CreditConsultResponseDto>> GetByNumeroNfseAsync(string numeroNfse);
    Task<IEnumerable<CreditConsultResponseDto>> GetByTipoCreditoAsync(string tipoCredito);
    Task<CreditConsultResponseDto> UpdateAsync(long id, CreditConsultRequestDto requestDto);
    Task<bool> DeleteAsync(long id);
}
