#if ZLINQ

using System;
using System.Collections.Generic;
using ZLinq;

namespace TigerUtilsLib;

partial class TigerClasses {
    #region Filter
#if NET9_0_OR_GREATER
    public ref struct
#else
    public struct
#endif
        Filter<TEnumerator, TSource, TResult>(TEnumerator source, Func<TSource, Existable<TResult>> filter) : IValueEnumerator<TResult>
        where TEnumerator : struct, IValueEnumerator<TSource>
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
#pragma warning disable IDE0044 // 添加只读修饰符
#pragma warning disable IDE0251 // 将成员设为“readonly”
        private TEnumerator source = source;

        public bool TryGetNext(out TResult current) {
            while (source.TryGetNext(out var value)) {
                var box = filter(value);
                if (box.HasValue) {
                    current = box.Value;
                    return true;
                }
            }
            current = default!;
            return false;
        }
        
        public bool TryGetNonEnumeratedCount(out int count) {
            count = 0;
            return false;
        }
        public bool TryGetSpan(out ReadOnlySpan<TResult> span) {
            span = default;
            return false;
        }
        public bool TryCopyTo(scoped Span<TResult> destination, Index offset) => false;
        public void Dispose() => source.Dispose();
#pragma warning restore IDE0044 // 添加只读修饰符
#pragma warning restore IDE0251 // 将成员设为“readonly”
    }
    #endregion
    #region WhereNotNull
#if NET9_0_OR_GREATER
    public ref struct
#else
    public struct
#endif
        WhereNotNull<TEnumerator, TSource>(TEnumerator source) : IValueEnumerator<TSource>
        where TEnumerator : struct, IValueEnumerator<TSource?>
        where TSource : notnull
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
#pragma warning disable IDE0044 // 添加只读修饰符
#pragma warning disable IDE0251 // 将成员设为“readonly”
        private TEnumerator source = source;

        public bool TryGetNext(out TSource current) {
            while (source.TryGetNext(out var value)) {
                if (value != null) {
                    current = value;
                    return true;
                }
            }
            current = default!;
            return false;
        }

        public bool TryGetNonEnumeratedCount(out int count) {
            count = 0;
            return false;
        }
        public bool TryGetSpan(out ReadOnlySpan<TSource> span) {
            span = default;
            return false;
        }
        public bool TryCopyTo(scoped Span<TSource> destination, Index offset) => false;
        public void Dispose() => source.Dispose();
#pragma warning restore IDE0044 // 添加只读修饰符
#pragma warning restore IDE0251 // 将成员设为“readonly”
    }
    #endregion
}

static partial class TigerExtensions {
    #region this ValueEnumerable
    #region AsEnumerable
    public static IEnumerable<TSource> AsEnumerable<TEnumerator, TSource>(this ValueEnumerable<TEnumerator, TSource> source)
        where TEnumerator : struct, IValueEnumerator<TSource>
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
#if NET9_0_OR_GREATER
        using var arrayPool = source.ToArrayPool();
        var e = arrayPool.AsValueEnumerable();
        foreach (var t in e) {
            yield return t;
        }
#else
        using var e = source.Enumerator;
        while (e.TryGetNext(out var current)) {
            yield return current;
        }
#endif
    }
    #endregion
    #region ForeachDo
    public static void ForeachDo<TEnumerator, TSource>(this ValueEnumerable<TEnumerator, TSource> source, Action<TSource> action)
    where TEnumerator : struct, IValueEnumerator<TSource>
#if NET9_0_OR_GREATER
    , allows ref struct
#endif
    {
        using var e = source.Enumerator;
        if (e.TryGetSpan(out var span)) {
            foreach (var item in span) {
                action(item);
            }
        }
        else {
            while (e.TryGetNext(out var item)) {
                action(item);
            }
        }
    }
    #endregion
    #region Filter
    public static ValueEnumerable<Filter<TEnumerator, TSource, TResult>, TResult> Filter<TEnumerator, TSource, TResult>(
        this ValueEnumerable<TEnumerator, TSource> source, Func<TSource, Existable<TResult>> filter)
        where TEnumerator : struct, IValueEnumerator<TSource>
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
        => new(new(source.Enumerator, filter));
    #endregion
    #region WhereNotNull
    public static ValueEnumerable<WhereNotNull<TEnumerator, TSource>, TSource> WhereNotNull<TEnumerator, TSource>(
        this ValueEnumerable<TEnumerator, TSource?> source)
        where TEnumerator : struct, IValueEnumerator<TSource?>
        where TSource : notnull
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
        => new(new(source.Enumerator));
    #endregion
    #endregion
    #region ICollection 扩展 (AddRange, RemoveRange)
    public static void AddRange<TEnumerator, TSource>(this ICollection<TSource> self, ValueEnumerable<TEnumerator, TSource> valueEnumerable)
        where TEnumerator : struct, IValueEnumerator<TSource>
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        using var e = valueEnumerable.Enumerator;
        if (e.TryGetSpan(out var span)) {
            foreach (var item in span) {
                self.Add(item);
            }
        }
        else {
            while (e.TryGetNext(out var item)) {
                self.Add(item);
            }
        }
    }

    public static void RemoveRange<TEnumerator, TSource>(this ICollection<TSource> self, ValueEnumerable<TEnumerator, TSource> valueEnumerable)
        where TEnumerator : struct, IValueEnumerator<TSource>
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        using var e = valueEnumerable.Enumerator;
        if (e.TryGetSpan(out var span)) {
            foreach (var item in span) {
                self.Remove(item);
            }
        }
        else {
            while (e.TryGetNext(out var item)) {
                self.Add(item);
            }
        }
    }
    #endregion
}

#endif
