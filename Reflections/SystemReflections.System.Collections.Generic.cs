#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using SOpCodes = System.Reflection.Emit.OpCodes;

namespace TigerUtilsLib.Reflections {
    public static partial class SystemReflections {
        public static partial class System {
            public static partial class Collections {
                public static partial class Generic {
                    #region ArraySortHelper
                    public static class ArraySortHelper {
                        public static Type TypeG1 { get; } = Type.GetType("System.Collections.Generic.ArraySortHelper`1", true) ?? throw new NullReferenceException();
                        public static Type TypeG2 { get; } = Type.GetType("System.Collections.Generic.ArraySortHelper`2", true) ?? throw new NullReferenceException();
                        #region BinarySearch
                        public static int BinarySearch<T>(T[] array, int index, int length, T value, IComparer<T>? comparer = null)
                            => ArraySortHelper<T>.BinarySearch(array, index, length, value, comparer);
                        #endregion
                        #region Sort
                        public static void Sort<T>(Span<T> key, IComparer<T>? comparer = null)
                            => ArraySortHelper<T>.Sort(key, comparer);
                        public static void Sort<T>(Span<T> key, Comparison<T> comparer)
                            => ArraySortHelper<T>.Sort(key, comparer);


                        public static void Sort<TKey, TValue>(Span<TKey> keys, Span<TValue> values, IComparer<TKey>? comparer = null)
                            => ArraySortHelper<TKey, TValue>.Sort(keys, values, comparer);
                        public static void Sort<TKey, TValue>(Span<TKey> keys, Span<TValue> values, Comparison<TKey> comparer)
                            => ArraySortHelper<TKey, TValue>.Sort(keys, values, comparer);
                        #endregion
                    }
                    public static class ArraySortHelper<T> {
                        public static Type Type { get; } = ArraySortHelper.TypeG1.MakeGenericType(typeof(T));
                        #region Default
                        public static FieldInfo Field_Default { get; } = Type.GetField("Default", BFS) ?? throw new NullReferenceException();
                        #endregion
                        #region BinarySearch
                        private delegate int Delegate_BinarySearch(T[] array, int index, int length, T value, IComparer<T>? comparer);
                        public static MethodInfo Method_BinarySearch { get; } = Type.GetMethod("BinarySearch", BFI) ?? throw new NullReferenceException();
                        private static readonly Delegate_BinarySearch binarySearch = CreateMethod<Delegate_BinarySearch>("BinarySearch",
                            (SOpCodes.Ldsfld, Field_Default),
                            (SOpCodes.Ldarg_0, null),
                            (SOpCodes.Ldarg_1, null),
                            (SOpCodes.Ldarg_2, null),
                            (SOpCodes.Ldarg_3, null),
                            (SOpCodes.Ldarg, 4),
                            (SOpCodes.Call, Method_BinarySearch),
                            (SOpCodes.Ret, null)
                        );
                        public static int BinarySearch(T[] array, int index, int length, T value, IComparer<T>? comparer = null) => binarySearch(array, index, length, value, comparer);
                        #endregion
                        #region Sort
                        private delegate void Delegate_SortWithIComparer(Span<T> key, IComparer<T>? comparer);
                        private delegate void Delegate_SortWithComparison(Span<T> key, Comparison<T> comparer);
                        public static MethodInfo Method_Sort_IComparer { get; } = Type.GetMethod("Sort", BFI, [typeof(Span<T>), typeof(IComparer<T>)]) ?? throw new NullReferenceException();
                        public static MethodInfo Method_Sort_Comparison { get; } = Type.GetMethod("Sort", BFI, [typeof(Span<T>), typeof(Comparison<T>)]) ?? throw new NullReferenceException();
                        private static readonly Delegate_SortWithIComparer sortWithIComparer = CreateMethod<Delegate_SortWithIComparer>("Sort",
                            (SOpCodes.Ldsfld, Field_Default),
                            (SOpCodes.Ldarg_0, null),
                            (SOpCodes.Ldarg_1, null),
                            (SOpCodes.Call, Method_Sort_IComparer),
                            (SOpCodes.Ret, null)
                        );
                        private static readonly Delegate_SortWithComparison sortWithComparison = CreateMethod<Delegate_SortWithComparison>("Sort",
                            (SOpCodes.Ldsfld, Field_Default),
                            (SOpCodes.Ldarg_0, null),
                            (SOpCodes.Ldarg_1, null),
                            (SOpCodes.Call, Method_Sort_Comparison),
                            (SOpCodes.Ret, null)
                        );
                        public static void Sort(Span<T> key, IComparer<T>? comparer = null) => sortWithIComparer(key, comparer);
                        public static void Sort(Span<T> key, Comparison<T> comparer) => sortWithComparison(key, comparer);
                        #endregion
                    }
                    public static class ArraySortHelper<TKey, TValue> {
                        public static Type Type { get; } = ArraySortHelper.TypeG2.MakeGenericType(typeof(TKey), typeof(TValue));
                        #region Default
                        public static FieldInfo Field_Default { get; } = Type.GetField("Default", BFS) ?? throw new NullReferenceException();
                        #endregion
                        #region Sort
                        private delegate void Delegate_SortWithIComparer(Span<TKey> keys, Span<TValue> values, IComparer<TKey>? comparer);
                        private delegate void Delegate_SortWithComparison(Span<TKey> keys, Span<TValue> values, Comparison<TKey> comparer);
                        public static MethodInfo Method_Sort_IComparer { get; } = Type.GetMethod("Sort", BFI, [typeof(Span<TKey>), typeof(Span<TValue>), typeof(IComparer<TKey>)]) ?? throw new NullReferenceException();
                        public static MethodInfo Method_Sort_Comparison { get; } = Type.GetMethod("Sort", BFI, [typeof(Span<TKey>), typeof(Span<TValue>), typeof(Comparison<TKey>)]) ?? throw new NullReferenceException();
                        private static readonly Delegate_SortWithIComparer sortWithIComparer = CreateMethod<Delegate_SortWithIComparer>("Sort",
                            (SOpCodes.Ldsfld, Field_Default),
                            (SOpCodes.Ldarg_0, null),
                            (SOpCodes.Ldarg_1, null),
                            (SOpCodes.Ldarg_2, null),
                            (SOpCodes.Call, Method_Sort_IComparer),
                            (SOpCodes.Ret, null)
                        );
                        private static readonly Delegate_SortWithComparison sortWithComparison = CreateMethod<Delegate_SortWithComparison>("Sort",
                            (SOpCodes.Ldsfld, Field_Default),
                            (SOpCodes.Ldarg_0, null),
                            (SOpCodes.Ldarg_1, null),
                            (SOpCodes.Ldarg_2, null),
                            (SOpCodes.Call, Method_Sort_Comparison),
                            (SOpCodes.Ret, null)
                        );
                        public static void Sort(Span<TKey> keys, Span<TValue> values, IComparer<TKey>? comparer = null) => sortWithIComparer(keys, values, comparer);
                        public static void Sort(Span<TKey> keys, Span<TValue> values, Comparison<TKey> comparer) => sortWithComparison(keys, values, comparer);
                        #endregion
                    }
                    #endregion
                    #region List
                    public static class List {
                        public static Type TypeG1 { get; } = typeof(global::System.Collections.Generic.List<>);
                        #region _items
                        public static T[] GetItems<T>(global::System.Collections.Generic.List<T> self) => List<T>.GetItems(self);
                        public static void SetItems<T>(global::System.Collections.Generic.List<T> self, T[] value) => List<T>.SetItems(self, value);
                        public static T[] SetItemsL<T>(global::System.Collections.Generic.List<T> self, T[] value) => List<T>.SetItemsL(self, value);
                        #endregion
                        #region _size
                        public static int GetSize<T>(global::System.Collections.Generic.List<T> self) => List<T>.GetSize(self);
                        public static void SetSize<T>(global::System.Collections.Generic.List<T> self, int value) => List<T>.SetSize(self, value);
                        public static int SetSizeL<T>(global::System.Collections.Generic.List<T> self, int value) => List<T>.SetSizeL(self, value);
                        #endregion
                        #region _version
                        public static int GetVersion<T>(global::System.Collections.Generic.List<T> self) => List<T>.GetVersion(self);
                        public static void SetVersion<T>(global::System.Collections.Generic.List<T> self, int value) => List<T>.SetVersion(self, value);
                        public static int SetVersionL<T>(global::System.Collections.Generic.List<T> self, int value) => List<T>.SetVersionL(self, value);
                        #endregion
                    }
                    public static class List<T> {
                        public static Type Type { get; } = typeof(global::System.Collections.Generic.List<T>);
                        #region _items
                        public static FieldInfo Field_Items { get; } = Type.GetField("_items", BFI) ?? throw new NullReferenceException();
                        private static readonly Func<global::System.Collections.Generic.List<T>, T[]> getItems = GetGetFieldDelegate<global::System.Collections.Generic.List<T>, T[]>(Field_Items);
                        private static readonly Action<global::System.Collections.Generic.List<T>, T[]> setItems = GetSetFieldDelegate<global::System.Collections.Generic.List<T>, T[]>(Field_Items);
                        public static T[] GetItems(global::System.Collections.Generic.List<T> self) => getItems(self);
                        public static void SetItems(global::System.Collections.Generic.List<T> self, T[] value) => setItems(self, value);
                        public static T[] SetItemsL(global::System.Collections.Generic.List<T> self, T[] value) { setItems(self, value); return value; }
                        #endregion
                        #region _size
                        public static FieldInfo Field_Size { get; } = Type.GetField("_size", BFI) ?? throw new NullReferenceException();
                        private static readonly Func<global::System.Collections.Generic.List<T>, int> getSize = GetGetFieldDelegate<global::System.Collections.Generic.List<T>, int>(Field_Size);
                        private static readonly Action<global::System.Collections.Generic.List<T>, int> setSize = GetSetFieldDelegate<global::System.Collections.Generic.List<T>, int>(Field_Size);
                        public static int GetSize(global::System.Collections.Generic.List<T> self) => getSize(self);
                        public static void SetSize(global::System.Collections.Generic.List<T> self, int value) => setSize(self, value);
                        public static int SetSizeL(global::System.Collections.Generic.List<T> self, int value) { setSize(self, value); return value; }
                        #endregion
                        #region _version
                        public static FieldInfo Field_Version { get; } = Type.GetField("_version", BFI) ?? throw new NullReferenceException();
                        private static readonly Func<global::System.Collections.Generic.List<T>, int> getVersion = GetGetFieldDelegate<global::System.Collections.Generic.List<T>, int>(Field_Version);
                        private static readonly Action<global::System.Collections.Generic.List<T>, int> setVersion = GetSetFieldDelegate<global::System.Collections.Generic.List<T>, int>(Field_Version);
                        public static int GetVersion(global::System.Collections.Generic.List<T> self) => getVersion(self);
                        public static void SetVersion(global::System.Collections.Generic.List<T> self, int value) => setVersion(self, value);
                        public static int SetVersionL(global::System.Collections.Generic.List<T> self, int value) { setVersion(self, value); return value; }
                        #endregion
                    }
                    #endregion
                }
            }
        }
    }
}
