using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace CreditConsult.HealthChecks;

public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public RabbitMQHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
            var port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
            var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
            var password = _configuration["RabbitMQ:Password"] ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Verifica se a conexão está aberta
            if (!connection.IsOpen || !channel.IsOpen)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "RabbitMQ connection is not open"));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                $"RabbitMQ is healthy. Connected to {hostName}:{port}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "RabbitMQ health check failed",
                ex));
        }
    }
}

