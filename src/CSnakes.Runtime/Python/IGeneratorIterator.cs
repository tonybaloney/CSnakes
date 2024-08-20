namespace CSnakes.Runtime.Python;

public interface IGeneratorIterator<TYield, TSend, TReturn>: IEnumerator<TYield>, IEnumerable<TYield>
{
    TYield Send(TSend value);
}
