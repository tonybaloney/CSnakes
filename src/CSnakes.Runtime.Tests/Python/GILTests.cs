using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class GILTests : RuntimeTestBase
{
    [Fact]
    public void DisposeIsIdempotentOnceDisposed()
    {
        var gil = GIL.Acquire();
        gil.Dispose();
        gil.Dispose();
    }
}
