#if TERRARIA
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Terraria;
using Terraria.Utilities;

namespace TigerUtilsLib.ExtensionsByUnifiedRandom;

public static class TigerIEnumerableRandomExtensionsByUnifiedRandom {
    private static Func<UnifiedRandom> DefaultUnifiedRandomGetter { get; set; } = () => Main.rand;
    #region IEnumerable
    #region 不带 UnifiedRandom 参数
    /// <inheritdoc cref="Random{T}(IEnumerable{T}, UnifiedRandom)"/>
    public static T? Random<T>(this IEnumerable<T> enumerable) => Random(enumerable, DefaultUnifiedRandomGetter());
    /// <inheritdoc cref="RandomF{T}(IEnumerable{T}, UnifiedRandom)"/>
    public static T RandomF<T>(this IEnumerable<T> enumerable) => RandomF(enumerable, DefaultUnifiedRandomGetter());
    /// <inheritdoc cref="Random{T}(IEnumerable{T}, Func{T, double}, UnifiedRandom, bool)"/>
    public static T? Random<T>(this IEnumerable<T> enumerable, Func<T, double> weight, bool uncheckNegative = false) => Random(enumerable, weight, DefaultUnifiedRandomGetter(), uncheckNegative);
    /// <inheritdoc cref="Random{T}(IEnumerable{T}, Func{T, float}, UnifiedRandom, bool)"/>
    public static T? Random<T>(this IEnumerable<T> enumerable, Func<T, float> weight, bool uncheckNegative = false) => Random(enumerable, weight, DefaultUnifiedRandomGetter(), uncheckNegative);
    /// <inheritdoc cref="Random{T}(IEnumerable{T}, Func{T, int}, UnifiedRandom, bool)"/>
    public static T? Random<T>(this IEnumerable<T> enumerable, Func<T, int> weight, bool uncheckNegative = false) => Random(enumerable, weight, DefaultUnifiedRandomGetter(), uncheckNegative);

    public static T? RandomS<T>(this IEnumerable<T> enumerable) => RandomS(enumerable, DefaultUnifiedRandomGetter());
    public static T? RandomS<T>(this IEnumerable<T> enumerable, Func<T, double> weight, bool uncheckNegative = false) => RandomS(enumerable, weight, DefaultUnifiedRandomGetter(), uncheckNegative);
    #endregion
    #region 带有 UnifiedRandom 参数
    /// <summary>
    /// <br/>需确保<paramref name="enumerable"/>不会变化长度
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IEnumerable{T}, UnifiedRandom)"/>
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> enumerable, UnifiedRandom rand) {
        int length = enumerable.Count();
        if (length == 0) {
            return default;
        }
        return enumerable.ElementAt(rand.Next(length));
    }
    /// <summary>
    /// 需确保<paramref name="enumerable"/>不会变化长度且长度非0
    /// </summary>
    public static T RandomF<T>(this IEnumerable<T> enumerable, UnifiedRandom rand) => enumerable.ElementAt(rand.Next(enumerable.Count()));
    /// <summary>
    /// <br/>需确保<paramref name="enumerable"/>不会变化长度且<paramref name="weight"/>在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IEnumerable{T}, Func{T, double}, UnifiedRandom?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> enumerable, Func<T, double> weight, UnifiedRandom rand, bool uncheckNegative = false) {
        if (uncheckNegative) {
            double totalWeight = enumerable.Sum(t => weight(t));
            double r = rand.NextDouble(totalWeight);
            foreach (var t in enumerable) {
                var w = weight(t);
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
        else {
            double totalWeight = enumerable.Sum(t => weight(t).WithMin(0));
            if (totalWeight <= 0) {
                return default;
            }
            double r = rand.NextDouble(totalWeight);
            foreach (var t in enumerable) {
                var w = weight(t);
                if (w <= 0) {
                    continue;
                }
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
    }
    /// <summary>
    /// <br/>需确保<paramref name="enumerable"/>不会变化长度且<paramref name="weight"/>在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IEnumerable{T}, Func{T, float}, UnifiedRandom?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> enumerable, Func<T, float> weight, UnifiedRandom rand, bool uncheckNegative = false) {
        if (uncheckNegative) {
            float totalWeight = enumerable.Sum(t => weight(t));
            float r = rand.NextFloat(totalWeight);
            foreach (var t in enumerable) {
                var w = weight(t);
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
        else {
            float totalWeight = enumerable.Sum(t => weight(t).WithMin(0));
            if (totalWeight <= 0) {
                return default;
            }
            float r = rand.NextFloat(totalWeight);
            foreach (var t in enumerable) {
                var w = weight(t);
                if (w <= 0) {
                    continue;
                }
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
    }
    /// <summary>
    /// <br/>需确保<paramref name="enumerable"/>不会变化长度且<paramref name="weight"/>在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IEnumerable{T}, Func{T, int}, UnifiedRandom?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> enumerable, Func<T, int> weight, UnifiedRandom rand, bool uncheckNegative = false) {
        if (uncheckNegative) {
            int totalWeight = enumerable.Sum(t => weight(t));
            int r = rand.Next(totalWeight);
            foreach (var t in enumerable) {
                var w = weight(t);
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
        else {
            int totalWeight = enumerable.Sum(t => weight(t).WithMin(0));
            if (totalWeight <= 0) {
                return default;
            }
            int r = rand.Next(totalWeight);
            foreach (var t in enumerable) {
                var w = weight(t);
                if (w <= 0) {
                    continue;
                }
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
    }

    public static T? RandomS<T>(this IEnumerable<T> enumerable, UnifiedRandom rand) {
        T[] list = [.. enumerable];
        var len = list.Length;
        if (len == 0) {
            return default;
        }
        return list[rand.Next(len)];
    }
    public static T? RandomS<T>(this IEnumerable<T> enumerable, Func<T, double> weight, UnifiedRandom? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultUnifiedRandomGetter();
        double w = default;
        double totalWeight = default;
        (double weight, T value)[] list = uncheckNegative ? [.. enumerable.Select(t => GetRight(totalWeight += w = weight(t), (w, t)))]
            : [.. enumerable.Select<T, (double weight, T value)>(t => (weight(t), t)).Where(p => p.weight > 0).WithAction(p => totalWeight += p.weight)];
        double randDouble = rand.NextDouble() * totalWeight;
        return list.FirstOrDefault(p => p.weight > randDouble || Do(randDouble -= p.weight)).value;
    }
    public static T? RandomS<T>(this IEnumerable<T> enumerable, Func<T, float> weight, UnifiedRandom? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultUnifiedRandomGetter();
        float w = default;
        float totalWeight = default;
        (float weight, T value)[] list = uncheckNegative ? [.. enumerable.Select(t => GetRight(totalWeight += w = weight(t), (w, t)))]
            : [.. enumerable.Select<T, (float weight, T value)>(t => (weight(t), t)).Where(p => p.weight > 0).WithAction(p => totalWeight += p.weight)];
        float randFloat = rand.NextFloat() * totalWeight;
        return list.FirstOrDefault(p => p.weight > randFloat || Do(randFloat -= p.weight)).value;
    }
    public static T? RandomS<T>(this IEnumerable<T> enumerable, Func<T, int> weight, UnifiedRandom? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultUnifiedRandomGetter();
        int w = default;
        int totalWeight = default;
        (int weight, T value)[] list = uncheckNegative ? [.. enumerable.Select(t => GetRight(totalWeight += w = weight(t), (w, t)))]
            : [.. enumerable.Select<T, (int weight, T value)>(t => (weight(t), t)).Where(p => p.weight > 0).WithAction(p => totalWeight += p.weight)];
        int randInt = rand.Next(totalWeight);
        return list.FirstOrDefault(p => p.weight > randInt || Do(randInt -= p.weight)).value;
    }
    #endregion
    #endregion
    #region IList
    #region 不带 UnifiedRandom 参数
    /// <inheritdoc cref="Random{T}(IList{T}, UnifiedRandom)"/>
    public static T? Random<T>(this IList<T> list) => Random(list, DefaultUnifiedRandomGetter());
    /// <inheritdoc cref="RandomF{T}(IList{T}, UnifiedRandom)"/>
    public static T RandomF<T>(this IList<T> list) => RandomF(list, DefaultUnifiedRandomGetter());
    /// <inheritdoc cref="Random{T}(IList{T}, Func{T, double}, UnifiedRandom, bool)"/>
    public static T? Random<T>(this IList<T> list, Func<T, double> weight, bool uncheckNegative = false) => Random(list, weight, DefaultUnifiedRandomGetter(), uncheckNegative);
    /// <inheritdoc cref="Random{T}(IList{T}, Func{T, float}, UnifiedRandom, bool)"/>
    public static T? Random<T>(this IList<T> list, Func<T, float> weight, bool uncheckNegative = false) => Random(list, weight, DefaultUnifiedRandomGetter(), uncheckNegative);
    /// <inheritdoc cref="Random{T}(IList{T}, Func{T, int}, UnifiedRandom, bool)"/>
    public static T? Random<T>(this IList<T> list, Func<T, int> weight, bool uncheckNegative = false) => Random(list, weight, DefaultUnifiedRandomGetter(), uncheckNegative);
    
    /// <inheritdoc cref="RandomS{T}(IList{T}, Func{T, double}, UnifiedRandom, bool)"/>
    public static T? RandomS<T>(this IList<T> list, Func<T, double> weight, bool uncheckNegative = false) => RandomS(list, weight, DefaultUnifiedRandomGetter(), uncheckNegative);
    /// <inheritdoc cref="RandomS{T}(IList{T}, Func{T, float}, UnifiedRandom, bool)"/>
    public static T? RandomS<T>(this IList<T> list, Func<T, float> weight, bool uncheckNegative = false) => RandomS(list, weight, DefaultUnifiedRandomGetter(), uncheckNegative);
    /// <inheritdoc cref="RandomS{T}(IList{T}, Func{T, int}, UnifiedRandom, bool)"/>
    public static T? RandomS<T>(this IList<T> list, Func<T, int> weight, bool uncheckNegative = false) => RandomS(list, weight, DefaultUnifiedRandomGetter(), uncheckNegative);
    #endregion
    #region 带有 UnifiedRandom 参数
    public static T? Random<T>(this IList<T> list, UnifiedRandom rand) {
        int count = list.Count;
        if (count <= 0) {
            return default;
        }
        return list.ElementAt(rand.Next(list.Count));
    }
    /// <summary>
    /// 需确保 <paramref name="list"/> 的长度非0
    /// </summary>
    public static T RandomF<T>(this IList<T> list, UnifiedRandom rand) => list.ElementAt(rand.Next(list.Count));
    /// <summary>
    /// <br/>需确保 <paramref name="weight"/> 在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用 <see cref="RandomS{T}(IList{T}, Func{T, double}, UnifiedRandom?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IList<T> list, Func<T, double> weight, UnifiedRandom rand, bool uncheckNegative = false) {
        if (uncheckNegative) {
            double totalWeight = list.Sum(t => weight(t));
            double r = rand.NextDouble(totalWeight);
            foreach (var t in list) {
                var w = weight(t);
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
        else {
            double totalWeight = list.Sum(t => weight(t).WithMin(0));
            if (totalWeight <= 0) {
                return default;
            }
            double r = rand.NextDouble(totalWeight);
            foreach (var t in list) {
                var w = weight(t);
                if (w <= 0) {
                    continue;
                }
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
    }
    /// <summary>
    /// <br/>需确保 <paramref name="weight"/> 在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用 <see cref="RandomS{T}(IList{T}, Func{T, float}, UnifiedRandom?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IList<T> list, Func<T, float> weight, UnifiedRandom rand, bool uncheckNegative = false) {
        if (uncheckNegative) {
            float totalWeight = list.Sum(t => weight(t));
            float r = rand.NextFloat(totalWeight);
            foreach (var t in list) {
                var w = weight(t);
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
        else {
            float totalWeight = list.Sum(t => weight(t).WithMin(0));
            if (totalWeight <= 0) {
                return default;
            }
            float r = rand.NextFloat(totalWeight);
            foreach (var t in list) {
                var w = weight(t);
                if (w <= 0) {
                    continue;
                }
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
    }
    /// <summary>
    /// <br/>需确保 <paramref name="weight"/> 在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IList{T}, Func{T, int}, UnifiedRandom?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IList<T> list, Func<T, int> weight, UnifiedRandom rand, bool uncheckNegative = false) {
        if (uncheckNegative) {
            int totalWeight = list.Sum(t => weight(t));
            int r = rand.Next(totalWeight);
            foreach (var t in list) {
                var w = weight(t);
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
        else {
            int totalWeight = list.Sum(t => weight(t).WithMin(0));
            if (totalWeight <= 0) {
                return default;
            }
            int r = rand.Next(totalWeight);
            foreach (var t in list) {
                var w = weight(t);
                if (w <= 0) {
                    continue;
                }
                if (w > r) {
                    return t;
                }
                r -= w;
            }
            return default;
        }
    }

    public static T? RandomS<T>(this IList<T> list, Func<T, double> weight, UnifiedRandom rand, bool uncheckNegative = false) {
        double w = default;
        double totalWeight = default;
        double[] weights = uncheckNegative ? [.. list.Select(t => GetRight(totalWeight += w = weight(t), w))]
        : [.. list.Select(t => GetRight(totalWeight += w = weight(t).WithMin(0), w))];
        double randDouble = rand.NextDouble() * totalWeight;
        int index = Range(list.Count).FirstOrDefault(i => weights[i] > randDouble || Do(randDouble -= weights[i]), -1);
        if (index == -1) {
            return default;
        }
        return list.ElementAt(index);
    }
    public static T? RandomS<T>(this IList<T> list, Func<T, float> weight, UnifiedRandom rand, bool uncheckNegative = false) {
        float w = default;
        float totalWeight = default;
        float[] weights = uncheckNegative ? [.. list.Select(t => GetRight(totalWeight += w = weight(t), w))]
        : [.. list.Select(t => GetRight(totalWeight += w = weight(t).WithMin(0), w))];
        float randFloat = rand.NextFloat() * totalWeight;
        int index = Range(list.Count).FirstOrDefault(i => weights[i] > randFloat || Do(randFloat -= weights[i]), -1);
        if (index == -1) {
            return default;
        }
        return list.ElementAt(index);
    }
    public static T? RandomS<T>(this IList<T> list, Func<T, int> weight, UnifiedRandom rand, bool uncheckNegative = false) {
        int w = default;
        int totalWeight = default;
        int[] weights = uncheckNegative ? [.. list.Select(t => GetRight(totalWeight += w = weight(t), w))]
        : [.. list.Select(t => GetRight(totalWeight += w = weight(t).WithMin(0), w))];
        int randInt = rand.Next(totalWeight);
        int index = Range(list.Count).FirstOrDefault(i => weights[i] > randInt || Do(randInt -= weights[i]), -1);
        if (index == -1) {
            return default;
        }
        return list.ElementAt(index);
    }
    #endregion
    #endregion
    #region IList<(weight, value)>
    public static T? RandomW<T>(this IList<(double, T)> list, bool uncheckNegative = true) => RandomW(list, DefaultUnifiedRandomGetter(), uncheckNegative);
    public static T? RandomW<T>(this IList<(float, T)> list, bool uncheckNegative = true) => RandomW(list, DefaultUnifiedRandomGetter(), uncheckNegative);
    public static T? RandomW<T>(this IList<(int, T)> list, bool uncheckNegative = true) => RandomW(list, DefaultUnifiedRandomGetter(), uncheckNegative);
    public static T? RandomW<T>(this IList<(double, T)> list, UnifiedRandom rand, bool uncheckNegative = true) {
        double totalWeight = 0;
        if (uncheckNegative) {
            foreach (var (w, _) in list) {
                totalWeight += w;
            }
            double r = rand.NextDouble(totalWeight);
            foreach (var (w, v) in list) {
                if (w > r) {
                    return v;
                }
                r -= w;
            }
            return default;
        }
        else {
            foreach (var (w, _) in list) {
                if (w > 0) {
                    totalWeight += w;
                }
            }
            double r = rand.NextDouble(totalWeight);
            foreach (var (w, v) in list) {
                if (w <= 0) {
                    continue;
                }
                if (w > r) {
                    return v;
                }
                r -= w;
            }
            return default;
        }
    }
    public static T? RandomW<T>(this IList<(float, T)> list, UnifiedRandom rand, bool uncheckNegative = true) {
        float totalWeight = 0;
        if (uncheckNegative) {
            foreach (var (w, _) in list) {
                totalWeight += w;
            }
            float r = rand.NextFloat(totalWeight);
            foreach (var (w, v) in list) {
                if (w > r) {
                    return v;
                }
                r -= w;
            }
            return default;
        }
        else {
            foreach (var (w, _) in list) {
                if (w > 0) {
                    totalWeight += w;
                }
            }
            float r = rand.NextFloat(totalWeight);
            foreach (var (w, v) in list) {
                if (w <= 0) {
                    continue;
                }
                if (w > r) {
                    return v;
                }
                r -= w;
            }
            return default;
        }
    }
    public static T? RandomW<T>(this IList<(int, T)> list, UnifiedRandom rand, bool uncheckNegative = true) {
        int totalWeight = 0;
        if (uncheckNegative) {
            foreach (var (w, _) in list) {
                totalWeight += w;
            }
            int r = rand.Next(totalWeight);
            foreach (var (w, v) in list) {
                if (w > r) {
                    return v;
                }
                r -= w;
            }
            return default;
        }
        else {
            foreach (var (w, _) in list) {
                if (w > 0) {
                    totalWeight += w;
                }
            }
            int r = rand.Next(totalWeight);
            foreach (var (w, v) in list) {
                if (w <= 0) {
                    continue;
                }
                if (w > r) {
                    return v;
                }
                r -= w;
            }
            return default;
        }
    }
    #endregion
}
#endif
