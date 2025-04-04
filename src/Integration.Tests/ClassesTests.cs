using CSnakes.Runtime.Python;
using CSnakes.Runtime.Reflection;


namespace Integration.Tests;
public class ClassesTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    public class Person {
        public required string Name { get; set; }
        public required long Age { get; set; }
        public required double Height { get; set; }
        public required IReadOnlyList<string> Phobias { get; set; } = Array.Empty<string>();
    }

    [Fact]
    public void ConvertToClass()
    {
        var mod = Env.TestClasses();
        using PyObject result = mod.TestPerson();
        Person person = result.As<Person>();
        Assert.Equal("John Doe", person.Name);
        Assert.Equal(30, person.Age);
        Assert.Equal(5.9, person.Height);
        Assert.Equal(2, person.Phobias.Count);
        Assert.Equal("spiders", person.Phobias[0]);
        Assert.Equal("heights", person.Phobias[1]);
    }

    public class PersonWithSelf
    {
        public required string Name { get; set; }
        public required long Age { get; set; }
        public required double Height { get; set; }
        public required IReadOnlyList<string> Phobias { get; set; } = Array.Empty<string>();
        public required PyObject Self { get; set; } = PyObject.None;
    }

    [Fact]
    public void ConvertToClassWithSelf()
    {
        var mod = Env.TestClasses();
        using PyObject result = mod.TestPerson();
        PersonWithSelf person = result.As<PersonWithSelf>();
        Assert.Equal("John Doe", person.Name);
        Assert.Same(result, person.Self);
    }

    [Fact]
    public void TryConvertToNonHeapAllocatedType()
    {
        var mod = Env.TestClasses();
        using PyObject result = mod.TestNotHeap();
        Assert.Throws<InvalidCastException>(() => result.As<Person>());
    }

    public class PersonWithCustomAttributes
    {
        [PythonField("name")]
        public required string LegalName { get; set; }
        [PythonField("self", self: true)]
        public required PyObject PythonObject { get; set; }
    }

    [Fact]
    public void ConvertToClassWithCustomAttributes()
    {
        var mod = Env.TestClasses();
        using PyObject result = mod.TestPerson();
        PersonWithCustomAttributes person = result.As<PersonWithCustomAttributes>();
        Assert.Equal("John Doe", person.LegalName);
        Assert.Same(result, person.PythonObject);
    }

    public class PersonWithMethod
    {
        public required string Name { get; set; }
        public required PyObject Self { get; set; } = PyObject.None;
        public bool ScaredOf(string phobia)
        {
            using var arg1 = PyObject.From(phobia);
            return Self.GetAttr("scared_of").Call(arg1).As<bool>();
        }
    }

    [Fact]
    public void ConvertToClassWithMethod()
    {
        var mod = Env.TestClasses();
        using PyObject result = mod.TestPerson();
        PersonWithMethod person = result.As<PersonWithMethod>();
        Assert.Equal("John Doe", person.Name);
        Assert.True(person.ScaredOf("spiders"));
        Assert.False(person.ScaredOf("snakes"));
    }
}
