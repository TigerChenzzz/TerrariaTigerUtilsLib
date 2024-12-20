using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FastInvoker = MonoMod.Utils.FastReflectionHelper.FastInvoker;

namespace TigerUtilsLib.ReflectionExtensionsLib;

/// <summary>
/// 暂不支持静态成员
/// </summary>
public static class ReflectionExtensions {
    // TODO: 静态成员支持
    #region Cache
    public static bool CacheNull { get; set; } = false;
    public static void ClearCache() => Cache.ClearAll();
    private abstract class Cache {
        protected readonly static Dictionary<Type, Cache> All = [];
        private static Cache GetCache(Type type) {
            if (All.TryGetValue(type, out var cache)) {
                return cache;
            }
            return (Cache)typeof(Cache<>).MakeGenericType(type).GetProperty("Instance", BFS)!.GetValue(null)!;
        }
        public static void ClearAll() {
            foreach (var cache in All.Values) {
                cache.Clear();
            }
        }
        #region GetField
        public static bool TryGetGetField(Type type, string fieldName, [NotNullWhen(true)] out Func<object, object?>? func) {
            if (!All.TryGetValue(type, out var cache)) {
                func = null;
                return false;
            }
            return cache.getFields.TryGetValue(fieldName, out func);
        }
        public static bool TryGetGetField<T>(Type type, string fieldName, [NotNullWhen(true)] out Func<T, object?>? func) {
            if (!All.TryGetValue(type, out var cache)) {
                func = null;
                return false;
            }
            return cache.GetCacheT<T>().getFields.TryGetValue(fieldName, out func);
        }
        public static bool TryGetGetFieldR<TResult>(Type type, string fieldName, [NotNullWhen(true)] out Func<object, TResult>? func) {
            if (!All.TryGetValue(type, out var cache)) {
                func = null;
                return false;
            }
            return cache.GetCacheR<TResult>().getFields.TryGetValue(fieldName, out func);
        }
        public static bool TryGetGetField<T, TResult>(Type type, string fieldName, [NotNullWhen(true)] out Func<T, TResult>? func) {
            if (!All.TryGetValue(type, out var cache)) {
                func = null;
                return false;
            }
            return cache.GetCacheTR<T, TResult>().getFields.TryGetValue(fieldName, out func);
        }

        public static Action<Func<object, object?>?> ToAddGetField(Type type, string fieldName) => GetCache(type).getFields.AddFP(fieldName);
        public static Action<Func<T, object?>?> ToAddGetField<T>(Type type, string fieldName) => GetCache(type).GetCacheT<T>().getFields.AddFP(fieldName);
        public static Action<Func<object, TResult>?> ToAddGetFieldR<TResult>(Type type, string fieldName) => GetCache(type).GetCacheR<TResult>().getFields.AddFP(fieldName);
        public static Action<Func<T, TResult>?> ToAddGetField<T, TResult>(Type type, string fieldName) => GetCache(type).GetCacheTR<T, TResult>().getFields.AddFP(fieldName);
        #endregion
        #region SetField
        public static bool TryGetSetField(Type type, string fieldName, [NotNullWhen(true)] out Action<object, object?>? action) {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.setFields.TryGetValue(fieldName, out action);
        }
        public static bool TryGetSetField<T>(Type type, string fieldName, [NotNullWhen(true)] out Action<T, object?>? action) {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.GetCacheT<T>().setFields.TryGetValue(fieldName, out action);
        }
        public static bool TryGetSetFieldR<TResult>(Type type, string fieldName, [NotNullWhen(true)] out Action<object, TResult>? action) {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.GetCacheR<TResult>().setFields.TryGetValue(fieldName, out action);
        }
        public static bool TryGetSetField<T, TResult>(Type type, string fieldName, [NotNullWhen(true)] out Action<T, TResult>? action) {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.GetCacheTR<T, TResult>().setFields.TryGetValue(fieldName, out action);
        }
        public static bool TryGetSetFieldN<T>(Type type, string fieldName, [NotNullWhen(true)] out RefSetMemberDelegate<T>? action) where T : struct {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.GetCacheT<T>().setFieldsN.TryGetValue(fieldName, out action);
        }
        public static bool TryGetSetFieldN<T, TResult>(Type type, string fieldName, [NotNullWhen(true)] out RefSetMemberDelegate<T, TResult>? action) where T : struct {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.GetCacheTR<T, TResult>().setFieldsN.TryGetValue(fieldName, out action);
        }

        public static Action<Action<object, object?>?> ToAddSetField(Type type, string fieldName) => GetCache(type).setFields.AddFP(fieldName);
        public static Action<Action<T, object?>?> ToAddSetField<T>(Type type, string fieldName) => GetCache(type).GetCacheT<T>().setFields.AddFP(fieldName);
        public static Action<Action<object, TResult>?> ToAddSetFieldR<TResult>(Type type, string fieldName) => GetCache(type).GetCacheR<TResult>().setFields.AddFP(fieldName);
        public static Action<Action<T, TResult>?> ToAddSetField<T, TResult>(Type type, string fieldName) => GetCache(type).GetCacheTR<T, TResult>().setFields.AddFP(fieldName);
        public static Action<RefSetMemberDelegate<T>?> ToAddSetFieldN<T>(Type type, string fieldName) where T : struct => GetCache(type).GetCacheT<T>().setFieldsN.AddFP(fieldName);
        public static Action<RefSetMemberDelegate<T, TResult>?> ToAddSetFieldN<T, TResult>(Type type, string fieldName) where T : struct => GetCache(type).GetCacheTR<T, TResult>().setFieldsN.AddFP(fieldName);
        public static void AddSetFieldN<T>(Type type, string fieldName, RefSetMemberDelegate<T>? setField) where T : struct => GetCache(type).GetCacheT<T>().setFieldsN.Add(fieldName, setField);
        public static void AddSetFieldN<T, TResult>(Type type, string fieldName, RefSetMemberDelegate<T, TResult>? setField) where T : struct => GetCache(type).GetCacheTR<T, TResult>().setFieldsN.Add(fieldName, setField);
        #endregion
        #region GetProperty
        public static bool TryGetGetProperty(Type type, string propertyName, [NotNullWhen(true)] out Func<object, object?>? func) {
            if (!All.TryGetValue(type, out var cache)) {
                func = null;
                return false;
            }
            return cache.getProperties.TryGetValue(propertyName, out func);
        }
        public static bool TryGetGetProperty<T>(Type type, string propertyName, [NotNullWhen(true)] out Func<T, object?>? func) {
            if (!All.TryGetValue(type, out var cache)) {
                func = null;
                return false;
            }
            return cache.GetCacheT<T>().getProperties.TryGetValue(propertyName, out func);
        }
        public static bool TryGetGetPropertyR<TResult>(Type type, string propertyName, [NotNullWhen(true)] out Func<object, TResult>? func) {
            if (!All.TryGetValue(type, out var cache)) {
                func = null;
                return false;
            }
            return cache.GetCacheR<TResult>().getProperties.TryGetValue(propertyName, out func);
        }
        public static bool TryGetGetProperty<T, TResult>(Type type, string propertyName, [NotNullWhen(true)] out Func<T, TResult>? func) {
            if (!All.TryGetValue(type, out var cache)) {
                func = null;
                return false;
            }
            return cache.GetCacheTR<T, TResult>().getProperties.TryGetValue(propertyName, out func);
        }

        public static Action<Func<object, object?>?> ToAddGetProperty(Type type, string propertyName) => GetCache(type).getProperties.AddFP(propertyName);
        public static Action<Func<T, object?>?> ToAddGetProperty<T>(Type type, string propertyName) => GetCache(type).GetCacheT<T>().getProperties.AddFP(propertyName);
        public static Action<Func<object, TResult>?> ToAddGetPropertyR<TResult>(Type type, string propertyName) => GetCache(type).GetCacheR<TResult>().getProperties.AddFP(propertyName);
        public static Action<Func<T, TResult>?> ToAddGetProperty<T, TResult>(Type type, string propertyName) => GetCache(type).GetCacheTR<T, TResult>().getProperties.AddFP(propertyName);
        #endregion
        #region SetProperty
        public static bool TryGetSetProperty(Type type, string propertyName, [NotNullWhen(true)] out Action<object, object?>? action) {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.setProperties.TryGetValue(propertyName, out action);
        }
        public static bool TryGetSetProperty<T>(Type type, string propertyName, [NotNullWhen(true)] out Action<T, object?>? action) {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.GetCacheT<T>().setProperties.TryGetValue(propertyName, out action);
        }
        public static bool TryGetSetPropertyR<TResult>(Type type, string propertyName, [NotNullWhen(true)] out Action<object, TResult>? action) {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.GetCacheR<TResult>().setProperties.TryGetValue(propertyName, out action);
        }
        public static bool TryGetSetProperty<T, TResult>(Type type, string propertyName, [NotNullWhen(true)] out Action<T, TResult>? action) {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.GetCacheTR<T, TResult>().setProperties.TryGetValue(propertyName, out action);
        }
        public static bool TryGetSetPropertyN<T>(Type type, string propertyName, [NotNullWhen(true)] out RefSetMemberDelegate<T>? action) where T : struct {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.GetCacheT<T>().setPropertiesN.TryGetValue(propertyName, out action);
        }
        public static bool TryGetSetPropertyN<T, TResult>(Type type, string propertyName, [NotNullWhen(true)] out RefSetMemberDelegate<T, TResult>? action) where T : struct {
            if (!All.TryGetValue(type, out var cache)) {
                action = null;
                return false;
            }
            return cache.GetCacheTR<T, TResult>().setPropertiesN.TryGetValue(propertyName, out action);
        }

        public static Action<Action<object, object?>?> ToAddSetProperty(Type type, string propertyName) => GetCache(type).setProperties.AddFP(propertyName);
        public static Action<Action<T, object?>?> ToAddSetProperty<T>(Type type, string propertyName) => GetCache(type).GetCacheT<T>().setProperties.AddFP(propertyName);
        public static Action<Action<object, TResult>?> ToAddSetPropertyR<TResult>(Type type, string propertyName) => GetCache(type).GetCacheR<TResult>().setProperties.AddFP(propertyName);
        public static Action<Action<T, TResult>?> ToAddSetProperty<T, TResult>(Type type, string propertyName) => GetCache(type).GetCacheTR<T, TResult>().setProperties.AddFP(propertyName);
        public static Action<RefSetMemberDelegate<T>?> ToAddSetPropertyN<T>(Type type, string propertyName) where T : struct => GetCache(type).GetCacheT<T>().setPropertiesN.AddFP(propertyName);
        public static Action<RefSetMemberDelegate<T, TResult>?> ToAddSetPropertyN<T, TResult>(Type type, string propertyName) where T : struct => GetCache(type).GetCacheTR<T, TResult>().setPropertiesN.AddFP(propertyName);
        public static void AddSetPropertyN<T>(Type type, string propertyName, RefSetMemberDelegate<T>? setProperty) where T : struct => GetCache(type).GetCacheT<T>().setPropertiesN.Add(propertyName, setProperty);
        public static void AddSetPropertyN<T, TResult>(Type type, string propertyName, RefSetMemberDelegate<T, TResult>? setProperty) where T : struct => GetCache(type).GetCacheTR<T, TResult>().setPropertiesN.Add(propertyName, setProperty);
        #endregion
        #region InvokeMethod
        public static bool TryGetInvokeMethods(Type type, string methodName, [NotNullWhen(true)] out FastInvoker? invoker) {
            if (!All.TryGetValue(type, out var cache)) {
                invoker = null;
                return false;
            }
            return cache.invokeMethods.TryGetValue(methodName, out invoker);
        }

        public static Action<FastInvoker?> ToAddInvokeMethods(Type type, string methodName) => GetCache(type).invokeMethods.AddFP(methodName);
        public static void AddInvokeMethods(Type type, string methodName, FastInvoker? invoker) => GetCache(type).invokeMethods.Add(methodName, invoker);
        #endregion

        protected Dictionary<string, Func<object, object?>?> getFields = [];
        protected Dictionary<string, Action<object, object?>?> setFields = [];
        protected Dictionary<string, Func<object, object?>?> getProperties = [];
        protected Dictionary<string, Action<object, object?>?> setProperties = [];
        protected Dictionary<string, FastInvoker?> invokeMethods = [];
        protected readonly Dictionary<Type, CacheT> AllT = [];
        protected readonly Dictionary<Type, CacheR> AllR = [];
        protected void Clear() {
            getFields.Clear();
            setFields.Clear();
            getProperties.Clear();
            setProperties.Clear();
            invokeMethods.Clear();
            foreach (var cacheT in AllT.Values) {
                cacheT.Clear();
            }
            foreach (var cacheR in AllR.Values) {
                cacheR.Clear();
            }
        }

        protected abstract CacheTBase<T> GetCacheT<T>();
        protected abstract CacheRBase<T> GetCacheR<T>();
        protected abstract CacheTRBase<T, TResult> GetCacheTR<T, TResult>();
        protected abstract class CacheT {
            public abstract void Clear();
        }
        protected abstract class CacheTBase<T> : CacheT {
            public readonly Dictionary<string, Func<T, object?>?> getFields = [];
            public readonly Dictionary<string, Action<T, object?>?> setFields = [];
            public readonly Dictionary<string, Func<T, object?>?> getProperties = [];
            public readonly Dictionary<string, Action<T, object?>?> setProperties = [];
            public readonly Dictionary<string, RefSetMemberDelegate<T>?> setFieldsN = [];
            public readonly Dictionary<string, RefSetMemberDelegate<T>?> setPropertiesN = [];
            public override void Clear() {
                getFields.Clear();
                setFields.Clear();
                getProperties.Clear();
                setProperties.Clear();
                setFieldsN.Clear();
                setPropertiesN.Clear();
            }
        }
        protected abstract class CacheR {
            public abstract void Clear();
        }
        protected abstract class CacheRBase<TResult> : CacheR {
            public readonly Dictionary<string, Func<object, TResult>?> getFields = [];
            public readonly Dictionary<string, Action<object, TResult>?> setFields = [];
            public readonly Dictionary<string, Func<object, TResult>?> getProperties = [];
            public readonly Dictionary<string, Action<object, TResult>?> setProperties = [];
            public sealed override void Clear() {
                getFields.Clear();
                setFields.Clear();
                getProperties.Clear();
                setProperties.Clear();
            }
        }
        public abstract class CacheTR {
            public abstract void Clear();
        }
        public abstract class CacheTRBase<T, TResult> : CacheTR {
            public readonly Dictionary<string, Func<T, TResult>?> getFields = [];
            public readonly Dictionary<string, Action<T, TResult>?> setFields = [];
            public readonly Dictionary<string, Func<T, TResult>?> getProperties = [];
            public readonly Dictionary<string, Action<T, TResult>?> setProperties = [];
            public readonly Dictionary<string, RefSetMemberDelegate<T, TResult>?> setFieldsN = [];
            public readonly Dictionary<string, RefSetMemberDelegate<T, TResult>?> setPropertiesN = [];
            public sealed override void Clear() {
                getFields.Clear();
                setFields.Clear();
                getProperties.Clear();
                setProperties.Clear();
                setFieldsN.Clear();
                setPropertiesN.Clear();
            }
        }
    }
    private class Cache<RealType> : Cache {
        private static Cache<RealType> Instance { get; } = new();
        static Cache() => All.Add(typeof(RealType), Instance);
        protected sealed override CacheTBase<T> GetCacheT<T>() => CacheT<T>.Instance;
        protected sealed override CacheRBase<T> GetCacheR<T>() => CacheR<T>.Instance;
        protected sealed override CacheTRBase<T, TResult> GetCacheTR<T, TResult>() => CacheT<T>.CacheTR<TResult>.Instance;
        private class CacheT<T> : CacheTBase<T> {
            protected readonly Dictionary<Type, CacheTR> AllTR = [];
            public sealed override void Clear() {
                base.Clear();
                foreach (var cacheTR in AllTR.Values) {
                    cacheTR.Clear();
                }
            }
            public static CacheT<T> Instance { get; } = new();
            static CacheT() => Cache<RealType>.Instance.AllT.Add(typeof(T), Instance);
            public class CacheTR<TResult> : CacheTRBase<T, TResult> {
                public static CacheTR<TResult> Instance { get; } = new();
                static CacheTR() => CacheT<T>.Instance.AllTR.Add(typeof(TResult), Instance);
            }
        }
        private class CacheR<TResult> : CacheRBase<TResult> {
            public static CacheR<TResult> Instance { get; } = new();
            static CacheR() => Cache<RealType>.Instance.AllR.Add(typeof(TResult), Instance);
        }
    }
    #endregion
    private static TResult ThrowOnFalse<TResult>(bool check, TResult result) {
        if (!check) {
            throw new Exception();
        }
        return result;
    }
    private static void ThrowOnFalse(bool check) {
        if (!check) {
            throw new Exception();
        }
    }
    private static bool DoReflection<TDelegate>(
        Func<(bool hasCache, TDelegate? cachedDelegate)> getCacheDelegate,
        Action<TDelegate> invokeDelegate,
        Func<TDelegate?> createDelegate,
        Action<TDelegate?> addCache) where TDelegate : Delegate {
        var (hasCache, cachedDelegate) = getCacheDelegate();
        if (hasCache) {
            if (cachedDelegate == null) {
                return false;
            }
            invokeDelegate(cachedDelegate);
            return true;
        }
        var @delegate = createDelegate();
        if (@delegate == null) {
            if (CacheNull) {
                addCache(null);
            }
            return false;
        }
        addCache(@delegate);
        invokeDelegate(@delegate);
        return true;
    }
    #region GetField, TryGetField
    public static object? GetField(this object self, string fieldName)
        => ThrowOnFalse(TryGetField(self, fieldName, out var field), field);
    public static object? GetField<T>(this T self, string fieldName) where T : notnull
        => ThrowOnFalse(TryGetField(self, fieldName, out var field), field);
    public static TField? GetField<TField>(this object self, string fieldName)
        => ThrowOnFalse(TryGetField(self, fieldName, out TField? field), field);
    public static TField? GetField<T, TField>(this T self, string fieldName) where T : notnull
        => ThrowOnFalse(TryGetField(self, fieldName, out TField? field), field);

    public static bool TryGetField(this object self, string fieldName, out object? field) {
        /*
        var type = self.GetType();
        if (UseCache && getFields.TryGetValue((type, fieldName), out var getField)) {
            if (getField == null) {
                field = null;
                return false;
            }
            field = getField(self);
            return true;
        }
        var fieldInfo = type.GetField(fieldName, BFI);
        if (fieldInfo == null) {
            if (UseCache && CacheNull) {
                getFields.Add((type, fieldName), null);
            }
            field = null;
            return false;
        }
        getField = GetGetFieldDelegate(type, fieldInfo);
        if (UseCache) {
            getFields.Add((type, fieldName), getField);
        }
        field = getField(self);
        return true;
        */
        var type = self.GetType();
        object? fieldInner = default;
        bool result = DoReflection(
            () => (Cache.TryGetGetField(type, fieldName, out var @delegate), @delegate),
            d => fieldInner = d(self),
            () => {
                var fieldInfo = type.GetField(fieldName, BFI);
                return fieldInfo == null ? null : GetGetFieldDelegate(type, fieldInfo);
            },
            Cache.ToAddGetField(type, fieldName));
        field = fieldInner;
        return result;
    }
    public static bool TryGetField<T>(this T self, string fieldName, out object? field) where T : notnull {
        var type = self.GetType();
        object? fieldInner = default;
        bool result = DoReflection(
            () => (Cache.TryGetGetField<T>(type, fieldName, out var @delegate), @delegate),
            d => fieldInner = d(self),
            () => {
                var fieldInfo = type.GetField(fieldName, BFI);
                return fieldInfo == null ? null : GetGetFieldDelegate<T>(fieldInfo);
            },
            Cache.ToAddGetField<T>(type, fieldName));
        field = fieldInner;
        return result;
    }
    public static bool TryGetField<TField>(this object self, string fieldName, out TField? field) {
        var type = self.GetType();
        TField? fieldInner = default;
        bool result = DoReflection(
            () => (Cache.TryGetGetFieldR<TField>(type, fieldName, out var @delegate), @delegate),
            d => fieldInner = d(self),
            () => {
                var fieldInfo = type.GetField(fieldName, BFI);
                return fieldInfo == null ? null : GetGetFieldDelegate<TField>(type, fieldInfo);
            },
            Cache.ToAddGetFieldR<TField>(type, fieldName));
        field = fieldInner;
        return result;
    }
    public static bool TryGetField<T, TField>(this T self, string fieldName, out TField? field) where T : notnull {
        var type = self.GetType();
        TField? fieldInner = default;
        bool result = DoReflection(
            () => (Cache.TryGetGetField<T, TField>(type, fieldName, out var @delegate), @delegate),
            d => fieldInner = d(self),
            () => {
                var fieldInfo = type.GetField(fieldName, BFI);
                return fieldInfo == null ? null : GetGetFieldDelegate<T, TField>(fieldInfo);
            },
            Cache.ToAddGetField<T, TField>(type, fieldName));
        field = fieldInner;
        return result;
    }
    #endregion
    #region SetField, TrySetField
    public static void SetField(this object self, string fieldName, object? field)
        => ThrowOnFalse(TrySetField(self, fieldName, field));
    public static void SetField<T>(this T self, string fieldName, object? field) where T : class
        => ThrowOnFalse(TrySetField(self, fieldName, field));
    public static void SetField<T>(ref this T self, string fieldName, object? field) where T : struct
        => ThrowOnFalse(TrySetField(ref self, fieldName, field));
    public static void SetField<TResult>(this object self, string fieldName, TResult field)
        => ThrowOnFalse(TrySetField(self, fieldName, field));
    public static void SetField<T, TResult>(this T self, string fieldName, TResult field) where T : class
        => ThrowOnFalse(TrySetField(self, fieldName, field));
    public static void SetField<T, TResult>(ref this T self, string fieldName, TResult field) where T : struct
        => ThrowOnFalse(TrySetField(ref self, fieldName, field));

    public static bool TrySetField(this object self, string fieldName, object? field) {
        var type = self.GetType();
        bool result = DoReflection(
            () => (Cache.TryGetSetField(type, fieldName, out var @delegate), @delegate),
            d => d(self, field),
            () => {
                var fieldInfo = type.GetField(fieldName, BFI);
                return fieldInfo == null ? null : GetSetFieldDelegate(type, fieldInfo);
            },
            Cache.ToAddSetField(type, fieldName));
        return result;
    }
    public static bool TrySetField<T>(this T self, string fieldName, object? field) where T : class {
        var type = self.GetType();
        bool result = DoReflection(
            () => (Cache.TryGetSetField<T>(type, fieldName, out var @delegate), @delegate),
            d => d(self, field),
            () => {
                var fieldInfo = type.GetField(fieldName, BFI);
                return fieldInfo == null ? null : GetSetFieldDelegate<T>(fieldInfo);
            },
            Cache.ToAddSetField<T>(type, fieldName));
        return result;
    }
    public static bool TrySetField<T>(ref this T self, string fieldName, object? field) where T : struct {
        var type = self.GetType();
        // bool result = DoReflection(
        //     () => (Cache.TryGetSetFieldN<T>(type, fieldName, out var @delegate), @delegate),
        //     d => d(ref self, field),
        //     () => {
        //         var fieldInfo = type.GetField(fieldName, BFI);
        //         return fieldInfo == null ? null : GetSetFieldDelegateN<T>(fieldInfo);
        //     },
        //     Cache.ToAddSetFieldN<T>(type, fieldName));
        // return result;
        if (Cache.TryGetSetFieldN<T>(type, fieldName, out var @delegate)) {
            if (@delegate == null) {
                return false;
            }
            goto InvokeDelegate;
        }
        var fieldInfo = type.GetField(fieldName, BFI);
        @delegate = fieldInfo == null ? null : GetSetFieldDelegateN<T>(fieldInfo);
        if (@delegate == null) {
            if (CacheNull) {
                Cache.AddSetFieldN(type, fieldName, @delegate);
            }
            return false;
        }
        Cache.AddSetFieldN(type, fieldName, @delegate);
    InvokeDelegate:
        @delegate(ref self, field);
        return true;
    }
    public static bool TrySetField<TResult>(this object self, string fieldName, TResult field) {
        var type = self.GetType();
        bool result = DoReflection(
            () => (Cache.TryGetSetFieldR<TResult>(type, fieldName, out var @delegate), @delegate),
            d => d(self, field),
            () => {
                var fieldInfo = type.GetField(fieldName, BFI);
                return fieldInfo == null ? null : GetSetFieldDelegate<TResult>(type, fieldInfo);
            },
            Cache.ToAddSetFieldR<TResult>(type, fieldName));
        return result;
    }
    public static bool TrySetField<T, TResult>(this T self, string fieldName, TResult field) where T : class {
        var type = self.GetType();
        bool result = DoReflection(
            () => (Cache.TryGetSetField<T, TResult>(type, fieldName, out var @delegate), @delegate),
            d => d(self, field),
            () => {
                var fieldInfo = type.GetField(fieldName, BFI);
                return fieldInfo == null ? null : GetSetFieldDelegate<T, TResult>(fieldInfo);
            },
            Cache.ToAddSetField<T, TResult>(type, fieldName));
        return result;
    }
    public static bool TrySetField<T, TResult>(ref this T self, string fieldName, TResult field) where T : struct {
        var type = self.GetType();
        // bool result = DoReflection(
        //     () => (Cache.TryGetSetFieldN<T, TResult>(type, fieldName, out var @delegate), @delegate),
        //     d => d(ref self, field),
        //     () => {
        //         var fieldInfo = type.GetField(fieldName, BFI);
        //         return fieldInfo == null ? null : GetSetFieldDelegateN<T, TResult>(fieldInfo);
        //     },
        //     Cache.ToAddSetFieldN<T, TResult>(type, fieldName));
        // return result;
        if (Cache.TryGetSetFieldN<T, TResult>(type, fieldName, out var @delegate)) {
            if (@delegate == null) {
                return false;
            }
            goto InvokeDelegate;
        }
        var fieldInfo = type.GetField(fieldName, BFI);
        @delegate = fieldInfo == null ? null : GetSetFieldDelegateN<T, TResult>(fieldInfo);
        if (@delegate == null) {
            if (CacheNull) {
                Cache.AddSetFieldN(type, fieldName, @delegate);
            }
            return false;
        }
        Cache.AddSetFieldN(type, fieldName, @delegate);
    InvokeDelegate:
        @delegate(ref self, field);
        return true;
    }
    #endregion
    #region GetProperty, TryGetProperty
    public static object? GetProperty(this object self, string propertyName)
        => ThrowOnFalse(TryGetProperty(self, propertyName, out var property), property);
    public static object? GetProperty<T>(this T self, string propertyName) where T : notnull
        => ThrowOnFalse(TryGetProperty(self, propertyName, out var property), property);
    public static TProperty? GetProperty<TProperty>(this object self, string propertyName)
        => ThrowOnFalse(TryGetProperty(self, propertyName, out TProperty? property), property);
    public static TProperty? GetProperty<T, TProperty>(this T self, string propertyName) where T : notnull
        => ThrowOnFalse(TryGetProperty(self, propertyName, out TProperty? property), property);

    public static bool TryGetProperty(this object self, string propertyName, out object? property) {
        /*
        var type = self.GetType();
        if (UseCache && getProperties.TryGetValue((type, propertyName), out var getProperty)) {
            if (getProperty == null) {
                property = null;
                return false;
            }
            property = getProperty(self);
            return true;
        }
        var propertyInfo = type.GetProperty(propertyName, BFI);
        if (propertyInfo == null) {
            if (UseCache && CacheNull) {
                getProperties.Add((type, propertyName), null);
            }
            property = null;
            return false;
        }
        getProperty = GetGetPropertyDelegate(type, propertyInfo);
        if (UseCache) {
            getProperties.Add((type, propertyName), getProperty);
        }
        property = getProperty(self);
        return true;
        */
        var type = self.GetType();
        object? propertyInner = default;
        bool result = DoReflection(
            () => (Cache.TryGetGetProperty(type, propertyName, out var @delegate), @delegate),
            d => propertyInner = d(self),
            () => {
                var propertyInfo = type.GetProperty(propertyName, BFI);
                return propertyInfo == null ? null : GetGetPropertyDelegate(type, propertyInfo);
            },
            Cache.ToAddGetProperty(type, propertyName));
        property = propertyInner;
        return result;
    }
    public static bool TryGetProperty<T>(this T self, string propertyName, out object? property) where T : notnull {
        var type = self.GetType();
        object? propertyInner = default;
        bool result = DoReflection(
            () => (Cache.TryGetGetProperty<T>(type, propertyName, out var @delegate), @delegate),
            d => propertyInner = d(self),
            () => {
                var propertyInfo = type.GetProperty(propertyName, BFI);
                return propertyInfo == null ? null : GetGetPropertyDelegate<T>(propertyInfo);
            },
            Cache.ToAddGetProperty<T>(type, propertyName));
        property = propertyInner;
        return result;
    }
    public static bool TryGetProperty<TProperty>(this object self, string propertyName, out TProperty? property) {
        var type = self.GetType();
        TProperty? propertyInner = default;
        bool result = DoReflection(
            () => (Cache.TryGetGetPropertyR<TProperty>(type, propertyName, out var @delegate), @delegate),
            d => propertyInner = d(self),
            () => {
                var propertyInfo = type.GetProperty(propertyName, BFI);
                return propertyInfo == null ? null : GetGetPropertyDelegate<TProperty>(type, propertyInfo);
            },
            Cache.ToAddGetPropertyR<TProperty>(type, propertyName));
        property = propertyInner;
        return result;
    }
    public static bool TryGetProperty<T, TProperty>(this T self, string propertyName, out TProperty? property) where T : notnull {
        var type = self.GetType();
        TProperty? propertyInner = default;
        bool result = DoReflection(
            () => (Cache.TryGetGetProperty<T, TProperty>(type, propertyName, out var @delegate), @delegate),
            d => propertyInner = d(self),
            () => {
                var propertyInfo = type.GetProperty(propertyName, BFI);
                return propertyInfo == null ? null : GetGetPropertyDelegate<T, TProperty>(propertyInfo);
            },
            Cache.ToAddGetProperty<T, TProperty>(type, propertyName));
        property = propertyInner;
        return result;
    }
    #endregion
    #region SetProperty, TrySetProperty
    public static void SetProperty(this object self, string propertyName, object? property)
        => ThrowOnFalse(TrySetProperty(self, propertyName, property));
    public static void SetProperty<T>(this T self, string propertyName, object? property) where T : class
        => ThrowOnFalse(TrySetProperty(self, propertyName, property));
    public static void SetProperty<T>(ref this T self, string propertyName, object? property) where T : struct
        => ThrowOnFalse(TrySetProperty(ref self, propertyName, property));
    public static void SetProperty<TResult>(this object self, string propertyName, TResult property)
        => ThrowOnFalse(TrySetProperty(self, propertyName, property));
    public static void SetProperty<T, TResult>(this T self, string propertyName, TResult property) where T : class
        => ThrowOnFalse(TrySetProperty(self, propertyName, property));
    public static void SetProperty<T, TResult>(ref this T self, string propertyName, TResult property) where T : struct
        => ThrowOnFalse(TrySetProperty(ref self, propertyName, property));

    public static bool TrySetProperty(this object self, string propertyName, object? property) {
        var type = self.GetType();
        bool result = DoReflection(
            () => (Cache.TryGetSetProperty(type, propertyName, out var @delegate), @delegate),
            d => d(self, property),
            () => {
                var propertyInfo = type.GetProperty(propertyName, BFI);
                return propertyInfo == null ? null : GetSetPropertyDelegate(type, propertyInfo);
            },
            Cache.ToAddSetProperty(type, propertyName));
        return result;
    }
    public static bool TrySetProperty<T>(this T self, string propertyName, object? property) where T : class {
        var type = self.GetType();
        bool result = DoReflection(
            () => (Cache.TryGetSetProperty<T>(type, propertyName, out var @delegate), @delegate),
            d => d(self, property),
            () => {
                var propertyInfo = type.GetProperty(propertyName, BFI);
                return propertyInfo == null ? null : GetSetPropertyDelegate<T>(propertyInfo);
            },
            Cache.ToAddSetProperty<T>(type, propertyName));
        return result;
    }
    public static bool TrySetProperty<T>(ref this T self, string propertyName, object? property) where T : struct {
        var type = self.GetType();
        // bool result = DoReflection(
        //     () => (Cache.TryGetSetPropertyN<T>(type, propertyName, out var @delegate), @delegate),
        //     d => d(ref self, property),
        //     () => {
        //         var propertyInfo = type.GetProperty(propertyName, BFI);
        //         return propertyInfo == null ? null : GetSetPropertyDelegateN<T>(propertyInfo);
        //     },
        //     Cache.ToAddSetPropertyN<T>(type, propertyName));
        // return result;
        if (Cache.TryGetSetPropertyN<T>(type, propertyName, out var @delegate)) {
            if (@delegate == null) {
                return false;
            }
            goto InvokeDelegate;
        }
        var propertyInfo = type.GetProperty(propertyName, BFI);
        @delegate = propertyInfo == null ? null : GetSetPropertyDelegateN<T>(propertyInfo);
        if (@delegate == null) {
            if (CacheNull) {
                Cache.AddSetPropertyN(type, propertyName, @delegate);
            }
            return false;
        }
        Cache.AddSetPropertyN(type, propertyName, @delegate);
    InvokeDelegate:
        @delegate(ref self, property);
        return true;
    }
    public static bool TrySetProperty<TResult>(this object self, string propertyName, TResult property) {
        var type = self.GetType();
        bool result = DoReflection(
            () => (Cache.TryGetSetPropertyR<TResult>(type, propertyName, out var @delegate), @delegate),
            d => d(self, property),
            () => {
                var propertyInfo = type.GetProperty(propertyName, BFI);
                return propertyInfo == null ? null : GetSetPropertyDelegate<TResult>(type, propertyInfo);
            },
            Cache.ToAddSetPropertyR<TResult>(type, propertyName));
        return result;
    }
    public static bool TrySetProperty<T, TResult>(this T self, string propertyName, TResult property) where T : class {
        var type = self.GetType();
        bool result = DoReflection(
            () => (Cache.TryGetSetProperty<T, TResult>(type, propertyName, out var @delegate), @delegate),
            d => d(self, property),
            () => {
                var propertyInfo = type.GetProperty(propertyName, BFI);
                return propertyInfo == null ? null : GetSetPropertyDelegate<T, TResult>(propertyInfo);
            },
            Cache.ToAddSetProperty<T, TResult>(type, propertyName));
        return result;
    }
    public static bool TrySetProperty<T, TResult>(ref this T self, string propertyName, TResult property) where T : struct {
        var type = self.GetType();
        // bool result = DoReflection(
        //     () => (Cache.TryGetSetPropertyN<T, TResult>(type, propertyName, out var @delegate), @delegate),
        //     d => d(ref self, property),
        //     () => {
        //         var propertyInfo = type.GetProperty(propertyName, BFI);
        //         return propertyInfo == null ? null : GetSetPropertyDelegateN<T, TResult>(propertyInfo);
        //     },
        //     Cache.ToAddSetPropertyN<T, TResult>(type, propertyName));
        // return result;
        if (Cache.TryGetSetPropertyN<T, TResult>(type, propertyName, out var @delegate)) {
            if (@delegate == null) {
                return false;
            }
            goto InvokeDelegate;
        }
        var propertyInfo = type.GetProperty(propertyName, BFI);
        @delegate = propertyInfo == null ? null : GetSetPropertyDelegateN<T, TResult>(propertyInfo);
        if (@delegate == null) {
            if (CacheNull) {
                Cache.AddSetPropertyN(type, propertyName, @delegate);
            }
            return false;
        }
        Cache.AddSetPropertyN(type, propertyName, @delegate);
    InvokeDelegate:
        @delegate(ref self, property);
        return true;
    }
    #endregion
    #region InvokeMethod
    public static object? InvokeMethod(this object self, string methodName, params object?[] args)
        => ThrowOnFalse(TryInvokeMethod(self, methodName, out var result, args), result);
    public static TResult? InvokeMethod<TResult>(this object self, string methodName, params object?[] args)
        => ThrowOnFalse(TryInvokeMethod(self, methodName, out TResult? result, args), result);
    public static object? InvokeMethodN<T>(ref this T self, string methodName, params object?[] args) where T : struct
        => ThrowOnFalse(TryInvokeMethodN(ref self, methodName, out var result, args), result);

    public static bool TryInvokeMethod(this object self, string methodName, out object? result, params object?[] args) {
        var type = self.GetType();
        object? resultInner = null;
        bool res = DoReflection(
            () => (Cache.TryGetInvokeMethods(type, methodName, out var @delegate), @delegate),
            d => resultInner = d(self, args),
            () => {
                var methodInfo = type.GetMethod(methodName, BFI);
                return methodInfo?.GetFastInvoker();
            },
            Cache.ToAddInvokeMethods(type, methodName));
        result = resultInner;
        return res;
    }
    public static bool TryInvokeMethod(this object self, string methodName, params object?[] args) => TryInvokeMethod(self, methodName, out _, args);
    public static bool TryInvokeMethod<TResult>(this object self, string methodName, out TResult? result, params object?[] args) {
        if (!TryInvokeMethod(self, methodName, out var res, args)) {
            result = default;
            return false;
        }
        result = (TResult?)res;
        return true;
    }
    public static bool TryInvokeMethodN<T>(ref this T self, string methodName, out object? result, params object?[] args) where T : struct {
        var type = self.GetType();
        // object? resultInner = null;
        // bool res = DoReflection(
        //     () => (Cache.TryGetInvokeMethodsN(type, methodName, out var @delegate), @delegate),
        //     d => resultInner = d(ref self, args),
        //     () => {
        //         var methodInfo = type.GetMethod(methodName, BFI);
        //         return methodInfo == null ? null : FastReflectionHelper.GetFastStructInvoker(methodInfo);
        //     },
        //     Cache.ToAddInvokeMethodsN(type, methodName));
        // result = resultInner;
        // return res;
        if (Cache.TryGetInvokeMethods(type, methodName, out var @delegate)) {
            if (@delegate == null) {
                result = null;
                return false;
            }
            goto InvokeDelegate;
        }
        var methodInfo = type.GetMethod(methodName, BFI);
        @delegate = methodInfo?.GetFastInvoker();
        if (@delegate == null) {
            if (CacheNull) {
                Cache.AddInvokeMethods(type, methodName, @delegate);
            }
            result = null;
            return false;
        }
        Cache.AddInvokeMethods(type, methodName, @delegate);
    InvokeDelegate:
        object selfObj = self;
        result = @delegate(selfObj, args);
        self = (T)selfObj;
        return true;
    }
    public static bool TryInvokeMethodN<T>(ref this T self, string methodName, params object?[] args) where T : struct => TryInvokeMethodN(ref self, methodName, out _, args);
    public static bool TryInvokeMethodN<T, TResult>(ref this T self, string methodName, out TResult? result, params object?[] args) where T : struct {
        if (!TryInvokeMethodN(ref self, methodName, out var res, args)) {
            result = default;
            return false;
        }
        result = (TResult?)res;
        return true;
    }
    #endregion
}
