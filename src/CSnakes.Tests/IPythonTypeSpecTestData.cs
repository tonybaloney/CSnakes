using CSnakes.Parser.Types;

namespace CSnakes.Tests;

/// <summary>
/// Defines static test data for PythonTypeSpec test classes.
/// </summary>
/// <typeparam name="T">The PythonTypeSpec type being tested.</typeparam>
public interface IPythonTypeSpecTestData<T> where T : PythonTypeSpec
{
    static abstract T CreateInstance();
    static abstract T CreateEquivalentInstance();
    static abstract PythonTypeSpec CreateDifferentInstance();
    static abstract string ExpectedName { get; }
    static abstract string ExpectedToString { get; }
}
