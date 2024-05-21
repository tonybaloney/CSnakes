internal static class ServiceBusTracingExtensions
{
    public static IResourceBuilder<T> WithAzureTracing<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEnvironment
    {
        return builder.WithEnvironment("AZURE_EXPERIMENTAL_ENABLE_ACTIVITY_SOURCE", "true");
    }
}
