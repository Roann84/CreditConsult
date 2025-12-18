using CreditConsult.Data.Context;
using CreditConsult.Data.Repositories;
using CreditConsult.Data.Repositories.Interfaces;
using CreditConsult.HealthChecks;
using CreditConsult.Middleware;
using CreditConsult.Services;
using CreditConsult.Services.Background;
using CreditConsult.Services.Background.Interfaces;
using CreditConsult.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreditConsult
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            
            // Database Configuration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null)));

            // Health Checks
            builder.Services.AddHealthChecks()
                .AddNpgSql(
                    connectionString,
                    name: "postgresql",
                    tags: new[] { "db", "sql", "postgresql", "ready" })
                .AddCheck<RabbitMQHealthCheck>(
                    "rabbitmq",
                    tags: new[] { "rabbitmq", "queue", "ready" });

            // Repositories
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<ICreditConsultRepository, CreditConsultRepository>();

            // RabbitMQ Configuration
            builder.Services.AddSingleton<IServiceBusProcessor>(serviceProvider =>
            {
                return new Services.Background.ServiceBusProcessor(
                    serviceProvider,
                    serviceProvider.GetRequiredService<ILogger<Services.Background.ServiceBusProcessor>>(),
                    builder.Configuration);
            });

            // Services
            builder.Services.AddScoped<ICreditConsultService, CreditConsultService>();
            builder.Services.AddScoped<ICreditProcessingService, CreditProcessingService>();
            builder.Services.AddSingleton<IRabbitMQPublisherService, RabbitMQPublisherService>();

            // Background Services
            builder.Services.AddHostedService<CreditProcessingBackgroundService>();

            // Controllers
            builder.Services.AddControllers();
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            
            // Exception Handling Middleware (deve ser o primeiro)
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // Health Checks endpoints
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            });
            app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = _ => false
            });

            // Custom health endpoints
            // /self - Liveness probe: verifica se o serviço está respondendo (sem dependências)
            app.MapHealthChecks("/self", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = _ => false, // Não executa nenhum health check, apenas verifica se o endpoint responde
                AllowCachingResponses = false
            });

            // /ready - Readiness probe: verifica se o serviço está pronto para receber tráfego (com dependências)
            app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"), // Verifica PostgreSQL e RabbitMQ
                AllowCachingResponses = false
            });

            app.MapControllers();

            // Graceful shutdown
            app.Lifetime.ApplicationStopping.Register(() =>
            {
                app.Logger.LogInformation("Aplicação está sendo encerrada graciosamente...");
            });

            app.Run();
        }
    }
}
