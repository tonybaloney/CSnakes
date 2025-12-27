using CSnakes.Runtime.Python;
using System.Threading.Tasks;

namespace Integration.Tests;
public class AwaitableTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CustomAwaitable()
    {
        var mod = Env.TestAwaitables();
        using var awaitable = mod.TestAwaitable();
        var result = await awaitable.WaitAsync(TestContext.Current.CancellationToken);
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task CustomAwaitable_GetAttr()
    {
        var mod = Env.TestAwaitables();
        using var awaitable = mod.TestAwaitable();
        using var obj = awaitable.GetPyObject();
        using var resultFloat = obj.GetAttr("seconds");
        var result = resultFloat.As<double>();
        _ = await awaitable.WaitAsync(TestContext.Current.CancellationToken);
        Assert.Equal(0.1, result);
    }
}
