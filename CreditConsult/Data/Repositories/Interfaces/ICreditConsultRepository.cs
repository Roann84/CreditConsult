using CreditConsult.Models;

namespace CreditConsult.Data.Repositories.Interfaces;

public interface ICreditConsultRepository : IRepository<CreditConsult>
{
    Task<IEnumerable<CreditConsult>> GetByNumeroCreditoAsync(string numeroCredito);
    Task<IEnumerable<CreditConsult>> GetByNumeroNfseAsync(string numeroNfse);
    Task<IEnumerable<CreditConsult>> GetByTipoCreditoAsync(string tipoCredito);
    Task<IEnumerable<CreditConsult>> GetByDataConstituicaoAsync(DateTime dataConstituicao);
    Task<IEnumerable<CreditConsult>> GetBySimplesNacionalAsync(bool simplesNacional);
}

