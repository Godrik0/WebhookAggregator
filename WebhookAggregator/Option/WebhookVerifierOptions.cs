// <copyright file="WebhookVerifierOptions.cs" company="Dmitry Fedoseev">
// Copyright (c) Dmitry Fedoseev. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace WebhookAggregator.Option;

public class WebhookVerifierOptions
{
    [Required]
    public string Secret { get; init; } = string.Empty;
}