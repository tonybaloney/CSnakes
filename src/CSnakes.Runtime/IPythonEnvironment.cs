using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

public interface IPythonEnvironment
{
    public ILogger<IPythonEnvironment> Logger { get; }
}
