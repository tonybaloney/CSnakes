using CSnakes.Runtime.CPython;
using System.Collections;
using System.Diagnostics.CodeAnalysis;


namespace CSnakes.Runtime.Python
{
    public class PyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly PyObject _dictionaryObject;
        public PyDictionary(PyObject dictionary) {
            _dictionaryObject = dictionary;
            _dictionary = [];
        }

        public TValue this[TKey key] {
            get
            {
                if (_dictionary.ContainsKey(key))
                {
                    return _dictionary[key];
                } else
                {
                    using (GIL.Acquire())
                    {
                        using PyObject keyPyObject = PyObject.From<TKey>(key);
                        using PyObject value = PyObject.Create(CPythonAPI.PyMapping_GetItem(_dictionaryObject, keyPyObject));
                        var managedValue = value.As<TValue>();

                        _dictionary[key] = managedValue;
                        return managedValue;
                    }
                }
            }
        }

        public IEnumerable<TKey> Keys {
            get {
                using (GIL.Acquire()) {
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
            else
            {
                using (GIL.Acquire())
                {
                    using PyObject keyPyObject = PyObject.From<TKey>(key);
                    return CPythonAPI.PyMapping_HasKey(_dictionaryObject, keyPyObject) == 1;
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            using (GIL.Acquire())
            {
                using var items = PyObject.Create(CPythonAPI.PyMapping_Items(_dictionaryObject));
                return new PyKeyValuePairEnumerable<TKey, TValue>(items).GetEnumerator();
            }
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
