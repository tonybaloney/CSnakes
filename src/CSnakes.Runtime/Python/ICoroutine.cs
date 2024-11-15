﻿namespace CSnakes.Runtime.Python;

public interface ICoroutine<TYield, TSend, TReturn> : ICoroutine
{
    public Task<TYield> AsTask();
}

public interface ICoroutine { }
