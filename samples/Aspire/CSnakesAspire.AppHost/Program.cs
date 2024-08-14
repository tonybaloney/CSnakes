var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.CSnakesAspire_ApiService>("apiservice");

builder.AddProject<Projects.CSnakesAspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
