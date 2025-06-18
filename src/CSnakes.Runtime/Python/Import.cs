using CSnakes.Runtime.CPython;
using System.Text;

namespace CSnakes.Runtime.Python;

public static class Import
{
    public static PyObject ImportModule(string module)
    {
        using (GIL.Acquire())
            return CPythonAPI.Import(module);
    }

    public static PyObject ImportModule(string module, string source, string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(module);
        ArgumentException.ThrowIfNullOrEmpty(source);
        ArgumentException.ThrowIfNullOrEmpty(path);

        return ImportModule(module, Encoding.UTF8.GetBytes(source), path);
    }

    public static PyObject ImportModule(string module, ReadOnlySpan<byte> u8Source, string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(module);
        if (u8Source.Length == 0)
            throw new ArgumentException("Source code cannot be empty.", nameof(u8Source));
        ArgumentException.ThrowIfNullOrEmpty(path);

        using (GIL.Acquire())
        {
            return CPythonAPI.Import(module, u8Source, path);
        }
    }

    public static void ReloadModule(ref PyObject module)
    {
        using (GIL.Acquire())
        {
            var newModule = CPythonAPI.ReloadModule(module);
            module.Dispose();
            module = newModule;
        }
    }
}
