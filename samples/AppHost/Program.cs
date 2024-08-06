var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.WebApp>("webapp");

builder.Build().Run();
