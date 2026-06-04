using Krepim.Payment.Api.Endpoints;
using Krepim.Payment.Application;
using Krepim.Payment.Infrastructure;
using Krepim.Payment.Infrastructure.Database;
using Krepim.SharedKernel.Exceptions;
using Krepim.SharedKernel.Extensions;
using Krepim.SharedKernel.Results.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddKrepimJwtAuth(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsAsync<PaymentDbContext>();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapWebhookEndpoints();

var apiGroup = app.MapGroup("")
    .AddEndpointFilter<ResultEndpointFilter>();

apiGroup.MapPaymentEndpoints();

app.Run();
