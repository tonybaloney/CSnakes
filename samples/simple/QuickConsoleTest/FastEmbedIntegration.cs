using CSnakes.Runtime.Python;
using CSnakes.Runtime;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace QuickConsoleTest;
public sealed class FastEmbedIntegration : IDisposable
{
    private readonly PyObject _module;

    private readonly ILogger logger;

    internal FastEmbedIntegration(IPythonEnvironment env)
    {
        this.logger = env.Logger;
        using (GIL.Acquire())
        {
            logger.LogInformation("Importing module {ModuleName}", "fast_embed");
            _module = Import.ImportModule("fast_embed");
        }
    }

    public void Dispose() => _module.Dispose();

    public IReadOnlyList<IReadOnlyList<double>> Generate(IReadOnlyList<string> items)
    {
        using (GIL.Acquire())
        {
            using PyObject __underlyingPythonFunc = this._module.GetAttr("generate_embeddings");
            using PyObject itemsArg = PyObject.From(items);
            using PyObject __result_pyObject = __underlyingPythonFunc.Call(itemsArg);
            return __result_pyObject.As<IReadOnlyList<IReadOnlyList<double>>>();
        }
    }
}
