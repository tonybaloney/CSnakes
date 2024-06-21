using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PythonEnvironments
{
    public class PythonEnvironment(string home, string pythonLocation, string version = "3.10.0")
    {
        private readonly string versionPath = MapVersion(version);
        private PythonEnvironmentInternal env;

        private static string MapVersion(string version, string sep = "")
        {
            // split on . then take the first two segments and join them without spaces
            var versionParts = version.Split('.');
            return string.Join("", versionParts.Take(2));
        }

        public PythonEnvironment(string version = "3.10") : this("", TryLocatePython(version), version)
        {
        }

        public PythonEnvironment(string home, string version = "3.10") : this(home, TryLocatePython(version), version)
        {
        }

        public IPythonEnvironment Build()
        {
            if (PythonEngine.IsInitialized)
            {
                // Raise exception?
                return env;
            }
            env = new PythonEnvironmentInternal(pythonLocation, versionPath, home, this);

            return env;
        }

        public PyObject LoadModule(string module)
        {
            return Py.Import(module);
        }

        private static string TryLocatePython(string version)
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

        public override bool Equals(object obj)
        {
            return obj is PythonEnvironment environment &&
                   versionPath == environment.versionPath;
        }

        public override int GetHashCode()
        {
            int hashCode = 955711454;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(versionPath);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(home);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(pythonLocation);
            return hashCode;
        }

        internal class PythonEnvironmentInternal : IPythonEnvironment
        {
            private readonly PythonEnvironment pythonEnvironment;

            public PythonEnvironmentInternal(string pythonLocation, string versionPath, string home, PythonEnvironment pythonEnvironment)
            {
                Runtime.PythonDLL = Path.Combine(pythonLocation, string.Format("python{0}.dll", versionPath));

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
                this.pythonEnvironment = pythonEnvironment;
            }

            public void Dispose()
            {
                PythonEngine.Shutdown();
                pythonEnvironment.Destroy();
            }
        }

        private void Destroy()
        {
            env = null;
        }
    }

    public interface IPythonEnvironment : IDisposable
    {
    }
}
