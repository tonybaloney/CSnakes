#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif
using CommunityToolkit.HighPerformance;

namespace CSnakes.Runtime;

// Delegates for "Span" and "ReadOnlySpan".

public delegate TResult ReadOnlySpanFunc<T, out TResult>(ReadOnlySpan<T> span);

public delegate TResult ReadOnlySpanFunc<T, in TArg, out TResult>(ReadOnlySpan<T> span, TArg arg)
#if NET9_0_OR_GREATER
    where TArg : allows ref struct
#endif
    ;

public delegate TResult ReadOnlySpanFunc<T, in TArg1, in TArg2, out TResult>(ReadOnlySpan<T> span, TArg1 arg1, TArg2 arg2)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
#endif
    ;

public delegate TResult ReadOnlySpanFunc<T, in TArg1, in TArg2, in TArg3, out TResult>(ReadOnlySpan<T> span, TArg1 arg1, TArg2 arg2, TArg3 arg3)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct
#endif
    ;

public delegate void SpanAction<T>(Span<T> span);

// "SpanAction" with a single additional argument is already supplied by in the BCL:
// https://learn.microsoft.com/en-us/dotnet/api/system.buffers.spanaction-2?view=net-10.0

public delegate void SpanAction<T, in TArg1, in TArg2>(Span<T> span, TArg1 arg1, TArg2 arg2)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
#endif
    ;

public delegate void SpanAction<T, in TArg1, in TArg2, in TArg3>(Span<T> span, TArg1 arg1, TArg2 arg2, TArg3 arg3)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct
#endif
    ;

// Delegates for "Span2D" and "ReadOnlySpan2D" from "CommunityToolkit.HighPerformance".

public delegate TResult ReadOnlySpan2DFunc<T, out TResult>(ReadOnlySpan2D<T> span);

public delegate TResult ReadOnlySpan2DFunc<T, in TArg, out TResult>(ReadOnlySpan2D<T> span, TArg arg)
#if NET9_0_OR_GREATER
    where TArg : allows ref struct
#endif
    ;

public delegate TResult ReadOnlySpan2DFunc<T, in TArg1, in TArg2, out TResult>(ReadOnlySpan2D<T> span, TArg1 arg1, TArg2 arg2)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
#endif
    ;

public delegate TResult ReadOnlySpan2DFunc<T, in TArg1, in TArg2, in TArg3, out TResult>(ReadOnlySpan2D<T> span, TArg1 arg1, TArg2 arg2, TArg3 arg3)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct
#endif
    ;

public delegate void Span2DAction<T>(Span2D<T> span);

public delegate void Span2DAction<T, in TArg>(Span2D<T> span, TArg arg)
#if NET9_0_OR_GREATER
    where TArg : allows ref struct
#endif
    ;

public delegate void Span2DAction<T, in TArg1, in TArg2>(Span2D<T> span, TArg1 arg1, TArg2 arg2)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
#endif
    ;

public delegate void Span2DAction<T, in TArg1, in TArg2, in TArg3>(Span2D<T> span, TArg1 arg1, TArg2 arg2, TArg3 arg3)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct
#endif
    ;

#if NET9_0_OR_GREATER

// Delegates for "TensorSpan" and "ReadOnlyTensorSpan".

public delegate TResult ReadOnlyTensorSpanFunc<T, out TResult>(ReadOnlyTensorSpan<T> span);

public delegate TResult ReadOnlyTensorSpanFunc<T, in TArg, out TResult>(ReadOnlyTensorSpan<T> span, TArg arg)
    where TArg : allows ref struct;

public delegate TResult ReadOnlyTensorSpanFunc<T, in TArg1, in TArg2, out TResult>(ReadOnlyTensorSpan<T> span, TArg1 arg1, TArg2 arg2)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct;

public delegate TResult ReadOnlyTensorSpanFunc<T, in TArg1, in TArg2, in TArg3, out TResult>(ReadOnlyTensorSpan<T> span, TArg1 arg1, TArg2 arg2, TArg3 arg3)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct;

public delegate void TensorSpanAction<T>(TensorSpan<T> span);

public delegate void TensorSpanAction<T, in TArg>(TensorSpan<T> span, TArg arg)
    where TArg : allows ref struct;

public delegate void TensorSpanAction<T, in TArg1, in TArg2>(TensorSpan<T> span, TArg1 arg1, TArg2 arg2)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct;

public delegate void TensorSpanAction<T, in TArg1, in TArg2, in TArg3>(TensorSpan<T> span, TArg1 arg1, TArg2 arg2, TArg3 arg3)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct;

#endif // NET9_0_OR_GREATER
