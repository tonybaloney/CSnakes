using System.Formats.Tar;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using ZstdSharp;

namespace CSnakes.Runtime.Locators;

internal class ManagedPythonLocator(ILogger logger) : PythonLocator
{
    private const string standaloneRelease = "20250106";
    private static readonly Version defaultVersion = new(3, 12, 8, 0);
    private static object _installLock = new();
    protected override Version Version { get; } = defaultVersion;

    protected override string GetPythonExecutablePath(string folder, bool freeThreaded = false)
    {
        string suffix = freeThreaded ? "t" : "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(folder, $"python{suffix}.exe");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(folder, "bin", $"python{Version.Major}.{Version.Minor}{suffix}");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(folder, "bin", $"python{Version.Major}.{Version.Minor}{suffix}");
        }

        throw new PlatformNotSupportedException($"Unsupported platform: '{RuntimeInformation.OSDescription}'.");
    }

    public override PythonLocationMetadata LocatePython() {
        var downloadPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CSnakes", $"python{Version.Major}.{Version.Minor}");
        var installPath = Path.Join(downloadPath, "python", "install");

        // Before entering the lock, check if the install path already exists to save waiting
        if (Directory.Exists(installPath))
        {
            return LocatePythonInternal(installPath);
        }

        lock (_installLock)
        {
            // Check again if the install path exists, in case another thread has already installed it
            if (Directory.Exists(installPath))
            {
                return LocatePythonInternal(installPath);
            }
            // Determine binary name, see https://gregoryszorc.com/docs/python-build-standalone/main/running.html#obtaining-distributions
            string platform;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "i686-pc-windows-msvc-shared-pgo-full",
                    Architecture.X64 => "x86_64-pc-windows-msvc-shared-pgo-full",
                    _ => throw new PlatformNotSupportedException($"Unsupported architecture: '{RuntimeInformation.ProcessArchitecture}'.")
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = RuntimeInformation.ProcessArchitecture switch
                {
                    // No such thing as i686 mac
                    Architecture.X64 => "x86_64-apple-darwin-pgo+lto-full",
                    Architecture.Arm64 => "aarch64-apple-darwin-pgo+lto-full",
                    _ => throw new PlatformNotSupportedException($"Unsupported architecture: '{RuntimeInformation.ProcessArchitecture}'.")
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "i686-unknown-linux-gnu-pgo+lto-full",
                    Architecture.X64 => "x86_64-unknown-linux-gnu-pgo+lto-full",
                    Architecture.Arm64 => "aarch64-unknown-linux-gnu-pgo+lto-full",
                    // .NET doesn't run on armv7 anyway.. don't try that
                    _ => throw new PlatformNotSupportedException($"Unsupported architecture: '{RuntimeInformation.ProcessArchitecture}'.")
                };
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
            Directory.CreateDirectory(downloadPath);
            ExtractTar(tarFilePath, downloadPath, logger);
            logger.LogInformation("Extracted Python to {downloadPath}", downloadPath);
            return LocatePythonInternal(installPath);
        }
    }

    protected override string GetLibPythonPath(string folder, bool freeThreaded = false)
    {
        string suffix = freeThreaded ? "t" : "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(folder, $"python{Version.Major}{Version.Minor}{suffix}.dll");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(folder, "lib", $"libpython{Version.Major}.{Version.Minor}{suffix}.dylib");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(folder, "lib", $"libpython{Version.Major}.so");
        }

        throw new PlatformNotSupportedException($"Unsupported platform: '{RuntimeInformation.OSDescription}'.");
    }

    private static async Task<string> DownloadFileToTempDirectoryAsync(string fileUrl)
    {
        using HttpClient client = new();
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

    private static void ExtractTar(string tarFilePath, string extractPath, ILogger logger)
    {
        using FileStream tarStream = File.OpenRead(tarFilePath);
        using TarReader tarReader = new(tarStream);
        TarEntry? entry;

        while ((entry = tarReader.GetNextEntry()) != null)
        {
            string entryPath = Path.Combine(extractPath, entry.Name);
            if (entry.EntryType == TarEntryType.Directory)
            {
                Directory.CreateDirectory(entryPath);
                logger.LogDebug("Creating directory: {EntryPath}", entryPath);
            }
            else if (entry.EntryType == TarEntryType.RegularFile)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);
                entry.ExtractToFile(entryPath, true);
            } else if (entry.EntryType == TarEntryType.SymbolicLink) {
                // TODO: 
                logger.LogWarning("Symbolic link not supported: {EntryPath}", entryPath);
            }
        }
    }
}
