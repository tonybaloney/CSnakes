
namespace CSnakes;

public static class NamespaceHelper
{
    /// <summary>
    /// Convert a file path to a Python import path.
    /// e.g. foo/bar/baz.py -&gt; foo.bar.baz
    /// or foo/bar/__init__.py -&gt; foo.bar
    /// </summary>
    /// <param name="path">Relative path</param>
    public static string AsPythonImportPath(string path)
    {
        if (Path.IsPathRooted(path))
            throw new ArgumentException("Path must be relative.", nameof(path));
        if (Path.GetExtension(path) is var ext && !".py".Equals(ext, StringComparison.OrdinalIgnoreCase) && !".pyi".Equals(ext, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Path must be a Python file.", nameof(path));

        string filename = Path.GetFileNameWithoutExtension(path);
        string directory = Path.GetDirectoryName(path) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(directory))
        {
            return filename;
        }

        var folders = directory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Select(p => p.Replace(".", "_"))
            .Where(p => !string.IsNullOrWhiteSpace(p));

        // If the file is __init__.py, we don't need to include it in the import path.
        // Otherwise, we include the filename in the import path.
        return string.Join(".", folders) + (filename == "__init__" ? string.Empty : $".{filename}");
    }

    public static string AsDotNetNamespace(string path)
    {
        string filename = Path.GetFileNameWithoutExtension(path);
        string directory = Path.GetDirectoryName(path) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(directory))
        {
            return string.Empty;
        }

        var folders = directory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Select(p => p.Replace(".", "_"))
            .Where(p => !string.IsNullOrWhiteSpace(p));

        if (filename == "__init__")
        {
            if (folders.Count() <= 1)
            {
                // If the path is just the filename, we return an empty string.
                return string.Empty;
            }

            // If the file is __init__.py, we don't need to include it in the namespace.
            return string.Join(".", folders.Take(folders.Count() - 1).Select(s => s.ToPascalCase()));
        }
        // Otherwise, we include the filename in the namespace.
        return string.Join(".", folders.Select(s => s.ToPascalCase()));
    }

    public static string AsDotNetClassName(string path)
    {
        string filename = Path.GetFileNameWithoutExtension(path);
        if (filename == "__init__")
        {
            return Path.GetDirectoryName(path).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Last().ToPascalCase();
        }
        // Otherwise, we return the filename as a PascalCase class name.
        return filename.ToPascalCase();
    }
}
