using Microsoft.Extensions.Logging;
using System.Formats.Tar;
using System.Runtime.InteropServices;
using System.Threading;
using ZstdSharp;

namespace CSnakes.Runtime.Locators;

public sealed record RedistributablePythonVersion
{
    public static readonly RedistributablePythonVersion Python3_9  = new(new Version(3, 9, 23));
    public static readonly RedistributablePythonVersion Python3_10 = new(new Version(3, 10, 18));
    public static readonly RedistributablePythonVersion Python3_11 = new(new Version(3, 11, 13));
    public static readonly RedistributablePythonVersion Python3_12 = new(new Version(3, 12, 11));
    public static readonly RedistributablePythonVersion Python3_13 = new(new Version(3, 13, 7)) { SupportsFreeThreading = true };
    public static readonly RedistributablePythonVersion Python3_14 = new(new Version(3, 14, 0), "rc2") { SupportsFreeThreading = true };

    private RedistributablePythonVersion(Version version, string? preRelease = null) =>
        (Version, PreRelease) = (version, preRelease);

    internal Version Version { get; init; }
    internal string? PreRelease { get; init; }
    internal bool SupportsFreeThreading { get; init; }

    internal string DottedString => $"{Version}{PreRelease}"; // e.g. 3.14.0rc1
}

internal class RedistributableLocator(ILogger<RedistributableLocator>? logger, RedistributablePythonVersion version, int installerTimeout = 360, bool debug = false, bool freeThreaded = false) : PythonLocator
{
    private const string standaloneRelease = "20250902";
    
    /// <summary>
    /// Name of the completion marker file that indicates a Python installation has been successfully completed.
    /// This file is created only after the entire installation process (download, extraction, validation) is complete.
    /// The presence of this file, along with the installation directory, indicates that the Python runtime
    /// is ready for use and prevents race conditions between multiple processes attempting to use the runtime
    /// before installation is complete.
    /// </summary>
    private const string CompletionMarkerFileName = "installation_complete.marker";

    protected override Version Version => version.Version;
    protected bool SupportsFreeThreading => version.SupportsFreeThreading;

    protected override string GetPythonExecutablePath(string folder, bool freeThreaded = false)
    {
        if (!SupportsFreeThreading && freeThreaded)
            throw new NotSupportedException($"Free-threaded Python is not supported for version {Version}.");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && debug)
        {
            return Path.Combine(folder, "python_d.exe");
        }
        return base.GetPythonExecutablePath(folder, freeThreaded);
    }

    public override PythonLocationMetadata LocatePython()
    {
        string dottedVersion = version.DottedString;
        if (debug)
        {
            dottedVersion += "d";
        }
        if (freeThreaded)
        {
            dottedVersion += "t";
        }
        // There are no Windows debug builds
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && debug)
        {
            throw new NotSupportedException($"Debug builds are not supported on Windows for version {Version}.");
        }
        // No debug builds for Python 3.9 and 3.10 on macOS (they're broken)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && debug && (Version.Major == 3 && Version.Minor < 11))
        {
            throw new NotSupportedException($"Debug builds are not supported on macOS for version {Version}.");
        }
        // No Arm64 builds for Python 3.9 or 3.10 on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.Arm64 && (Version.Major == 3 && Version.Minor < 11))
        {
            throw new NotSupportedException($"Arm64 builds are not supported on Windows for version {Version}.");
        }

        var appDataPath = Environment.GetEnvironmentVariable("CSNAKES_REDIST_CACHE");
        if (string.IsNullOrWhiteSpace(appDataPath))
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
        var downloadPath = Path.Join(appDataPath, "CSnakes", $"python{dottedVersion}");
        var installPath = Path.Join(downloadPath, "python", "install");
        var completionMarkerPath = Path.Combine(installPath, CompletionMarkerFileName);
        var mutexName = $"CSnakes_Python_{dottedVersion}_Installation";

        // Wait for any existing installation to complete using mutex-based locking
        if (!WaitForInstallationMutex(mutexName, installerTimeout))
        {
            throw new TimeoutException($"Python installation timed out after {installerTimeout} seconds waiting for installation mutex.");
        }

        // Check if installation exists and is valid (this will validate essential files)
        if (Directory.Exists(installPath) && File.Exists(completionMarkerPath))
        {
            try
            {
                return LocatePythonInternal(installPath, freeThreaded);
            }
            catch (DirectoryNotFoundException)
            {
                // Installation is corrupted, remove completion marker and reinstall
                logger?.LogWarning("Python installation at {InstallPath} is corrupted, removing completion marker and reinstalling", installPath);
                try
                {
                    File.Delete(completionMarkerPath);
                    if (Directory.Exists(downloadPath))
                        DeleteDirectoryRobust(downloadPath, logger);
                }
                catch (Exception cleanupEx)
                {
                    logger?.LogWarning(cleanupEx, "Failed to clean up corrupted installation at {DownloadPath}", downloadPath);
                }
                // Fall through to reinstall
            }
        }

        // Acquire installation mutex to indicate installation in progress
        Directory.CreateDirectory(downloadPath);
        using var installationMutex = AcquireInstallationMutex(mutexName);
        if (installationMutex == null)
        {
            throw new InvalidOperationException("Failed to acquire installation mutex. Another installation may be in progress.");
        }
        logger?.LogDebug("Acquired installation mutex {MutexName}", mutexName);

        try
        {
            // TODO: Find a better way to determine the OS platform enum at runtime.
            OSPlatform osPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OSPlatform.Windows :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSPlatform.OSX :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OSPlatform.Linux :
                throw new PlatformNotSupportedException($"Unsupported platform: '{RuntimeInformation.OSDescription}'.");

            string downloadUrl = GetDownloadUrl(osPlatform, RuntimeInformation.ProcessArchitecture, freeThreaded, debug, version);

            // Download and extract the Zstd tarball
            logger?.LogDebug("Downloading Python from {DownloadUrl}", downloadUrl);
            string tempFilePath = DownloadFileToTempDirectoryAsync(downloadUrl).GetAwaiter().GetResult();
            string tarFilePath = DecompressZstFile(tempFilePath);
            ExtractTar(tarFilePath, downloadPath, logger);
            logger?.LogDebug("Extracted Python to {downloadPath}", downloadPath);

            // Delete the tarball and temp file
            File.Delete(tarFilePath);
            File.Delete(tempFilePath);

            // Validate installation before creating completion marker
            logger?.LogDebug("Validating Python installation at {InstallPath}", installPath);
            if (!ValidatePythonInstallation(installPath, freeThreaded))
            {
                logger?.LogError("Python installation validation failed at {InstallPath}. Essential files or modules are missing. Attempting to clean up and retry.", installPath);
                
                // Clean up the failed installation
                try
                {
                    DeleteDirectoryRobust(downloadPath, logger);
                    logger?.LogDebug("Cleaned up failed Python installation at {DownloadPath}", downloadPath);
                }
                catch (Exception cleanupEx)
                {
                    logger?.LogWarning(cleanupEx, "Failed to clean up corrupted installation at {DownloadPath}", downloadPath);
                }
                
                throw new InvalidOperationException($"Python installation validation failed at {installPath}. Essential files or modules are missing. The installation has been cleaned up and will be retried on next attempt.");
            }
            
            // Perform full LocatePythonInternal validation as well
            var validationResult = LocatePythonInternal(installPath, freeThreaded);
            logger?.LogDebug("Python installation validation successful");

            // Only create completion marker after successful validation
            File.WriteAllText(completionMarkerPath, $"Python {dottedVersion} installation completed at {DateTime.UtcNow:O}");
            logger?.LogDebug("Created installation completion marker at {CompletionMarkerPath}", completionMarkerPath);
            
            return validationResult;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to download and extract Python");
            // If the install failed somewhere, delete the folder incase it's half downloaded
            if (Directory.Exists(downloadPath))
            {
                try
                {
                    DeleteDirectoryRobust(downloadPath, logger);
                }
                catch (Exception cleanupEx)
                {
                    logger?.LogWarning(cleanupEx, "Failed to clean up installation directory {DownloadPath}. This may leave temporary files.", downloadPath);
                }
            }

            throw;
        }
        // Mutex is automatically released when disposed at end of using block
    }

    internal static string GetDownloadUrl(OSPlatform platform, Architecture architecture, bool freeThreaded, bool debug, RedistributablePythonVersion version)
    {
        string optFlags = platform == OSPlatform.Windows ? "pgo" : "pgo+lto";
        string platformLabel;
        string build;
        if (freeThreaded)
        {
            build = debug ? "freethreaded+debug" : $"freethreaded+{optFlags}";
        }
        else
        {
            build = debug ? "debug" : optFlags;
        }

        if (platform == OSPlatform.Windows)
        {
            platformLabel = architecture switch
            {
                Architecture.X86 => $"i686-pc-windows-msvc-{build}-full",
                Architecture.X64 => $"x86_64-pc-windows-msvc-{build}-full",
                Architecture.Arm64 => $"aarch64-pc-windows-msvc-{build}-full",
                _ => throw new PlatformNotSupportedException($"Unsupported architecture: '{architecture}'.")
            };
        }
        else if (platform == OSPlatform.OSX)
        {
            platformLabel = architecture switch
            {
                // No such thing as i686 mac
                Architecture.X64 => $"x86_64-apple-darwin-{build}-full",
                Architecture.Arm64 => $"aarch64-apple-darwin-{build}-full",
                _ => throw new PlatformNotSupportedException($"Unsupported architecture: '{architecture}'.")
            };
        }
        else if (platform == OSPlatform.Linux)
        {
            platformLabel = architecture switch
            {
                Architecture.X86 => $"i686-unknown-linux-gnu-{build}-full",
                Architecture.X64 => $"x86_64-unknown-linux-gnu-{build}-full",
                Architecture.Arm64 => $"aarch64-unknown-linux-gnu-{build}-full",
                // .NET doesn't run on armv7 anyway.. don't try that
                _ => throw new PlatformNotSupportedException($"Unsupported architecture: '{architecture}'.")
            };
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform: '{platform}'.");
        }

        return $"https://github.com/astral-sh/python-build-standalone/releases/download/{standaloneRelease}/cpython-{version.DottedString}+{standaloneRelease}-{platformLabel}.tar.zst";
    }

    protected override string GetLibPythonPath(string folder, bool freeThreaded = false)
    {
        if (!SupportsFreeThreading && freeThreaded)
            throw new NotSupportedException($"Free-threaded Python is not supported for version {Version}.");
        string suffix = freeThreaded ? "t" : "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(folder, $"python{Version.Major}{Version.Minor}{suffix}.dll");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(folder, "lib", $"libpython{Version.Major}.{Version.Minor}{suffix}{(debug ? "d" : string.Empty)}.dylib");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(folder, "lib", $"libpython{Version.Major}.{Version.Minor}{suffix}{(debug ? "d" : string.Empty)}.so");
        }

        throw new PlatformNotSupportedException($"Unsupported platform: '{RuntimeInformation.OSDescription}'.");
    }

    private static async Task<string> DownloadFileToTempDirectoryAsync(string fileUrl)
    {
        using HttpClient client = new();
        var contentStream = await client.GetStreamAsync(fileUrl).ConfigureAwait(false);
        await using (contentStream.ConfigureAwait(false))
        {
            string tempFilePath = Path.GetTempFileName();
            var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using (fileStream.ConfigureAwait(false))
            {
                await contentStream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            return tempFilePath;
        }
    }

    private static string DecompressZstFile(string zstFilePath)
    {
        string tarFilePath = Path.ChangeExtension(zstFilePath, ".tar");
        using var inputStream = new FileStream(zstFilePath, FileMode.Open, FileAccess.Read);
        using var decompressor = new DecompressionStream(inputStream);
        using var outputStream = new FileStream(tarFilePath, FileMode.Create, FileAccess.Write);
        decompressor.CopyTo(outputStream);
        return tarFilePath;
    }

    private static void ExtractTar(string tarFilePath, string extractPath, ILogger? logger)
    {
        using FileStream tarStream = File.OpenRead(tarFilePath);
        using TarReader tarReader = new(tarStream);
        TarEntry? entry;
        List<(string, string)> symlinks = [];
        while ((entry = tarReader.GetNextEntry()) is not null)
        {
            string entryPath = Path.Combine(extractPath, entry.Name);
            if (entry.EntryType == TarEntryType.Directory)
            {
                Directory.CreateDirectory(entryPath);
                logger?.LogDebug("Creating directory: {EntryPath}", entryPath);
            }
            else if (entry.EntryType == TarEntryType.RegularFile)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);
                entry.ExtractToFile(entryPath, true);
            }
            else if (entry.EntryType == TarEntryType.SymbolicLink)
            {
                // Delay the creation of symlinks until after all files have been extracted
                symlinks.Add((entryPath, entry.LinkName));
            }
            else
            {
                logger?.LogDebug("Skipping entry: {EntryPath} ({EntryType})", entryPath, entry.EntryType);
            }
        }
        foreach (var (path, link) in symlinks)
        {
            logger?.LogDebug("Creating symlink: {Path} -> {Link}", path, link);
            try
            {
                File.CreateSymbolicLink(path, link);
            }
            catch (DirectoryNotFoundException ex)
            {
                // This is common in the packages
                logger?.LogWarning(ex, "Failed to create symlink: {Path} -> {Link}", path, link);
            }
        }
    }

    /// <summary>
    /// Validates that a Python installation is complete and functional by checking essential files
    /// </summary>
    /// <param name="installPath">Path to the Python installation</param>
    /// <param name="freeThreaded">Whether this is a free-threaded installation</param>
    /// <returns>True if the installation appears complete and functional</returns>
    private bool ValidatePythonInstallation(string installPath, bool freeThreaded)
    {
        try
        {
            // Check that essential executables exist
            var pythonExecutablePath = GetPythonExecutablePath(installPath, freeThreaded);
            if (!File.Exists(pythonExecutablePath))
            {
                logger?.LogDebug("Python executable missing at {PythonExecutablePath}", pythonExecutablePath);
                return false;
            }

            // Check that essential library exists
            var libPythonPath = GetLibPythonPath(installPath, freeThreaded);
            if (!File.Exists(libPythonPath))
            {
                logger?.LogDebug("Python library missing at {LibPythonPath}", libPythonPath);
                return false;
            }

            // Check that essential Python standard library modules exist
            var libPath = Path.Combine(installPath, "lib", $"python{Version.Major}.{Version.Minor}{(freeThreaded ? "t" : "")}");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                libPath = Path.Combine(installPath, "Lib");
            }

            if (!Directory.Exists(libPath))
            {
                logger?.LogDebug("Python standard library directory missing at {LibPath}", libPath);
                return false;
            }

            // Check for some essential standard library modules that should always be present as files
            // Note: Built-in modules like 'sys', 'io' are compiled into the interpreter and won't exist as .py files
            string[] essentialModules = ["collections", "os", "re", "json", "functools"];
            var missingModules = new List<string>();
            
            foreach (var module in essentialModules)
            {
                var modulePath = Path.Combine(libPath, $"{module}.py");
                var modulePackagePath = Path.Combine(libPath, module, "__init__.py");
                
                if (!File.Exists(modulePath) && !File.Exists(modulePackagePath))
                {
                    missingModules.Add(module);
                }
            }
            
            if (missingModules.Count > 0)
            {
                logger?.LogDebug("Essential Python modules missing from standard library at {LibPath}: {MissingModules}", libPath, string.Join(", ", missingModules));
                return false;
            }

            logger?.LogDebug("Python installation validation passed for {InstallPath}", installPath);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "Exception during Python installation validation for {InstallPath}", installPath);
            return false;
        }
    }

    /// <summary>
    /// Waits for any existing installation mutex to be available, with timeout handling.
    /// This method implements mutex-based locking to prevent concurrent installations.
    /// </summary>
    /// <param name="mutexName">Name of the installation mutex</param>
    /// <param name="timeoutSeconds">Timeout in seconds to wait for the mutex</param>
    /// <returns>True if mutex is available, false if timeout occurred</returns>
    private bool WaitForInstallationMutex(string mutexName, int timeoutSeconds)
    {
        try
        {
            // Try to open existing mutex to check if installation is in progress
            using var existingMutex = Mutex.OpenExisting(mutexName);
            logger?.LogDebug("Installation mutex {MutexName} exists, waiting for it to be released", mutexName);
            
            // Wait for the mutex to be released
            var timeoutMs = timeoutSeconds * 1000;
            bool acquired = existingMutex.WaitOne(timeoutMs);
            
            if (acquired)
            {
                // Release immediately since we just wanted to wait for it
                existingMutex.ReleaseMutex();
                logger?.LogDebug("Installation mutex {MutexName} was released", mutexName);
                return true;
            }
            else
            {
                logger?.LogError("Timed out waiting for installation mutex {MutexName} after {TimeoutSeconds} seconds", mutexName, timeoutSeconds);
                return false;
            }
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // Mutex doesn't exist, so no installation is in progress
            logger?.LogDebug("No installation mutex {MutexName} found, proceeding with installation", mutexName);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Error while checking installation mutex {MutexName}", mutexName);
            return true; // Proceed if we can't determine the state
        }
    }

    /// <summary>
    /// Acquires an installation mutex to prevent concurrent installations.
    /// </summary>
    /// <param name="mutexName">Name of the installation mutex</param>
    /// <returns>The acquired mutex, or null if it couldn't be acquired</returns>
    private Mutex? AcquireInstallationMutex(string mutexName)
    {
        try
        {
            var mutex = new Mutex(true, mutexName, out bool createdNew);
            
            if (createdNew)
            {
                logger?.LogDebug("Created new installation mutex {MutexName}", mutexName);
                return mutex;
            }
            else
            {
                // Another process created it first, but we might still be able to acquire it
                bool acquired = mutex.WaitOne(0); // Don't wait, just try immediately
                if (acquired)
                {
                    logger?.LogDebug("Acquired existing installation mutex {MutexName}", mutexName);
                    return mutex;
                }
                else
                {
                    logger?.LogDebug("Could not acquire installation mutex {MutexName}, another installation is in progress", mutexName);
                    mutex.Dispose();
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to acquire installation mutex {MutexName}", mutexName);
            return null;
        }
    }

    /// <summary>
    /// Validates that the Python installation is complete by checking for the presence of the completion marker file.
    /// This override ensures that redistributable Python installations are not used until they are fully installed
    /// and validated, preventing issues with partially completed or corrupted installations.
    /// For legacy installations without a completion marker, validates the installation by checking for essential files.
    /// </summary>
    /// <param name="folder">The folder path to the Python installation</param>
    /// <param name="freeThreaded">Whether to locate the free-threaded version</param>
    /// <returns>Python location metadata if the installation is complete</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the installation is incomplete or corrupted</exception>
    protected override PythonLocationMetadata LocatePythonInternal(string folder, bool freeThreaded = false)
    {
        var completionMarkerPath = Path.Combine(folder, CompletionMarkerFileName);
        
        if (!File.Exists(completionMarkerPath))
        {
            // For legacy installations without completion marker, validate by checking essential files
            logger?.LogDebug("Completion marker not found at {CompletionMarkerPath}, validating legacy installation", completionMarkerPath);
            
            var pythonExecutablePath = GetPythonExecutablePath(folder, freeThreaded);
            var libPythonPath = GetLibPythonPath(folder, freeThreaded);
            
            if (!File.Exists(pythonExecutablePath))
            {
                throw new DirectoryNotFoundException($"Python installation in '{folder}' is incomplete - Python executable not found at '{pythonExecutablePath}'. The installation may have been interrupted or corrupted.");
            }
            
            if (!File.Exists(libPythonPath))
            {
                throw new DirectoryNotFoundException($"Python installation in '{folder}' is incomplete - Python library not found at '{libPythonPath}'. The installation may have been interrupted or corrupted.");
            }
            
            // Legacy installation appears complete, create completion marker for future use
            try
            {
                File.Create(completionMarkerPath).Dispose();
                logger?.LogDebug("Created completion marker for legacy installation at {CompletionMarkerPath}", completionMarkerPath);
            }
            catch (Exception ex)
            {
                // Don't fail if we can't create the marker - the installation is still usable
                logger?.LogWarning(ex, "Could not create completion marker for legacy installation at {CompletionMarkerPath}", completionMarkerPath);
            }
        }
        else
        {
            // Even with completion marker, validate that installation is still complete and functional
            logger?.LogDebug("Completion marker found at {CompletionMarkerPath}, validating installation integrity", completionMarkerPath);
            
            if (!ValidatePythonInstallation(folder, freeThreaded))
            {
                logger?.LogWarning("Python installation marked as complete but validation failed at {Folder}, removing marker and requiring reinstall", folder);
                try
                {
                    File.Delete(completionMarkerPath);
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Failed to remove invalid completion marker at {CompletionMarkerPath}", completionMarkerPath);
                }
                throw new DirectoryNotFoundException($"Python installation in '{folder}' is incomplete or corrupted. Essential files or modules are missing. The installation will be reinstalled on next use.");
            }
        }

        return base.LocatePythonInternal(folder, freeThreaded);
    }

    /// <summary>
    /// Robustly deletes a directory and all its contents, including handling broken symlinks
    /// that can cause standard Directory.Delete to fail.
    /// </summary>
    /// <param name="directoryPath">The directory path to delete</param>
    /// <param name="logger">Optional logger for debug information</param>
    private static void DeleteDirectoryRobust(string directoryPath, ILogger? logger)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        try
        {
            // First try the standard deletion
            Directory.Delete(directoryPath, recursive: true);
            return;
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "Standard directory deletion failed for {DirectoryPath}, attempting robust cleanup", directoryPath);
        }

        // If standard deletion fails, manually handle the cleanup
        try
        {
            DeleteDirectoryContentsRobust(directoryPath, logger);
            Directory.Delete(directoryPath, false);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to robustly delete directory {DirectoryPath}", directoryPath);
            throw;
        }
    }

    /// <summary>
    /// Recursively deletes directory contents, handling broken symlinks and other edge cases.
    /// </summary>
    private static void DeleteDirectoryContentsRobust(string directoryPath, ILogger? logger)
    {
        var directoryInfo = new DirectoryInfo(directoryPath);
        
        foreach (var file in directoryInfo.GetFiles())
        {
            try
            {
                // Remove read-only attributes if present
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Failed to delete file {FilePath}", file.FullName);
                // For symlinks or other special files, try force deletion
                try
                {
                    File.Delete(file.FullName);
                }
                catch (Exception innerEx)
                {
                    logger?.LogDebug(innerEx, "Failed to force delete file {FilePath}", file.FullName);
                }
            }
        }

        foreach (var subDir in directoryInfo.GetDirectories())
        {
            try
            {
                DeleteDirectoryContentsRobust(subDir.FullName, logger);
                subDir.Delete(false);
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Failed to delete subdirectory {SubDirPath}", subDir.FullName);
                // Try to force delete empty directory
                try
                {
                    Directory.Delete(subDir.FullName, false);
                }
                catch (Exception innerEx)
                {
                    logger?.LogDebug(innerEx, "Failed to force delete subdirectory {SubDirPath}", subDir.FullName);
                }
            }
        }
    }
}
