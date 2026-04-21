#if NET9_0_OR_GREATER
using System.Numerics.Tensors;
#endif
using CommunityToolkit.HighPerformance;

namespace CSnakes.Runtime;

// Delegates for "Span" and "ReadOnlySpan".

public delegate TResult ReadOnlySpanFunc<T, out TResult>(scoped ReadOnlySpan<T> span);

public delegate TResult ReadOnlySpanFunc<T, TArg, out TResult>(scoped ReadOnlySpan<T> span, in TArg arg)
#if NET9_0_OR_GREATER
    where TArg : allows ref struct
#endif
    ;

public delegate TResult ReadOnlySpanFunc<T, TArg1, TArg2, out TResult>(scoped ReadOnlySpan<T> span, in TArg1 arg1, in TArg2 arg2)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
#endif
    ;

public delegate TResult ReadOnlySpanFunc<T, TArg1, TArg2, TArg3, out TResult>(scoped ReadOnlySpan<T> span, in TArg1 arg1, in TArg2 arg2, in TArg3 arg3)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct
#endif
    ;

public delegate void SpanAction<T>(Span<T> span);

public delegate void SpanAction<T, TArg>(Span<T> span, in TArg arg)
#if NET9_0_OR_GREATER
    where TArg : allows ref struct
#endif
;

public delegate void SpanAction<T, TArg1, TArg2>(Span<T> span, in TArg1 arg1, in TArg2 arg2)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
#endif
    ;

public delegate void SpanAction<T, TArg1, TArg2, TArg3>(Span<T> span, in TArg1 arg1, in TArg2 arg2, in TArg3 arg3)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct
#endif
    ;

// Delegates for "Span2D" and "ReadOnlySpan2D" from "CommunityToolkit.HighPerformance".

public delegate TResult ReadOnlySpan2DFunc<T, out TResult>(scoped in ReadOnlySpan2D<T> span);

public delegate TResult ReadOnlySpan2DFunc<T, TArg, out TResult>(scoped in ReadOnlySpan2D<T> span, in TArg arg)
#if NET9_0_OR_GREATER
    where TArg : allows ref struct
#endif
    ;

public delegate TResult ReadOnlySpan2DFunc<T, TArg1, TArg2, out TResult>(scoped in ReadOnlySpan2D<T> span, in TArg1 arg1, in TArg2 arg2)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
#endif
    ;

public delegate TResult ReadOnlySpan2DFunc<T, TArg1, TArg2, TArg3, out TResult>(scoped in ReadOnlySpan2D<T> span, in TArg1 arg1, in TArg2 arg2, in TArg3 arg3)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct
#endif
    ;

public delegate void Span2DAction<T>(in Span2D<T> span);

public delegate void Span2DAction<T, TArg>(in Span2D<T> span, in TArg arg)
#if NET9_0_OR_GREATER
    where TArg : allows ref struct
#endif
    ;

public delegate void Span2DAction<T, TArg1, TArg2>(in Span2D<T> span, in TArg1 arg1, in TArg2 arg2)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
#endif
    ;

public delegate void Span2DAction<T, TArg1, TArg2, TArg3>(in Span2D<T> span, in TArg1 arg1, in TArg2 arg2, in TArg3 arg3)
#if NET9_0_OR_GREATER
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct
#endif
    ;

#if NET9_0_OR_GREATER

// Delegates for "TensorSpan" and "ReadOnlyTensorSpan".

public delegate TResult ReadOnlyTensorSpanFunc<T, out TResult>(scoped in ReadOnlyTensorSpan<T> span);

public delegate TResult ReadOnlyTensorSpanFunc<T, TArg, out TResult>(scoped in ReadOnlyTensorSpan<T> span, in TArg arg)
    where TArg : allows ref struct;

public delegate TResult ReadOnlyTensorSpanFunc<T, TArg1, TArg2, out TResult>(scoped in ReadOnlyTensorSpan<T> span, in TArg1 arg1, in TArg2 arg2)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct;

public delegate TResult ReadOnlyTensorSpanFunc<T, TArg1, TArg2, TArg3, out TResult>(scoped in ReadOnlyTensorSpan<T> span, in TArg1 arg1, in TArg2 arg2, in TArg3 arg3)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct;

public delegate void TensorSpanAction<T>(in TensorSpan<T> span);

public delegate void TensorSpanAction<T, TArg>(in TensorSpan<T> span, in TArg arg)
    where TArg : allows ref struct;

public delegate void TensorSpanAction<T, TArg1, TArg2>(in TensorSpan<T> span, in TArg1 arg1, in TArg2 arg2)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct;

public delegate void TensorSpanAction<T, TArg1, TArg2, TArg3>(in TensorSpan<T> span, in TArg1 arg1, in TArg2 arg2, in TArg3 arg3)
    where TArg1 : allows ref struct
    where TArg2 : allows ref struct
    where TArg3 : allows ref struct;

#endif // NET9_0_OR_GREATER
