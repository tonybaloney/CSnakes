using CSnakes.Runtime.Python;
using System.Collections;

namespace CSnakes.Runtime;
internal partial class PyObjectTypeConverter
{
    static readonly Type listType = typeof(IReadOnlyList<>);
    static readonly Type collectionType = typeof(IEnumerable);
    static readonly Type dictionaryType = typeof(IReadOnlyDictionary<,>);
    static readonly Type pyObjectType = typeof(PyObject);
    static readonly Type generatorIteratorType = typeof(IGeneratorIterator<,,>);
    static readonly Type bufferType = typeof(IPyBuffer);
}
