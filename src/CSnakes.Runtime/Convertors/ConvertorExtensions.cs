using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CSnakes.Runtime.Convertors;
internal static class ConvertorExtensions
{
    public static IServiceCollection RegisterDefaultConvertors(this IServiceCollection services)
    {
        services.TryAddSingleton<IPythonConvertor<double>, DoubleConvertor>();
        services.TryAddSingleton<IPythonConvertor<long>, LongConvertor>();
        services.TryAddSingleton<IPythonConvertor<string>, StringConvertor>();
        services.TryAddSingleton<IPythonConvertor<ITuple>, TupleConvertor>();
        return services;
    }
}
