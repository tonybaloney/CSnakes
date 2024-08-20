namespace CSnakes.Runtime.Python;

public interface IGenerator<TYield, TSend, TReturn>
{
    TYield Send(TSend value);
    void Close();
}
