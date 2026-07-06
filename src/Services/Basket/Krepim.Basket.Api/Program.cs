using Krepim.Basket.Api.Endpoints;
using Krepim.Basket.Application;
using Krepim.Basket.Infrastructure;
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
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseAuthorization();

var apiGroup = app.MapGroup("/api/basket")
    .AddEndpointFilter<ResultEndpointFilter>();

apiGroup.MapBasketEndpoints();

app.Run();