// <copyright file="WebhookStoreContext.cs" company="Dmitry Fedoseev">
// Copyright (c) Dmitry Fedoseev. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using WebhookAggregator.Models;

namespace WebhookAggregator.Data;

public class WebhookStoreContext(DbContextOptions<WebhookStoreContext> options) : DbContext(options)
{
    public DbSet<WebhookEvent> WebhookEvents { get; set; }
}