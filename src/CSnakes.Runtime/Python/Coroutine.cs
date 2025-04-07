using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public sealed class Coroutine<TYield, TSend, TReturn>(PyObject coroutine) :
    Coroutine<TYield, TSend, TReturn,
              InternalServices.Converters.Runtime<TYield>,
              InternalServices.Converters.Runtime<TSend>,
              InternalServices.Converters.Runtime<TReturn>>(coroutine);

public class Coroutine<TYield, TSend, TReturn, TYieldConvert, TSendConvert, TReturnConvert>(PyObject coroutine) :
    ICoroutine<TYield, TSend, TReturn>
    where TYieldConvert : InternalServices.IConverter<TYield>
    where TSendConvert : InternalServices.IConverter<TSend>
    where TReturnConvert : InternalServices.IConverter<TReturn>
{
    private TYield current = default!;
    private TReturn @return = default!;

    public TYield Current => current;
    public TReturn Return => @return;

    public Task<TYield?> AsTask(CancellationToken? cancellationToken = null)
    {
        return Task.Run(
            () =>
            {
                try
                {
                    using (GIL.Acquire())
                    {
                        using PyObject result = CPythonAPI.GetEventLoop().RunTaskUntilComplete(coroutine, cancellationToken);
                        current = TYieldConvert.Convert(result);
                    }
                    return current;
                }
                catch (PythonInvocationException ex)
                {
                    if (ex.InnerException is PythonStopIterationException stopIteration)
                    {
                        using var @return = stopIteration.TakeValue();
                        this.@return = TReturnConvert.Convert(@return);

                        // Coroutine has finished
                        // TODO: define behavior for this case
                        return default;
                    }

                    throw;
                }
            }
        );
    }
}
