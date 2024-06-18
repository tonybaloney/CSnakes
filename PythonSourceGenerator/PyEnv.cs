using Python.Runtime;
using System;
using System.IO;
using System.Linq;

namespace PythonSourceGenerator
{
    public class PyEnv : IDisposable
    {
        private static string MapVersion(string version, string sep = "")
        {
            // split on . then take the first two segments and join them without spaces
            var versionParts = version.Split('.');
            return string.Join("", versionParts.Take(2));
        }

        public PyEnv(string home, string pythonLocation, string version = "3.10.0")
        {
            string versionPath = MapVersion(version);
            if (PythonEngine.IsInitialized)
            {
                // Raise exception?
                return;
            }
            Runtime.PythonDLL =  Path.Combine(pythonLocation, string.Format("python{0}.dll", versionPath));

            if (!string.IsNullOrEmpty(home))
            {
                PythonEngine.PythonHome = home;
                // TODO : Path sep is : on Unix
                PythonEngine.PythonPath = Path.Combine(pythonLocation, "Lib") + ";" + home;
            }
            else
            {
                PythonEngine.PythonPath = Path.Combine(pythonLocation, "Lib");
            }

            // TODO : Add virtual env paths
            PythonEngine.Initialize();
        }

        public PyObject LoadModule(string module)
        {
            return Py.Import(module);
        }

        public void Dispose()
        {
            PythonEngine.Shutdown();
        }

        public static string TryLocatePython(string version)
        {
            var versionPath = MapVersion(version);
            var windowsStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", "Python" + versionPath);
            var officialInstallerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python", MapVersion(version, "."));
            if (Directory.Exists(windowsStorePath))
            {
                return windowsStorePath;
            }
            else if (Directory.Exists(officialInstallerPath))
            {
                return officialInstallerPath;
            }
            else
            {
                // TODO : Use nuget package path?
                throw new Exception("Python not found");
            }
        }
    }
}
