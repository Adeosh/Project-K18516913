var builder = DistributedApplication.CreateBuilder(args);

#region Tools
var rabbitMq = builder.AddRabbitMQ("rabbitmq");
var redis = builder.AddRedis("redis");

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var mongo = builder.AddMongoDB("mongo")
    .WithMongoExpress();
#endregion
#region DataBases
var identityDb = postgres.AddDatabase("identitydb");
var catalogDb = postgres.AddDatabase("catalogdb");
var catalogMongoDb = mongo.AddDatabase("catalog-mongodb");
var orderingDb = postgres.AddDatabase("orderingdb");
var inventoryDb = postgres.AddDatabase("inventorydb");
var paymentDb = postgres.AddDatabase("paymentdb");
#endregion
#region APIs
var identityApi = builder.AddProject<Projects.Krepim_Identity_Api>("identity-api")
    .WithReference(identityDb)
    .WithReference(rabbitMq);

var catalogApi = builder.AddProject<Projects.Krepim_Catalog_Api>("catalog-api")
    .WithReference(catalogDb)
    .WithReference(catalogMongoDb)
    .WithReference(rabbitMq);

var basketApi = builder.AddProject<Projects.Krepim_Basket_Api>("basket-api")
    .WithReference(redis)
    .WithReference(rabbitMq);

var orderingApi = builder.AddProject<Projects.Krepim_Ordering_Api>("ordering-api")
    .WithReference(orderingDb)
    .WithReference(rabbitMq);

var inventoryApi = builder.AddProject<Projects.Krepim_Inventory_Api>("inventory-api")
    .WithReference(inventoryDb)
    .WithReference(rabbitMq);

var paymentApi = builder.AddProject<Projects.Krepim_Payment_Api>("payment-api")
    .WithReference(paymentDb)
    .WithReference(rabbitMq);

var apiGateway = builder.AddProject<Projects.Krepim_ApiGateway>("api-gateway")
    .WithReference(identityApi)
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(inventoryApi)
    .WithReference(orderingApi)
    .WithReference(paymentApi);
#endregion
#region Front
string frontEndPath = "../../Clients/Krepim.Web/krepim.web.client";

builder.AddNpmApp("krepim-web-client", frontEndPath, "dev")
    .WithReference(apiGateway)
    .WithEnvironment("BROWSER", "none")
    .WithEndpoint(scheme: "https", env: "PORT", isExternal: true, name: "vite")
    .PublishAsDockerFile();
#endregion

builder.Build().Run();