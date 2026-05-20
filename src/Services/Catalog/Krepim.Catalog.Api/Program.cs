using Krepim.Catalog.Api.Endpoints.Products;
using Krepim.Catalog.Application;
using Krepim.Catalog.Infrastructure;
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

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

var apiGroup = app.MapGroup("/api/products")
    .WithTags("Products")
    .AddEndpointFilter<ResultEndpointFilter>();

apiGroup.MapCreateProductEndpoint();
apiGroup.MapSearchProductsEndpoint();
apiGroup.MapManagerEndpoints();
apiGroup.MapClientEndpoints();

app.Run();