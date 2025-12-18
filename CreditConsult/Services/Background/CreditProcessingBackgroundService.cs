using CreditConsult.Data.Repositories.Interfaces;
using CreditConsult.Services.Background.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CreditConsult.Services.Background;

public class CreditProcessingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CreditProcessingBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(5); // Processa a cada 5 minutos

    public CreditProcessingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CreditProcessingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Credit Processing Background Service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Iniciando processamento de créditos em background...");

                using var scope = _serviceProvider.CreateScope();
                var processingService = scope.ServiceProvider.GetRequiredService<ICreditProcessingService>();
                
                await processingService.ProcessPendingCreditsAsync(stoppingToken);

                _logger.LogInformation("Processamento de créditos concluído. Aguardando próximo ciclo...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar créditos em background");
            }

            await Task.Delay(_period, stoppingToken);
        }

        _logger.LogInformation("Credit Processing Background Service finalizado");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Credit Processing Background Service está sendo encerrado...");
        await base.StopAsync(cancellationToken);
    }
}

