using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CSnakes.Runtime
{
    internal static class ProcessUtils
    {
        internal static Process ExecutePythonCommand(ILogger logger, PythonLocationMetadata pythonLocation, string arguments)
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
}
