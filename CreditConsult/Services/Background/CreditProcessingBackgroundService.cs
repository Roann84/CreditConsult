using CreditConsult.Services.Background.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CreditConsult.Services.Background;

public class CreditProcessingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CreditProcessingBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMilliseconds(500); // Verifica a cada 500ms

    public CreditProcessingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CreditProcessingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Credit Processing Background Service iniciado - Verificando RabbitMQ a cada 500ms");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var serviceBusProcessor = scope.ServiceProvider.GetRequiredService<IServiceBusProcessor>();
                
                await serviceBusProcessor.ProcessMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagens do RabbitMQ");
                // Continua o loop mesmo em caso de erro para não parar o serviço
            }

            // Aguarda 500ms antes da próxima verificação
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Credit Processing Background Service finalizado");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Credit Processing Background Service está sendo encerrado...");
        await base.StopAsync(cancellationToken);
    }
}

