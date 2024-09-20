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
};

var removeOutputOption = new CliOption<bool>("--remove-output", "-rm")
{
    Description = "Remove the output directory before generating the code.",
    DefaultValueFactory = (_) => false
};

var root = new CliRootCommand("Creates C# wrapper to call Python code.")
{
    fileOption,
    outputOption,
    directoryOption,
    removeOutputOption
};

root.SetAction(result =>
{
    FileInfo? fileInfo = result.GetValue(fileOption);
    DirectoryInfo? outputInfo = result.GetValue(outputOption);
    DirectoryInfo? directoryInfo = result.GetValue(directoryOption);
    bool removeOutput = result.GetValue(removeOutputOption);

    if (fileInfo is null && directoryInfo is null)
    {
        Console.Error.WriteLine("You must provide either a file or a directory.");
        return (int)ErrorCode.InvalidArguments;
    }

    if (fileInfo is not null && directoryInfo is not null)
    {
        Console.Error.WriteLine("You must provide either a file or a directory, not both.");
        return (int)ErrorCode.InvalidArguments;
    }

    if (directoryInfo is not null && outputInfo is null)
    {
        Console.Error.WriteLine("Streaming output to stdout for bulk files is not supported.");
        return (int)ErrorCode.InvalidArguments;
    }

    FileInfo[] files = (fileInfo, directoryInfo) switch
    {
        (not null, _) => [fileInfo],
        (_, not null) => directoryInfo.GetFiles("*.py"),
        _ => throw new InvalidOperationException()
    };

    if (outputInfo is not null && outputInfo.Exists && removeOutput)
    {
        outputInfo.Delete(true);
        outputInfo.Create();
    }
    else if (outputInfo is not null && !outputInfo.Exists)
    {
        outputInfo.Create();
    }

    foreach (var file in files)
    {
        var sourceFile = SourceText.From(File.ReadAllText(file.FullName));
        var pascalFileName = PythonStaticGenerator.GetPascalFileName(Path.GetFileNameWithoutExtension(file.Name));

        if (PythonStaticGenerator.TryGenerateCode((error) =>
        {
            Console.Error.WriteLine($"{file.FullName}:{error.StartLine}:{error.StartColumn}: error: {error.Message}");
        }, pascalFileName, Path.GetFileNameWithoutExtension(file.Name), sourceFile, out var source))
        {
            string outputFileName = $"{pascalFileName}.py.cs";
            if (outputInfo is not null)
            {
                File.WriteAllText(Path.Combine(outputInfo.FullName, outputFileName), source);
            }
            else
            {
                Console.Out.WriteLine(source);
            }
        }
        else
        {
            return (int)ErrorCode.PythonSyntaxError;
        }
    }

    if (outputInfo is not null)
    {
        // only write this message if we're writing to a file
        // otherwise we're writing something that'd be invalid Python code
        Console.Out.WriteLine($"Generated code for {files.Length} files.");
    }
    return (int)ErrorCode.Success;
});

return await new CliConfiguration(root).InvokeAsync(args);
