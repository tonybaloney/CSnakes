using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.ObjectPool;

internal static class SmtpExtensions
{
    public static IHostApplicationBuilder AddSmtpClient(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.Services.AddSingleton<ISmtpClient, SmtpClientWithTelemetry>();

        // SmtpClient isn't thread safe. We're going to pool them
        builder.Services.AddSingleton(sp =>
        {
            return ObjectPool.Create(new DependencyInjectionObjectPoolPolicy<SmtpClient>(sp));
        });

        // This is the factory for creating SmtpClient instances, the pool will call this when it needs a new instance
        UriBuilder? smtpUri = null;
        builder.Services.AddTransient(_ =>
        {
            smtpUri ??= new UriBuilder(builder.Configuration.GetConnectionString(connectionName) ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found."));
            var smtpClient = new SmtpClient(smtpUri.Host, smtpUri.Port);

            if (smtpUri.Host != "localhost")
            {
                smtpClient.EnableSsl = true;
            }

            if (smtpUri.UserName != null)
            {
                smtpClient.Credentials = new NetworkCredential(smtpUri.UserName, smtpUri.Password);
            }
            return smtpClient;
        });

        builder.Services.AddOpenTelemetry()
            .WithTracing(t => t.AddSource(SmtpClientWithTelemetry.ActivitySourceName));

        return builder;
    }
}


// This is a simple object pool policy that uses the service provider to create new instances of T
// T should be a transient service
class DependencyInjectionObjectPoolPolicy<T>(IServiceProvider sp) : IPooledObjectPolicy<T> where T : class, new()
{
    public T Create() => sp.GetRequiredService<T>();

    public bool Return(T obj) => true;
}

public interface ISmtpClient
{
    Task SendMailAsync(MailMessage message);
}

class SmtpClientWithTelemetry(ObjectPool<SmtpClient> pool) : ISmtpClient
{
    public const string ActivitySourceName = "Smtp";
    private ActivitySource ActivitySource { get; } = new(ActivitySourceName);

    public async Task SendMailAsync(MailMessage message)
    {
        var activity = ActivitySource.StartActivity("SendMail", ActivityKind.Client);

        var client = pool.Get();

        if (activity is not null)
        {
            activity.AddTag("mail.from", message.From);
            activity.AddTag("mail.to", message.To);
            activity.AddTag("mail.subject", message.Subject);
            activity.AddTag("peer.service", $"{client.Host}:{client.Port}");
        }

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            if (activity is not null)
            {
                activity.AddTag("exception.message", ex.Message);
                activity.AddTag("exception.stacktrace", ex.ToString());
                activity.AddTag("exception.type", ex.GetType().FullName);
                activity.SetStatus(ActivityStatusCode.Error);
            }

            throw;
        }
        finally
        {
            activity?.Stop();

            pool.Return(client);
        }
    }
}