using CreditConsult.DTOs;
using CreditConsult.Services.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CreditConsult.Services;

public class RabbitMQPublisherService : IRabbitMQPublisherService, IDisposable
{
    private IConnection? _connection;
    private IModel? _channel;
    private readonly ILogger<RabbitMQPublisherService> _logger;
    private readonly string _queueName;
    private readonly string _hostName;
    private readonly string _userName;
    private readonly string _password;
    private readonly int _port;
    private readonly object _lockObject = new object();

    public RabbitMQPublisherService(
        IConfiguration configuration,
        ILogger<RabbitMQPublisherService> logger)
    {
        _logger = logger;
        _queueName = configuration["RabbitMQ:QueueName"] ?? "integrar-credito-constituido-entry";
        _hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
        _userName = configuration["RabbitMQ:UserName"] ?? "guest";
        _password = configuration["RabbitMQ:Password"] ?? "guest";
        _port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");

        try
        {
            EnsureConnection();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível conectar ao RabbitMQ na inicialização. A conexão será tentada novamente quando necessário. Host: {HostName}:{Port}",
                _hostName, _port);
            // Não lança exceção - permite que a aplicação inicie mesmo sem RabbitMQ
        }
    }

    public Task PublishMessageAsync(CreditConsultRequestDto message, CancellationToken cancellationToken = default)
    {
        return PublishMessagesAsync(new[] { message }, cancellationToken);
    }

    private void EnsureConnection()
    {
        lock (_lockObject)
        {
            if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
            {
                return; // Já está conectado
            }

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    Port = _port,
                    UserName = _userName,
                    Password = _password,
                    DispatchConsumersAsync = true
                };

                // Fecha conexões antigas se existirem
                try
                {
                    _channel?.Close();
                    _channel?.Dispose();
                }
                catch { }

                try
                {
                    _connection?.Close();
                    _connection?.Dispose();
                }
                catch { }

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declara a fila (cria se não existir)
                _channel.QueueDeclare(
                    queue: _queueName,
                    durable: true, // Fila persiste após reinicialização
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation("RabbitMQ Publisher conectado. Fila: {QueueName}, Host: {HostName}:{Port}",
                    _queueName, _hostName, _port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao conectar ao RabbitMQ. Verifique se o RabbitMQ está rodando em {HostName}:{Port}",
                    _hostName, _port);
                throw;
            }
        }
    }

    public Task PublishMessagesAsync(IEnumerable<CreditConsultRequestDto> messages, CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureConnection();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Não foi possível conectar ao RabbitMQ para publicar mensagens");
            throw new InvalidOperationException("RabbitMQ connection is not available", ex);
        }

        if (_channel == null || _connection == null || !_connection.IsOpen)
        {
            throw new InvalidOperationException("RabbitMQ connection is not available");
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            int publishedCount = 0;
            foreach (var message in messages)
            {
                var json = JsonSerializer.Serialize(message, options);
                var body = Encoding.UTF8.GetBytes(json);

                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: _queueName,
                    basicProperties: null,
                    body: body);

                publishedCount++;
                _logger.LogDebug("Mensagem publicada na fila {QueueName}. NumeroCredito: {NumeroCredito}",
                    _queueName, message.NumeroCredito);
            }

            _logger.LogInformation("{Count} mensagens publicadas na fila {QueueName}", publishedCount, _queueName);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagens no RabbitMQ");
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

