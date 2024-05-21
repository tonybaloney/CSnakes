var builder = DistributedApplication.CreateBuilder(args);

var identityDb = builder.AddSqlServer("sql", port: 1433)
                        .WithDataVolume()
                        .AddDatabase("identityDb");

var smtpServer = builder.ExecutionContext.IsRunMode
    ? builder.AddMailDev("mailserver", httpPort: 1080)
    : builder.AddConnectionString("mailserver");

var sb = builder.AddAzureServiceBus("sb")
                .AddQueue("emails");

builder.AddProject<Projects.AppWithIdentity>("webapp")
       .WithReference(identityDb, "DefaultConnection")
       .WithReference(sb)
       .WithExternalHttpEndpoints()
       .WithAzureTracing();

var fromMail = builder.AddParameter("frommail");

builder.AddProject<Projects.EmailWorker>("emailworker")
    .WithReference(sb)
    .WithReference(smtpServer)
    .WithEnvironment("Email__From", fromMail)
    .WithAzureTracing();


builder.Build().Run();