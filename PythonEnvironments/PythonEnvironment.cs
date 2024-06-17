using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PythonEnvironments
{
    public class PythonEnvironment
    {
        private List<string> paths = new List<string>();

        private static string MapVersion(string version)
        {
            return version.Replace(".", "");
        }

        public PythonEnvironment(string home, string version = "3.10")
        {
            string versionPath = MapVersion(version);
            //if (PythonEngine.IsInitialized)
            //{
            //    // Raise exception?
            //    return;
            //}

            ////string dllPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            ////dllPath = Path.Combine(dllPath, "Programs", "Python", "Python"+versionPath);
            ////Runtime.PythonDLL = Path.Combine(dllPath, string.Format("python{0}.dll", versionPath));

            ////if (!String.IsNullOrEmpty(home))
            ////{
            ////    PythonEngine.PythonHome = home;
            ////    // TODO : Path sep is : on Unix
            ////    PythonEngine.PythonPath = Path.Combine(dllPath, "Lib") + ";" + home;
            ////    paths.Add(Path.Combine(dllPath, "Lib"));
            ////    paths.Add(home);
            ////} else {
            ////    PythonEngine.PythonPath = Path.Combine(dllPath, "Lib");
            ////    paths.Add(Path.Combine(dllPath, "Lib"));
            ////}

            //// TODO : Add virtual env paths
            //PythonEngine.Initialize();
        }

        //public PyObject LoadModule (string module)
        //{
        //    return Py.Import(module);
        //}

        //public void AddPath(string path)
        //{
        //    if (paths.Contains(path))
        //        return;
        //    paths.Add(path);
        //    PythonEngine.PythonPath = string.Join(";", paths);
        //}
    }
}
