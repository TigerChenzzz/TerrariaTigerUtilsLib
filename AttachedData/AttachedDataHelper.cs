using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TigerUtilsLib.AttachedData;

/// <summary>
/// 实际上就是对 <see cref="ConditionalWeakTable{TKey, TValue}"/> 的封装
/// </summary>
public static class AttachedDataHelper {
    private static class AttachedDataEntry<TAttached> where TAttached : class {
        static AttachedDataEntry() => InvokeStaticConstructor<TAttached>();
        private static readonly ConditionalWeakTable<object, TAttached> table = [];
        private static Func<object, TAttached?>? initializeFunc;
        public static TAttached Get(object self) {
            if (table.TryGetValue(self, out var attached)) {
                return attached;
            }
            var result = (initializeFunc == null ? Activator.CreateInstance<TAttached>() : initializeFunc(self))
                ?? throw new Exception($"Attached type {typeof(TAttached)} is not allowed to be attached to the object");
            table.Add(self, result);
            return result;
        }
        public static bool TryGet(object self, [NotNullWhen(true)] out TAttached? attached) => table.TryGetValue(self, out attached);
        public static void Set(object self, TAttached? attached) {
            if (attached != null) {
                table.AddOrUpdate(self, attached);
            }
            else {
                table.Remove(self);
            }
        }
        public static void Add(object self, TAttached attached) => table.Add(self, attached);
        public static bool TryAdd(object self, TAttached attached) => table.TryAdd(self, attached);
        public static void AddOrUpdate(object self, TAttached attached) => table.AddOrUpdate(self, attached);
        public static bool Remove(object self) => table.Remove(self);
        public static void Clear() => table.Clear();
        public static void SetInitializeFunction(Func<object, TAttached?>? func) => initializeFunc = func;
    }
    /// <summary>
    /// <br/>获取 <paramref name="self"/> 所关联的 <typeparamref name="TAttached"/> 数据
    /// <br/>如果没有则会创建一个, 如果不能关联则会报错
    /// <br/><paramref name="self"/> 需要为引用类型
    /// </summary>
    public static TAttached GetAttachedData<TAttached>(this object self) where TAttached : class => AttachedDataEntry<TAttached>.Get(self);
    /// <summary>
    /// <br/>获取 <paramref name="self"/> 所关联的 <typeparamref name="TAttached"/> 数据, 并通过 out 参数 <paramref name="attached"/> 传出
    /// <br/>如果没有则会创建一个, 如果不能关联则会报错
    /// <br/><paramref name="self"/> 需要为引用类型
    /// </summary>
    public static TAttached GetAttachedData<TAttached>(this object self, out TAttached attached) where TAttached : class => attached = AttachedDataEntry<TAttached>.Get(self);
    /// <summary>
    /// <br/>尝试获取 <paramref name="self"/> 所关联的 <typeparamref name="TAttached"/> 数据
    /// <br/>如果没有则会返回 <see langword="false"/>
    /// <br/><paramref name="self"/> 需要为引用类型
    /// </summary>
    public static bool TryGetAttachedData<TAttached>(this object self, [NotNullWhen(true)] out TAttached? attached) where TAttached : class => AttachedDataEntry<TAttached>.TryGet(self, out attached);
    /// <summary>
    /// <br/>返回 <paramref name="self"/> 是否已经有所关联的 <typeparamref name="TAttached"/> 数据
    /// <br/><paramref name="self"/> 需要为引用类型
    /// </summary>
    public static bool HasAttachedData<TAttached>(this object self) where TAttached : class => AttachedDataEntry<TAttached>.TryGet(self, out _);
    /// <summary>
    /// <br/>设置 <paramref name="self"/> 所关联的 <typeparamref name="TAttached"/> 数据为 <paramref name="attached"/>
    /// <br/>如果 <paramref name="attached"/> 为 <see langword="null"/> 则会移除所关联的 <typeparamref name="TAttached"/> 数据
    /// <br/><paramref name="self"/> 需要为引用类型
    /// </summary>
    public static void SetAttachedData<TAttached>(this object self, TAttached? attached) where TAttached : class => AttachedDataEntry<TAttached>.Set(self, attached);
    /// <summary>
    /// <br/>将 <paramref name="attached"/> 关联到 <paramref name="self"/>
    /// <br/>如果已经有关联的数据则会报错
    /// <br/><paramref name="self"/> 需要为引用类型
    /// </summary>
    public static void AddAttachedData<TAttached>(this object self, TAttached attached) where TAttached : class => AttachedDataEntry<TAttached>.Add(self, attached);
    /// <summary>
    /// <br/>尝试将 <paramref name="attached"/> 关联到 <paramref name="self"/>
    /// <br/>如果已经有关联的数据则会返回 <see langword="false"/>
    /// <br/><paramref name="self"/> 需要为引用类型
    /// </summary>
    public static bool TryAddAttachedData<TAttached>(this object self, TAttached attached) where TAttached : class => AttachedDataEntry<TAttached>.TryAdd(self, attached);
    /// <summary>
    /// <br/>将 <paramref name="attached"/> 关联到 <paramref name="self"/>
    /// <br/>如果已经有关联的数据则会替换掉原来的
    /// <br/><paramref name="self"/> 需要为引用类型
    /// </summary>
    public static void AddOrUpdateAttachedData<TAttached>(this object self, TAttached attached) where TAttached : class => AttachedDataEntry<TAttached>.AddOrUpdate(self, attached);
    /// <summary>
    /// <br/>移除 <paramref name="self"/> 所关联的 <typeparamref name="TAttached"/> 数据
    /// <br/>返回原来是否有关联的数据
    /// <br/><paramref name="self"/> 需要为引用类型
    /// </summary>
    public static bool RemoveAttachedData<TAttached>(this object self) where TAttached : class => AttachedDataEntry<TAttached>.Remove(self);
    /// <summary>
    /// <br/>清除所有 <typeparamref name="TAttached"/> 的关联数据
    /// </summary>
    public static void ClearAttachedData<TAttached>() where TAttached : class => AttachedDataEntry<TAttached>.Clear();
    /// <summary>
    /// <br/>设置创建 <typeparamref name="TAttached"/> 实例所用的方法
    /// <br/>默认为 <see langword="null"/>, 表示使用公开无参构造
    /// <para/>以 <paramref name="func"/> 返回 <see langword="null"/> 表示不能关联,
    /// <br/>这样在使用 <see cref="GetAttachedData{TAttached}(object)"/> 获取还未关联的数据时则会报错,
    /// <br/>但无法影响 <see cref="AddAttachedData{TAttached}(object, TAttached)"/> 等直接添加关联数据的方法
    /// <para/>可以放在 <typeparamref name="TAttached"/> 的静态构造中
    /// </summary>
    public static void SetAttachedDataInitializeFunction<TAttached>(Func<object, TAttached?>? func) where TAttached : class => AttachedDataEntry<TAttached>.SetInitializeFunction(func);
    /// <summary>
    /// <br/>设置创建 <typeparamref name="TAttached"/> 实例所用的方法
    /// <br/>如果 <paramref name="func"/> 为 <see langword="null"/>, 则表示使用公开无参构造
    /// <br/>以 <paramref name="func"/> 返回 <see langword="null"/> 表示不能关联,
    /// <br/>这样在使用 <see cref="GetAttachedData{TAttached}(object)"/> 获取还未关联的数据时则会报错,
    /// <br/>但无法影响 <see cref="AddAttachedData{TAttached}(object, TAttached)"/> 等直接添加关联数据的方法;
    /// <br/>并额外限制其所关联的类型为 <typeparamref name="T"/> (即使传入的 <paramref name="func"/> 为 <see langword="null"/> 也是如此)
    /// <para/>可以放在 <typeparamref name="TAttached"/> 的静态构造中
    /// </summary>
    /// <param name="strictTypeCheck">是否严格限制所关联的类型为 <typeparamref name="T"/>, 否则只要可以用 <see langword="as"/> 转换为 <typeparamref name="T"/> 的都允许关联</param>
    public static void SetAttachedDataInitializeFunction<T, TAttached>(Func<T, TAttached?>? func, bool strictTypeCheck = false) where T : class where TAttached : class {
        func ??= _ => Activator.CreateInstance<TAttached>();
        if (strictTypeCheck) {
            SetAttachedDataInitializeFunction(o => o.GetType() != typeof(T) ? null : func.Invoke((T)o));
            return;
        }
        SetAttachedDataInitializeFunction(o => o is not T t ? null : func.Invoke(t));
    }
    #region AutoAttached
    private static class AutoAttachedEntry<T, TAttached> where T : class where TAttached : class {
        public static ILHook[]? Hooks { get; set; }
        public static bool Attached {
            get => Hooks != null && Hooks.Length > 0 && Hooks[0].IsApplied;
            set {
                if (!value) {
                    if (Hooks == null || Hooks.Length == 0) {
                        return;
                    }
                    if (!Hooks[0].IsApplied) {
                        return;
                    }
                    foreach (var hook in Hooks) {
                        hook.Undo();
                    }
                    return;
                }
                if (Hooks == null) {
                    Hooks = [.. typeof(T).GetConstructors(BFI).Select(c => new ILHook(c, il => {
                        ILCursor cursor = new(il) { Next = il.Instrs[^1] };
                        cursor.MoveAfterLabels();
                        cursor.EmitLdarg0();
                        cursor.EmitDelegate(CreateAttachedData);
                    }))];
                    return;
                }
                if (Hooks.Length == 0 || Hooks[0].IsApplied) {
                    return;
                }
                foreach (var hook in Hooks) {
                    hook.Apply();
                }
            }
        }
        public static void Clear() {
            if (Hooks == null) {
                return;
            }
            foreach (var hook in Hooks) {
                hook.Undo();
                hook.Dispose();
            }
            Hooks = null;
        }
        private static void CreateAttachedData(T self) => self.GetAttachedData<TAttached>();
    }
    /// <summary>
    /// <br/>设置是否在 <typeparamref name="T"/> 的构造后立即创建关联的 <typeparamref name="TAttached"/> 数据, 默认为否
    /// <br/>对于在此句之前就构造好了的实例不起作用
    /// </summary>
    public static void SetAutoAttached<T, TAttached>(bool attached = true) where T : class where TAttached : class => AutoAttachedEntry<T, TAttached>.Attached = attached;
    /// <summary>
    /// 获取是否在 <typeparamref name="T"/> 的构造后立即创建关联的 <typeparamref name="TAttached"/> 数据, 默认为否
    /// </summary>
    public static bool GetAutoAttached<T, TAttached>() where T : class where TAttached : class => AutoAttachedEntry<T, TAttached>.Attached;
    /// <summary>
    /// 清除并释放自动关联所产生的钩子, 会取消自动关联
    /// </summary>
    public static void ClearAutoAttachedHooks<T, TAttached>() where T : class where TAttached : class => AutoAttachedEntry<T, TAttached>.Clear();
    #endregion
}
