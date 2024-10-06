using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;


namespace CSnakes.Runtime.EnvironmentManagement;
internal class CondaEnvironmentManagement : IEnvironmentManagement
{
    private readonly string name;
    private readonly bool ensureExists;
    private CondaLocator conda;

    internal CondaEnvironmentManagement(string name, bool ensureExists, CondaLocator conda)
    {
        this.name = name;
        this.conda = conda;
        this.ensureExists = ensureExists;
    }

    public void EnsureEnvironment(ILogger logger, PythonLocationMetadata pythonLocation)
    {
        if (!ensureExists)
            return;


        var fullPath = Path.GetFullPath(GetPath());
        if (!Directory.Exists(fullPath))
        {
            logger.LogInformation("Creating conda environment at {fullPath} using {PythonBinaryPath}", fullPath, pythonLocation.PythonBinaryPath);
            using Process process1 = ExecutePythonCommand(pythonLocation, fullPath, $"-VV");
            using Process process2 = ExecutePythonCommand(pythonLocation, fullPath, $"-m conda env create -n {name}");
        }
        else
        {
            logger.LogDebug("Conda environment already exists at {fullPath}", fullPath);
        }

        Process ExecutePythonCommand(PythonLocationMetadata pythonLocation, string? venvPath, string arguments)
        {
            ProcessStartInfo startInfo = new()
            {
                WorkingDirectory = pythonLocation.Folder,
                FileName = pythonLocation.PythonBinaryPath,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            Process process = new() { StartInfo = startInfo };
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.LogInformation("{Data}", e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.LogError("{Data}", e.Data);
                }
            };

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            return process;
        }
    }

    public string GetPath()
    {
        return Path.Combine(conda.CondaHome, "envs", name);
    }
}
