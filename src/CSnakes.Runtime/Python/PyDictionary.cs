using CSnakes.Runtime.CPython;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace CSnakes.Runtime.Python;

internal class PyDictionary<TKey, TValue>(PyObject dictionary) : IReadOnlyDictionary<TKey, TValue>, IDisposable, ICloneable
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary = [];
    private readonly PyObject _dictionaryObject = dictionary;

    public TValue this[TKey key]
    {
        get
        {
            if (_dictionary.TryGetValue(key, out TValue? value))
            {
                return value;
            }
            using (GIL.Acquire())
            {
                using PyObject keyPyObject = PyObject.From(key);
                using PyObject pyObjValue = PyObject.Create(CPythonAPI.PyMapping_GetItem(_dictionaryObject, keyPyObject));
                TValue managedValue = pyObjValue.As<TValue>();

                _dictionary[key] = managedValue;
                return managedValue;
            }
        }
    }

    public IEnumerable<TKey> Keys
    {
        get
        {
            using (GIL.Acquire())
            {
                return new PyEnumerable<TKey>(PyObject.Create(CPythonAPI.PyMapping_Keys(_dictionaryObject)));
            }
        }
    }

    public IEnumerable<TValue> Values
    {
        get
        {
            using (GIL.Acquire())
            {
                return new PyEnumerable<TValue>(PyObject.Create(CPythonAPI.PyMapping_Values(_dictionaryObject)));
            }
        }
    }

    public int Count
    {
        get
        {
            using (GIL.Acquire())
            {
                return (int)CPythonAPI.PyMapping_Size(_dictionaryObject);
            }
        }
    }

    public bool ContainsKey(TKey key)
    {
        if (_dictionary.ContainsKey(key))
        {
            return true;
        }

        using (GIL.Acquire())
        {
            using PyObject keyPyObject = PyObject.From(key);
            return CPythonAPI.PyMapping_HasKey(_dictionaryObject, keyPyObject) == 1;
        }
    }

    public void Dispose() => _dictionaryObject.Dispose();

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        using (GIL.Acquire())
        {
            using var items = PyObject.Create(CPythonAPI.PyMapping_Items(_dictionaryObject));
            return new PyEnumerable<KeyValuePair<TKey, TValue>, PyObjectImporter<TKey, TValue>>(items).GetEnumerator();
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (ContainsKey(key))
        {
            value = this[key];
            return true;
        }

        value = default;
        return false;
    }

    PyObject ICloneable.Clone() => _dictionaryObject.Clone();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
