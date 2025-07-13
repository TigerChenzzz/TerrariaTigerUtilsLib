using System;
using System.Collections.Generic;
using System.Linq;

namespace TigerUtilsLib.ExtensionsByRandom;

public static class TigerExtensionsByRandom {
    private static Random DefaultRandom => Random.Shared;
    #region 打乱数组/列表 Shuffle
    #region Array
    #region 不带 Random 参数
    /// <inheritdoc cref="Shuffle{T}(T[], Random)"/>
    public static T[] Shuffle<T>(this T[] array) => Shuffle(array, DefaultRandom);
    /// <inheritdoc cref="Shuffled{T}(T[], Random)"/>
    public static T[] Shuffled<T>(this T[] array) => Shuffled(array, DefaultRandom);
    /// <inheritdoc cref="Shuffle{T}(T[], int, int, Random)"/>
    public static T[] Shuffle<T>(this T[] array, int offset, int count) => Shuffle(array, offset, count, DefaultRandom);
    /// <inheritdoc cref="Shuffled{T}(T[], int, int, Random)"/>
    public static T[] Shuffled<T>(this T[] array, int offset, int count) => Shuffled(array, offset, count, DefaultRandom);
    /// <inheritdoc cref="Shuffle{T}(T[], Range, Random)"/>
    public static T[] Shuffle<T>(this T[] array, Range range) => Shuffle(array, range, DefaultRandom);
    /// <inheritdoc cref="Shuffled{T}(T[], Range, Random)"/>
    public static T[] Shuffled<T>(this T[] array, Range range) => Shuffled(array, range, DefaultRandom);
    #endregion
    #region 带有 Random 参数
    /// <inheritdoc cref="Shuffle{T}(T[], int, int, Random)"/>
    public static T[] Shuffle<T>(this T[] array, Random rand) => Shuffle(array, 0, array.Length, rand);
    /// <inheritdoc cref="Shuffled{T}(T[], int, int, Random)"/>
    public static T[] Shuffled<T>(this T[] array, Random rand) => array.ToArray().Shuffle(rand);
    /// <summary>
    /// 直接在此数组上打乱整个数组
    /// </summary>
    public static T[] Shuffle<T>(this T[] array, int offset, int count, Random rand) {
        if (array.Length == 0) {
            return array;
        }
        var span = array.ToSpan()[offset..(offset + count)];
        for (int i = span.Length - 1; i > 0; --i) {
            int randint = rand.Next(0, i + 1);
            (span[i], span[randint]) = (span[randint], span[i]);
        }
        return array;
    }
    /// <summary>
    /// 返回一个打乱了的数组, 原数组不变
    /// </summary>
    public static T[] Shuffled<T>(this T[] array, int offset, int count, Random rand) => array.ToArray().Shuffle(offset, count, rand);
    /// <inheritdoc cref="Shuffle{T}(T[], int, int, Random)"/>
    public static T[] Shuffle<T>(this T[] array, Range range, Random rand) {
        var (offset, count) = range.GetOffsetAndLength(array.Length);
        return Shuffle(array, offset, count, rand);
    }
    /// <inheritdoc cref="Shuffled{T}(T[], int, int, Random)"/>
    public static T[] Shuffled<T>(this T[] array, Range range, Random rand) => array.ToArray().Shuffle(range, rand);
    #endregion
    #endregion
    #region List
    #region 不带 Random 参数
    /// <inheritdoc cref="Shuffle{T}(List{T}, Random)"/>
    public static List<T> Shuffle<T>(this List<T> list) => Shuffle(list, DefaultRandom);
    /// <inheritdoc cref="Shuffled{T}(List{T}, Random)"/>
    public static List<T> Shuffled<T>(this List<T> list) => Shuffled(list, DefaultRandom);
    /// <inheritdoc cref="Shuffle{T}(List{T}, int, int, Random)"/>
    public static List<T> Shuffle<T>(this List<T> list, int offset, int count) => Shuffle(list, offset, count, DefaultRandom);
    /// <inheritdoc cref="Shuffled{T}(List{T}, int, int, Random)"/>
    public static List<T> Shuffled<T>(this List<T> list, int offset, int count) => Shuffled(list, offset, count, DefaultRandom);
    /// <inheritdoc cref="Shuffle{T}(List{T}, Range, Random)"/>
    public static List<T> Shuffle<T>(this List<T> list, Range range) => Shuffle(list, range, DefaultRandom);
    /// <inheritdoc cref="Shuffled{T}(List{T}, Range, Random)"/>
    public static List<T> Shuffled<T>(this List<T> list, Range range) => Shuffled(list, range, DefaultRandom);
    #endregion
    #region 带有 Random 参数
    /// <inheritdoc cref="Shuffle{T}(List{T}, int, int, Random)"/>
    public static List<T> Shuffle<T>(this List<T> list, Random rand) => Shuffle(list, 0, list.Count, rand);
    /// <inheritdoc cref="Shuffled{T}(List{T}, int, int, Random)"/>
    public static List<T> Shuffled<T>(this List<T> list, Random rand) => list.ToList().Shuffle(rand);
    /// <summary>
    /// 直接在此列表上打乱整个列表
    /// </summary>
    public static List<T> Shuffle<T>(this List<T> list, int offset, int count, Random rand) {
        if (list.Count == 0) {
            return list;
        }
        var span = list.ToSpan()[offset..(offset + count)];
        for (int i = span.Length - 1; i > 0; --i) {
            int randint = rand.Next(0, i + 1);
            (span[i], span[randint]) = (span[randint], span[i]);
        }
        return list;
    }
    /// <summary>
    /// 返回一个打乱了的列表, 原列表不变
    /// </summary>
    public static List<T> Shuffled<T>(this List<T> list, int offset, int count, Random rand) => list.ToList().Shuffle(offset, count, rand);
    /// <inheritdoc cref="Shuffle{T}(List{T}, int, int, Random)"/>
    public static List<T> Shuffle<T>(this List<T> list, Range range, Random rand) {
        var (offset, count) = range.GetOffsetAndLength(list.Count);
        return Shuffle(list, offset, count, rand);
    }
    /// <inheritdoc cref="Shuffled{T}(List{T}, int, int, Random)"/>
    public static List<T> Shuffled<T>(this List<T> list, Range range, Random rand) => list.ToList().Shuffle(range, rand);
    #endregion
    #endregion
    #region IList
    #region 不带 Random 参数
    /// <inheritdoc cref="Shuffle{T}(IList{T}, Random)"/>
    public static IList<T> Shuffle<T>(this IList<T> list) => Shuffle(list, DefaultRandom);
    /// <inheritdoc cref="Shuffled{T}(IList{T}, Random)"/>
    public static IList<T> Shuffled<T>(this IList<T> list) => Shuffled(list, DefaultRandom);
    /// <inheritdoc cref="Shuffle{T}(IList{T}, int, int, Random)"/>
    public static IList<T> Shuffle<T>(this IList<T> list, int offset, int count) => Shuffle(list, offset, count, DefaultRandom);
    /// <inheritdoc cref="Shuffled{T}(IList{T}, int, int, Random)"/>
    public static IList<T> Shuffled<T>(this IList<T> list, int offset, int count) => Shuffled(list, offset, count, DefaultRandom);
    /// <inheritdoc cref="Shuffle{T}(IList{T}, Range, Random)"/>
    public static IList<T> Shuffle<T>(this IList<T> list, Range range) => Shuffle(list, range, DefaultRandom);
    /// <inheritdoc cref="Shuffled{T}(IList{T}, Range, Random)"/>
    public static IList<T> Shuffled<T>(this IList<T> list, Range range) => Shuffled(list, range, DefaultRandom);
    #endregion
    #region 带有 Random 参数
    /// <inheritdoc cref="Shuffle{T}(IList{T}, int, int, Random)"/>
    public static IList<T> Shuffle<T>(this IList<T> list, Random rand) => Shuffle(list, 0, list.Count, rand);
    /// <inheritdoc cref="Shuffled{T}(IList{T}, int, int, Random)"/>
    public static IList<T> Shuffled<T>(this IList<T> list, Random rand) => list.ToList().Shuffle(rand);
    /// <summary>
    /// 直接在此列表上打乱整个列表
    /// </summary>
    public static IList<T> Shuffle<T>(this IList<T> list, int offset, int count, Random rand) {
        if (list.Count == 0) {
            return list;
        }
        if (list is T[] array) {
            return array.Shuffle(offset, count, rand);
        }
        if (list is List<T> tList) {
            return tList.Shuffle(offset, count, rand);
        }
        for (int i = offset + count - 1; i > offset; --i) {
            int randint = rand.Next(offset, offset + i + 1);
            (list[i], list[randint]) = (list[randint], list[i]);
        }
        return list;
    }
    /// <summary>
    /// 返回一个打乱了的列表, 原列表不变
    /// </summary>
    public static IList<T> Shuffled<T>(this IList<T> list, int offset, int count, Random rand) => list.ToList().Shuffle(offset, count, rand);
    /// <inheritdoc cref="Shuffle{T}(IList{T}, int, int, Random)"/>
    public static IList<T> Shuffle<T>(this IList<T> list, Range range, Random rand) {
        var (offset, count) = range.GetOffsetAndLength(list.Count);
        return Shuffle(list, offset, count, rand);
    }
    /// <inheritdoc cref="Shuffled{T}(IList{T}, int, int, Random)"/>
    public static IList<T> Shuffled<T>(this IList<T> list, Range range, Random rand) => list.ToList().Shuffle(range, rand);
    #endregion
    #endregion
    #endregion
}
