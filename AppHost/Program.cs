var builder = DistributedApplication.CreateBuilder(args);

var identityDb = builder.AddSqlServer("sql", port: 1433)
                        .WithDataVolume()
                        .AddDatabase("identityDb");

var smtpServer = builder.AddMailDev("mailserver", httpPort: 1080);

var sb = builder.AddAzureServiceBus("sb")
                .AddQueue("emails");

builder.AddProject<Projects.AppWithIdentity>("webapp")
       .WithReference(identityDb, "DefaultConnection")
       .WithReference(sb)
       .WithAzureTracing();

builder.AddProject<Projects.EmailWorker>("emailworker")
    .WithReference(sb)
    .WithReference(smtpServer)
    .WithAzureTracing();


builder.Build().Run();