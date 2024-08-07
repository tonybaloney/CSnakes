using Microsoft.Extensions.DependencyInjection;
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
        services.AddSingleton<IPythonConvertor<double>, DoubleConvertor>();
        services.AddSingleton<IPythonConvertor<long>, LongConvertor>();
        services.AddSingleton<IPythonConvertor<string>, StringConvertor>();
        services.AddSingleton<IPythonConvertor<ITuple>, TupleConvertor>();
        return services;
    }
}
