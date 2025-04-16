#if TERRARIA
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Utilities;

namespace TigerUtilsLib.ExtensionsByUnifiedRandom;

public static class TigerExtensionsByUnifiedRandom {
    private static UnifiedRandom DefaultUnifiedRandom => Main.rand;
    #region 打乱数组/列表 Shuffle
    #region 不带 UnifiedRandom 参数
    /// <inheritdoc cref="Shuffle{T}(List{T}, UnifiedRandom)"/>
    public static List<T> Shuffle<T>(this List<T> list) => Shuffle(list, DefaultUnifiedRandom);
    /// <inheritdoc cref="Shuffle{T}(T[], UnifiedRandom)"/>
    public static T[] Shuffle<T>(this T[] array) => Shuffle(array, DefaultUnifiedRandom);
    /// <inheritdoc cref="Shuffled{T}(List{T}, UnifiedRandom)"/>
    public static List<T> Shuffled<T>(this List<T> list) => Shuffled(list, DefaultUnifiedRandom);
    /// <inheritdoc cref="Shuffled{T}(T[], UnifiedRandom)"/>
    public static T[] Shuffled<T>(this T[] array) => Shuffled(array, DefaultUnifiedRandom);
    #endregion
    #region 带有 UnifiedRandom 参数
    /// <summary>
    /// 直接在此列表上打乱整个列表
    /// </summary>
    public static List<T> Shuffle<T>(this List<T> list, UnifiedRandom rand) {
        if (list.Count == 0) {
            return list;
        }
        var span = list.ToSpan();
        foreach (int i in Range(span.Length - 1, 0, RangeType.Negative)) {
            int randint = rand.Next(0, i + 1);
            (span[i], span[randint]) = (span[randint], span[i]);
        }
        return list;
    }
    /// <summary>
    /// 直接在此数组上打乱整个数组
    /// </summary>
    public static T[] Shuffle<T>(this T[] array, UnifiedRandom rand) {
        if (array.Length == 0) {
            return array;
        }
        var span = array.ToSpan();
        foreach (int i in Range(span.Length - 1, 0, RangeType.Negative)) {
            int randint = rand.Next(0, i + 1);
            (span[i], span[randint]) = (span[randint], span[i]);
        }
        return array;
    }
    /// <summary>
    /// 返回一个打乱了的列表, 原列表不变
    /// </summary>
    public static List<T> Shuffled<T>(this List<T> list, UnifiedRandom rand) => list.ToList().Shuffle(rand);
    /// <summary>
    /// 返回一个打乱了的数组, 原数组不变
    /// </summary>
    public static T[] Shuffled<T>(this T[] array, UnifiedRandom rand) => array.ToArray().Shuffle(rand);
    #endregion
    #endregion
}
#endif
