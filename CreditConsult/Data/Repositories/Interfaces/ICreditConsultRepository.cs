using CreditConsult.Models;

namespace CreditConsult.Data.Repositories.Interfaces;

public interface ICreditConsultRepository : IRepository<CreditConsultModel>
{
    Task<IEnumerable<CreditConsultModel>> GetByNumeroCreditoAsync(string numeroCredito);
    Task<IEnumerable<CreditConsultModel>> GetByNumeroNfseAsync(string numeroNfse);
    Task<IEnumerable<CreditConsultModel>> GetByTipoCreditoAsync(string tipoCredito);
    Task<IEnumerable<CreditConsultModel>> GetByDataConstituicaoAsync(DateTime dataConstituicao);
    Task<IEnumerable<CreditConsultModel>> GetBySimplesNacionalAsync(bool simplesNacional);
}

