using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace PythonSourceGenerator
{
    internal class PyEnv
    {
        private readonly List<string> paths = new List<string>();

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

            string dllPath = @"C:\Users\aapowell\.nuget\packages\python\3.12.4\tools";
            //dllPath = Path.Combine(dllPath, "Programs", "Python", "Python" + versionPath);
            Runtime.PythonDLL =  Path.Combine(dllPath, string.Format("python{0}.dll", versionPath));

            if (!string.IsNullOrEmpty(home))
            {
                PythonEngine.PythonHome = home;
                // TODO : Path sep is : on Unix
                PythonEngine.PythonPath = Path.Combine(dllPath, "Lib") + ";" + home;
                paths.Add(Path.Combine(dllPath, "Lib"));
                paths.Add(home);
            }
            else
            {
                PythonEngine.PythonPath = Path.Combine(dllPath, "Lib");
                paths.Add(Path.Combine(dllPath, "Lib"));
            }

            // TODO : Add virtual env paths
            PythonEngine.Initialize();
        }

        public PyObject LoadModule(string module)
        {
            return Py.Import(module);
        }

        public void AddPath(string path)
        {
            if (paths.Contains(path))
                return;
            paths.Add(path);
            PythonEngine.PythonPath = string.Join(";", paths);
        }
    }
}
