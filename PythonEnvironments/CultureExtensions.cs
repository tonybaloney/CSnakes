using System.Globalization;
using Python.Runtime;

namespace PythonEnvironments
{
    public static class CultureExtensions
    {
        public static long ToInt64(this PyObject obj)
        {
            return obj.ToInt64(CultureInfo.InvariantCulture);
        }

        public static double ToDouble(this PyObject obj)
        {
            return obj.ToDouble(CultureInfo.InvariantCulture);
        }
    }
}
