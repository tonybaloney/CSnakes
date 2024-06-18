using Python.Runtime;
using System;
using System.IO;

namespace PythonSourceGenerator
{
    internal class PyEnv : IDisposable
    {
        private static string MapVersion(string version)
        {
            return version.Replace(".", "");
        }

        public PyEnv(string home, string version = "3.10")
        {
            string versionPath = MapVersion(version);
            if (PythonEngine.IsInitialized)
            {
                // Raise exception?
                return;
            }

            string dllPath = $@"{Environment.GetEnvironmentVariable("USERPROFILE")}\.nuget\packages\python\3.12.4\tools";
            //dllPath = Path.Combine(dllPath, "Programs", "Python", "Python" + versionPath);
            Runtime.PythonDLL =  Path.Combine(dllPath, string.Format("python{0}.dll", versionPath));

            if (!string.IsNullOrEmpty(home))
            {
                PythonEngine.PythonHome = home;
                // TODO : Path sep is : on Unix
                PythonEngine.PythonPath = Path.Combine(dllPath, "Lib") + ";" + home;
            }
            else
            {
                PythonEngine.PythonPath = Path.Combine(dllPath, "Lib");
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
    }
}
