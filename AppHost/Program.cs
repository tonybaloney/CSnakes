var builder = DistributedApplication.CreateBuilder(args);

var identityDb = builder.AddPostgres("pg", port: 5432)
                        .WithDataVolume()
                        .PublishAsAzurePostgresFlexibleServer()
                        .AddDatabase("identityDb");

var smtpServer = builder.ExecutionContext.IsRunMode
    ? builder.AddMailDev("mailserver", httpPort: 1080)
    : builder.AddConnectionString("mailserver");

var sb = builder.AddAzureServiceBus("sb")
                .AddQueue("emails");

builder.AddProject<Projects.AppWithIdentity>("webapp")
       .WithReference(identityDb)
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