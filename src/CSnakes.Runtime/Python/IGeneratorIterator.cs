namespace CSnakes.Runtime.Python;

public interface IGeneratorIterator<TYield, TSend, TReturn>: IEnumerator<TYield>, IEnumerable<TYield>, IGeneratorIterator
{
    TYield Send(TSend value);
}

public interface IGeneratorIterator { }
