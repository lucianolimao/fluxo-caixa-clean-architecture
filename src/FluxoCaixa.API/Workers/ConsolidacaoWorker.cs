namespace FluxoCaixa.API.Workers;

using Application.UseCases;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class ConsolidacaoWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConsolidacaoWorker> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public ConsolidacaoWorker(
        IServiceProvider serviceProvider,
        ILogger<ConsolidacaoWorker> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var rabbitMqHost = _configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
            var factory = new ConnectionFactory { HostName = rabbitMqHost };
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.QueueDeclare(
                queue: "lancamento-criado",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var evento = JsonSerializer.Deserialize<LancamentoCriadoEvento>(message);

                    if (evento != null)
                    {
                        _logger.LogInformation("Processando consolidação para data: {Data}", evento.Data);
                        
                        using var scope = _serviceProvider.CreateScope();
                        var useCase = scope.ServiceProvider.GetRequiredService<ProcessarConsolidacaoUseCase>();
                        await useCase.ExecutarAsync(evento.Data);
                        
                        _logger.LogInformation("Consolidação processada com sucesso para data: {Data}", evento.Data);
                    }

                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem de consolidação");
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: "lancamento-criado", autoAck: false, consumer: consumer);
            
            _logger.LogInformation("ConsolidacaoWorker iniciado e aguardando mensagens...");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar ConsolidacaoWorker");
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

    private class LancamentoCriadoEvento
    {
        public Guid LancamentoId { get; set; }
        public DateTime Data { get; set; }
    }
}
