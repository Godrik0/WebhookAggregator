// <copyright file="RabbitMqMessageBus.cs" company="Dmitry Fedoseev">
// Copyright (c) Dmitry Fedoseev. All rights reserved.
// </copyright>

using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using WebhookAggregator.Option;

namespace WebhookAggregator.Services;

public class RabbitMqMessageBus : IMessageBus, IAsyncDisposable
{
    private readonly ILogger<RabbitMqMessageBus> _logger;
    private readonly RabbitMqOptions _options;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;

    private bool _isDisposed;

    public RabbitMqMessageBus(ILogger<RabbitMqMessageBus> logger, IOptions<RabbitMqOptions> options)
    {
        _logger = logger;
        _options = options.Value;

        _factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password,
            Port = _options.Port
        };

        TryConnect();
    }

    private bool IsConnected => _connection?.IsOpen ?? false;

    private async Task TryConnect()
    {
        if (_isDisposed) return;

        if (IsConnected) return;

        try
        {
            await CloseConnectionAndChannel();
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(queue: "webhooks",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not create RabbitMQ connection");
            await CloseConnectionAndChannel();
        }
    }

    public async Task SendAsync(string message)
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(RabbitMqMessageBus));

        if (!IsConnected)
        {
            _logger.LogWarning("RabbitMQ connection is not established");
            await TryConnect();

            if (!IsConnected)
            {
                _logger.LogError("RabbitMQ connection is not established");
                throw new Exception($"RabbitMQ connection is not established");
            }
        }

        try
        {
            var body = Encoding.UTF8.GetBytes(message);


            await _channel.BasicPublishAsync(exchange: "", routingKey: "webhooks", body: body);
            _logger.LogInformation("RabbitMQ message sent");
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ message failed");
            await CloseConnectionAndChannel();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        await CloseConnectionAndChannel();
        _logger.LogTrace("RabbitMQ connection closed");
    }

    private async Task CloseConnectionAndChannel()
    {
        try
        {
            await _channel.CloseAsync();
            _channel.Dispose();
            _logger.LogTrace("RabbitMQ connection closed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not close connection");
        }

        try
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
                _logger.LogTrace("RabbitMQ connection closed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not close connection");
        }
    }
}
