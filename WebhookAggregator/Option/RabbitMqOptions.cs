// <copyright file="RabbitMqOptions.cs" company="Dmitry Fedoseev">
// Copyright (c) Dmitry Fedoseev. All rights reserved.
// </copyright>

namespace WebhookAggregator.Option;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    
    public int Port { get; set; } = 5672;
    
    public string UserName { get; set; } = "guest";
    
    public string Password { get; set; } = "guest";
}