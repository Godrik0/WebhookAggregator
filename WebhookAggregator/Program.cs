using WebhookAggregator.Data;
using WebhookAggregator.Option;
using WebhookAggregator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

var connectionString = builder.Configuration.GetConnectionString("WebhookStore") ?? "Data Source=WebhookStore.db";
builder.Services.AddSqlite<WebhookStoreContext>(connectionString);


builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(key: nameof(RabbitMqOptions)))
    .AddSingleton<IMessageBus, RabbitMqMessageBus>();

builder.Services
    .Configure<WebhookVerifierOptions>(builder.Configuration.GetSection(key: nameof(WebhookVerifierOptions)))
    .AddScoped<IWebhookVerifier, WebhookVerifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();