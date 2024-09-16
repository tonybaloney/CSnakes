using System.CommandLine;

var fileOption = new CliOption<FileInfo?>("--file", "-f")
{
    Description = "Path to the Python file to generate the C# code from."
};

var directoryOption = new CliOption<DirectoryInfo?>("--directory", "-d")
{
    Description = "Path to the directory containing the Python files to generate the C# code from."
};

var outputOption = new CliOption<FileInfo?>("--output", "-o")
{
    Description = "Path to the output C# file.",
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
    FileInfo? outputInfo = result.GetValue(outputOption);
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

    if (fileInfo is not null)
    {
        Console.WriteLine($"You want to generate C# from {fileInfo.FullName}");
    }

    if (directoryInfo is not null)
    {
        var files = directoryInfo.GetFiles("*.py");
        Console.WriteLine($"You want to generate C# from {directoryInfo.FullName}, which contains {files.Length} Python files.");
    }

    Console.WriteLine($"The output will be written to {outputInfo.FullName}");
});

await new CliConfiguration(root).InvokeAsync(args);
