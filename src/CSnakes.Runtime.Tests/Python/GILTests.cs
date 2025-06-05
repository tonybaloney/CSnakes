using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class GILTests : RuntimeTestBase
{
    [Fact]
    public void IsAcquired_WhenGilIsNotAcquired_ReturnsFalse()
    {
        Assert.False(GIL.IsAcquired);
    }

    [Fact]
    public void IsAcquired_AfterGilIsAcquired_ReturnsTrue()
    {
        using var gil = GIL.Acquire();

        Assert.True(GIL.IsAcquired);
    }

    [Fact]
    public void Dispose_LastReference_ReleasesGil()
    {
        var gil = GIL.Acquire();
        Assert.True(GIL.IsAcquired);

        gil.Dispose();
        Assert.False(GIL.IsAcquired);
    }

    [Fact]
    public void Dispose_OnceGilIsReleased_IsIdempotent()
    {
        var gil = GIL.Acquire();

        gil.Dispose();
        Assert.False(GIL.IsAcquired);   // GIL is released

        gil.Dispose();                  // Should be harmless
        Assert.False(GIL.IsAcquired);   // GIL is still released
    }

    [Fact]
    public void Acquire_NestedScopes_IsReentrant()
    {
        Assert.False(GIL.IsAcquired);

        using (var outer = GIL.Acquire())
        {
            Assert.True(GIL.IsAcquired);

            using (var inner = GIL.Acquire())
            {
                Assert.True(GIL.IsAcquired);
                Assert.True(outer.Equals(inner)); // Should be the same lock
            }

            Assert.True(GIL.IsAcquired); // After inner disposal, GIL should still be held
        }

        Assert.False(GIL.IsAcquired); // After outer dispose, GIL should be released
    }

    [Fact]
    public void MultipleThreads_ShouldMaintainCorrectGILState()
    {
        var thread1State = new
        {
            GilIsAcquired = false,
            GilAcquiredEvent = new ManualResetEventSlim(),
        };

        var thread2State = new
        {
            GilIsAcquired = false,
            GilAcquiredEvent = new ManualResetEventSlim(),
        };

        var thread1 = new Thread(() =>
        {
            using var gil = GIL.Acquire();
            thread1State = thread1State with { GilIsAcquired = GIL.IsAcquired }; // capture
            thread1State.GilAcquiredEvent.Set(); // signal
            thread2State.GilAcquiredEvent.Wait(); // wait for thread 2 to capture
        });

        var thread2 = new Thread(() =>
        {
            thread1State.GilAcquiredEvent.Wait(); // wait for thread 1 to capture
            thread2State = thread2State with { GilIsAcquired = GIL.IsAcquired }; // capture
            thread2State.GilAcquiredEvent.Set(); // signal
        });

        thread1.Start(); // Start thread 1 which acquires the GIL
        thread2.Start(); // Start thread 2 which checks the GIL state

        // Wait for both threads to complete

        thread1.Join(TimeSpan.FromSeconds(30));
        thread2.Join(TimeSpan.FromSeconds(30));

        // Assert that each thread saw its own state for the GIL, one acquired
        // while the other did not.

        Assert.True(thread1State.GilIsAcquired);
        Assert.False(thread2State.GilIsAcquired);
    }
}
