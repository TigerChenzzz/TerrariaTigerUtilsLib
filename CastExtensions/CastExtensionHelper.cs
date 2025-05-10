using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TigerUtilsLib.CastExtensions;

public static class CastExtensionHelper {
    #region Register
    private static readonly Dictionary<Type, Dictionary<Type, ICastHolder>> _casts = [];
    public static void RegisterCastExtension<TSource, TResult>(Func<TSource, TResult> cast) {
        var sourceType = typeof(TSource);
        var resultType = typeof(TResult);
        ref var values = ref CollectionsMarshal.GetValueRefOrAddDefault(_casts, sourceType, out _);
        (values ??= []).Add(resultType, NewCastHolder(cast));
    }
    public static void RegisterCastExtensionRecursive<TSource, TResult>(Func<TSource, TResult> cast) {
        var sourceType = typeof(TSource);
        var resultType = typeof(TResult);
        ref var values = ref CollectionsMarshal.GetValueRefOrAddDefault(_casts, sourceType, out _);
        values ??= [];
        var castHolder = NewCastHolder(cast);
        values.Add(resultType, castHolder);
        foreach (var i in resultType.GetInterfaces())
            values.Add(i, castHolder);
    }
    #region Cached
    public static void RegisterCachedCastExtension<TSource, TResult>(Func<TSource, TResult> cast) where TSource : class where TResult : class?
        => RegisterCastExtension<TSource, TResult>(NewCachedCastStruct(cast).Cast);
    public static void RegisterCachedCastExtensionRecursive<TSource, TResult>(Func<TSource, TResult> cast) where TSource : class where TResult : class?
        => RegisterCastExtensionRecursive<TSource, TResult>(NewCachedCastStruct(cast).Cast);
    private readonly struct CachedCastStruct<TSource, TResult>(Func<TSource, TResult> cast) where TSource : class where TResult : class? {
        public TResult Cast(TSource source) {
            var cache = CastExtensionCache<TResult>.Cache;
            if (cache.TryGetValue(source, out var result))
                return result;
            result = cast(source);
            cache.Add(source, result);
            return result;
        }
    }
    private static CachedCastStruct<TSource, TResult> NewCachedCastStruct<TSource, TResult>(Func<TSource, TResult> cast) where TSource : class where TResult : class? => new(cast);
    private class CastExtensionCache<TResult> where TResult : class? {
        public static ConditionalWeakTable<object, TResult> Cache { get; } = [];
    }
    #endregion
    #endregion
    #region Usage
    public static TResult? As<TSource, TResult>(TSource source) {
        if (source is TResult result)
            return result;
        if (source == null)
            return default;
        var castHolder = GetCastHolder(source.GetType(), typeof(TResult));
        if (castHolder == null)
            return default;
        return castHolder.TypedCast<TSource, TResult>(source);
    }
    public static TResult? As<TResult>(object? source) => As<object?, TResult>(source);
    public static bool Is<TSource, TResult>(TSource source, [NotNullWhen(true)] out TResult? result) where TResult : notnull {
        if (source is TResult directResult) {
            result = directResult;
            return true;
        }
        if (source == null) {
            result = default;
            return false;
        }
        var castHolder = GetCastHolder(source.GetType(), typeof(TResult));
        if (castHolder == null) {
            result = default;
            return false;
        }
        result = castHolder.TypedCast<TSource, TResult>(source);
        return true;
    }
    public static bool Is<TSource, TResult>(TSource source)
        => source is TResult || source != null && GetCastHolder(source.GetType(), typeof(TResult)) != null;
    public static bool Is<TResult>(object? source) => Is<object?, TResult>(source);
    public static Func<TSource?, TResult?>? GetCastFunc<TSource, TResult>() {
        if (typeof(TSource).IsAssignableTo(typeof(TResult)))
            return static source => source is TResult result ? result : default;
        if (GetCastHolder<TSource, TResult>() is not { } castHolder)
            return null;
        return source => source == null ? default : castHolder.Cast(source);
    }
    public static Func<TSource, TResult>? GetCastFuncN<TSource, TResult>() where TSource : notnull where TResult : notnull {
        if (typeof(TSource).IsAssignableTo(typeof(TResult)))
            return static source => source is TResult result ? result : default!;
        return GetCastHolder<TSource, TResult>()?.CastFunc;
    }
    public static Func<TSource?, TResult?>? GetCastFunc<TSource, TResult>(TSource source) where TSource : notnull {
        if (source is TResult)
            return static source => source is TResult result ? result : default;
        if (GetCastHolder(source.GetType(), typeof(TResult)) is not ICastHolder<TSource, TResult> castHolder)
            return null;
        return source => source == null ? default : castHolder.Cast(source);
    }
    public static Func<TSource, TResult>? GetCastFuncN<TSource, TResult>(TSource source) where TSource : notnull where TResult : notnull {
        if (source is TResult)
            return static source => source is TResult result ? result : default!;
        return GetCastHolder<TSource, TResult>(source)?.CastFunc;
    }
    public static Func<object?, TResult?>? GetCastFunc<TResult>(object source) => GetCastFunc<object, TResult>(source);
    public static Func<object, TResult>? GetCastFuncN<TResult>(object source) where TResult : notnull => GetCastFuncN<object, TResult>(source);
    #endregion
    #region CastHolder
    private static ICastHolder? GetCastHolder(Type sourceType, Type resultType) {
        if (_casts.TryGetValue(sourceType, out var values) && values.TryGetValue(resultType, out var result))
            return result;
        var baseType = sourceType.BaseType;
        if (baseType == null)
            return null;
        result = GetCastHolder(baseType, resultType);
        if (result == null)
            return null;
        if (values != null)
            values.Add(resultType, result);
        else
            _casts.Add(sourceType, new(){ { resultType, result } });
        return result;
    }
    private static ICastHolder<TSource, TResult>? GetCastHolder<TSource, TResult>() => GetCastHolder(typeof(TSource), typeof(TResult)) as ICastHolder<TSource, TResult>;
    private static ICastHolder<TSource, TResult>? GetCastHolder<TSource, TResult>(TSource source) where TSource : notnull => GetCastHolder(source.GetType(), typeof(TResult)) as ICastHolder<TSource, TResult>;
    private interface ICastHolder {
        TResult TypedCast<TResult>(object source) => ((ICastHolder<TResult>)this).CastObject(source);
        TResult TypedCast<TSource, TResult>([DisallowNull] TSource source) {
            if (this is ICastHolder<TSource, TResult> typedCastHolder)
                return typedCastHolder.Cast(source);
            return TypedCast<TResult>(source);
        }
    }
    private interface ICastHolder<out TResult> : ICastHolder {
        TResult CastObject(object source);
    }
    private interface ICastHolder<in TSource, out TResult> : ICastHolder<TResult> {
        Func<TSource, TResult> CastFunc { get; }
        TResult Cast(TSource source) => CastFunc(source);
        TResult ICastHolder<TResult>.CastObject(object source) => Cast((TSource)source);
    }
    private class CastHolder<TSource, TResult>(Func<TSource, TResult> cast) : ICastHolder<TSource, TResult> {
        public Func<TSource, TResult> CastFunc => cast;
    }
    private static CastHolder<TSource, TResult> NewCastHolder<TSource, TResult>(Func<TSource, TResult> cast) => new(cast);
    #endregion
}
