namespace CreditConsult.Services.Background.Interfaces;

public interface IServiceBusProcessor
{
    Task ProcessMessagesAsync(CancellationToken cancellationToken);
}

