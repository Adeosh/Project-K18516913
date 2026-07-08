using Microsoft.Extensions.Configuration;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

#region Config

var useVolumes = builder.Configuration.GetValue<bool>("AppHostConfiguration:UseVolumes");
var useDedicatedPorts = builder.Configuration.GetValue<bool>("AppHostConfiguration:UseDedicatedPorts");
var pgPort = builder.Configuration.GetValue<int>("AppHostConfiguration:Postgres:Port", 15432);
var pgPassword = builder.AddParameter("postgres-password", secret: true);
var jwtSecret = builder.AddParameter("JwtSecret", secret: true); // dotnet user-secrets init

#endregion

#region Tools & Infrastructure

var rabbitMq = builder.AddRabbitMQ("rabbitmq");
var redis = builder.AddRedis("redis");
var postgres = builder.AddPostgres("postgres", password: pgPassword, port: useDedicatedPorts ? pgPort : null)
    .WithImageTag(builder.Configuration["AppHostConfiguration:Postgres:ImageTag"] ?? "17.2")
    .WithLifetime(ContainerLifetime.Persistent);

if (useVolumes)
    postgres.WithDataVolume("krepim-postgres-data");

if (builder.Configuration.GetValue<bool>("AppHostConfiguration:Postgres:UsePgWeb"))
    postgres.WithPgWeb();

var mongo = builder.AddMongoDB("mongo")
    .WithLifetime(ContainerLifetime.Persistent);

if (useVolumes)
    mongo.WithDataVolume("krepim-mongo-data");

if (builder.Configuration.GetValue<bool>("AppHostConfiguration:Mongo:UseExpress"))
    mongo.WithMongoExpress();

var scalar = builder.AddScalarApiReference("scalar");

var minio = builder.AddMinioContainer("minio")
                   .WithHttpEndpoint(port: 9000, targetPort: 9000)
                   .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
                   .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
                   .WithDataVolume("krepim-minio-data");

#endregion

#region DataBases

var identityDb = postgres.AddDatabase("IdentityDb");
var catalogDb = postgres.AddDatabase("CatalogDb");
var orderingDb = postgres.AddDatabase("OrderingDb");
var inventoryDb = postgres.AddDatabase("InventoryDb");
var paymentDb = postgres.AddDatabase("PaymentDb");

var catalogMongoDb = mongo.AddDatabase("Catalog-MongoDb");

#endregion

#region APIs + Scalar

var identityApi = builder.AddProject<Projects.Krepim_Identity_Api>("identity-api")
    .WithReference(identityDb)
    .WithReference(rabbitMq)
    .WithEnvironment("Jwt__SecretKey", jwtSecret)
    .WaitFor(postgres);
scalar.WithApiReference(identityApi);

var catalogApi = builder.AddProject<Projects.Krepim_Catalog_Api>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(catalogMongoDb)
    .WithReference(rabbitMq)
    .WithEnvironment("Jwt__SecretKey", jwtSecret)
    .WaitFor(postgres);
scalar.WithApiReference(catalogApi);

var basketApi = builder.AddProject<Projects.Krepim_Basket_Api>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithEnvironment("Jwt__SecretKey", jwtSecret);
scalar.WithApiReference(basketApi);

var orderingApi = builder.AddProject<Projects.Krepim_Ordering_Api>("ordering-api")
    .WithReference(orderingDb)
    .WithReference(rabbitMq)
    .WithEnvironment("ClientApp__FrontendUrl", "https://192.168.0.114:63137")
    .WithEnvironment("Jwt__SecretKey", jwtSecret)
    .WaitFor(postgres);
scalar.WithApiReference(orderingApi);

var inventoryApi = builder.AddProject<Projects.Krepim_Inventory_Api>("inventory-api")
    .WithReference(inventoryDb)
    .WithReference(rabbitMq)
    .WithEnvironment("Jwt__SecretKey", jwtSecret)
    .WaitFor(postgres);
scalar.WithApiReference(inventoryApi);

var paymentApi = builder.AddProject<Projects.Krepim_Payment_Api>("payment-api")
    .WithReference(paymentDb)
    .WithReference(rabbitMq)
    .WithEnvironment("Jwt__SecretKey", jwtSecret)
    .WaitFor(postgres);
scalar.WithApiReference(paymentApi);

var apiGateway = builder.AddProject<Projects.Krepim_ApiGateway>("api-gateway")
    .WithReference(identityApi)
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(inventoryApi)
    .WithReference(orderingApi)
    .WithReference(paymentApi)
    .WithEndpoint(port: 7115, scheme: "https", isProxied: false)
    .WithEndpoint(port: 5078, scheme: "http", isProxied: false);

#endregion

#region Frontend

string frontEndPath = "../../Clients/Krepim.Web/krepim.web.client";
builder.AddNpmApp("krepim-web-client", frontEndPath, "dev")
    .WithReference(apiGateway)
    .WithEnvironment("BROWSER", "none")
    .WithEndpoint(port: 63137, scheme: "https", env: "PORT", isExternal: true, name: "vite", isProxied: false)
    .PublishAsDockerFile();

#endregion

await builder.Build().RunAsync();