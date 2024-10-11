
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class ImportTests : RuntimeTestBase
{
    [Fact]
    public void TestImportModule()
    {
        using (GIL.Acquire())
        {
            using PyObject? sys = Import.ImportModule("sys");
            Assert.NotNull(sys);
            Assert.Equal("<module 'sys' (built-in)>", sys!.ToString());
        }
    }

    [Fact]
    public void TestReloadModule()
    {
        using (GIL.Acquire())
        {
            using PyObject? sys = Import.ImportModule("sys");
            Assert.NotNull(sys);
            using PyObject? reloaded = Import.ReloadModule(sys!);
            Assert.NotNull(reloaded);
            Assert.Equal("<module 'sys' (built-in)>", reloaded!.ToString());
        }
    }

    [Fact]
    public void TestReloadModuleThatIsntAModule()
    {
        using (GIL.Acquire())
        {
            using PyObject? sys = PyObject.From<int>(42); // definitely not a module
            Assert.NotNull(sys);
            Assert.Throws<PythonInvocationException>(() => Import.ReloadModule(sys!));
        }
    }
}
