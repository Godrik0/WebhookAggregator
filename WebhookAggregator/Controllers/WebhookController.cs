// <copyright file="WebhookController.cs" company="Dmitry Fedoseev">
// Copyright (c) Dmitry Fedoseev. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebhookAggregator.Data;
using WebhookAggregator.Models;
using WebhookAggregator.Services;

namespace WebhookAggregator.Controllers;

[ApiController]
[Route("webhook/github")]
public class WebhookController : Controller
{
    private readonly ILogger<WebhookController> _logger;

    private readonly WebhookStoreContext _context;

    private readonly IWebhookVerifier _webhookVerifier;

    private readonly IMessageBus _messageBus;

    public WebhookController(ILogger<WebhookController> logger, IWebhookVerifier webhookVerifier,
        WebhookStoreContext context, IMessageBus messageBus)
    {
        _logger = logger;
        _context = context;
        _webhookVerifier = webhookVerifier;
        _messageBus = messageBus;
    }

    // POST
    [HttpPost]
    public async Task<IActionResult> HandleGitHubWebhookAsync()
    {
        _logger.LogInformation("Received external events");

        var eventType = Request.Headers["X-GitHub-Event"];
        if (string.IsNullOrEmpty(eventType))
        {
            _logger.LogWarning("Missing 'X-GitHub-Event' header.");
            return BadRequest("Missing 'X-GitHub-Event' header.");
        }

        var rawJson = await ReadRequestBodyAsync();

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            _logger.LogWarning("Received empty request body.");
            return BadRequest("Received empty request body.");
        }

        var signature = Request.Headers["X-Hub-Signature-256"];
        var isSignatureValid = _webhookVerifier.IsValid(rawJson, signature);

        var webhookEvent = CreateWebhookEvent(eventType, rawJson);

        if (!isSignatureValid)
        {
            await SaveEventAsync(webhookEvent, false);
            return BadRequest("Invalid signature.");
        }

        await SaveEventAsync(webhookEvent, true);
        await SendToMessageBusAsync(eventType, rawJson);

        return Ok();
    }

    private WebhookEvent CreateWebhookEvent(string eventType, string rawJson) =>
        new()
        {
            EventType = eventType,
            RawData = rawJson,
            ReceivedAt = DateTimeOffset.UtcNow
        };

    private async Task<string> ReadRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);
        return await reader.ReadToEndAsync();
    }

    private async Task SaveEventAsync(WebhookEvent webhookEvent, bool isValid)
    {
        try
        {
            await _context.WebhookEvents.AddAsync(webhookEvent);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved {Validity} event type {EventType} (DB Id: {DbId}).",
                isValid ? "valid" : "invalid", webhookEvent.EventType, webhookEvent.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An exception occurred while saving event.");
            throw;
        }
    }

    private async Task SendToMessageBusAsync(string eventType, string rawJson)
    {
        try
        {
            await _messageBus.SendAsync(rawJson);
            _logger.LogInformation("Sent event {EventType} to RabbitMq.", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to send event {EventType} to RabbitMq.", eventType);
            throw;
        }
    }
}