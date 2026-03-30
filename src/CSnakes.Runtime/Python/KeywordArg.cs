namespace CSnakes.Runtime.Python;

/// <summary>
/// Represents a keyword argument to be passed to a Python callable.
/// </summary>
/// <param name="Name">
/// The argument keyword/name that must be a Python string, e.g.
/// one created using <see cref="PyObject.From(string)"/>. Otherwise, a
/// <see cref="PythonInvocationException" /> will be thrown when a call is made
/// via, e.g. <see cref="PyObject.Call(ReadOnlySpan{PyObject},ReadOnlySpan{KeywordArg})"/>.
/// </param>
/// <param name="Value">The argument value.</param>
/// <remarks>
/// An instance of this struct does not own a reference to the <see
/// cref="Value"/> therefore it is the responsibility of its user to make sure
/// the reference is kept alive for as long as a <see cref="KeywordArg"/> is in
/// use. The most common use case is to create a <see cref="KeywordArg"/> for
/// the purpose of passing it to a Python callable via <see
/// cref="PyObject.Call(ReadOnlySpan{PyObject},ReadOnlySpan{KeywordArg})"/> and
/// other overloads accepting keyword arguments. General usage otherwise is
/// discouraged to avoid ownership and lifetime bugs.
/// </remarks>
public readonly record struct KeywordArg(PyObject Name, PyObject Value);
