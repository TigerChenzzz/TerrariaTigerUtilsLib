using System;
using System.Collections.Generic;
using System.Linq;

namespace TigerUtilsLib.ExtensionsByRandom;

public static class TigerIEnumerableRandomExtensionsByRandom {
    private static Func<Random> DefaultRandomGetter { get; set; } = () => System.Random.Shared;
    #region IEnumerable
    /// <summary>
    /// <br/>需确保<paramref name="enumerable"/>不会变化长度
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IEnumerable{T}, Random)"/>
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> enumerable, Random? rand = null) {
        rand ??= DefaultRandomGetter();
        int length = enumerable.Count();
        if (length == 0) {
            return default;
        }
        return enumerable.ElementAt(rand.Next(length));
    }
    /// <summary>
    /// 需确保<paramref name="enumerable"/>不会变化长度且长度非0
    /// </summary>
    public static T RandomF<T>(this IEnumerable<T> enumerable, Random? rand = null) {
        rand ??= DefaultRandomGetter();
        int length = enumerable.Count();
        return enumerable.ElementAt(rand.Next(length));
    }
    /// <summary>
    /// <br/>需确保<paramref name="enumerable"/>不会变化长度且<paramref name="weight"/>在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IEnumerable{T}, Func{T, double}, Random?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> enumerable, Func<T, double> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
        double w = default;
        if (uncheckNegative) {
            double totalWeight = enumerable.Sum(t => weight(t));
            double randDouble = rand.NextDouble() * totalWeight;
            return enumerable.FirstOrDefault(t => GetRight(w = weight(t), w > randDouble || Do(randDouble -= w)));
        }
        else {
            double totalWeight = enumerable.Sum(t => weight(t).WithMin(0));
            double randDouble = rand.NextDouble() * totalWeight;
            return totalWeight <= 0 ? default : enumerable.FirstOrDefault(t => GetRight(w = weight(t).WithMin(0), w > randDouble || Do(randDouble -= w)));
        }

    }
    /// <summary>
    /// <br/>需确保<paramref name="enumerable"/>不会变化长度且<paramref name="weight"/>在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IEnumerable{T}, Func{T, float}, Random?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> enumerable, Func<T, float> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
        float w = default;
        if (uncheckNegative) {
            float totalWeight = enumerable.Sum(t => weight(t));
            float randFloat = rand.NextSingle() * totalWeight;
            return enumerable.FirstOrDefault(t => GetRight(w = weight(t), w > randFloat || Do(randFloat -= w)));
        }
        else {
            float totalWeight = enumerable.Sum(t => weight(t).WithMin(0f));
            float randFloat = rand.NextSingle() * totalWeight;
            return totalWeight <= 0 ? default : enumerable.FirstOrDefault(t => GetRight(w = weight(t).WithMin(0f), w > randFloat || Do(randFloat -= w)));
        }

    }
    /// <summary>
    /// <br/>需确保<paramref name="enumerable"/>不会变化长度且<paramref name="weight"/>在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IEnumerable{T}, Func{T, int}, Random?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> enumerable, Func<T, int> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
        int w = default;
        if (uncheckNegative) {
            int totalWeight = enumerable.Sum(t => weight(t));
            int randInt = rand.Next(totalWeight);
            return enumerable.FirstOrDefault(t => GetRight(w = weight(t), w > randInt || Do(randInt -= w)));
        }
        else {
            int totalWeight = enumerable.Sum(t => weight(t).WithMin(0));
            int randInt = rand.Next(totalWeight);
            return totalWeight <= 0 ? default : enumerable.FirstOrDefault(t => GetRight(w = weight(t).WithMin(0), w > randInt || Do(randInt -= w)));
        }
    }
    public static T? RandomS<T>(this IEnumerable<T> enumerable, Random? rand = null) {
        rand ??= DefaultRandomGetter();
        T[] list = [.. enumerable];
        if (list.Length == 0) {
            return default;
        }
        return list[rand.Next(list.Length)];
    }
    public static T? RandomS<T>(this IEnumerable<T> enumerable, Func<T, double> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
        double w = default;
        double totalWeight = default;
        (double weight, T value)[] list = uncheckNegative ? [.. enumerable.Select(t => GetRight(totalWeight += w = weight(t), (w, t)))]
            : [.. enumerable.Select<T, (double weight, T value)>(t => (weight(t), t)).Where(p => p.weight > 0).WithAction(p => totalWeight += p.weight)];
        double randDouble = rand.NextDouble() * totalWeight;
        return list.FirstOrDefault(p => p.weight > randDouble || Do(randDouble -= p.weight)).value;
    }
    public static T? RandomS<T>(this IEnumerable<T> enumerable, Func<T, float> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
        float w = default;
        float totalWeight = default;
        (float weight, T value)[] list = uncheckNegative ? [.. enumerable.Select(t => GetRight(totalWeight += w = weight(t), (w, t)))]
            : [.. enumerable.Select<T, (float weight, T value)>(t => (weight(t), t)).Where(p => p.weight > 0).WithAction(p => totalWeight += p.weight)];
        float randFloat = rand.NextSingle() * totalWeight;
        return list.FirstOrDefault(p => p.weight > randFloat || Do(randFloat -= p.weight)).value;
    }
    public static T? RandomS<T>(this IEnumerable<T> enumerable, Func<T, int> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
        int w = default;
        int totalWeight = default;
        (int weight, T value)[] list = uncheckNegative ? [.. enumerable.Select(t => GetRight(totalWeight += w = weight(t), (w, t)))]
            : [.. enumerable.Select<T, (int weight, T value)>(t => (weight(t), t)).Where(p => p.weight > 0).WithAction(p => totalWeight += p.weight)];
        int randInt = rand.Next(totalWeight);
        return list.FirstOrDefault(p => p.weight > randInt || Do(randInt -= p.weight)).value;
    }
    #endregion
    #region IList
    public static T? Random<T>(this IList<T> list, Random? rand = null) {
        int count = list.Count;
        if (count <= 0) {
            return default;
        }
        rand ??= DefaultRandomGetter();
        return list.ElementAt(rand.Next(list.Count));
    }
    /// <summary>
    /// 需确保<paramref name="list"/>的长度非0
    /// </summary>
    public static T RandomF<T>(this IList<T> list, Random? rand = null) {
        rand ??= DefaultRandomGetter();
        return list.ElementAt(rand.Next(list.Count));
    }
    /// <summary>
    /// <br/>需确保<paramref name="weight"/>在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IList{T}, Func{T, double}, Random?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IList<T> list, Func<T, double> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
        double w = default;
        if (uncheckNegative) {
            double totalWeight = list.Sum(t => weight(t));
            double randDouble = rand.NextDouble() * totalWeight;
            return list.FirstOrDefault(t => GetRight(w = weight(t), w > randDouble || Do(randDouble -= w)));
        }
        else {
            double totalWeight = list.Sum(t => weight(t).WithMin(0));
            double randDouble = rand.NextDouble() * totalWeight;
            return totalWeight <= 0 ? default : list.FirstOrDefault(t => GetRight(w = weight(t).WithMin(0), w > randDouble || Do(randDouble -= w)));
        }

    }
    /// <summary>
    /// <br/>需确保<paramref name="weight"/>在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IList{T}, Func{T, float}, Random?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IList<T> list, Func<T, float> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
        float w = default;
        if (uncheckNegative) {
            float totalWeight = list.Sum(t => weight(t));
            float randFloat = rand.NextSingle() * totalWeight;
            return list.FirstOrDefault(t => GetRight(w = weight(t), w > randFloat || Do(randFloat -= w)));
        }
        else {
            float totalWeight = list.Sum(t => weight(t).WithMin(0f));
            float randFloat = rand.NextSingle() * totalWeight;
            return totalWeight <= 0 ? default : list.FirstOrDefault(t => GetRight(w = weight(t).WithMin(0f), w > randFloat || Do(randFloat -= w)));
        }

    }
    /// <summary>
    /// <br/>需确保<paramref name="weight"/>在固定参数下的返回值不变
    /// <br/>若可能会变化, 请调用<see cref="RandomS{T}(IList{T}, Func{T, int}, Random?, bool)"/>
    /// </summary>
    public static T? Random<T>(this IList<T> list, Func<T, int> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
        int w = default;
        if (uncheckNegative) {
            int totalWeight = list.Sum(t => weight(t));
            int randInt = rand.Next(totalWeight);
            return list.FirstOrDefault(t => GetRight(w = weight(t), w < randInt || Do(randInt -= w)));
        }
        else {
            int totalWeight = list.Sum(t => weight(t).WithMin(0));
            int randInt = rand.Next(totalWeight);
            return totalWeight <= 0 ? default : list.FirstOrDefault(t => GetRight(w = weight(t).WithMin(0), w > randInt || Do(randInt -= w)));
        }

    }
    public static T? RandomS<T>(this IList<T> list, Func<T, double> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
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
    public static T? RandomS<T>(this IList<T> list, Func<T, float> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
        float w = default;
        float totalWeight = default;
        float[] weights = uncheckNegative ? [.. list.Select(t => GetRight(totalWeight += w = weight(t), w))]
        : [.. list.Select(t => GetRight(totalWeight += w = weight(t).WithMin(0), w))];
        float randFloat = rand.NextSingle() * totalWeight;
        int index = Range(list.Count).FirstOrDefault(i => weights[i] > randFloat || Do(randFloat -= weights[i]), -1);
        if (index == -1) {
            return default;
        }
        return list.ElementAt(index);
    }
    public static T? RandomS<T>(this IList<T> list, Func<T, int> weight, Random? rand = null, bool uncheckNegative = false) {
        rand ??= DefaultRandomGetter();
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
}
