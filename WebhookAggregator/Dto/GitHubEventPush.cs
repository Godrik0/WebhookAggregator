// <copyright file="GitHubEventPush.cs" company="Dmitry Fedoseev">
// Copyright (c) Dmitry Fedoseev. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace WebhookAggregator.Dto;

public record GitHubEventPush
{
    [JsonPropertyName("id")] public int Id { get; init; }

    [JsonPropertyName("repository")] public required Repository Repository { get; init; }

    [JsonPropertyName("head_commit")] public required Commit Commit { get; init; }
}

public record Repository
{
    [JsonPropertyName("id")] public int Id { get; init; }

    [JsonPropertyName("name")] public required string Name { get; init; }

    [JsonPropertyName("html_url")] public required Uri Url { get; init; }
}

public record Commit
{
    [JsonPropertyName("id")] public required string Id { get; init; }

    [JsonPropertyName("message")] public string? Message { get; init; }

    [JsonPropertyName("timestamp")] public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("url")] public Uri? Url { get; init; }
}