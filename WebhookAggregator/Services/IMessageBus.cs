// <copyright file="IMessageBus.cs" company="Dmitry Fedoseev">
// Copyright (c) Dmitry Fedoseev. All rights reserved.
// </copyright>

namespace WebhookAggregator.Services;

public interface IMessageBus
{
    Task SendAsync(string message);
}