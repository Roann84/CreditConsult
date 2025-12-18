using CreditConsult.Data.Repositories.Interfaces;
using CreditConsult.DTOs;
using CreditConsult.Models;
using CreditConsult.Services.Background.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CreditConsult.Services.Background;

public class ServiceBusProcessor : IServiceBusProcessor, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly ICreditConsultRepository _repository;
    private readonly ILogger<ServiceBusProcessor> _logger;
    private readonly string _queueName;
    private readonly string _hostName;
    private readonly string _userName;
    private readonly string _password;
    private readonly int _port;

    public ServiceBusProcessor(
        ICreditConsultRepository repository,
        ILogger<ServiceBusProcessor> logger,
        IConfiguration configuration)
    {
        _repository = repository;
        _logger = logger;
        _queueName = configuration["RabbitMQ:QueueName"] ?? "credit-consult-requests";
        _hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        _userName = configuration["RabbitMQ:UserName"] ?? "guest";
        _password = configuration["RabbitMQ:Password"] ?? "guest";
        _port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password,
                Port = _port
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declara a fila (cria se não existir)
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true, // Fila persiste após reinicialização
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("RabbitMQ conectado. Fila: {QueueName}, Host: {HostName}:{Port}", 
                _queueName, _hostName, _port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar ao RabbitMQ. Verifique se o RabbitMQ está rodando em {HostName}:{Port}", 
                _hostName, _port);
            // Continua sem conexão - o ProcessMessagesAsync verificará isso
        }
    }

    public async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        if (_connection == null || _channel == null || !_connection.IsOpen)
        {
            // Não loga warning a cada 500ms para não poluir os logs
            return;
        }

        try
        {
            // Busca mensagens da fila (BasicGet é não-bloqueante)
            var result = _channel.BasicGet(_queueName, autoAck: false);

            if (result == null)
            {
                // Nenhuma mensagem disponível
                return;
            }

            var body = result.Body.ToArray();
            var messageBody = Encoding.UTF8.GetString(body);
            var deliveryTag = result.DeliveryTag;

            try
            {
                await ProcessMessageAsync(messageBody, deliveryTag, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem. DeliveryTag: {DeliveryTag}", deliveryTag);
                // Nack a mensagem (requeue = true para reprocessar)
                _channel.BasicNack(deliveryTag, multiple: false, requeue: true);
            }

            // Continua processando mais mensagens (até 10 por ciclo)
            int processedCount = 1;
            while (processedCount < 10 && !cancellationToken.IsCancellationRequested)
            {
                result = _channel.BasicGet(_queueName, autoAck: false);
                if (result == null)
                {
                    break; // Não há mais mensagens
                }

                body = result.Body.ToArray();
                messageBody = Encoding.UTF8.GetString(body);
                deliveryTag = result.DeliveryTag;

                try
                {
                    await ProcessMessageAsync(messageBody, deliveryTag, cancellationToken);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem. DeliveryTag: {DeliveryTag}", deliveryTag);
                    _channel.BasicNack(deliveryTag, multiple: false, requeue: true);
                }
            }

            if (processedCount > 0)
            {
                _logger.LogDebug("Processadas {Count} mensagens do RabbitMQ", processedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagens do RabbitMQ");
            throw;
        }
    }

    private async Task ProcessMessageAsync(string messageBody, ulong deliveryTag, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Processando mensagem. DeliveryTag: {DeliveryTag}, Body: {Body}", deliveryTag, messageBody);

            // Deserializa a mensagem JSON para o DTO
            var creditConsultDto = JsonSerializer.Deserialize<CreditConsultRequestDto>(messageBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (creditConsultDto == null)
            {
                throw new InvalidOperationException($"Não foi possível deserializar a mensagem. DeliveryTag: {deliveryTag}");
            }

            // Validação básica
            if (string.IsNullOrWhiteSpace(creditConsultDto.NumeroCredito))
            {
                throw new ArgumentException($"NumeroCredito é obrigatório. DeliveryTag: {deliveryTag}");
            }

            // Converte DTO para entidade
            var entity = new CreditConsultModel
            {
                NumeroCredito = creditConsultDto.NumeroCredito,
                NumeroNfse = creditConsultDto.NumeroNfse,
                DataConstituicao = creditConsultDto.DataConstituicao,
                ValorIssqn = creditConsultDto.ValorIssqn,
                TipoCredito = creditConsultDto.TipoCredito,
                SimplesNacional = creditConsultDto.SimplesNacional,
                Aliquota = creditConsultDto.Aliquota,
                ValorFaturado = creditConsultDto.ValorFaturado,
                ValorDeducao = creditConsultDto.ValorDeducao,
                BaseCalculo = creditConsultDto.BaseCalculo
            };

            // Insere no banco de dados de forma individual (não bulk)
            var created = await _repository.AddAsync(entity);
            _logger.LogInformation("Crédito inserido com sucesso. ID: {Id}, DeliveryTag: {DeliveryTag}", 
                created.Id, deliveryTag);

            // Acknowledge a mensagem (remove da fila) - apenas após sucesso
            if (_channel != null && _channel.IsOpen)
            {
                _channel.BasicAck(deliveryTag, multiple: false);
                _logger.LogDebug("Mensagem processada e confirmada com sucesso. DeliveryTag: {DeliveryTag}", deliveryTag);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar mensagem. DeliveryTag: {DeliveryTag}", deliveryTag);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem. DeliveryTag: {DeliveryTag}", deliveryTag);
            throw;
        }
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
}
