namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    /// <summary>
    /// Return the object from dictionary p which has a key `key`. 
    /// </summary>
    /// <param name="dict">Dictionary Object</param>
    /// <param name="key">Key Object</param>
    /// <exception cref="KeyNotFoundException">If the key is not found</exception>
    /// <returns>New reference.</returns>
    internal static nint GetItemOfPyMapping(MPyOPtr map, MPyOPtr key) => PyObject_GetItem(map, key);

    /// <summary>
    /// Insert val into the dictionary p with a key of key. 
    /// key must be hashable; if it isn’t, TypeError will be raised.  
    /// This function adds a reference to val and key if successful.
    /// </summary>
    /// <param name="dict">PyDict object</param>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <returns>Return 0 on success or -1 on failure.</returns>
    internal static bool SetItemInPyMapping(MPyOPtr dict, MPyOPtr key, MPyOPtr value) => PyObject_SetItem(dict, key, value) == 0;
}
