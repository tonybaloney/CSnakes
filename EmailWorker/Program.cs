using EmailWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddSmtpClient("mailserver");
builder.AddAzureServiceBusClient("sb");

builder.Services.AddOptions<EmailOptions>()
    .BindConfiguration("Email")
    .ValidateDataAnnotations();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
