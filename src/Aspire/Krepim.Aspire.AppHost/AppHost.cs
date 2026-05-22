var builder = DistributedApplication.CreateBuilder(args);

var rabbitMq = builder.AddRabbitMQ("rabbitmq");
var redis = builder.AddRedis("redis");

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var identityDb = postgres.AddDatabase("identitydb");
var catalogDb = postgres.AddDatabase("catalogdb");
var orderingDb = postgres.AddDatabase("orderingdb");

var identityApi = builder.AddProject<Projects.Krepim_Identity_Api>("identity-api")
    .WithReference(identityDb)
    .WithReference(rabbitMq);

var catalogApi = builder.AddProject<Projects.Krepim_Catalog_Api>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(rabbitMq);

var basketApi = builder.AddProject<Projects.Krepim_Basket_Api>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitMq);

var orderingApi = builder.AddProject<Projects.Krepim_Ordering_Api>("ordering-api")
    .WithReference(orderingDb)
    .WithReference(rabbitMq);


var inventoryApi = builder.AddProject<Projects.Krepim_Inventory_Api>("inventory-api");
var paymentApi = builder.AddProject<Projects.Krepim_Payment_Api>("payment-api");

var apiGateway = builder.AddProject<Projects.Krepim_ApiGateway>("api-gateway")
    .WithReference(identityApi)
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(inventoryApi)
    .WithReference(orderingApi)
    .WithReference(paymentApi);

builder.AddProject<Projects.Krepim_Web_Server>("krepim-web-server")
    .WithReference(apiGateway)
    .WithExternalHttpEndpoints();

builder.Build().Run();
