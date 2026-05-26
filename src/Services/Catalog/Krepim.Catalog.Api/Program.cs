using Krepim.Catalog.Api.Endpoints.Products;
using Krepim.Catalog.Application;
using Krepim.Catalog.Infrastructure;
using Krepim.Catalog.Infrastructure.Database;
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
builder.Services.AddAuthorization();

var app = builder.Build();

await app.ApplyMigrationsAsync<CatalogDbContext>();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthorization();

var apiGroup = app.MapGroup("/api/products")
    .WithTags("Products")
    .AddEndpointFilter<ResultEndpointFilter>();

apiGroup.MapCreateProductEndpoint();
apiGroup.MapSearchProductsEndpoint();
apiGroup.MapManagerEndpoints();
apiGroup.MapClientEndpoints();

app.Run();