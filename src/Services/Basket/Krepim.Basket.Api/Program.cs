using Krepim.Basket.Api.Endpoints;
using Krepim.Basket.Application;
using Krepim.Basket.Infrastructure;
using Krepim.SharedKernel.Exceptions;
using Krepim.SharedKernel.Results.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseAuthorization();

var apiGroup = app.MapGroup("")
    .AddEndpointFilter<ResultEndpointFilter>();

apiGroup.MapBasketEndpoints();

app.Run();