using CSnakes.Runtime.Python;

internal interface IPyObjectImporter<out T>
{
    static abstract T Import(PyObject pyObj);
}
