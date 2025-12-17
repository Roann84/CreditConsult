using CreditConsult.Data.Context;
using CreditConsult.Data.Repositories.Interfaces;
using CreditConsult.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditConsult.Data.Repositories;

public class CreditConsultRepository : Repository<CreditConsult>, ICreditConsultRepository
{
    public CreditConsultRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<CreditConsult>> GetByNumeroCreditoAsync(string numeroCredito)
    {
        return await _dbSet
            .Where(x => x.NumeroCredito == numeroCredito)
            .OrderByDescending(x => x.DataConstituicao)
            .ToListAsync();
    }

    public async Task<IEnumerable<CreditConsult>> GetByNumeroNfseAsync(string numeroNfse)
    {
        return await _dbSet
            .Where(x => x.NumeroNfse == numeroNfse)
            .OrderByDescending(x => x.DataConstituicao)
            .ToListAsync();
    }

    public async Task<IEnumerable<CreditConsult>> GetByTipoCreditoAsync(string tipoCredito)
    {
        return await _dbSet
            .Where(x => x.TipoCredito == tipoCredito)
            .OrderByDescending(x => x.DataConstituicao)
            .ToListAsync();
    }

    public async Task<IEnumerable<CreditConsult>> GetByDataConstituicaoAsync(DateTime dataConstituicao)
    {
        return await _dbSet
            .Where(x => x.DataConstituicao.Date == dataConstituicao.Date)
            .OrderByDescending(x => x.DataConstituicao)
            .ToListAsync();
    }

    public async Task<IEnumerable<CreditConsult>> GetBySimplesNacionalAsync(bool simplesNacional)
    {
        return await _dbSet
            .Where(x => x.SimplesNacional == simplesNacional)
            .OrderByDescending(x => x.DataConstituicao)
            .ToListAsync();
    }
}

