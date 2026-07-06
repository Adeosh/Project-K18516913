using Krepim.Inventory.Api.Endpoints;
using Krepim.Inventory.Application;
using Krepim.Inventory.Infrastructure;
using Krepim.Inventory.Infrastructure.Database;
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

await app.ApplyMigrationsAsync<InventoryDbContext>();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

var apiGroup = app.MapGroup("/api/inventory")
    .AddEndpointFilter<ResultEndpointFilter>();

apiGroup.MapInventoryEndpoints();

app.Run();
