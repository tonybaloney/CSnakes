using Microsoft.Extensions.DependencyInjection;

namespace CSnakes.Runtime.Convertors;
internal static class ConvertorExtensions
{
    public static IServiceCollection RegisterDefaultConvertors(this IServiceCollection services)
    {
        services.AddSingleton<IPythonConvertor, DoubleConvertor>();
        services.AddSingleton<IPythonConvertor, LongConvertor>();
        services.AddSingleton<IPythonConvertor, StringConvertor>();
        services.AddSingleton<IPythonConvertor, TupleConvertor>();
        return services;
    }
}
