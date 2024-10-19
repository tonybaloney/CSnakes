﻿namespace CSnakes.Runtime.Python;

public interface IGeneratorIterator<out TYield, in TSend, out TReturn>: IEnumerator<TYield>, IEnumerable<TYield>, IGeneratorIterator
{
    TYield Send(TSend value);
}

public interface IGeneratorIterator { }
