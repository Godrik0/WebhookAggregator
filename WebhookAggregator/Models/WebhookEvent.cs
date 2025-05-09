// <copyright file="WebhookEvent.cs" company="Dmitry Fedoseev">
// Copyright (c) Dmitry Fedoseev. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebhookAggregator.Models;

public record WebhookEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; init; }

    [Required] [MaxLength(100)] public required string EventType { get; init; }

    [Required] public required string RawData { get; init; }

    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}