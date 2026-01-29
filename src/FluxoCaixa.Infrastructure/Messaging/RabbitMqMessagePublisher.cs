namespace FluxoCaixa.Infrastructure.Messaging;

using System.Text;
using System.Text.Json;
using Application.Interfaces;
using RabbitMQ.Client;

public class RabbitMqMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "lancamento-criado";

    public RabbitMqMessagePublisher(string hostName = "localhost")
    {
        var factory = new ConnectionFactory { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public Task PublicarLancamentoCriadoAsync(Guid lancamentoId, DateTime data)
    {
        var mensagem = new
        {
            LancamentoId = lancamentoId,
            Data = data,
            DataPublicacao = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(mensagem);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(
            exchange: string.Empty,
            routingKey: QueueName,
            basicProperties: properties,
            body: body
        );

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
