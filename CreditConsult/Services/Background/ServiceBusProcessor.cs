using CreditConsult.Data.Repositories.Interfaces;
using CreditConsult.DTOs;
using CreditConsult.Models;
using CreditConsult.Services.Background.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CreditConsult.Services.Background;

public class ServiceBusProcessor : IServiceBusProcessor, IDisposable
{
    private const int MaxMessagesPerCycle = 10;
    private const string DefaultQueueName = "integrar-credito-constituido-entry";
    private const string DefaultHostName = "localhost";
    private const string DefaultUserName = "guest";
    private const string DefaultPassword = "guest";
    private const int DefaultPort = 5672;

    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceBusProcessor> _logger;
    private readonly RabbitMQOptions _options;

    public ServiceBusProcessor(
        IServiceProvider serviceProvider,
        ILogger<ServiceBusProcessor> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _options = LoadRabbitMQOptions(configuration);
        
        try
        {
            (_connection, _channel) = InitializeRabbitMQConnection(_options);
            EnsureQueueExists(_channel, _options.QueueName);
            
            _logger.LogInformation(
                "RabbitMQ conectado com sucesso. Fila: {QueueName}, Host: {HostName}:{Port}", 
                _options.QueueName, _options.HostName, _options.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Erro ao conectar ao RabbitMQ. Verifique se o RabbitMQ está rodando em {HostName}:{Port}. " +
                "O processamento de mensagens será retomado quando a conexão estiver disponível.", 
                _options.HostName, _options.Port);
            // Continua sem conexão - o ProcessMessagesAsync verificará isso
        }
    }

    public async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        if (!IsConnectionAvailable())
        {
            return;
        }

        try
        {
            var processedCount = await ProcessAvailableMessagesAsync(cancellationToken);
            
            if (processedCount > 0)
            {
                _logger.LogDebug("Processadas {Count} mensagens do RabbitMQ na fila {QueueName}", 
                    processedCount, _options.QueueName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagens do RabbitMQ");
            throw;
        }
    }

    private async Task<int> ProcessAvailableMessagesAsync(CancellationToken cancellationToken)
    {
        var processedCount = 0;
        
        while (processedCount < MaxMessagesPerCycle && !cancellationToken.IsCancellationRequested)
        {
            var result = _channel!.BasicGet(_options.QueueName, autoAck: false);
            
            if (result == null)
            {
                break; // Não há mais mensagens disponíveis
            }

            var messageBody = Encoding.UTF8.GetString(result.Body.ToArray());
            var deliveryTag = result.DeliveryTag;

            try
            {
                await ProcessMessageAsync(messageBody, deliveryTag, cancellationToken);
                processedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem. DeliveryTag: {DeliveryTag}", deliveryTag);
                NackMessage(deliveryTag, requeue: true);
            }
        }

        return processedCount;
    }

    private async Task ProcessMessageAsync(string messageBody, ulong deliveryTag, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICreditConsultRepository>();

        _logger.LogDebug("Processando mensagem. DeliveryTag: {DeliveryTag}", deliveryTag);

        var creditConsultDto = DeserializeMessage(messageBody, deliveryTag);
        ValidateMessage(creditConsultDto, deliveryTag);
        
        var entity = MapToEntity(creditConsultDto);
        var created = await repository.AddAsync(entity);
        
        _logger.LogInformation(
            "Crédito inserido com sucesso. ID: {Id}, NumeroCredito: {NumeroCredito}, DeliveryTag: {DeliveryTag}", 
            created.Id, created.NumeroCredito, deliveryTag);

        AcknowledgeMessage(deliveryTag);
    }

    private CreditConsultRequestDto DeserializeMessage(string messageBody, ulong deliveryTag)
    {
        try
        {
            var dto = JsonSerializer.Deserialize<CreditConsultRequestDto>(
                messageBody, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto == null)
            {
                throw new InvalidOperationException(
                    $"Não foi possível deserializar a mensagem. DeliveryTag: {deliveryTag}");
            }

            return dto;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar mensagem JSON. DeliveryTag: {DeliveryTag}", deliveryTag);
            throw;
        }
    }

    private static void ValidateMessage(CreditConsultRequestDto dto, ulong deliveryTag)
    {
        if (string.IsNullOrWhiteSpace(dto.NumeroCredito))
        {
            throw new ArgumentException(
                $"NumeroCredito é obrigatório. DeliveryTag: {deliveryTag}");
        }
    }

    private static CreditConsultModel MapToEntity(CreditConsultRequestDto dto)
    {
        return new CreditConsultModel
        {
            NumeroCredito = dto.NumeroCredito,
            NumeroNfse = dto.NumeroNfse,
            DataConstituicao = dto.DataConstituicao,
            ValorIssqn = dto.ValorIssqn,
            TipoCredito = dto.TipoCredito,
            SimplesNacional = dto.SimplesNacional,
            Aliquota = dto.Aliquota,
            ValorFaturado = dto.ValorFaturado,
            ValorDeducao = dto.ValorDeducao,
            BaseCalculo = dto.BaseCalculo
        };
    }

    private void AcknowledgeMessage(ulong deliveryTag)
    {
        if (IsChannelAvailable())
        {
            _channel!.BasicAck(deliveryTag, multiple: false);
            _logger.LogDebug("Mensagem confirmada com sucesso. DeliveryTag: {DeliveryTag}", deliveryTag);
        }
    }

    private void NackMessage(ulong deliveryTag, bool requeue)
    {
        if (IsChannelAvailable())
        {
            _channel!.BasicNack(deliveryTag, multiple: false, requeue: requeue);
        }
    }

    private static RabbitMQOptions LoadRabbitMQOptions(IConfiguration configuration)
    {
        var portString = configuration["RabbitMQ:Port"] ?? DefaultPort.ToString();
        
        if (!int.TryParse(portString, out var port))
        {
            port = DefaultPort;
        }

        return new RabbitMQOptions
        {
            QueueName = configuration["RabbitMQ:QueueName"] ?? DefaultQueueName,
            HostName = configuration["RabbitMQ:HostName"] ?? DefaultHostName,
            UserName = configuration["RabbitMQ:UserName"] ?? DefaultUserName,
            Password = configuration["RabbitMQ:Password"] ?? DefaultPassword,
            Port = port
        };
    }

    private static (IConnection connection, IModel channel) InitializeRabbitMQConnection(RabbitMQOptions options)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            UserName = options.UserName,
            Password = options.Password,
            Port = options.Port
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        return (connection, channel);
    }

    private static void EnsureQueueExists(IModel channel, string queueName)
    {
        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    private bool IsConnectionAvailable()
    {
        return _connection != null && _channel != null && _connection.IsOpen;
    }

    private bool IsChannelAvailable()
    {
        return _channel != null && _channel.IsOpen;
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fechar conexão RabbitMQ");
        }
    }

    private record RabbitMQOptions
    {
        public string QueueName { get; init; } = string.Empty;
        public string HostName { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public int Port { get; init; }
    }
}
