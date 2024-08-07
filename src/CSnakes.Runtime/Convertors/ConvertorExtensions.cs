using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSnakes.Runtime.Convertors;
internal static class ConvertorExtensions
{
    public static IServiceCollection RegisterDefaultConvertors(this IServiceCollection services)
    {
        services.TryAddSingleton<IPythonConvertor, DoubleConvertor>();
        services.TryAddSingleton<IPythonConvertor, LongConvertor>();
        services.TryAddSingleton<IPythonConvertor, StringConvertor>();
        services.TryAddSingleton<IPythonConvertor, TupleConvertor>();
        return services;
    }
}
