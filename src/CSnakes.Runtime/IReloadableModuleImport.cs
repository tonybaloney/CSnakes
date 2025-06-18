namespace CSnakes.Runtime;

public interface IReloadableModuleImport : IDisposable
{
    /// <summary>
    /// Reload the module, useful for development if the Python code has changed.
    /// </summary>
    void ReloadModule();
}
