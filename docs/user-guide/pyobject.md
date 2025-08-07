# Handling Python Objects

## Object comparisons

The `PyObject` types can be compared with one another using the `is`, `==` and `!=` operators from Python. 

The equivalent to the `x is y` operator in Python is :

```csharp
// Small numbers are the same object in Python (weird implementation detail)
PyObject obj1 = PyObject.From(true);
PyObject obj2 = PyObject.From(true);
if (obj1!.Is(obj2))
    Console.WriteLine("Objects are the same!");
```

Equality can be accessed from the `.Equals` method or using the `==` operators in C#:

```csharp
PyObject obj1 = PyObject.From(3.0);
PyObject obj2 = PyObject.From(3);
if (obj1 == obj2) 
    Console.WriteLine("Objects are equal!");
```

Inequality can be accessed from the `.NotEquals` method or using the `!=` operators in C#:

```csharp
PyObject obj1 = PyObject.From(3.0);
PyObject obj2 = PyObject.From(3);
if (obj1 != obj2) 
    Console.WriteLine("Objects are not equal!");
```
