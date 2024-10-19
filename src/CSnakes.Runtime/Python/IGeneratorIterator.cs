using System.Collections;

namespace CSnakes.Runtime.Python;

public interface IGeneratorIterator<TYield, TSend, out TReturn>: IEnumerator<TYield>, IEnumerable<TYield>, IGeneratorIterator
{
    /// <remarks>
    /// The value is undefined until either <see cref="IEnumerator.MoveNext"/> or <see cref="Send"/>
    /// has been called and returned <see langword="null"/>.
    /// </remarks>
    TReturn Return { get; }

    bool Send(TSend value);
}

public interface IGeneratorIterator { }
