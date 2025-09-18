using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;

namespace CSnakes.Runtime;

public static class PythonRunString
{
    private static void Merge(this IDictionary<string, PyObject> left, PyObject variablesDict) =>
        left.Merge(variablesDict.BareImportAs<IReadOnlyDictionary<string, PyObject>, PyObjectImporters.Dictionary<string, PyObject, PyObjectImporters.String, PyObjectImporters.Clone>>());

    private static void Merge(this IDictionary<string, PyObject> left, IReadOnlyDictionary<string, PyObject> right)
    {
        foreach (var entry in right)
        {
            left[entry.Key] = entry.Value;
        }
    }

    /// <summary>
    /// Execute a single expression in Python and return the result,
    /// e.g. `1 + 1` or `len([1, 2, 3])`
    /// </summary>
    /// <param name="code">The Python code</param>
    /// <returns>The resulting Python object</returns>
    public static PyObject ExecuteExpression(this IPythonEnvironment env, string code)
    {
        using (GIL.Acquire())
        {
            using var globals = PyObject.Create(CPythonAPI.PyDict_New());
            using var locals = PyObject.Create(CPythonAPI.PyDict_New());
            return CPythonAPI.PyRun_String(code, CPythonAPI.InputType.Py_eval_input, globals, locals);
        }
    }

    /// <summary>
    /// Execute a single expression in Python and return the result, with locals
    /// e.g. `1 + b`
    /// </summary>
    /// <param name="code">The Python code</param>
    /// <param name="locals">A dictionary of local variables</param>
    /// <returns>The resulting Python object</returns>
    public static PyObject ExecuteExpression(this IPythonEnvironment env, string code, IDictionary<string, PyObject> locals)
    {
        using (GIL.Acquire())
        {
            using var localsPyDict = PyObject.From(locals);
            using var globalsPyDict = PyObject.Create(CPythonAPI.PyDict_New());
            var result = CPythonAPI.PyRun_String(code, CPythonAPI.InputType.Py_eval_input, globalsPyDict, localsPyDict);
            locals.Merge(localsPyDict);
            return result;
        }
    }

    /// <summary>
    /// Execute a single expression in Python and return the result, with locals
    /// e.g. `1 + b`
    /// </summary>
    /// <param name="code">The Python code</param>
    /// <param name="locals">A dictionary of local variables</param>
    /// <param name="globals">A dictionary of global variables</param>
    /// <returns>The resulting Python object</returns>
    public static PyObject ExecuteExpression(this IPythonEnvironment env, string code, IDictionary<string, PyObject> locals, IDictionary<string, PyObject> globals)
    {
        using (GIL.Acquire())
        {
            using var localsPyDict = PyObject.From(locals);
            using var globalsPyDict = PyObject.From(globals);
            var result = CPythonAPI.PyRun_String(code, CPythonAPI.InputType.Py_eval_input, globalsPyDict, localsPyDict);
            locals.Merge(localsPyDict);
            globals.Merge(globalsPyDict);
            return result;
        }
    }

    /// <summary>
    /// Execute Python program from a string, typically multiple lines of code
    /// </summary>
    /// <param name="code">The Python code</param>
    /// <param name="locals">A dictionary of local variables</param>
    /// <param name="globals">A dictionary of global variables</param>
    /// <returns>Normally nothing</returns>
    public static PyObject Execute(this IPythonEnvironment env, string code, IDictionary<string, PyObject> locals, IDictionary<string, PyObject> globals)
    {
        using (GIL.Acquire())
        {
            using var localsPyDict = PyObject.From(locals);
            using var globalsPyDict = PyObject.From(globals);
            var result = CPythonAPI.PyRun_String(code, CPythonAPI.InputType.Py_file_input, globalsPyDict, localsPyDict);
            locals.Merge(localsPyDict);
            globals.Merge(globalsPyDict);
            return result;
        }
    }
}
