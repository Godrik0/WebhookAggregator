// <copyright file="IWebhookVerifier.cs" company="Dmitry Fedoseev">
// Copyright (c) Dmitry Fedoseev. All rights reserved.
// </copyright>

namespace WebhookAggregator.Services;

public interface IWebhookVerifier
{
    public bool IsValid(string payload, string? signature);
}