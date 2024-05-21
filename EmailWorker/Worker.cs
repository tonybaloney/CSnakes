using System.Net.Mail;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;

namespace EmailWorker;

public class Worker(IOptions<EmailOptions> options, ILogger<Worker> logger, ServiceBusClient client, ISmtpClient smtpClient) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var fromEmail = options.Value.From;

        logger.LogInformation("Worker starting with from email: {fromEmail}", fromEmail);

        var processor = client.CreateProcessor("emails");

        processor.ProcessMessageAsync += async args =>
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Received email message: {message}", args.Message.Body.ToString());
            }

            var message = args.Message;

            var emailMessage = message.Body.ToObjectFromJson<EmailMessage>();

            var mailMessage = new MailMessage(fromEmail, emailMessage.To, emailMessage.Subject, emailMessage.Body)
            {
                IsBodyHtml = true
            };

            await smtpClient.SendMailAsync(mailMessage);

            await args.CompleteMessageAsync(message);
        };

        processor.ProcessErrorAsync += args =>
        {
            logger.LogError(args.Exception, "Error processing message");
            return Task.CompletedTask;
        };

        return processor.StartProcessingAsync(stoppingToken);
    }

    record EmailMessage(string To, string Subject, string Body);
}
