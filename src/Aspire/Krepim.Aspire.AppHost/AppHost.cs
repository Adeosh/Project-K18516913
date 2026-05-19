var builder = DistributedApplication.CreateBuilder(args);

var identityApi = builder.AddProject<Projects.Krepim_Identity_Api>("identity-api");
var catalogApi = builder.AddProject<Projects.Krepim_Catalog_Api>("catalog-api");
var basketApi = builder.AddProject<Projects.Krepim_Basket_Api>("basket-api");
var inventoryApi = builder.AddProject<Projects.Krepim_Inventory_Api>("inventory-api");
var orderingApi = builder.AddProject<Projects.Krepim_Ordering_Api>("ordering-api");
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
