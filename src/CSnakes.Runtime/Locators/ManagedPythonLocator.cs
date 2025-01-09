﻿using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using ZstdSharp;

namespace CSnakes.Runtime.Locators;

internal class ManagedPythonLocator(ILogger logger) : PythonLocator
{
    private const string standaloneRelease = "20250106";
    private static Version defaultVersion = new(3, 12, 8, 0);

    protected override Version Version { get; } = defaultVersion;

    public override PythonLocationMetadata LocatePython() {
        // Determine binary name, see https://gregoryszorc.com/docs/python-build-standalone/main/running.html#obtaining-distributions
        string platform;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // TODO: i686 is available.. 
            platform = "x86_64-pc-windows-msvc-shared-pgo-full";
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // If aarch64-apple-darwin is available, use it
            platform = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x86_64-apple-darwin-pgo+lto-full",
                Architecture.Arm64 => "aarch64-apple-darwin-pgo+lto-full",
                _ => throw new PlatformNotSupportedException($"Unsupported architecture: '{RuntimeInformation.ProcessArchitecture}'.")
            };
            platform = "x86_64-apple-darwin-pgo+lto-full";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // TODO;
            throw new PlatformNotSupportedException($"Unsupported architecture: '{RuntimeInformation.ProcessArchitecture}'.");
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform: '{RuntimeInformation.OSDescription}'.");
        }
        string downloadUrl = $"https://github.com/astral-sh/python-build-standalone/releases/download/{standaloneRelease}/cpython-{Version.Major}.{Version.Minor}.{Version.Build}+{standaloneRelease}-{platform}.tar.zst";

        // Download and extract the tarball
        logger.LogInformation("Downloading Python from {DownloadUrl}", downloadUrl);
        string tempFilePath = DownloadFileToTempDirectoryAsync(downloadUrl).GetAwaiter().GetResult();
        string tarFilePath = DecompressZstFile(tempFilePath);
        string extractPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(extractPath);
        ExtractTar(tarFilePath, extractPath);
        logger.LogInformation("Extracted Python to {ExtractPath}", extractPath);
        return LocatePythonInternal(Path.Join(extractPath, "python", "install"));
    }

    private static async Task<string> DownloadFileToTempDirectoryAsync(string fileUrl)
    {
        using HttpClient client = new HttpClient();
        using HttpResponseMessage response = await client.GetAsync(fileUrl);
        response.EnsureSuccessStatusCode();

        string tempFilePath = Path.GetTempFileName();
        using FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);

        return tempFilePath;
    }

    private static string DecompressZstFile(string zstFilePath)
    {
        string tarFilePath = Path.ChangeExtension(zstFilePath, ".tar");
        using (var inputStream = new FileStream(zstFilePath, FileMode.Open, FileAccess.Read))
        using (var decompressor = new DecompressionStream(inputStream))
        using (var outputStream = new FileStream(tarFilePath, FileMode.Create, FileAccess.Write))
        {
            decompressor.CopyTo(outputStream);
        }
        return tarFilePath;
    }

    private static void ExtractTar(string tarFilePath, string extractPath)
    {
        using var tarArchive = TarArchive.Open(tarFilePath);
        foreach (var entry in tarArchive.Entries.Where(entry => !entry.IsDirectory))
        {
            entry.WriteToDirectory(extractPath, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
        }
    }
}
