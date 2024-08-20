using CSnakes.Runtime.Python;
using System.Collections;


namespace Integration.Tests;
public class GeneratorTests: IntegrationTestBase
{

    public class DemoEnumerator : IEnumerator<string>, IEnumerable<string>
    {
        private readonly PyObject generator;
        private readonly PyObject next_func;
        private string current = string.Empty;

        public DemoEnumerator(PyObject generator)
        {
            this.generator = generator;
            this.next_func = generator.GetAttr("__next__");
        }

        public string Current => current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            generator.Dispose();
            next_func.Dispose();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            try
            {
                using PyObject result = next_func.Call(this.generator);
                current = result.As<string>();
                return true;
            } catch (PythonInvocationException pyO)
            {
                if (pyO.PythonExceptionType == "StopIteration")
                {
                    return false;
                }
                throw;
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }

    [Fact]
    public void TestGenerator()
    {
        var mod = Env.TestGenerators();
        var iter = mod.ExampleGenerator(1);
        var generator = new DemoEnumerator(iter);

        foreach (var x in generator)
        {
            Console.WriteLine(x);
        }
    }
}
