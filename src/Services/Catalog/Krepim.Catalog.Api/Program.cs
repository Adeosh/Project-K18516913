using Krepim.Catalog.Api.Endpoints.Categories;
using Krepim.Catalog.Api.Endpoints.Products;
using Krepim.Catalog.Application;
using Krepim.Catalog.Infrastructure;
using Krepim.Catalog.Infrastructure.Database;
using Krepim.SharedKernel.Exceptions;
using Krepim.SharedKernel.Extensions;
using Krepim.SharedKernel.Results.Filters;
using Microsoft.AspNetCore.RateLimiting;
using System.Runtime.Intrinsics.Arm;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDBClient("Catalog-MongoDb");
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddKrepimJwtAuth(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PublicSearchLimit", httpContext => // защита публичного поиска
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.AddConcurrencyLimiter("ImageUploadLimit", opt => // защита CPU/RAM от массовой загрузки картинок
    {
        opt.PermitLimit = 5;
        opt.QueueLimit = 20;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Слишком много запросов. Пожалуйста, подождите.", token);
    };
});

var app = builder.Build();

await app.ApplyMigrationsAsync<CatalogDbContext>();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

var apiGroup = app.MapGroup("/api/products")
    .WithTags("Products")
    .AddEndpointFilter<ResultEndpointFilter>();

apiGroup.MapCreateProductEndpoint();
apiGroup.MapManagerEndpoints();
apiGroup.MapClientEndpoints();
apiGroup.MapCategoryEndpoints();
apiGroup.MapImageEndpoints();

app.Run();