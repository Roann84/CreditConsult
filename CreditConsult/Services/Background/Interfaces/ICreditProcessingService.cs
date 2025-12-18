namespace CreditConsult.Services.Background.Interfaces;

public interface ICreditProcessingService
{
    Task ProcessPendingCreditsAsync(CancellationToken cancellationToken);
}

