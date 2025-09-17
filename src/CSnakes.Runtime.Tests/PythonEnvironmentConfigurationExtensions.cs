using CSnakes.Runtime;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Logging;

namespace CSnakes.Tests;

public static class PythonEnvironmentConfigurationExtensions
{
    public static PythonEnvironmentConfiguration WithXUnitLogging(this PythonEnvironmentConfiguration configuration, ITestOutputHelper? testOutputHelper) =>
        configuration.WithLogger(new LoggerFactory([new XUnitLoggerProvider(new TestOutputHelperAccessor(testOutputHelper), new())]));

    sealed class TestOutputHelperAccessor(ITestOutputHelper? helper) : ITestOutputHelperAccessor
    {
        private ITestOutputHelper? helper = helper;

        public ITestOutputHelper? OutputHelper
        {
            get => this.helper ?? TestContext.Current.TestOutputHelper;
            set => this.helper = value;
        }
    }
}
