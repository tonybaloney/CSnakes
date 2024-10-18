namespace Integration.Tests;

public class CustomTypesTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void CreatePerson()
    {
        var testModule = Env.TestCustomClass();
        var person = testModule.CreatePerson("Jane", "Doe").As<Person>();
        Assert.Equal("Jane", person.FirstName);
        Assert.Equal("Doe", person.Surname);
    }
}

public class Person
{
    public required string FirstName { get; set; }
    [PythonName("last_name")]
    public required string Surname { get; set; }
}
