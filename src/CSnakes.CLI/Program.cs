using CSnakes;
using Microsoft.CodeAnalysis.Text;
using System.CommandLine;

var fileOption = new CliOption<FileInfo?>("--file", "-f")
{
    Description = "Path to the Python file to generate the C# code from."
};

var directoryOption = new CliOption<DirectoryInfo?>("--directory", "-d")
{
    Description = "Path to the directory containing the Python files to generate the C# code from."
};

var outputOption = new CliOption<DirectoryInfo?>("--output", "-o")
{
    Description = "Path to the directory to output the C# files to.",
    Required = true
};

var root = new CliRootCommand("csnakes -f <file>")
{
    fileOption,
    outputOption,
    directoryOption
};

root.SetAction(result =>
{
    FileInfo? fileInfo = result.GetValue(fileOption);
    DirectoryInfo? outputInfo = result.GetValue(outputOption);
    DirectoryInfo? directoryInfo = result.GetValue(directoryOption);

    if (fileInfo is null && directoryInfo is null)
    {
        Console.WriteLine("You must provide either a file or a directory.");
        return;
    }

    if (fileInfo is not null && directoryInfo is not null)
    {
        Console.WriteLine("You must provide either a file or a directory, not both.");
        return;
    }

    if (outputInfo is null)
    {
        Console.WriteLine("You must provide an output file.");
        return;
    }

    FileInfo[] files = (fileInfo, directoryInfo) switch
    {
        (not null, _) => [fileInfo],
        (_, not null) => directoryInfo.GetFiles("*.py"),
        _ => throw new InvalidOperationException()
    };

    if (outputInfo.Exists)
    {
        outputInfo.Delete(true);
    }

    outputInfo.Create();

    foreach (var file in files)
    {
        var sourceFile = SourceText.From(File.ReadAllText(file.FullName));
        var pascalFileName = PythonStaticGenerator.GetPascalFileName(Path.GetFileNameWithoutExtension(file.Name));

        if (PythonStaticGenerator.TryGenerateCode((error) =>
        {
            Console.WriteLine($"Error: {error.Message}");
        }, pascalFileName, Path.GetFileNameWithoutExtension(file.Name), sourceFile, out var source))
        {
            string outputFileName = $"{pascalFileName}.py.cs";
            File.WriteAllText(Path.Combine(outputInfo.FullName, outputFileName), source);
        }
    }

    Console.WriteLine($"Generated code for {files.Length} files.");
});

await new CliConfiguration(root).InvokeAsync(args);
