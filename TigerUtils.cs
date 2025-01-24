// #define TIGER_REFLECTION_EXTENSIONS

global using static TigerUtilsLib.TigerClasses;
global using static TigerUtilsLib.TigerStatics;
global using static TigerUtilsLib.TigerUtils;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using RList = TigerUtilsLib.Reflections.SystemReflections.System.Collections.Generic.List;
using SOpCode = System.Reflection.Emit.OpCode;
using SOpCodes = System.Reflection.Emit.OpCodes;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace TigerUtilsLib;

public static partial class TigerUtils {
    #region Lerp
    public enum LerpType {
        Linear,
        Quadratic,
        Cubic,
        CubicByK,
        Sin,
        Stay,
    }
    public static global::System.Numerics.Matrix4x4 NewMatrix(Vector4 v1, Vector4 v2, Vector4 v3, Vector4 v4) {
        return new(v1.X, v1.Y, v1.Z, v1.W,
                    v2.X, v2.Y, v2.Z, v2.W,
                    v3.X, v3.Y, v3.Z, v3.W,
                    v4.X, v4.Y, v4.Z, v4.W);
    }
    public static float NewLerpValue(float val, bool clamped, LerpType type, params float[] pars) {
        #region 边界检查
        if (clamped) {
            if (val <= 0) {
                return 0;
            }
            if (val >= 1) {
                return 1;
            }
        }
        if (val == 0) {
            return 0;
        }
        if (val == 1) {
            return 1;
        }
        #endregion
        switch (type) {
        case LerpType.Linear:
            return val;
        case LerpType.Quadratic:
            //pars[0]:二次函数的极点
            if (pars.Length <= 0) {
                throw new TargetParameterCountException("pars not enough");
            }
            if (pars[0] == 0.5f) {
                return 0;
            }
            return val * (val - 2 * pars[0]) / (1 - 2 * pars[0]);
        case LerpType.Cubic:
            //pars[0], pars[1]:三次函数的两个极点
            if (pars.Length <= 1) {
                throw new TargetParameterCountException("pars not enough");
            }
            return ((val - 3 * (pars[0] + pars[1]) / 2) * val + 3 * pars[0] * pars[1]) * val /
                (1 - 3 * (pars[0] + pars[1]) / 2 + 3 * pars[0] * pars[1]);
        case LerpType.CubicByK:
            //pars[0], pars[1]:两处的斜率
            //par[2], par[3](若存在):宽度和高度
            if (pars.Length < 2) {
                throw new TargetParameterCountException("pars not enough");
            }
            float par2 = pars.Length < 3 ? 1 : pars[2], par3 = pars.Length < 4 ? 1 : pars[3];
            if (par2 == 0) {
                return 0;
            }
            Vector4 va = new(0, par2 * par2 * par2, 0, 3 * par2 * par2);
            Vector4 vb = new(0, par2 * par2, 0, 2 * par2);
            Vector4 vc = new(0, par2, 1, 1);
            Vector4 vd = new(1, 1, 0, 0);
            Vector4 v0 = new(0, par3, pars[0], pars[1]);
            var d0 = NewMatrix(va, vb, vc, vd);
            var da = NewMatrix(v0, vb, vc, vd);
            var db = NewMatrix(va, v0, vc, vd);
            var dc = NewMatrix(va, vb, v0, vd);
            var dd = NewMatrix(va, vb, vc, v0);
            if (d0.GetDeterminant() == 0) {
                return 0;
            }
            if (par3 == 0) {
                return (((da.GetDeterminant() * val + db.GetDeterminant()) * val + dc.GetDeterminant()) * val + dd.GetDeterminant()) / d0.GetDeterminant();
            }
            return (((da.GetDeterminant() * val + db.GetDeterminant()) * val + dc.GetDeterminant()) * val + dd.GetDeterminant()) / d0.GetDeterminant() / par3;
        case LerpType.Sin:
            //pars[0], pars[1] : 两相位的四分之一周期数
            if (pars.Length < 2) {
                throw new TargetParameterCountException("pars not enough");
            }
            float x1 = (float)(Math.PI / 2 * pars[0]), x2 = (float)(Math.PI / 2 * pars[1]), x = Lerp(x1, x2, val);
            float y1 = (float)Math.Sin(x1), y2 = (float)Math.Sin(x2), y = (float)Math.Sin(x);
            if ((pars[0] - pars[1]) % 4 == 0 || (pars[0] + pars[1]) % 4 == 2) {
                return y - y1;
            }
            return (y - y1) / (y2 - y1);
        case LerpType.Stay:
            return val > 1 ? 1 : 0;
        }
        return val;
    }
    public static Vector2 NewVector2(double x, double y) => new((float)x, (float)y);
    public static Vector3 NewVector3(double x, double y, double z) => new((float)x, (float)y, (float)z);
    public static Vector4 NewVector4(double x, double y, double z, double w) => new((float)x, (float)y, (float)z, (float)w);
    public static double NewLerpValue(double val, bool clamped, LerpType type, params double[] pars) {

        #region 边界检查
        if (clamped) {
            if (val <= 0) {
                return 0;
            }
            if (val >= 1) {
                return 1;
            }
        }
        if (val == 0) {
            return 0;
        }
        if (val == 1) {
            return 1;
        }
        #endregion
        switch (type) {
        case LerpType.Linear:
            return val;
        case LerpType.Quadratic:
            //pars[0]:二次函数的极点
            if (pars.Length <= 0) {
                throw new TargetParameterCountException("pars not enough");
            }
            if (pars[0] == 0.5f) {
                return 0;
            }
            return val * (val - 2 * pars[0]) / (1 - 2 * pars[0]);
        case LerpType.Cubic:
            //pars[0], pars[1]:三次函数的两个极点
            if (pars.Length <= 1) {
                throw new TargetParameterCountException("pars not enough");
            }
            return ((val - 3 * (pars[0] + pars[1]) / 2) * val + 3 * pars[0] * pars[1]) * val /
                (1 - 3 * (pars[0] + pars[1]) / 2 + 3 * pars[0] * pars[1]);
        case LerpType.CubicByK:
            //pars[0], pars[1]:两处的斜率
            //par[2], par[3](若存在):宽度和高度
            if (pars.Length < 2) {
                throw new TargetParameterCountException("pars not enough");
            }
            double par2 = pars.Length < 3 ? 1 : pars[2], par3 = pars.Length < 4 ? 1 : pars[3];
            if (par2 == 0) {
                return 0;
            }
            Vector4 va = NewVector4(0, par2 * par2 * par2, 0, 3 * par2 * par2);
            Vector4 vb = NewVector4(0, par2 * par2, 0, 2 * par2);
            Vector4 vc = NewVector4(0, par2, 1, 1);
            Vector4 vd = NewVector4(1, 1, 0, 0);
            Vector4 v0 = NewVector4(0, par3, pars[0], pars[1]);
            var d0 = NewMatrix(va, vb, vc, vd);
            var da = NewMatrix(v0, vb, vc, vd);
            var db = NewMatrix(va, v0, vc, vd);
            var dc = NewMatrix(va, vb, v0, vd);
            var dd = NewMatrix(va, vb, vc, v0);
            if (d0.GetDeterminant() == 0) {
                return 0;
            }
            if (par3 == 0) {
                return (((da.GetDeterminant() * val + db.GetDeterminant()) * val + dc.GetDeterminant()) * val + dd.GetDeterminant()) / d0.GetDeterminant();
            }
            return (((da.GetDeterminant() * val + db.GetDeterminant()) * val + dc.GetDeterminant()) * val + dd.GetDeterminant()) / d0.GetDeterminant() / par3;
        case LerpType.Sin:
            //pars[0], pars[1] : 两相位的四分之一周期数
            if (pars.Length < 2) {
                throw new TargetParameterCountException("pars not enough");
            }
            double x1 = (Math.PI / 2 * pars[0]), x2 = (Math.PI / 2 * pars[1]), x = Lerp(x1, x2, val);
            double y1 = Math.Sin(x1), y2 = Math.Sin(x2), y = Math.Sin(x);
            if ((pars[0] - pars[1]) % 4 == 0 || (pars[0] + pars[1]) % 4 == 2) {
                return y - y1;
            }
            return (y - y1) / (y2 - y1);
        case LerpType.Stay:
            return val > 1 ? 1 : 0;
        }
        return val;
    }
    public static float Lerp(float left, float right, float val, bool clamped = false, LerpType type = LerpType.Linear, params float[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return left * (1 - val) + right * val;
    }
    public static int Lerp(int left, int right, float val, bool clamped = false, LerpType type = LerpType.Linear, params float[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return (int)(left * (1 - val) + right * val);
    }
    public static Vector2 Lerp(Vector2 left, Vector2 right, float val, bool clamped = false, LerpType type = LerpType.Linear, params float[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return left * (1 - val) + right * val;
    }
    public static Vector3 Lerp(Vector3 left, Vector3 right, float val, bool clamped = false, LerpType type = LerpType.Linear, params float[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return left * (1 - val) + right * val;
    }
    public static Vector4 Lerp(Vector4 left, Vector4 right, float val, bool clamped = false, LerpType type = LerpType.Linear, params float[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return left * (1 - val) + right * val;
    }
    public static double Lerp(double left, double right, double val, bool clamped = false, LerpType type = LerpType.Linear, params double[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return left * (1 - val) + right * val;
    }
    public static float Lerp(float left, float right, double val, bool clamped = false, LerpType type = LerpType.Linear, params double[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return (float)(left * (1 - val) + right * val);
    }
    public static int Lerp(int left, int right, double val, bool clamped = false, LerpType type = LerpType.Linear, params double[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return (int)(left * (1 - val) + right * val);
    }
    public static Vector2 Lerp(Vector2 left, Vector2 right, double val, bool clamped = false, LerpType type = LerpType.Linear, params double[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return NewVector2(Lerp(left.X, right.X, val), Lerp(left.Y, right.Y, val));
    }
    public static Vector3 Lerp(Vector3 left, Vector3 right, double val, bool clamped = false, LerpType type = LerpType.Linear, params double[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return NewVector3(Lerp(left.X, right.X, val), Lerp(left.Y, right.Y, val), Lerp(left.Z, right.Z, val));
    }
    public static Vector4 Lerp(Vector4 left, Vector4 right, double val, bool clamped = false, LerpType type = LerpType.Linear, params double[] pars) {
        val = NewLerpValue(val, clamped, type, pars);
        return NewVector4(Lerp(left.X, right.X, val), Lerp(left.Y, right.Y, val), Lerp(left.Z, right.Z, val), Lerp(left.W, right.W, val));
    }
    #endregion
    #region Lua的 And / Or 体系
    /// <summary>
    /// 若i判定为假则将o赋值给i
    /// 对于引用类型, 一般相当于 ??=
    /// </summary>
    public static T LuaOrAssignFrom<T>(ref T i, T o) {
        if (!Convert.ToBoolean(i)) {
            i = o;
        }
        return i;
    }
    /// <summary>
    /// 若i判定为假则将o赋值给i
    /// </summary>
    public static T LuaAndAssignFrom<T>(ref T i, T o) {
        if (Convert.ToBoolean(i)) {
            i = o;
        }
        return i;
    }
    #endregion
    #region Clamp
    /*
    /// <summary>
    /// please make sure left is not greater than right, else use ClampS instead
    /// </summary>
    public static double Clamp(double val, double left, double right) => Math.Max(left, Math.Min(right, val));
    /// <summary>
    /// please make sure left is not greater than right, else use ClampS instead
    /// </summary>
    public static float Clamp(float val, float left, float right) => MathF.Max(left, MathF.Min(right, val));
    /// <summary>
    /// please make sure left is not greater than right, else use ClampS instead
    /// </summary>
    public static int Clamp(int val, int left, int right) => Math.Max(left, Math.Min(right, val));
    public static double ClampS(double val, double left, double right) => GetRight((left > right) ? (left, right) = (right, left) : null, Clamp(val, left, right));
    public static float ClampS(float val, float left, float right) => GetRight((left > right) ? (left, right) = (right, left) : null, Clamp(val, left, right));
    public static int ClampS(int val, int left, int right) => GetRight((left > right) ? (left, right) = (right, left) : null, Clamp(val, left, right));
    */
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>最好要保证<paramref name="left"/> 不大于 <paramref name="right"/>, 
    /// <br/>否则最好使用<see cref="ClampS{T}(T, T, T)"/>
    /// <br/>优先保证不小于<paramref name="left"/>
    /// </summary>
    public static T Clamp<T>(T self, T left, T right) where T : IComparable<T>
        => self.CompareTo(left) < 0 ? left : self.CompareTo(right) > 0 ? right : self;
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>最好要保证<paramref name="left"/> 不大于 <paramref name="right"/>, 
    /// <br/>否则最好使用<see cref="ClampToS{T}(ref T, T, T)"/>
    /// <br/>优先保证不小于<paramref name="left"/>
    /// </summary>
    public static ref T ClampTo<T>(ref T self, T left, T right) where T : IComparable<T>
        => ref Assign(ref self, self.CompareTo(left) < 0 ? left : self.CompareTo(right) > 0 ? right : self);
    /// <summary>
    /// 得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// 自动判断<paramref name="left"/>和<paramref name="right"/>的大小关系
    /// </summary>
    public static T ClampS<T>(T self, T left, T right) where T : IComparable<T>
        => left.CompareTo(right) > 0 ? self.Clamp(right, left) : self.Clamp(left, right);
    /// <summary>
    /// 得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// 自动判断<paramref name="left"/>和<paramref name="right"/>的大小关系
    /// </summary>
    public static ref T ClampToS<T>(ref T self, T left, T right) where T : IComparable<T>
        => ref left.CompareTo(right) > 0 ? ref ClampTo(ref self, right, left) : ref ClampTo(ref self, left, right);
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>最好要保证<paramref name="left"/> 不大于 <paramref name="right"/>, 
    /// <br/>否则最好使用<see cref="ClampS{T}(T, T, T)"/>
    /// <br/>优先保证不大于<paramref name="right"/>
    /// </summary>
    public static T ClampR<T>(T self, T left, T right) where T : IComparable<T>
        => self.CompareTo(right) > 0 ? right : self.CompareTo(left) < 0 ? left : self;
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>最好要保证<paramref name="left"/> 不大于 <paramref name="right"/>, 
    /// <br/>否则最好使用<see cref="ClampToS{T}(ref T, T, T)"/>
    /// <br/>优先保证不大于<paramref name="right"/>
    /// </summary>
    public static ref T ClampToR<T>(ref T self, T left, T right) where T : IComparable<T>
        => ref Assign(ref self, self.CompareTo(right) > 0 ? right : self.CompareTo(left) < 0 ? left : self);
    #endregion
    #region IEnumerable拓展(包括Range)
    #region Range
    public enum RangeType {
        Positive,
        Negative,
        Automatic
    }
    public static IEnumerable<int> Range(int end, RangeType type = RangeType.Positive) {
        if (type == RangeType.Positive || type == RangeType.Automatic && end > 0) {
            for (int i = 0; i < end; ++i) {
                yield return i;
            }
        }
        else if (type == RangeType.Negative || type == RangeType.Automatic && end < 0) {
            for (int i = 0; i > end; --i) {
                yield return i;
            }
        }
    }
    public static IEnumerable<int> Range(int start, int end, RangeType type = RangeType.Positive) {
        if (type == RangeType.Positive || type == RangeType.Automatic && start < end) {
            for (int i = start; i < end; ++i) {
                yield return i;
            }
        }
        else if (type == RangeType.Negative || type == RangeType.Automatic && start > end) {
            for (int i = start; i > end; --i) {
                yield return i;
            }
        }
    }
    /// <summary>
    /// <paramref name="step"/>为0会按<see cref="Range(int, int, RangeType)"/>处理(自动模式)
    /// </summary>
    public static IEnumerable<int> Range(int start, int end, int step) {
        if (step == 0) {
            if (start < end) {
                for (int i = start; i < end; ++i) {
                    yield return i;
                }
            }
            else if (start > end) {
                for (int i = start; i > end; --i) {
                    yield return i;
                }
            }
        }
        else if (step > 0) {
            for (int i = start; i < end; i += step) {
                yield return i;
            }
        }
        else {
            for (int i = start; i > end; i += step) {
                yield return i;
            }
        }
    }
    /// <summary>
    /// <br/>一直执行到准备返回<paramref name="end"/>之前都不会停止, 但不会返回<paramref name="end"/>
    /// <br/>如果传入-1, 则会一直执行下去(不考虑<see cref="int.MaxValue"/>的情况下)
    /// </summary>
    public static IEnumerable<int> RangeN(int end, RangeType type = RangeType.Positive) {
        if (type == RangeType.Positive || type == RangeType.Automatic && end > 0) {
            for (int i = 0; ; ++i) {
                if (i == end) {
                    break;
                }
                yield return i;
            }
        }
        else if (type == RangeType.Negative || type == RangeType.Automatic && end < 0) {
            for (int i = 0; ; --i) {
                if (i == end) {
                    break;
                }
                yield return i;
            }
        }
    }
    /// <summary>
    /// 一直执行到准备返回<paramref name="end"/>之前都不会停止, 但不会返回<paramref name="end"/>
    /// </summary>
    public static IEnumerable<int> RangeN(int start, int end, RangeType type = RangeType.Positive) {
        if (type == RangeType.Positive || type == RangeType.Automatic && start < end) {
            for (int i = start; ; ++i) {
                if (i == end) {
                    break;
                }
                yield return i;
            }
        }
        else if (type == RangeType.Negative || type == RangeType.Automatic && start > end) {
            for (int i = start; ; --i) {
                if (i == end) {
                    break;
                }
                yield return i;
            }
        }
    }
    /// <summary>
    /// <br/>一直执行到准备返回<paramref name="end"/>之前都不会停止, 但不会返回<paramref name="end"/>
    /// <br/><paramref name="step"/>为0会按<see cref="Range(int, int, RangeType)"/>处理(自动模式)
    /// </summary>
    public static IEnumerable<int> RangeN(int start, int end, int step) {
        if (step == 0) {
            if (start < end) {
                for (int i = start; ; ++i) {
                    if (i == end) {
                        break;
                    }
                    yield return i;
                }
            }
            else if (start > end) {
                for (int i = start; ; --i) {
                    if (i == end) {
                        break;
                    }
                    yield return i;
                }
            }
        }
        else if (step > 0) {
            for (int i = start; ; i += step) {
                if (i == end) {
                    break;
                }
                yield return i;
            }
        }
        else {
            for (int i = start; ; i += step) {
                if (i == end) {
                    break;
                }
                yield return i;
            }
        }
    }

    /// <returns>(序号, 迭代值) 其中序号从0开始</returns>
    public static IEnumerable<(int, int)> RangeWithIndex(int end, RangeType type = RangeType.Positive) {
        if (type == RangeType.Positive || type == RangeType.Automatic && end > 0) {
            for (int i = 0; i < end; ++i) {
                yield return (i, i);
            }
        }
        else if (type == RangeType.Negative || type == RangeType.Automatic && end < 0) {
            for (int i = 0; i > end; --i) {
                yield return (-i, i);
            }
        }
    }
    /// <returns>(序号, 迭代值) 其中序号从0开始</returns>
    public static IEnumerable<(int, int)> RangeWithIndex(int start, int end, RangeType type = RangeType.Positive) {
        if (type == RangeType.Positive || type == RangeType.Automatic && start < end) {
            for (int i = start; i < end; ++i) {
                yield return (i - start, i);
            }
        }
        else if (type == RangeType.Negative || type == RangeType.Automatic && start > end) {
            for (int i = start; i > end; --i) {
                yield return (start - i, i);
            }
        }
    }
    /// <summary>
    /// <paramref name="step"/>为0会按<see cref="RangeWithIndex(int, int, RangeType)"/>处理(自动模式)
    /// </summary>
    /// <returns>(序号, 迭代值) 其中序号从0开始</returns>
    public static IEnumerable<(int, int)> RangeWithIndex(int start, int end, int step) {
        if (step == 0) {
            if (start < end) {
                for (int i = start; i < end; ++i) {
                    yield return (i - start, i);
                }
            }
            else if (start > end) {
                for (int i = start; i > end; --i) {
                    yield return (start - i, i);
                }
            }
        }
        else if (step > 0) {
            for (int i = start, index = 0; i < end; i += step, ++index) {
                yield return (index, i);
            }
        }
        else {
            for (int i = start, index = 0; i > end; i += step, ++index) {
                yield return (index, i);
            }
        }
    }
    #endregion
    #region ApplyOneToOne
    public static int ApplyOneToOne<T1, T2>(Func<IEnumerator<T1>?>? getEnumerator1, Func<IEnumerator<T2>?>? getEnumerator2, Func<T1, T2, bool>? condition, Action<T1, T2>? action, Action<T1>? applyToFail = null) {
#if false
        //简单暴力待优化
        for(var left = getEnumerator1(); left.MoveNext();) {
            for(var right = getEnumerator2(); right.MoveNext();) {
                if(condition(left.Current, right.Current)) {
                    action(left.Current, right.Current);
                    break;
                }
            }
            return false;
        }
        return true;
#else
        if (getEnumerator1 == null) {
            return 0;
        }
        if (getEnumerator2 == null) {
            return 0;
        }
        var e1 = getEnumerator1();
        if (e1?.MoveNext() != true) {
            return 0;
        }
#if false
        for (int failRounds = 1; failRounds < 2; ++failRounds)
        {

            for (var e2 = getEnumerator2(); e2.MoveNext();)
            {
                if (condition(e1.Current, e2.Current))
                {
                    failRounds = 0;
                    action(e1.Current, e2.Current);
                    if (!e1.MoveNext())
                    {
                        return 1;
                    }
                    continue;
                }
            }
        }
#endif
        int applyCount = 0;
        int failRounds = 1;
        int lastPosition = -1;
    Enumerate2:
        for (var (e2, j) = (getEnumerator2(), 0); e2?.MoveNext() == true; ++j) {
            if (condition?.Invoke(e1.Current, e2.Current) != false) {
                action?.Invoke(e1.Current, e2.Current);
                applyCount += 1;
                if (!e1.MoveNext()) {
                    goto Return;
                }
                failRounds = 0;
                lastPosition = j;
            }
            else if (lastPosition == j) {
                applyToFail?.Invoke(e1.Current);
                if (!e1.MoveNext()) {
                    goto Return;
                }
                failRounds = 0;
            }
        }
        failRounds += 1;
        if (lastPosition == -1 || failRounds >= 2) {
            applyToFail?.Invoke(e1.Current);
            if (!e1.MoveNext()) {
                goto Return;
            }
            lastPosition = -1;
            failRounds = 1;
        }
        goto Enumerate2;
    Return:
        return applyCount;
#endif
    }
    public static int ApplyOneToOne<T1, T2>(IEnumerable<T1>? e1, IEnumerable<T2>? e2, Func<T1, T2, bool>? condition, Action<T1, T2>? action, Action<T1>? applyToFail = null)
        => ApplyOneToOne(e1 == null ? null : e1.GetEnumerator, e2 == null ? null : e2.GetEnumerator, condition, action, applyToFail);
    public static int ApplyOneToOne<T1, TKey, TValue>(Func<IEnumerator<T1>?>? getEnumerator1, IDictionary<TKey, TValue>? dict, Func<T1, TKey?> toKey, Action<T1, TValue>? action, Action<T1>? applyToFail = null) {
        if (getEnumerator1 == null || dict == null) {
            return 0;
        }
        var e1 = getEnumerator1();
        if (e1 == null) {
            return 0;
        }
        int applyCount = 0;
        while (e1.MoveNext() == true) {
            TKey? key = toKey(e1.Current);
            if (key != null && dict.TryGetValue(key, out TValue? value)) {
                action?.Invoke(e1.Current, value);
                applyCount += 1;
            }
            else {
                applyToFail?.Invoke(e1.Current);
            }
        }
        return applyCount;
    }
    public static int ApplyOneToOne<T1, TKey, TValue>(IEnumerable<T1>? e1, IDictionary<TKey, TValue>? dict, Func<T1, TKey?> toKey, Action<T1, TValue>? action, Action<T1>? applyToFail = null)
        => ApplyOneToOne(e1 == null ? null : e1.GetEnumerator, dict, toKey, action, applyToFail);
    public static int ApplyOneToOne<T1, T2>(Func<IEnumerator<T1>?>? getEnumerator1, IList<T2>? list, Func<T1, int> toIndex, Action<T1, T2>? action, Action<T1>? applyToFail = null) {
        if (getEnumerator1 == null || list == null) {
            return 0;
        }
        var e1 = getEnumerator1();
        if (e1 == null) {
            return 0;
        }
        int applyCount = 0;
        while (e1.MoveNext()) {
            int index = toIndex(e1.Current);
            if (index >= 0 && list.Count > index) {
                action?.Invoke(e1.Current, list[index]);
                applyCount += 1;
            }
            else {
                applyToFail?.Invoke(e1.Current);
            }
        }
        return applyCount;
    }
    public static int ApplyOneToOne<T1, T2>(IEnumerable<T1>? e1, IList<T2>? list, Func<T1, int> toIndex, Action<T1, T2>? action, Action<T1>? applyToFail = null)
        => ApplyOneToOne(e1 == null ? null : e1.GetEnumerator, list, toIndex, action, applyToFail);
    #endregion
    #region EmptyEnumerable, SingleEnumerable
    public static IEnumerable<T> EmptyEnumerable<T>() {
        yield break;
    }
    public static IEnumerable EmptyEnumerable() {
        yield break;
    }
    public static IEnumerable<T> SingleEnumerable<T>(T t) {
        yield return t;
    }
    public static IEnumerable SingleEnumerable(object? o) {
        yield return o;
    }
    #endregion
    #endregion
    #region Random
    public static class MyRandom {
        public static double RandomAverage(double min, double max, Random? rand = null) {
            if (min == max) {
                return min;
            }
            rand ??= new();
            return min + (max - min) * rand.NextDouble();
        }
        public static double RandomNormal(double μ, double σ, Random? rand = null)//产生正态分布随机数
        {
            rand ??= new();
            double r1 = rand.NextDouble();
            double r2 = rand.NextDouble();
            double standardNormal = Math.Sqrt(-2 * Math.Log(r1)) * Math.Sin(2 * Math.PI * r2);
            return standardNormal * σ + μ;
        }
        public static double RandomNormalRangeApproximate(double min, double max, double μ, double σ, Random? rand = null, double width = 3) {
            double value = RandomNormal(μ, σ, rand);
            return value.ClampWithTanh(min, max, width);
        }
        /// <summary>
        /// 拟正态分布(但完全不像)
        /// 置若罔闻, 不堪回首
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="μ">峰值</param>
        /// <param name="sharpness">尖锐度, 此值越大随机结果越集中, 为0时为平均分布</param>
        /// <returns></returns>
        public static double RandomDistribution(double min, double max, double μ, double sharpness, Random? rand = null) {
            if (sharpness == 0) {
                return RandomAverage(min, max, rand);
            }
            return RandomNormalRangeApproximate(min, max, μ, Math.Max(Math.Abs(min - μ), Math.Abs(max - μ)) / sharpness, rand);
        }
        public static void RandomDistrubutionTest(double μ, double sharpness, Random? rand = null) {
            rand ??= new();
            int[] bottles = new int[11];
            for (int i = 0; i < 10000; ++i) {
                bottles[(int)RandomDistribution(0, 10, μ, sharpness, rand)] += 1;
            }
            for (int i = 0; i < 11; ++i) {
                Console.WriteLine("{0,-2}: {1}", i, bottles[i]);
            }
        }
        public static double Normal(double x, double μ, double σ) //正态分布概率密度函数
        {
            return 1 / (Math.Sqrt(2 * Math.PI) * σ) * Math.Exp((μ - x) * (x - μ) / (2 * σ * σ));
        }
        /// <summary>
        /// 将double转化为int
        /// 其中小数部分按概率转化为0或1
        /// </summary>
        public static int RandomD2I(double x, Random rand) {
            int floor = (int)Math.Floor(x);
            double delta = x - floor;
            return rand.NextDouble() < delta ? floor + 1 : floor;
        }
        /// <summary>
        /// 将double转化为bool
        /// 当大于1时为真, 小于0时为假
        /// 在中间则按概率
        /// </summary>
        /// <param name="x"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static bool RandomD2B(double x, Random rand) {
            return x > 1 - rand.NextDouble();
        }
        #region 随机多个总和固定的非负数 RandomNonnegetivesWithFixedSum
        public static void RandomNonnegetivesWithFixedSum(int[] result, int sum, Random? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            result[0] = rand.Next(int.MaxValue / count);
            for (int i = 1; i < count; ++i)
                result[i] = result[i - 1] + rand.Next(int.MaxValue / count);
            int newSum = result[count - 1];
            if (newSum == 0)
                return;
            double m = (double)sum / newSum;
            for (int i = 0; i < count - 1; ++i)
                result[i] = (int)Math.Round(result[i] * m);
            result[count - 1] = sum;
            for (int i = count - 1; i >= 1; --i)
                result[i] -= result[i - 1];
        }
        public static void RandomNonnegetivesWithFixedSum(int[] result, float[] weights, int sum, Random? rand = null) {
            int count = Math.Min(result.Length, weights.Length);
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            float[] rs = new float[count];
            rs[0] = rand.NextSingle() * weights[0];
            for (int i = 1; i < count; ++i)
                rs[i] = rs[i - 1] + rand.NextSingle() * weights[i];
            float newSum = result[count - 1];
            if (newSum == 0)
                return;
            float m = sum / newSum;
            for (int i = 0; i < count - 1; ++i)
                result[i] = (int)MathF.Round(rs[i] * m);
            rs[count - 1] = sum;
            for (int i = count - 1; i >= 1; --i)
                result[i] -= result[i - 1];
        }
        public static void RandomNonnegetivesWithFixedSum(int[] result, Func<int, float> weightByIndex, int sum, Random? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            float[] rs = new float[count];
            rs[0] = rand.NextSingle() * weightByIndex(0);
            for (int i = 1; i < count; ++i)
                rs[i] = rs[i - 1] + rand.NextSingle() * weightByIndex(i);
            float newSum = result[count - 1];
            if (newSum == 0)
                return;
            float m = sum / newSum;
            for (int i = 0; i < count - 1; ++i)
                result[i] = (int)MathF.Round(rs[i] * m);
            rs[count - 1] = sum;
            for (int i = count - 1; i >= 1; --i)
                result[i] -= result[i - 1];
        }
        public static void RandomNonnegetivesWithFixedSum(int[] result, double[] weights, int sum, Random? rand = null) {
            int count = Math.Min(result.Length, weights.Length);
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            double[] rs = new double[count];
            rs[0] = rand.NextDouble() * weights[0];
            for (int i = 1; i < count; ++i)
                rs[i] = rs[i - 1] + rand.NextDouble() * weights[i];
            double newSum = result[count - 1];
            if (newSum == 0)
                return;
            double m = sum / newSum;
            for (int i = 0; i < count - 1; ++i)
                result[i] = (int)Math.Round(rs[i] * m);
            rs[count - 1] = sum;
            for (int i = count - 1; i >= 1; --i)
                result[i] -= result[i - 1];
        }
        public static void RandomNonnegetivesWithFixedSum(int[] result, Func<int, double> weightByIndex, int sum, Random? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            double[] rs = new double[count];
            rs[0] = rand.NextDouble() * weightByIndex(0);
            for (int i = 1; i < count; ++i)
                rs[i] = rs[i - 1] + rand.NextDouble() * weightByIndex(i);
            double newSum = result[count - 1];
            if (newSum == 0)
                return;
            double m = sum / newSum;
            for (int i = 0; i < count - 1; ++i)
                result[i] = (int)Math.Round(rs[i] * m);
            rs[count - 1] = sum;
            for (int i = count - 1; i >= 1; --i)
                result[i] -= result[i - 1];
        }
        public static void RandomNonnegetivesWithFixedSum(float[] result, float sum = 1f, Random? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            float newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextSingle();
            if (newSum == 0)
                return;
            float m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static void RandomNonnegetivesWithFixedSum(float[] result, float[] weights, float sum = 1f, Random? rand = null) {
            int count = Math.Min(result.Length, weights.Length);
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            float newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextSingle() * weights[i];
            if (newSum == 0)
                return;
            float m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static void RandomNonnegetivesWithFixedSum(float[] result, Func<int, float> weightByIndex, float sum = 1f, Random? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            float newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextSingle() * weightByIndex(i);
            if (newSum == 0)
                return;
            float m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static void RandomNonnegetivesWithFixedSum(double[] result, double sum = 1.0, Random? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            double newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextDouble();
            if (newSum == 0)
                return;
            double m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static void RandomNonnegetivesWithFixedSum(double[] result, double[] weights, double sum = 1.0, Random? rand = null) {
            int count = Math.Min(result.Length, weights.Length);
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            double newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextDouble() * weights[i];
            if (newSum == 0)
                return;
            double m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static void RandomNonnegetivesWithFixedSum(double[] result, Func<int, double> weightByIndex, double sum = 1.0, Random? rand = null) {
            int count = result.Length;
            if (count <= 0)
                return;
            if (sum <= 0) {
                for (int i = 0; i < count; ++i)
                    result[i] = 0;
                return;
            }
            rand ??= new();
            double newSum = 0;
            for (int i = 0; i < count; ++i)
                newSum += result[i] = rand.NextDouble() * weightByIndex(i);
            if (newSum == 0)
                return;
            double m = sum / newSum;
            for (int i = 0; i < count; ++i) {
                result[i] *= m;
            }
        }
        public static int[] RandomNonnegetivesWithFixedSum(int count, int sum, Random? rand = null) {
            int[] result = new int[count];
            RandomNonnegetivesWithFixedSum(result, sum, rand);
            return result;
        }
        public static int[] RandomNonnegetivesWithFixedSum(int count, float[] weights, int sum, Random? rand = null) {
            int[] result = new int[count];
            RandomNonnegetivesWithFixedSum(result, weights, sum, rand);
            return result;
        }
        public static int[] RandomNonnegetivesWithFixedSum(int count, Func<int, float> weightByIndex, int sum, Random? rand = null) {
            int[] result = new int[count];
            RandomNonnegetivesWithFixedSum(result, weightByIndex, sum, rand);
            return result;
        }
        public static int[] RandomNonnegetivesWithFixedSum(int count, double[] weights, int sum, Random? rand = null) {
            int[] result = new int[count];
            RandomNonnegetivesWithFixedSum(result, weights, sum, rand);
            return result;
        }
        public static int[] RandomNonnegetivesWithFixedSum(int count, Func<int, double> weightByIndex, int sum, Random? rand = null) {
            int[] result = new int[count];
            RandomNonnegetivesWithFixedSum(result, weightByIndex, sum, rand);
            return result;
        }
        public static float[] RandomNonnegetivesWithFixedSum(int count, float sum, Random? rand = null) {
            float[] result = new float[count];
            RandomNonnegetivesWithFixedSum(result, sum, rand);
            return result;
        }
        public static float[] RandomNonnegetivesWithFixedSum(int count, float[] weights, float sum, Random? rand = null) {
            float[] result = new float[count];
            RandomNonnegetivesWithFixedSum(result, weights, sum, rand);
            return result;
        }
        public static float[] RandomNonnegetivesWithFixedSum(int count, Func<int, float> weightByIndex, float sum, Random? rand = null) {
            float[] result = new float[count];
            RandomNonnegetivesWithFixedSum(result, weightByIndex, sum, rand);
            return result;
        }
        public static double[] RandomNonnegetivesWithFixedSum(int count, double sum = 1.0, Random? rand = null) {
            double[] result = new double[count];
            RandomNonnegetivesWithFixedSum(result, sum, rand);
            return result;
        }
        public static double[] RandomNonnegetivesWithFixedSum(int count, double[] weights, double sum = 1.0, Random? rand = null) {
            double[] result = new double[count];
            RandomNonnegetivesWithFixedSum(result, weights, sum, rand);
            return result;
        }
        public static double[] RandomNonnegetivesWithFixedSum(int count, Func<int, double> weightByIndex, double sum = 1.0, Random? rand = null) {
            double[] result = new double[count];
            RandomNonnegetivesWithFixedSum(result, weightByIndex, sum, rand);
            return result;
        }
        #endregion
    }
    public static T RerollIf<T>(Func<T> randomFunc, params Func<T, bool>[] conditions) {
        T t = randomFunc();
        foreach (var condition in conditions) {
            if (condition(t)) {
                t = randomFunc();
            }
        }
        return t;
    }
    public static T RerollIf<T>(Func<T> randomFunc, params bool[] conditions) {
        T t = randomFunc();
        foreach (var condition in conditions) {
            if (condition) {
                t = randomFunc();
            }
        }
        return t;
    }
    #endregion
    #region 一些数学运算(约等于和取模)
    /// <summary>
    /// 约等于
    /// 实际返回两者之差是否在<paramref name="tolerance"/>之内
    /// </summary>
    public static bool RoughEqual(float a, float b, float tolerance = .01f) {
        return MathF.Abs(a - b) <= tolerance;
    }
    #region 取模
    public enum ModularType {
        /// <summary>
        /// 返回非负数
        /// </summary>
        Possitive,
        /// <summary>
        /// 与除数的符号相同
        /// </summary>
        WithB,
        /// <summary>
        /// 与被除数的符号相同(也是%取余的模式)
        /// </summary>
        WithA,
        /// <summary>
        /// 返回非正数
        /// </summary>
        Negative,
    }
    /// <summary>
    /// 取余, 默认为返回非负数
    /// </summary>
    public static int Modular(int a, int b, ModularType type = ModularType.Possitive) {
        int result = a % b;
        return type switch {
            ModularType.Possitive => result < 0 ? result + Math.Abs(b) : result,
            ModularType.WithB => (result ^ b) < 0 ? result + b : result,
            ModularType.WithA => result,
            ModularType.Negative => result > 0 ? result - Math.Abs(b) : result,
            _ => result,
        };
    }
    /// <summary>
    /// 取余, 默认为返回非负数
    /// </summary>
    public static long Modular(long a, long b, ModularType type = ModularType.Possitive) {
        long result = a % b;
        return type switch {
            ModularType.Possitive => result < 0 ? result + Math.Abs(b) : result,
            ModularType.WithB => (result ^ b) < 0 ? result + b : result,
            ModularType.WithA => result,
            ModularType.Negative => result > 0 ? result - Math.Abs(b) : result,
            _ => result,
        };
    }
    /// <summary>
    /// 取余, 默认为返回非负数
    /// </summary>
    public static short Modular(short a, short b, ModularType type = ModularType.Possitive) {
        short result = (short)(a % b);
        return type switch {
            ModularType.Possitive => result < 0 ? (short)(result + Math.Abs(b)) : result,
            ModularType.WithB => (result ^ b) < 0 ? (short)(result + b) : result,
            ModularType.WithA => result,
            ModularType.Negative => result > 0 ? (short)(result - Math.Abs(b)) : result,
            _ => result,
        };
    }
    /// <summary>
    /// 取余, 默认为返回非负数
    /// </summary>
    public static sbyte Modular(sbyte a, sbyte b, ModularType type = ModularType.Possitive) {
        sbyte result = (sbyte)(a % b);
        return type switch {
            ModularType.Possitive => result < 0 ? (sbyte)(result + Math.Abs(b)) : result,
            ModularType.WithB => (result ^ b) < 0 ? (sbyte)(result + b) : result,
            ModularType.WithA => result,
            ModularType.Negative => result > 0 ? (sbyte)(result - Math.Abs(b)) : result,
            _ => result,
        };
    }
    /// <summary>
    /// 取余, 默认为返回非负数
    /// </summary>
    public static float Modular(float a, float b, ModularType type = ModularType.Possitive) {
        float result = a % b;
        return type switch {
            ModularType.Possitive => result < 0 ? result + Math.Abs(b) : result,
            ModularType.WithB => result * b < 0 ? result + b : result,
            ModularType.WithA => result,
            ModularType.Negative => result > 0 ? result - Math.Abs(b) : result,
            _ => result,
        };
    }
    /// <summary>
    /// 取余, 默认为返回非负数
    /// </summary>
    public static double Modular(double a, double b, ModularType type = ModularType.Possitive) {
        double result = a % b;
        return type switch {
            ModularType.Possitive => result < 0 ? result + Math.Abs(b) : result,
            ModularType.WithB => result * b < 0 ? result + b : result,
            ModularType.WithA => result,
            ModularType.Negative => result > 0 ? result - Math.Abs(b) : result,
            _ => result,
        };
    }
    //byte, ushort, uint, ulong 就直接用 % 就可以了, 也不用担心符号问题
    public static decimal Modular(decimal a, decimal b, ModularType type = ModularType.Possitive) {
        decimal result = a % b;
        return type switch {
            ModularType.Possitive => result < 0 ? result + Math.Abs(b) : result,
            ModularType.WithB => result * b < 0 ? result + b : result,
            ModularType.WithA => result,
            ModularType.Negative => result > 0 ? result - Math.Abs(b) : result,
            _ => result,
        };
    }
    #endregion
    #endregion
    #region Min / Max (包括带有多个值)
    public static T Min<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) > 0 ? b : a;
    public static T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) < 0 ? b : a;
    public static T Min<T>(T a, T b, params T[] others) where T : IComparable<T> {
        T result = a;
        if (result.CompareTo(b) > 0) {
            result = b;
        }
        foreach (T other in others) {
            if (result.CompareTo(other) > 0) {
                result = other;
            }
        }
        return result;
    }
    public static T Max<T>(T a, T b, params T[] others) where T : IComparable<T> {
        T result = a;
        if (result.CompareTo(b) < 0) {
            result = b;
        }
        foreach (T other in others) {
            if (result.CompareTo(other) < 0) {
                result = other;
            }
        }
        return result;
    }
    #endregion
    #region Rectangle
    public static Rectangle NewRectangle(Vector2 position, Vector2 size, Vector2 anchor = default)
        => NewRectangle(position.X, position.Y, size.X, size.Y, anchor.X, anchor.Y);
    public static Rectangle NewRectangle(int x, int y, int width, int height, float anchorX, float anchorY)
        => new((int)(x - anchorX * width), (int)(y - anchorY * height), width, height);
    public static Rectangle NewRectangle(float x, float y, float width, float height, float anchorX, float anchorY)
        => new((int)(x - anchorX * width), (int)(y - anchorY * height), (int)width, (int)height);
    public static Rectangle NewRectangle(float x, float y, float width, float height) => new((int)x, (int)y, (int)width, (int)height);
    #endregion
    #region 流程简化
    #region Do
    /// <summary>
    /// 什么也不做, 返回false
    /// </summary>
    public static bool Do(object? expression) {
        _ = expression;
        return false;
    }

    #region 不带返回值的
    /// <summary>
    /// 执行<paramref name="action"/>
    /// </summary>
    /// <returns>false</returns>
    public static bool Do(Action action) { action(); return false; }
    public static bool Do<T>(Action<T> action, T t) { action(t); return false; }
    public static bool Do<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { action(t1, t2); return false; }
    public static bool Do<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { action(t1, t2, t3); return false; }
    public static bool Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { action(t1, t2, t3, t4); return false; }
    public static bool Do<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) { action(t1, t2, t3, t4, t5); return false; }
    public static bool Do<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) { action(t1, t2, t3, t4, t5, t6); return false; }
    public static bool Do<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) { action(t1, t2, t3, t4, t5, t6, t7); return false; }
    public static bool Do<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) { action(t1, t2, t3, t4, t5, t6, t7, t8); return false; }
    public static bool Do<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) { action(t1, t2, t3, t4, t5, t6, t7, t8, t9); return false; }
    #endregion
    #region 带有返回值的
    public static bool Do<TResult>(Func<TResult> action) => GetRight(action(), false);
    public static bool Do<TResult, T>(Func<T, TResult> action, T t) => GetRight(action(t), false);    
    public static bool Do<TResult, T1, T2>(Func<T1, T2, TResult> action, T1 t1, T2 t2) => GetRight(action(t1, t2), false);    
    public static bool Do<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> action, T1 t1, T2 t2, T3 t3) => GetRight(action(t1, t2, t3), false);    
    public static bool Do<TResult, T1, T2, T3, T4>(Func<T1, T2, T3, T4, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4) => GetRight(action(t1, t2, t3, t4), false);    
    public static bool Do<TResult, T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => GetRight(action(t1, t2, t3, t4, t5), false);    
    public static bool Do<TResult, T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => GetRight(action(t1, t2, t3, t4, t5, t6), false);    
    public static bool Do<TResult, T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => GetRight(action(t1, t2, t3, t4, t5, t6, t7), false);    
    public static bool Do<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) => GetRight(action(t1, t2, t3, t4, t5, t6, t7, t8), false);    
    public static bool Do<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) => GetRight(action(t1, t2, t3, t4, t5, t6, t7, t8, t9), false);
    #endregion
    #region 有一个out参数而不带返回值的
    public static bool Do<TOut>(VoidOut1Delegate<TOut> outDelegate, out TOut @out) { outDelegate(out @out); return false; }
    public static bool Do<TOut, T2>(VoidOut1Delegate<TOut, T2> outDelegate, out TOut @out, T2 t2) { outDelegate(out @out, t2); return false; }
    public static bool Do<T1, TOut>(VoidOut2Delegate<T1, TOut> outDelegate, T1 t1, out TOut @out) { outDelegate(t1, out @out); return false; }
    public static bool Do<TOut, T2, T3>(VoidOut1Delegate<TOut, T2, T3> outDelegate, out TOut @out, T2 t2, T3 t3) { outDelegate(out @out, t2, t3); return false; }
    public static bool Do<T1, TOut, T3>(VoidOut2Delegate<T1, TOut, T3> outDelegate, T1 t1, out TOut @out, T3 t3) { outDelegate(t1, out @out, t3); return false; }
    public static bool Do<T1, T2, TOut>(VoidOut3Delegate<T1, T2, TOut> outDelegate, T1 t1, T2 t2, out TOut @out) { outDelegate(t1, t2, out @out); return false; }
    public static bool Do<TOut, T2, T3, T4>(VoidOut1Delegate<TOut, T2, T3, T4> outDelegate, out TOut @out, T2 t2, T3 t3, T4 t4) { outDelegate(out @out, t2, t3, t4); return false; }
    public static bool Do<T1, TOut, T3, T4>(VoidOut2Delegate<T1, TOut, T3, T4> outDelegate, T1 t1, out TOut @out, T3 t3, T4 t4) { outDelegate(t1, out @out, t3, t4); return false; }
    public static bool Do<T1, T2, TOut, T4>(VoidOut3Delegate<T1, T2, TOut, T4> outDelegate, T1 t1, T2 t2, out TOut @out, T4 t4) { outDelegate(t1, t2, out @out, t4); return false; }
    public static bool Do<T1, T2, T3, TOut>(VoidOut4Delegate<T1, T2, T3, TOut> outDelegate, T1 t1, T2 t2, T3 t3, out TOut @out) { outDelegate(t1, t2, t3, out @out); return false; }
    #endregion
    #region 有一个out参数且带有返回值的
    public static bool Do<TOut, TResult>(Out1Delegate<TOut, TResult> outDelegate, out TOut @out) => GetRight(outDelegate(out @out), false);
    public static bool Do<TOut, T2, TResult>(Out1Delegate<TOut, T2, TResult> outDelegate, out TOut @out, T2 t2) => GetRight(outDelegate(out @out, t2), false);
    public static bool Do<T1, TOut, TResult>(Out2Delegate<T1, TOut, TResult> outDelegate, T1 t1, out TOut @out) => GetRight(outDelegate(t1, out @out), false);
    public static bool Do<TOut, T2, T3, TResult>(Out1Delegate<TOut, T2, T3, TResult> outDelegate, out TOut @out, T2 t2, T3 t3) => GetRight(outDelegate(out @out, t2, t3), false);
    public static bool Do<T1, TOut, T3, TResult>(Out2Delegate<T1, TOut, T3, TResult> outDelegate, T1 t1, out TOut @out, T3 t3) => GetRight(outDelegate(t1, out @out, t3), false);
    public static bool Do<T1, T2, TOut, TResult>(Out3Delegate<T1, T2, TOut, TResult> outDelegate, T1 t1, T2 t2, out TOut @out) => GetRight(outDelegate(t1, t2, out @out), false);
    public static bool Do<TOut, T2, T3, T4, TResult>(Out1Delegate<TOut, T2, T3, T4, TResult> outDelegate, out TOut @out, T2 t2, T3 t3, T4 t4) => GetRight(outDelegate(out @out, t2, t3, t4), false);
    public static bool Do<T1, TOut, T3, T4, TResult>(Out2Delegate<T1, TOut, T3, T4, TResult> outDelegate, T1 t1, out TOut @out, T3 t3, T4 t4) => GetRight(outDelegate(t1, out @out, t3, t4), false);
    public static bool Do<T1, T2, TOut, T4, TResult>(Out3Delegate<T1, T2, TOut, T4, TResult> outDelegate, T1 t1, T2 t2, out TOut @out, T4 t4) => GetRight(outDelegate(t1, t2, out @out, t4), false);
    public static bool Do<T1, T2, T3, TOut, TResult>(Out4Delegate<T1, T2, T3, TOut, TResult> outDelegate, T1 t1, T2 t2, T3 t3, out TOut @out) => GetRight(outDelegate(t1, t2, t3, out @out), false);
    #endregion
    #endregion
    #region ToDo
    public static Action ToDo<T>(Action<T> action, T t)
        => () => action.Invoke(t);
    public static Action ToDo<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2)
        => () => action.Invoke(t1, t2);
    public static Action ToDo<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        => () => action.Invoke(t1, t2, t3);
    public static Action ToDo<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
        => () => action.Invoke(t1, t2, t3, t4);
    public static Action ToDo<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        => () => action.Invoke(t1, t2, t3, t4, t5);
    public static Action ToDo<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6);
    public static Action ToDo<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6, t7);
    public static Action ToDo<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
    public static Action ToDo<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);

    public static Action ToDo<TResult, T>(Func<T, TResult> action, T t)
        => () => action.Invoke(t);
    public static Action ToDo<TResult, T1, T2>(Func<T1, T2, TResult> action, T1 t1, T2 t2)
        => () => action.Invoke(t1, t2);
    public static Action ToDo<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> action, T1 t1, T2 t2, T3 t3)
        => () => action.Invoke(t1, t2, t3);
    public static Action ToDo<TResult, T1, T2, T3, T4>(Func<T1, T2, T3, T4, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4)
        => () => action.Invoke(t1, t2, t3, t4);
    public static Action ToDo<TResult, T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        => () => action.Invoke(t1, t2, t3, t4, t5);
    public static Action ToDo<TResult, T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6);
    public static Action ToDo<TResult, T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6, t7);
    public static Action ToDo<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
    public static Action ToDo<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    #endregion
    #region Get
    /// <summary>
    /// 获得<paramref name="action"/>的返回值
    /// </summary>
    public static TResult Get<TResult>(Func<TResult> action)
        => action.Invoke();
    public static TResult Get<TResult, T>(Func<T, TResult> action, T t)
        => action.Invoke(t);
    public static TResult Get<TResult, T1, T2>(Func<T1, T2, TResult> action, T1 t1, T2 t2)
        => action.Invoke(t1, t2);
    public static TResult Get<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> action, T1 t1, T2 t2, T3 t3)
        => action.Invoke(t1, t2, t3);
    public static TResult Get<TResult, T1, T2, T3, T4>(Func<T1, T2, T3, T4, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4)
        => action.Invoke(t1, t2, t3, t4);
    public static TResult Get<TResult, T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        => action.Invoke(t1, t2, t3, t4, t5);
    public static TResult Get<TResult, T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        => action.Invoke(t1, t2, t3, t4, t5, t6);
    public static TResult Get<TResult, T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        => action.Invoke(t1, t2, t3, t4, t5, t6, t7);
    public static TResult Get<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        => action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
    public static TResult Get<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9)
        => action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);

    public static void Get<TResult>(Func<TResult> action, out TResult value)
        => value = action.Invoke();
    public static void Get<TResult, T>(Func<T, TResult> action, T t, out TResult value)
        => value = action.Invoke(t);
    public static void Get<TResult, T1, T2>(Func<T1, T2, TResult> action, T1 t1, T2 t2, out TResult value)
        => value = action.Invoke(t1, t2);
    public static void Get<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> action, T1 t1, T2 t2, T3 t3, out TResult value)
        => value = action.Invoke(t1, t2, t3);
    public static void Get<TResult, T1, T2, T3, T4>(Func<T1, T2, T3, T4, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, out TResult value)
        => value = action.Invoke(t1, t2, t3, t4);
    public static void Get<TResult, T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, out TResult value)
        => value = action.Invoke(t1, t2, t3, t4, t5);
    public static void Get<TResult, T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, out TResult value)
        => value = action.Invoke(t1, t2, t3, t4, t5, t6);
    public static void Get<TResult, T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, out TResult value)
        => value = action.Invoke(t1, t2, t3, t4, t5, t6, t7);
    public static void Get<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, out TResult value)
        => value = action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
    public static void Get<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, out TResult value)
        => value = action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    #endregion
    #region ToGet
    public static Func<TResult> ToGet<TResult, T>(Func<T, TResult> action, T t)
        => () => action.Invoke(t);
    public static Func<TResult> ToGet<TResult, T1, T2>(Func<T1, T2, TResult> action, T1 t1, T2 t2)
        => () => action.Invoke(t1, t2);
    public static Func<TResult> ToGet<TResult, T1, T2, T3>(Func<T1, T2, T3, TResult> action, T1 t1, T2 t2, T3 t3)
        => () => action.Invoke(t1, t2, t3);
    public static Func<TResult> ToGet<TResult, T1, T2, T3, T4>(Func<T1, T2, T3, T4, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4)
        => () => action.Invoke(t1, t2, t3, t4);
    public static Func<TResult> ToGet<TResult, T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        => () => action.Invoke(t1, t2, t3, t4, t5);
    public static Func<TResult> ToGet<TResult, T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6);
    public static Func<TResult> ToGet<TResult, T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6, t7);
    public static Func<TResult> ToGet<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
    public static Func<TResult> ToGet<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9)
        => () => action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    #endregion
    #region ToGetOut
    #region 带有返回值的
    public static Func<TOut> ToGetOut<TResult, TOut>(OutDelegate<TOut, TResult> outDelegate) => () => GetRight(outDelegate(out var @out), @out);
    public static Func<TOut> ToGetOut<TResult, TOut, T2>(Out1Delegate<TOut, T2, TResult> outDelegate, T2 t2) => () => GetRight(outDelegate(out var @out, t2), @out);
    public static Func<TOut> ToGetOut<TResult, T1, TOut>(Out2Delegate<T1, TOut, TResult> outDelegate, T1 t1) => () => GetRight(outDelegate(t1, out var @out), @out);
    public static Func<TOut> ToGetOut<TResult, TOut, T2, T3>(Out1Delegate<TOut, T2, T3, TResult> outDelegate, T2 t2, T3 t3) => () => GetRight(outDelegate(out var @out, t2, t3), @out);
    public static Func<TOut> ToGetOut<TResult, T1, TOut, T3>(Out2Delegate<T1, TOut, T3, TResult> outDelegate, T1 t1, T3 t3) => () => GetRight(outDelegate(t1, out var @out, t3), @out);
    public static Func<TOut> ToGetOut<TResult, T1, T2, TOut>(Out3Delegate<T1, T2, TOut, TResult> outDelegate, T1 t1, T2 t2) => () => GetRight(outDelegate(t1, t2, out var @out), @out);
    public static Func<TOut> ToGetOut<TResult, TOut, T2, T3, T4>(Out1Delegate<TOut, T2, T3, T4, TResult> outDelegate, T2 t2, T3 t3, T4 t4) => () => GetRight(outDelegate(out var @out, t2, t3, t4), @out);
    public static Func<TOut> ToGetOut<TResult, T1, TOut, T3, T4>(Out2Delegate<T1, TOut, T3, T4, TResult> outDelegate, T1 t1, T3 t3, T4 t4) => () => GetRight(outDelegate(t1, out var @out, t3, t4), @out);
    public static Func<TOut> ToGetOut<TResult, T1, T2, TOut, T4>(Out3Delegate<T1, T2, TOut, T4, TResult> outDelegate, T1 t1, T2 t2, T4 t4) => () => GetRight(outDelegate(t1, t2, out var @out, t4), @out);
    public static Func<TOut> ToGetOut<TResult, T1, T2, T3, TOut>(Out4Delegate<T1, T2, T3, TOut, TResult> outDelegate, T1 t1, T2 t2, T3 t3) => () => GetRight(outDelegate(t1, t2, t3, out var @out), @out);
    #endregion
    #region 不带返回值的
    public static Func<TOut> ToGetOut<TResult, TOut>(VoidOut1Delegate<TOut> outDelegate) => () => GetRight(Do(outDelegate, out var @out), @out);
    public static Func<TOut> ToGetOut<TResult, TOut, T2>(VoidOut1Delegate<TOut, T2> outDelegate, T2 t2) => () => GetRight(Do(outDelegate, out var @out, t2), @out);
    public static Func<TOut> ToGetOut<TResult, T1, TOut>(VoidOut2Delegate<T1, TOut> outDelegate, T1 t1) => () => GetRight(Do(outDelegate, t1, out var @out), @out);
    public static Func<TOut> ToGetOut<TResult, TOut, T2, T3>(VoidOut1Delegate<TOut, T2, T3> outDelegate, T2 t2, T3 t3) => () => GetRight(Do(outDelegate, out var @out, t2, t3), @out);
    public static Func<TOut> ToGetOut<TResult, T1, TOut, T3>(VoidOut2Delegate<T1, TOut, T3> outDelegate, T1 t1, T3 t3) => () => GetRight(Do(outDelegate, t1, out var @out, t3), @out);
    public static Func<TOut> ToGetOut<TResult, T1, T2, TOut>(VoidOut3Delegate<T1, T2, TOut> outDelegate, T1 t1, T2 t2) => () => GetRight(Do(outDelegate, t1, t2, out var @out), @out);
    public static Func<TOut> ToGetOut<TResult, TOut, T2, T3, T4>(VoidOut1Delegate<TOut, T2, T3, T4> outDelegate, T2 t2, T3 t3, T4 t4) => () => GetRight(Do(outDelegate, out var @out, t2, t3, t4), @out);
    public static Func<TOut> ToGetOut<TResult, T1, TOut, T3, T4>(VoidOut2Delegate<T1, TOut, T3, T4> outDelegate, T1 t1, T3 t3, T4 t4) => () => GetRight(Do(outDelegate, t1, out var @out, t3, t4), @out);
    public static Func<TOut> ToGetOut<TResult, T1, T2, TOut, T4>(VoidOut3Delegate<T1, T2, TOut, T4> outDelegate, T1 t1, T2 t2, T4 t4) => () => GetRight(Do(outDelegate, t1, t2, out var @out, t4), @out);
    public static Func<TOut> ToGetOut<TResult, T1, T2, T3, TOut>(VoidOut4Delegate<T1, T2, T3, TOut> outDelegate, T1 t1, T2 t2, T3 t3) => () => GetRight(Do(outDelegate, t1, t2, t3, out var @out), @out);
    #endregion
    #endregion
    #region ToGetResultAndOut
    public static Func<(TResult, TOut)> ToGetResultAndOut<TResult, TOut>(OutDelegate<TOut, TResult> outDelegate) => () => (outDelegate(out var @out), @out);
    public static Func<(TResult, TOut)> ToGetResultAndOut<TResult, TOut, T2>(Out1Delegate<TOut, T2, TResult> outDelegate, T2 t2) => () => (outDelegate(out var @out, t2), @out);
    public static Func<(TResult, TOut)> ToGetResultAndOut<TResult, T1, TOut>(Out2Delegate<T1, TOut, TResult> outDelegate, T1 t1) => () => (outDelegate(t1, out var @out), @out);
    public static Func<(TResult, TOut)> ToGetResultAndOut<TResult, TOut, T2, T3>(Out1Delegate<TOut, T2, T3, TResult> outDelegate, T2 t2, T3 t3) => () => (outDelegate(out var @out, t2, t3), @out);
    public static Func<(TResult, TOut)> ToGetResultAndOut<TResult, T1, TOut, T3>(Out2Delegate<T1, TOut, T3, TResult> outDelegate, T1 t1, T3 t3) => () => (outDelegate(t1, out var @out, t3), @out);
    public static Func<(TResult, TOut)> ToGetResultAndOut<TResult, T1, T2, TOut>(Out3Delegate<T1, T2, TOut, TResult> outDelegate, T1 t1, T2 t2) => () => (outDelegate(t1, t2, out var @out), @out);
    public static Func<(TResult, TOut)> ToGetResultAndOut<TResult, TOut, T2, T3, T4>(Out1Delegate<TOut, T2, T3, T4, TResult> outDelegate, T2 t2, T3 t3, T4 t4) => () => (outDelegate(out var @out, t2, t3, t4), @out);
    public static Func<(TResult, TOut)> ToGetResultAndOut<TResult, T1, TOut, T3, T4>(Out2Delegate<T1, TOut, T3, T4, TResult> outDelegate, T1 t1, T3 t3, T4 t4) => () => (outDelegate(t1, out var @out, t3, t4), @out);
    public static Func<(TResult, TOut)> ToGetResultAndOut<TResult, T1, T2, TOut, T4>(Out3Delegate<T1, T2, TOut, T4, TResult> outDelegate, T1 t1, T2 t2, T4 t4) => () => (outDelegate(t1, t2, out var @out, t4), @out);
    public static Func<(TResult, TOut)> ToGetResultAndOut<TResult, T1, T2, T3, TOut>(Out4Delegate<T1, T2, T3, TOut, TResult> outDelegate, T1 t1, T2 t2, T3 t3) => () => (outDelegate(t1, t2, t3, out var @out), @out);
    #endregion
    #region 流程控制 - 条件
    #region DoIf
    /// <summary>
    /// 若<paramref name="condition"/>为<see langword="true"/>则调用<paramref name="action"/>.
    /// </summary>
    /// <returns><paramref name="condition"/></returns>
    public static bool DoIf(bool condition, Action action) {
        if (condition) {
            action.Invoke();
        }
        return condition;
    }
    public static bool DoIf<T>(bool condition, Action<T> action, T t) {
        if (condition) {
            action.Invoke(t);
        }
        return condition;
    }
    public static bool DoIf<T1, T2>(bool condition, Action<T1, T2> action, T1 t1, T2 t2) {
        if (condition) {
            action.Invoke(t1, t2);
        }
        return condition;
    }
    public static bool DoIf<T1, T2, T3>(bool condition, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) {
        if (condition) {
            action.Invoke(t1, t2, t3);
        }
        return condition;
    }
    public static bool DoIf<T1, T2, T3, T4>(bool condition, Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4);
        }
        return condition;
    }
    public static bool DoIf<T1, T2, T3, T4, T5>(bool condition, Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5);
        }
        return condition;
    }
    public static bool DoIf<T1, T2, T3, T4, T5, T6>(bool condition, Action<T1, T2, T3, T4, T5, T6> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6);
        }
        return condition;
    }
    public static bool DoIf<T1, T2, T3, T4, T5, T6, T7>(bool condition, Action<T1, T2, T3, T4, T5, T6, T7> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        return condition;
    }
    public static bool DoIf<T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        return condition;
    }
    public static bool DoIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        return condition;
    }

    public static bool DoIf<TResult>(bool condition, Func<TResult> action) {
        if (condition) {
            action.Invoke();
        }
        return condition;
    }
    public static bool DoIf<TResult, T>(bool condition, Func<T, TResult> action, T t) {
        if (condition) {
            action.Invoke(t);
        }
        return condition;
    }
    public static bool DoIf<TResult, T1, T2>(bool condition, Func<T1, T2, TResult> action, T1 t1, T2 t2) {
        if (condition) {
            action.Invoke(t1, t2);
        }
        return condition;
    }
    public static bool DoIf<TResult, T1, T2, T3>(bool condition, Func<T1, T2, T3, TResult> action, T1 t1, T2 t2, T3 t3) {
        if (condition) {
            action.Invoke(t1, t2, t3);
        }
        return condition;
    }
    public static bool DoIf<TResult, T1, T2, T3, T4>(bool condition, Func<T1, T2, T3, T4, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4);
        }
        return condition;
    }
    public static bool DoIf<TResult, T1, T2, T3, T4, T5>(bool condition, Func<T1, T2, T3, T4, T5, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5);
        }
        return condition;
    }
    public static bool DoIf<TResult, T1, T2, T3, T4, T5, T6>(bool condition, Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6);
        }
        return condition;
    }
    public static bool DoIf<TResult, T1, T2, T3, T4, T5, T6, T7>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        return condition;
    }
    public static bool DoIf<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        return condition;
    }
    public static bool DoIf<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        return condition;
    }
    #endregion
    #region GetIf
    public static TResult? GetIf<TResult>(bool condition, Func<TResult> action, TResult? defaultResult = default) {
        if (condition) {
            return action.Invoke();
        }
        return defaultResult;
    }
    public static TResult? GetIf<TResult, T>(bool condition, Func<T, TResult> action, T t, TResult? defaultResult = default) {
        if (condition) {
            return action.Invoke(t);
        }
        return defaultResult;
    }
    public static TResult? GetIf<TResult, T1, T2>(bool condition, Func<T1, T2, TResult> action, T1 t1, T2 t2, TResult? defaultResult = default) {
        if (condition) {
            return action.Invoke(t1, t2);
        }
        return defaultResult;
    }
    public static TResult? GetIf<TResult, T1, T2, T3>(bool condition, Func<T1, T2, T3, TResult> action, T1 t1, T2 t2, T3 t3, TResult? defaultResult = default) {
        if (condition) {
            return action.Invoke(t1, t2, t3);
        }
        return defaultResult;
    }
    public static TResult? GetIf<TResult, T1, T2, T3, T4>(bool condition, Func<T1, T2, T3, T4, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, TResult? defaultResult = default) {
        if (condition) {
            return action.Invoke(t1, t2, t3, t4);
        }
        return defaultResult;
    }
    public static TResult? GetIf<TResult, T1, T2, T3, T4, T5>(bool condition, Func<T1, T2, T3, T4, T5, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, TResult? defaultResult = default) {
        if (condition) {
            return action.Invoke(t1, t2, t3, t4, t5);
        }
        return defaultResult;
    }
    public static TResult? GetIf<TResult, T1, T2, T3, T4, T5, T6>(bool condition, Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, TResult? defaultResult = default) {
        if (condition) {
            return action.Invoke(t1, t2, t3, t4, t5, t6);
        }
        return defaultResult;
    }
    public static TResult? GetIf<TResult, T1, T2, T3, T4, T5, T6, T7>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, TResult? defaultResult = default) {
        if (condition) {
            return action.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        return defaultResult;
    }
    public static TResult? GetIf<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, TResult? defaultResult = default) {
        if (condition) {
            return action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        return defaultResult;
    }
    public static TResult? GetIf<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, TResult? defaultResult = default) {
        if (condition) {
            return action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        return defaultResult;
    }
    #endregion
    #region DoIfNot & GetIfNot
    /// <summary>
    /// 若<paramref name="condition"/>为假则调用<paramref name="action"/>.
    /// 相当与DoIf(!<paramref name="condition"/>, <paramref name="action"/>)
    /// </summary>
    /// <returns>返回!<paramref name="condition"/></returns>
    /// <summary>
    /// 若<paramref name="condition"/>为真则调用<paramref name="action"/>, 否则调用<paramref name="altAction"/>.
    /// <br/>若为表达式推荐使用<see cref="Do"/>配合三目运算符
    /// </summary>
    /// <returns>返回<paramref name="condition"/></returns>
    public static bool DoIfNot(bool condition, Action action) {
        if (!condition) {
            action.Invoke();
        }
        return !condition;
    }
    public static bool DoIfNot<T>(bool condition, Action<T> action, T t) {
        if (!condition) {
            action.Invoke(t);
        }
        return !condition;
    }
    public static bool DoIfNot<T1, T2>(bool condition, Action<T1, T2> action, T1 t1, T2 t2) {
        if (!condition) {
            action.Invoke(t1, t2);
        }
        return !condition;
    }
    public static bool DoIfNot<T1, T2, T3>(bool condition, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) {
        if (!condition) {
            action.Invoke(t1, t2, t3);
        }
        return !condition;
    }
    public static bool DoIfNot<T1, T2, T3, T4>(bool condition, Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4);
        }
        return !condition;
    }
    public static bool DoIfNot<T1, T2, T3, T4, T5>(bool condition, Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4, t5);
        }
        return !condition;
    }
    public static bool DoIfNot<T1, T2, T3, T4, T5, T6>(bool condition, Action<T1, T2, T3, T4, T5, T6> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6);
        }
        return !condition;
    }
    public static bool DoIfNot<T1, T2, T3, T4, T5, T6, T7>(bool condition, Action<T1, T2, T3, T4, T5, T6, T7> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        return !condition;
    }
    public static bool DoIfNot<T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        return !condition;
    }
    public static bool DoIfNot<T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        return !condition;
    }

    public static bool DoIfNot<TResult>(bool condition, Func<TResult> action) {
        if (!condition) {
            action.Invoke();
        }
        return !condition;
    }
    public static bool DoIfNot<TResult, T>(bool condition, Func<T, TResult> action, T t) {
        if (!condition) {
            action.Invoke(t);
        }
        return !condition;
    }
    public static bool DoIfNot<TResult, T1, T2>(bool condition, Func<T1, T2, TResult> action, T1 t1, T2 t2) {
        if (!condition) {
            action.Invoke(t1, t2);
        }
        return !condition;
    }
    public static bool DoIfNot<TResult, T1, T2, T3>(bool condition, Func<T1, T2, T3, TResult> action, T1 t1, T2 t2, T3 t3) {
        if (!condition) {
            action.Invoke(t1, t2, t3);
        }
        return !condition;
    }
    public static bool DoIfNot<TResult, T1, T2, T3, T4>(bool condition, Func<T1, T2, T3, T4, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4);
        }
        return !condition;
    }
    public static bool DoIfNot<TResult, T1, T2, T3, T4, T5>(bool condition, Func<T1, T2, T3, T4, T5, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4, t5);
        }
        return !condition;
    }
    public static bool DoIfNot<TResult, T1, T2, T3, T4, T5, T6>(bool condition, Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6);
        }
        return !condition;
    }
    public static bool DoIfNot<TResult, T1, T2, T3, T4, T5, T6, T7>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        return !condition;
    }
    public static bool DoIfNot<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        return !condition;
    }
    public static bool DoIfNot<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) {
        if (!condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        return !condition;
    }

    public static TResult? GetIfNot<TResult>(bool condition, Func<TResult> action, TResult? defaultResult = default) {
        if (!condition) {
            return action.Invoke();
        }
        return defaultResult;
    }
    public static TResult? GetIfNot<TResult, T>(bool condition, Func<T, TResult> action, T t, TResult? defaultResult = default) {
        if (!condition) {
            return action.Invoke(t);
        }
        return defaultResult;
    }
    public static TResult? GetIfNot<TResult, T1, T2>(bool condition, Func<T1, T2, TResult> action, T1 t1, T2 t2, TResult? defaultResult = default) {
        if (!condition) {
            return action.Invoke(t1, t2);
        }
        return defaultResult;
    }
    public static TResult? GetIfNot<TResult, T1, T2, T3>(bool condition, Func<T1, T2, T3, TResult> action, T1 t1, T2 t2, T3 t3, TResult? defaultResult = default) {
        if (!condition) {
            return action.Invoke(t1, t2, t3);
        }
        return defaultResult;
    }
    public static TResult? GetIfNot<TResult, T1, T2, T3, T4>(bool condition, Func<T1, T2, T3, T4, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, TResult? defaultResult = default) {
        if (!condition) {
            return action.Invoke(t1, t2, t3, t4);
        }
        return defaultResult;
    }
    public static TResult? GetIfNot<TResult, T1, T2, T3, T4, T5>(bool condition, Func<T1, T2, T3, T4, T5, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, TResult? defaultResult = default) {
        if (!condition) {
            return action.Invoke(t1, t2, t3, t4, t5);
        }
        return defaultResult;
    }
    public static TResult? GetIfNot<TResult, T1, T2, T3, T4, T5, T6>(bool condition, Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, TResult? defaultResult = default) {
        if (!condition) {
            return action.Invoke(t1, t2, t3, t4, t5, t6);
        }
        return defaultResult;
    }
    public static TResult? GetIfNot<TResult, T1, T2, T3, T4, T5, T6, T7>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, TResult? defaultResult = default) {
        if (!condition) {
            return action.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        return defaultResult;
    }
    public static TResult? GetIfNot<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, TResult? defaultResult = default) {
        if (!condition) {
            return action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        return defaultResult;
    }
    public static TResult? GetIfNot<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, TResult? defaultResult = default) {
        if (!condition) {
            return action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        return defaultResult;
    }
    #endregion
    #region DoIfElse & GetIfElse
    /// <summary>
    /// 若<paramref name="condition"/>为真则调用<paramref name="action"/>, 否则调用<paramref name="altAction"/>.
    /// <br/>若为表达式推荐使用<see cref="Do"/>配合三目运算符
    /// </summary>
    /// <returns>返回<paramref name="condition"/></returns>
    public static bool DoIfElse(bool condition, Action action, Action altAction) {
        if (condition) {
            action.Invoke();
        }
        else {
            altAction.Invoke();
        }

        return condition;
    }
    public static bool DoIfElse<T>(bool condition, Action<T> action, Action<T> altAction, T t) {
        if (condition) {
            action.Invoke(t);
        }
        else {
            altAction.Invoke(t);
        }
        return condition;
    }
    public static bool DoIfElse<T1, T2>(bool condition, Action<T1, T2> action, Action<T1, T2> altAction, T1 t1, T2 t2) {
        if (condition) {
            action.Invoke(t1, t2);
        }
        else {
            altAction.Invoke(t1, t2);
        }
        return condition;
    }
    public static bool DoIfElse<T1, T2, T3>(bool condition, Action<T1, T2, T3> action, Action<T1, T2, T3> altAction, T1 t1, T2 t2, T3 t3) {
        if (condition) {
            action.Invoke(t1, t2, t3);
        }
        else {
            altAction.Invoke(t1, t2, t3);
        }
        return condition;
    }
    public static bool DoIfElse<T1, T2, T3, T4>(bool condition, Action<T1, T2, T3, T4> action, Action<T1, T2, T3, T4> altAction, T1 t1, T2 t2, T3 t3, T4 t4) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4);
        }
        return condition;
    }
    public static bool DoIfElse<T1, T2, T3, T4, T5>(bool condition, Action<T1, T2, T3, T4, T5> action, Action<T1, T2, T3, T4, T5> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4, t5);
        }
        return condition;
    }
    public static bool DoIfElse<T1, T2, T3, T4, T5, T6>(bool condition, Action<T1, T2, T3, T4, T5, T6> action, Action<T1, T2, T3, T4, T5, T6> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4, t5, t6);
        }
        return condition;
    }
    public static bool DoIfElse<T1, T2, T3, T4, T5, T6, T7>(bool condition, Action<T1, T2, T3, T4, T5, T6, T7> action, Action<T1, T2, T3, T4, T5, T6, T7> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        return condition;
    }
    public static bool DoIfElse<T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Action<T1, T2, T3, T4, T5, T6, T7, T8> action, Action<T1, T2, T3, T4, T5, T6, T7, T8> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        return condition;
    }
    public static bool DoIfElse<T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        return condition;
    }

    public static bool DoIfElse<TResult>(bool condition, Func<TResult> action, Func<TResult> altAction) {
        if (condition) {
            action.Invoke();
        }
        else {
            altAction.Invoke();
        }
        return condition;
    }
    public static bool DoIfElse<TResult, T>(bool condition, Func<T, TResult> action, Func<T, TResult> altAction, T t) {
        if (condition) {
            action.Invoke(t);
        }
        else {
            altAction.Invoke(t);
        }
        return condition;
    }
    public static bool DoIfElse<TResult, T1, T2>(bool condition, Func<T1, T2, TResult> action, Func<T1, T2, TResult> altAction, T1 t1, T2 t2) {
        if (condition) {
            action.Invoke(t1, t2);
        }
        else {
            altAction.Invoke(t1, t2);
        }
        return condition;
    }
    public static bool DoIfElse<TResult, T1, T2, T3>(bool condition, Func<T1, T2, T3, TResult> action, Func<T1, T2, T3, TResult> altAction, T1 t1, T2 t2, T3 t3) {
        if (condition) {
            action.Invoke(t1, t2, t3);
        }
        else {
            altAction.Invoke(t1, t2, t3);
        }
        return condition;
    }
    public static bool DoIfElse<TResult, T1, T2, T3, T4>(bool condition, Func<T1, T2, T3, T4, TResult> action, Func<T1, T2, T3, T4, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4);
        }
        return condition;
    }
    public static bool DoIfElse<TResult, T1, T2, T3, T4, T5>(bool condition, Func<T1, T2, T3, T4, T5, TResult> action, Func<T1, T2, T3, T4, T5, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4, t5);
        }
        return condition;
    }
    public static bool DoIfElse<TResult, T1, T2, T3, T4, T5, T6>(bool condition, Func<T1, T2, T3, T4, T5, T6, TResult> action, Func<T1, T2, T3, T4, T5, T6, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4, t5, t6);
        }
        return condition;
    }
    public static bool DoIfElse<TResult, T1, T2, T3, T4, T5, T6, T7>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, Func<T1, T2, T3, T4, T5, T6, T7, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        return condition;
    }
    public static bool DoIfElse<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        return condition;
    }
    public static bool DoIfElse<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) {
        if (condition) {
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        else {
            altAction.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        return condition;
    }

    public static TResult GetIfElse<TResult>(bool condition, Func<TResult> action, Func<TResult> altAction) {
        return condition ? action.Invoke() : altAction.Invoke();
    }
    public static TResult GetIfElse<TResult, T>(bool condition, Func<T, TResult> action, Func<T, TResult> altAction, T t) {
        return condition ? action.Invoke(t) : altAction.Invoke(t);
    }
    public static TResult GetIfElse<TResult, T1, T2>(bool condition, Func<T1, T2, TResult> action, Func<T1, T2, TResult> altAction, T1 t1, T2 t2) {
        return condition ? action.Invoke(t1, t2) : altAction.Invoke(t1, t2);
    }
    public static TResult GetIfElse<TResult, T1, T2, T3>(bool condition, Func<T1, T2, T3, TResult> action, Func<T1, T2, T3, TResult> altAction, T1 t1, T2 t2, T3 t3) {
        return condition ? action.Invoke(t1, t2, t3) : altAction.Invoke(t1, t2, t3);
    }
    public static TResult GetIfElse<TResult, T1, T2, T3, T4>(bool condition, Func<T1, T2, T3, T4, TResult> action, Func<T1, T2, T3, T4, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4) {
        return condition ? action.Invoke(t1, t2, t3, t4) : altAction.Invoke(t1, t2, t3, t4);
    }
    public static TResult GetIfElse<TResult, T1, T2, T3, T4, T5>(bool condition, Func<T1, T2, T3, T4, T5, TResult> action, Func<T1, T2, T3, T4, T5, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
        return condition ? action.Invoke(t1, t2, t3, t4, t5) : altAction.Invoke(t1, t2, t3, t4, t5);
    }
    public static TResult GetIfElse<TResult, T1, T2, T3, T4, T5, T6>(bool condition, Func<T1, T2, T3, T4, T5, T6, TResult> action, Func<T1, T2, T3, T4, T5, T6, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
        return condition ? action.Invoke(t1, t2, t3, t4, t5, t6) : altAction.Invoke(t1, t2, t3, t4, t5, t6);
    }
    public static TResult GetIfElse<TResult, T1, T2, T3, T4, T5, T6, T7>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, Func<T1, T2, T3, T4, T5, T6, T7, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
        return condition ? action.Invoke(t1, t2, t3, t4, t5, t6, t7) : altAction.Invoke(t1, t2, t3, t4, t5, t6, t7);
    }
    public static TResult GetIfElse<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
        return condition ? action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8) : altAction.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
    }
    public static TResult GetIfElse<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> altAction, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) {
        return condition ? action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9) : altAction.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    }
    #endregion
    #region AssignIf
    /// <summary>
    /// 若<paramref name="condition"/>为<see langword="true"/>则将<paramref name="right"/>赋值给<paramref name="left"/>.
    /// </summary>
    /// <returns><paramref name="left"/></returns>
    public static ref T AssignIf<T>(bool condition, ref T left, T right) {
        if (condition) {
            left = right;
        }
        return ref left;
    }
    public static ref T AssignIf<T>(bool condition, ref T left, Func<T> right) {
        if (condition) {
            left = right.Invoke();
        }
        return ref left;
    }
    public static ref T AssignIf<T, T1>(bool condition, ref T left, Func<T1, T> right, T1 t1) {
        if (condition) {
            left = right.Invoke(t1);
        }
        return ref left;
    }
    public static ref T AssignIf<T, T1, T2>(bool condition, ref T left, Func<T1, T2, T> right, T1 t1, T2 t2) {
        if (condition) {
            left = right.Invoke(t1, t2);
        }
        return ref left;
    }
    public static ref T AssignIf<T, T1, T2, T3>(bool condition, ref T left, Func<T1, T2, T3, T> right, T1 t1, T2 t2, T3 t3) {
        if (condition) {
            left = right.Invoke(t1, t2, t3);
        }
        return ref left;
    }
    public static ref T AssignIf<T, T1, T2, T3, T4>(bool condition, ref T left, Func<T1, T2, T3, T4, T> right, T1 t1, T2 t2, T3 t3, T4 t4) {
        if (condition) {
            left = right.Invoke(t1, t2, t3, t4);
        }
        return ref left;
    }
    public static ref T AssignIf<T, T1, T2, T3, T4, T5>(bool condition, ref T left, Func<T1, T2, T3, T4, T5, T> right, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
        if (condition) {
            left = right.Invoke(t1, t2, t3, t4, t5);
        }
        return ref left;
    }
    public static ref T AssignIf<T, T1, T2, T3, T4, T5, T6>(bool condition, ref T left, Func<T1, T2, T3, T4, T5, T6, T> right, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
        if (condition) {
            left = right.Invoke(t1, t2, t3, t4, t5, t6);
        }
        return ref left;
    }
    public static ref T AssignIf<T, T1, T2, T3, T4, T5, T6, T7>(bool condition, ref T left, Func<T1, T2, T3, T4, T5, T6, T7, T> right, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
        if (condition) {
            left = right.Invoke(t1, t2, t3, t4, t5, t6, t7);
        }
        return ref left;
    }
    public static ref T AssignIf<T, T1, T2, T3, T4, T5, T6, T7, T8>(bool condition, ref T left, Func<T1, T2, T3, T4, T5, T6, T7, T8, T> right, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
        if (condition) {
            left = right.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
        }
        return ref left;
    }
    public static ref T AssignIf<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, ref T left, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> right, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) {
        if (condition) {
            left = right.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
        }
        return ref left;
    }
    #endregion
    #region DoTrigger
    public static bool DoTrigger(ref bool trigger, Action action) {
        if (trigger) {
            trigger = false;
            action();
            return true;
        }
        return false;
    }
    public static bool DoTrigger<T>(ref bool trigger, Action<T> action, T t) {
        if (trigger) {
            trigger = false;
            action.Invoke(t);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<T1, T2>(ref bool trigger, Action<T1, T2> action, T1 t1, T2 t2) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<T1, T2, T3>(ref bool trigger, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<T1, T2, T3, T4>(ref bool trigger, Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<T1, T2, T3, T4, T5>(ref bool trigger, Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4, t5);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<T1, T2, T3, T4, T5, T6>(ref bool trigger, Action<T1, T2, T3, T4, T5, T6> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4, t5, t6);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<T1, T2, T3, T4, T5, T6, T7>(ref bool trigger, Action<T1, T2, T3, T4, T5, T6, T7> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4, t5, t6, t7);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<T1, T2, T3, T4, T5, T6, T7, T8>(ref bool trigger, Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<T1, T2, T3, T4, T5, T6, T7, T8, T9>(ref bool trigger, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
            return true;
        }
        return false;
    }
    
    public static bool DoTrigger<TResult>(ref bool trigger, Func<TResult> action) {
        if (trigger) {
            trigger = false;
            action.Invoke();
            return true;
        }
        return false;
    }
    public static bool DoTrigger<TResult, T>(ref bool trigger, Func<T, TResult> action, T t) {
        if (trigger) {
            trigger = false;
            action.Invoke(t);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<TResult, T1, T2>(ref bool trigger, Func<T1, T2, TResult> action, T1 t1, T2 t2) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<TResult, T1, T2, T3>(ref bool trigger, Func<T1, T2, T3, TResult> action, T1 t1, T2 t2, T3 t3) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<TResult, T1, T2, T3, T4>(ref bool trigger, Func<T1, T2, T3, T4, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<TResult, T1, T2, T3, T4, T5>(ref bool trigger, Func<T1, T2, T3, T4, T5, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4, t5);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<TResult, T1, T2, T3, T4, T5, T6>(ref bool trigger, Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4, t5, t6);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<TResult, T1, T2, T3, T4, T5, T6, T7>(ref bool trigger, Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4, t5, t6, t7);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(ref bool trigger, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8);
            return true;
        }
        return false;
    }
    public static bool DoTrigger<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(ref bool trigger, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) {
        if (trigger) {
            trigger = false;
            action.Invoke(t1, t2, t3, t4, t5, t6, t7, t8, t9);
            return true;
        }
        return false;
    }
    #endregion
    /// <summary>
    /// 若<paramref name="condition"/>为真则调用<paramref name="actions"/>中的第一项,
    /// 若第一项返回真则调用第二项...直到有任意一项返回假或者全部执行完
    /// </summary>
    /// <returns>若有任意一项返回假则是假(包含最后一项), 只有全部都返回真才是真</returns>
    public static bool DoIfElseIf(bool condition, params Func<bool>[] actions) {
        if (!condition) {
            return false;
        }
        foreach (var action in actions) {
            if (!action()) {
                return false;
            }
        }
        return true;
    }
    #endregion
    #region 流程控制 - 循环
    /// <summary>
    /// returns false when action or condition is null, else returns true.
    /// would still do action once when condition is null but action is not
    /// </summary>
    public static bool DoWhile(Action action, Func<bool> condition) {
        if (condition == null) {
            action?.Invoke();
            return false;
        }
        if (action == null) {
            return false;
        }
        do {
            action();
        }
        while (condition());
        return true;
    }
    /// <summary>
    /// if break out, returns true, else returns false.
    /// would still do action once and try break out when condition is null but action is not
    /// </summary>
    /// <param name="action">when get true, breaks out</param>
    public static bool DoWhileB(Func<bool> action, Func<bool> condition) {
        if (condition == null) {
            return action?.Invoke() == true;
        }
        if (action == null) {
            return false;
        }
        do {
            if (action()) {
                return true;
            }
        }
        while (condition());
        return false;
    }
    /// <summary>
    /// alwayss return false
    /// </summary>
    public static bool WhileDo(Func<bool> condition, Action action) {
        while (condition()) {
            action();
        }
        return false;
    }
    /// <summary>
    /// returns true when break out, else returns false
    /// </summary>
    /// <param name="action">breaks out when get true</param>
    public static bool WhileDoB(Func<bool> condition, Func<bool> action) {
        while (condition()) {
            if (action()) {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// always return false
    /// </summary>
    public static bool ForDo(Action? init, Func<bool>? condition, Action? iter, Action? action) {
        init?.Invoke();
        while (condition?.Invoke() != false) {
            action?.Invoke();
            iter?.Invoke();
        }
        return false;
    }
    /// <summary>
    /// returns true when break out, else returns false
    /// </summary>
    /// <param name="action">breaks out when get true</param>
    public static bool ForDoB(Action? init, Func<bool>? condition, Action? iter, Func<bool>? action) {
        init?.Invoke();
        while (condition?.Invoke() != false) {
            if (action?.Invoke() == true) {
                return true;
            }
            iter?.Invoke();
        }
        return false;
    }
    /// <summary>
    /// always return false
    /// </summary>
    public static bool ForeachDo<T>(IEnumerable<T> enumerable, Action<T> action) {
        foreach (T t in enumerable) {
            action(t);
        }
        return false;
    }
    /// <summary>
    /// returns true when break out, else returns false
    /// </summary>
    /// <param name="action">breaks out when get true</param>
    public static bool ForeachDoB<T>(IEnumerable<T> enumerable, Func<T, bool> action) {
        foreach (T t in enumerable) {
            if (action(t)) {
                return true;
            }
        }
        return false;
    }
    //foreach部分挪到TigerExtensions中 IEnumerable拓展 的 Foreach 区域了
    #endregion
    #region ref相关
    //ref拓展不知道为什么只能给值类型用, 但若不用拓展就可以
    /// <summary>
    /// 对<paramref name="self"/>执行<paramref name="action"/>
    /// </summary>
    /// <returns><paramref name="self"/>的引用</returns>
    public static ref T Do<T>(ref T self, Action<T> action) {
        action(self);
        return ref self;
    }
    /// <summary>
    /// 将<paramref name="other"/>的值赋给<paramref name="self"/>
    /// </summary>
    /// <returns><paramref name="self"/>的引用</returns>
    public static ref T Assign<T>(ref T self, T other) {
        self = other;
        return ref self;
    }
    #endregion
    #region 杂项
    /// <summary>
    /// 什么也不做
    /// </summary>
    /// <returns>false</returns>
    public static bool DoNothing() => false;
    /// <summary>
    /// 什么也不做
    /// </summary>
    /// <returns>false</returns>
    public static bool Dos(params object?[] objs) {
        _ = objs;
        return false;
    }

    /// <summary>
    /// 若其中有Action, 则自动执行
    /// </summary>
    /// <returns>false</returns>
    public static bool DosS(params object[] objs) => objs.ForeachDoB(o => o is Action action && Do(action));
    public static bool DosA(params Action[] actions) => actions.ForeachDo(a => a.Invoke());
    public static TRight GetRight<TLeft, TRight>(TLeft left, TRight right) {
        _ = left;
        return right;
    }

    public static T GetRightA<T>(Action left, T right) {
        left();
        return right;
    }
    public static TLeft GetLeft<TLeft, TRight>(TLeft left, TRight right) {
        _ = right;
        return left;
    }

    public static T GetLeftA<T>(T left, Action right) {
        T result = left;
        right();
        return result;
    }
    #endregion
    #endregion
    #region -1作无限
    public static int AddN1(int a, int b) => a == -1 || b == -1 ? -1 : a + b;
    public static bool GreaterN1(int a, int b) => b != -1 && (a == -1 || a > b);
    public static bool LesserN1(int a, int b) => a != -1 && (b == -1 || a < b);
    public static bool GreaterEqualN1(int a, int b) => a == -1 || b != -1 && a >= b;
    public static bool LesserEqualN1(int a, int b) => b == -1 || a != -1 && a <= b;
    public static int MaxN1(int a, int b) => a == -1 || b == -1 ? -1 : a >= b ? a : b;
    public static int MinN1(int a, int b) => a == -1 ? b : b == -1 ? a : a <= b ? a : b;
    /// <summary>
    /// 按照 uint 比较
    /// </summary>
    public static int MaxU(int a, int b) => ((uint)a >= (uint)b) ? a : b;
    /// <summary>
    /// 按照 uint 比较
    /// </summary>
    public static int MinU(int a, int b) => ((uint)a <= (uint)b) ? a : b;
    #endregion
    #region 一些简单的委托
    public static Func<T, bool> Lesser<T>(T value) where T : IComparable<T>
        => t => t.CompareTo(value) < 0;
    public static Func<T, bool> Greater<T>(T value) where T : IComparable<T>
        => t => t.CompareTo(value) > 0;
    public static Func<T, bool> LesserEqual<T>(T value) where T : IComparable<T>
        => t => t.CompareTo(value) <= 0;
    public static Func<T, bool> GreaterEqual<T>(T value) where T : IComparable<T>
        => t => t.CompareTo(value) >= 0;
    /// <summary>
    /// 返回参数是否在[<paramref name="left"/>, <paramref name="right"/>)中
    /// </summary>
    public static Func<T, bool> Between<T>(T left, T right) where T : IComparable<T>
        => t => t.CompareTo(left) >= 0 && t.CompareTo(right) < 0;
    /// <summary>
    /// 返回参数是否不在[<paramref name="left"/>, <paramref name="right"/>)中
    /// </summary>
    public static Func<T, bool> NotBetween<T>(T left, T right) where T : IComparable<T>
        => t => t.CompareTo(left) < 0 || t.CompareTo(right) >= 0;
    #endregion
    #region 委托的运算
    #region And
    public static Func<bool> And(Func<bool> left, Func<bool> right)
        => () => left() && right();
    public static Func<T, bool> And<T>(Func<T, bool> left, Func<T, bool> right)
        => t => left(t) && right(t);
    public static Func<T1, T2, bool> And<T1, T2>(Func<T1, T2, bool> left, Func<T1, T2, bool> right)
        => (t1, t2) => left(t1, t2) && right(t1, t2);
    public static Func<T1, T2, T3, bool> And<T1, T2, T3>(Func<T1, T2, T3, bool> left, Func<T1, T2, T3, bool> right)
        => (t1, t2, t3) => left(t1, t2, t3) && right(t1, t2, t3);
    public static Func<T1, T2, T3, T4, bool> And<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> left, Func<T1, T2, T3, T4, bool> right)
        => (t1, t2, t3, t4) => left(t1, t2, t3, t4) && right(t1, t2, t3, t4);
    public static Func<T1, T2, T3, T4, T5, bool> And<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> left, Func<T1, T2, T3, T4, T5, bool> right)
        => (t1, t2, t3, t4, t5) => left(t1, t2, t3, t4, t5) && right(t1, t2, t3, t4, t5);
    public static Func<T1, T2, T3, T4, T5, T6, bool> And<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, bool> left, Func<T1, T2, T3, T4, T5, T6, bool> right)
        => (t1, t2, t3, t4, t5, t6) => left(t1, t2, t3, t4, t5, t6) && right(t1, t2, t3, t4, t5, t6);
    public static Func<T1, T2, T3, T4, T5, T6, T7, bool> And<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, bool> left, Func<T1, T2, T3, T4, T5, T6, T7, bool> right)
        => (t1, t2, t3, t4, t5, t6, t7) => left(t1, t2, t3, t4, t5, t6, t7) && right(t1, t2, t3, t4, t5, t6, t7);
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> And<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> left, Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> right)
        => (t1, t2, t3, t4, t5, t6, t7, t8) => left(t1, t2, t3, t4, t5, t6, t7, t8) && right(t1, t2, t3, t4, t5, t6, t7, t8);
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> And<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> left, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> right)
        => (t1, t2, t3, t4, t5, t6, t7, t8, t9) => left(t1, t2, t3, t4, t5, t6, t7, t8, t9) && right(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    #endregion
    #region Or
    public static Func<bool> Or(Func<bool> left, Func<bool> right)
        => () => left() || right();
    public static Func<T, bool> Or<T>(Func<T, bool> left, Func<T, bool> right)
        => t => left(t) || right(t);
    public static Func<T1, T2, bool> Or<T1, T2>(Func<T1, T2, bool> left, Func<T1, T2, bool> right)
        => (t1, t2) => left(t1, t2) || right(t1, t2);
    public static Func<T1, T2, T3, bool> Or<T1, T2, T3>(Func<T1, T2, T3, bool> left, Func<T1, T2, T3, bool> right)
        => (t1, t2, t3) => left(t1, t2, t3) || right(t1, t2, t3);
    public static Func<T1, T2, T3, T4, bool> Or<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> left, Func<T1, T2, T3, T4, bool> right)
        => (t1, t2, t3, t4) => left(t1, t2, t3, t4) || right(t1, t2, t3, t4);
    public static Func<T1, T2, T3, T4, T5, bool> Or<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> left, Func<T1, T2, T3, T4, T5, bool> right)
        => (t1, t2, t3, t4, t5) => left(t1, t2, t3, t4, t5) || right(t1, t2, t3, t4, t5);
    public static Func<T1, T2, T3, T4, T5, T6, bool> Or<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, bool> left, Func<T1, T2, T3, T4, T5, T6, bool> right)
        => (t1, t2, t3, t4, t5, t6) => left(t1, t2, t3, t4, t5, t6) || right(t1, t2, t3, t4, t5, t6);
    public static Func<T1, T2, T3, T4, T5, T6, T7, bool> Or<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, bool> left, Func<T1, T2, T3, T4, T5, T6, T7, bool> right)
        => (t1, t2, t3, t4, t5, t6, t7) => left(t1, t2, t3, t4, t5, t6, t7) || right(t1, t2, t3, t4, t5, t6, t7);
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> Or<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> left, Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> right)
        => (t1, t2, t3, t4, t5, t6, t7, t8) => left(t1, t2, t3, t4, t5, t6, t7, t8) || right(t1, t2, t3, t4, t5, t6, t7, t8);
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> Or<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> left, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> right)
        => (t1, t2, t3, t4, t5, t6, t7, t8, t9) => left(t1, t2, t3, t4, t5, t6, t7, t8, t9) || right(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    #endregion
    #region Not
    public static Func<bool> Not(Func<bool> action)
        => () => !action();
    public static Func<T, bool> Not<T>(Func<T, bool> action)
        => t => !action(t);
    public static Func<T1, T2, bool> Not<T1, T2>(Func<T1, T2, bool> action)
        => (t1, t2) => !action(t1, t2);
    public static Func<T1, T2, T3, bool> Not<T1, T2, T3>(Func<T1, T2, T3, bool> action)
        => (t1, t2, t3) => !action(t1, t2, t3);
    public static Func<T1, T2, T3, T4, bool> Not<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> action)
        => (t1, t2, t3, t4) => !action(t1, t2, t3, t4);
    public static Func<T1, T2, T3, T4, T5, bool> Not<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> action)
        => (t1, t2, t3, t4, t5) => !action(t1, t2, t3, t4, t5);
    public static Func<T1, T2, T3, T4, T5, T6, bool> Not<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, bool> action)
        => (t1, t2, t3, t4, t5, t6) => !action(t1, t2, t3, t4, t5, t6);
    public static Func<T1, T2, T3, T4, T5, T6, T7, bool> Not<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, bool> action)
        => (t1, t2, t3, t4, t5, t6, t7) => !action(t1, t2, t3, t4, t5, t6, t7);
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> Not<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> action)
        => (t1, t2, t3, t4, t5, t6, t7, t8) => !action(t1, t2, t3, t4, t5, t6, t7, t8);
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> Not<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> action)
        => (t1, t2, t3, t4, t5, t6, t7, t8, t9) => !action(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    #endregion
    #region Xor
    public static Func<bool> Xor(Func<bool> left, Func<bool> right)
        => () => left() ^ right();
    public static Func<T, bool> Xor<T>(Func<T, bool> left, Func<T, bool> right)
        => t => left(t) ^ right(t);
    public static Func<T1, T2, bool> Xor<T1, T2>(Func<T1, T2, bool> left, Func<T1, T2, bool> right)
        => (t1, t2) => left(t1, t2) ^ right(t1, t2);
    public static Func<T1, T2, T3, bool> Xor<T1, T2, T3>(Func<T1, T2, T3, bool> left, Func<T1, T2, T3, bool> right)
        => (t1, t2, t3) => left(t1, t2, t3) ^ right(t1, t2, t3);
    public static Func<T1, T2, T3, T4, bool> Xor<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> left, Func<T1, T2, T3, T4, bool> right)
        => (t1, t2, t3, t4) => left(t1, t2, t3, t4) ^ right(t1, t2, t3, t4);
    public static Func<T1, T2, T3, T4, T5, bool> Xor<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> left, Func<T1, T2, T3, T4, T5, bool> right)
        => (t1, t2, t3, t4, t5) => left(t1, t2, t3, t4, t5) ^ right(t1, t2, t3, t4, t5);
    public static Func<T1, T2, T3, T4, T5, T6, bool> Xor<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, bool> left, Func<T1, T2, T3, T4, T5, T6, bool> right)
        => (t1, t2, t3, t4, t5, t6) => left(t1, t2, t3, t4, t5, t6) ^ right(t1, t2, t3, t4, t5, t6);
    public static Func<T1, T2, T3, T4, T5, T6, T7, bool> Xor<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, bool> left, Func<T1, T2, T3, T4, T5, T6, T7, bool> right)
        => (t1, t2, t3, t4, t5, t6, t7) => left(t1, t2, t3, t4, t5, t6, t7) ^ right(t1, t2, t3, t4, t5, t6, t7);
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> Xor<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> left, Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> right)
        => (t1, t2, t3, t4, t5, t6, t7, t8) => left(t1, t2, t3, t4, t5, t6, t7, t8) ^ right(t1, t2, t3, t4, t5, t6, t7, t8);
    public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> Xor<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> left, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> right)
        => (t1, t2, t3, t4, t5, t6, t7, t8, t9) => left(t1, t2, t3, t4, t5, t6, t7, t8, t9) ^ right(t1, t2, t3, t4, t5, t6, t7, t8, t9);
    #endregion
    #endregion
    #region 反射转委托
    public delegate void RefSetMemberDelegate<T, TField>(ref T self, TField field);
    public delegate void RefSetMemberDelegate<T>(ref T self, object? field);
    #region 方法的委托
    public static TDelegate GetMethodDelegate<T, TDelegate>(string methodName, BindingFlags flags = BFALL) where TDelegate : Delegate {
        return typeof(T).GetMethod(methodName, flags)!.CreateDelegate<TDelegate>();
    }
    public static TDelegate GetMethodDelegate<T, TDelegate>(string methodName, Type[] types, BindingFlags flags = BFALL) where TDelegate : Delegate {
        return typeof(T).GetMethod(methodName, flags, types)!.CreateDelegate<TDelegate>();
    }
    #endregion
    #region 获取字段的委托

    #region 获取实例字段的委托
    public static Func<T, TField> GetGetFieldDelegate<T, TField>(string fieldName, BindingFlags flags = BFI)
        => GetGetFieldDelegate<T, TField>(typeof(T).GetField(fieldName, flags)!);
    public static Func<object, TField> GetGetFieldDelegate<TField>(Type type, string fieldName, BindingFlags flags = BFI)
        => GetGetFieldDelegate<TField>(type, type.GetField(fieldName, flags)!);
    public static Func<T, object?> GetGetFieldDelegate<T>(string fieldName, BindingFlags flags = BFI)
        => GetGetFieldToObjectDelegate<T>(typeof(T), fieldName, flags);
    public static Func<object, object?> GetGetFieldDelegate(Type type, string fieldName, BindingFlags flags = BFI)
        => GetGetFieldDelegate(type, type.GetField(fieldName, flags)!);

    public static Func<object, TField> GetGetFieldFromObjectDelegate<TField>(Type type, string fieldName, BindingFlags flags = BFI)
        => GetGetFieldDelegate<TField>(type, fieldName, flags);
    public static Func<T, object?> GetGetFieldToObjectDelegate<T>(Type type, string fieldName, BindingFlags flags = BFI)
        => GetGetFieldDelegate<T>(type.GetField(fieldName, flags)!);

    public static Func<T, TField> GetGetFieldDelegate<T, TField>(FieldInfo field) {
        DynamicMethod method = new("GetField", typeof(TField), [typeof(T)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Ldfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<T, TField>>();
    }
    public static Func<object, TField> GetGetFieldDelegate<TField>(Type type, FieldInfo field) {
        DynamicMethod method = new("GetField", typeof(TField), [typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (type.IsValueType) {
            il.Emit(SOpCodes.Unbox, type);
        }
        else {
            il.Emit(SOpCodes.Castclass, type);
        }
        il.Emit(SOpCodes.Ldfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<object, TField>>();
    }
    public static Func<T, object?> GetGetFieldDelegate<T>(FieldInfo field) {
        DynamicMethod method = new("GetField", typeof(object), [typeof(T)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Ldfld, field);
        if (field.FieldType.IsValueType) {
            il.Emit(SOpCodes.Box, field.FieldType);
        }
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<T, object?>>();
    }
    public static Func<object, object?> GetGetFieldDelegate(Type type, FieldInfo field) {
        DynamicMethod method = new("GetField", typeof(object), [typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (type.IsValueType) {
            il.Emit(SOpCodes.Unbox, type);
        }
        else {
            il.Emit(SOpCodes.Castclass, type);
        }
        il.Emit(SOpCodes.Ldfld, field);
        if (field.FieldType.IsValueType) {
            il.Emit(SOpCodes.Box, field.FieldType);
        }
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<object, object?>>();
    }

    public static Func<object, object?> GetGetFieldDelegate(FieldInfo field)
        => GetGetFieldDelegate(field.DeclaringType!, field);
    public static Func<object, TField> GetGetFieldFromObjectDelegate<TField>(FieldInfo field)
        => GetGetFieldDelegate<TField>(field.DeclaringType!, field);
    public static Func<T, object?> GetGetFieldToObjectDelegate<T>(FieldInfo field)
        => GetGetFieldDelegate<T>(field);
    #endregion

    #region 获取静态字段的委托
    public static Func<TField> GetGetStaticFieldDelegate<T, TField>(string fieldName, BindingFlags flags = BFS)
        => GetGetStaticFieldDelegate<TField>(typeof(T), fieldName, flags);
    public static Func<TField> GetGetStaticFieldDelegate<TField>(Type type, string fieldName, BindingFlags flags = BFS)
        => GetGetStaticFieldDelegate<TField>(type.GetField(fieldName, flags)!);
    public static Func<object?> GetGetStaticFieldDelegate<T>(string fieldName, BindingFlags flags = BFS)
        => GetGetStaticFieldDelegate(typeof(T), fieldName, flags);
    public static Func<object?> GetGetStaticFieldDelegate(Type type, string fieldName, BindingFlags flags = BFS)
        => GetGetStaticFieldDelegate(type.GetField(fieldName, flags)!);

    public static Func<TField> GetGetStaticFieldDelegate<TField>(FieldInfo field) {
        DynamicMethod method = new("GetField", typeof(TField), null, true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldsfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<TField>>();
    }
    public static Func<object?> GetGetStaticFieldDelegate(FieldInfo field) {
        DynamicMethod method = new("GetField", typeof(object), null, true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldsfld, field);
        if (field.FieldType.IsValueType) {
            il.Emit(SOpCodes.Box, field.FieldType);
        }
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<object>>();
    }
    #endregion

    #endregion
    #region 设置字段的委托

    #region 设置实例字段的委托
    public static Action<T, TField> GetSetFieldDelegate<T, TField>(string fieldName, BindingFlags flags = BFI) where T : class
        => GetSetFieldDelegate<T, TField>(typeof(T).GetField(fieldName, flags)!);
    public static Action<object, TField> GetSetFieldDelegate<TField>(Type type, string fieldName, BindingFlags flags = BFI)
        => GetSetFieldDelegate<TField>(type, type.GetField(fieldName, flags)!);
    public static Action<T, object?> GetSetFieldDelegate<T>(string fieldName, BindingFlags flags = BFI) where T : class
        => GetSetFieldToObjectDelegate<T>(typeof(T), fieldName, flags);
    public static Action<object, object?> GetSetFieldDelegate(Type type, string fieldName, BindingFlags flags = BFI)
        => GetSetFieldDelegate(type, type.GetField(fieldName, flags)!);
    public static RefSetMemberDelegate<T, TField> GetSetFieldDelegateN<T, TField>(Type type, string fieldName, BindingFlags flags = BFI) where T : struct
        => GetSetFieldDelegateN<T, TField>(type.GetField(fieldName, flags)!);
    public static RefSetMemberDelegate<T> GetSetFieldDelegateN<T>(Type type, string fieldName, BindingFlags flags = BFI) where T : struct
        => GetSetFieldDelegateN<T>(type.GetField(fieldName, flags)!);

    public static Action<object, TField> GetSetFieldFromObjectDelegate<TField>(Type type, string fieldName, BindingFlags flags = BFI)
        => GetSetFieldDelegate<TField>(type, fieldName, flags);
    public static Action<T, object?> GetSetFieldToObjectDelegate<T>(Type type, string fieldName, BindingFlags flags = BFI) where T : class
        => GetSetFieldDelegate<T>(type.GetField(fieldName, flags)!);
    public static RefSetMemberDelegate<T> GetSetFieldToObjectDelegateN<T>(Type type, string fieldName, BindingFlags flags = BFI) where T : struct
        => GetSetFieldDelegateN<T>(type.GetField(fieldName, flags)!);

    public static Action<T, TField> GetSetFieldDelegate<T, TField>(FieldInfo field) where T : class {
        Type type = typeof(T);
        DynamicMethod method = new("SetField", null, [type, typeof(TField)], true);
        var il = method.GetILGenerator();
        // if (type.IsValueType)
        //     il.Emit(SOpCodes.Ldarga, 0);
        // else
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Ldarg_1);
        il.Emit(SOpCodes.Stfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Action<T, TField>>();
    }
    public static Action<object, TField> GetSetFieldDelegate<TField>(Type type, FieldInfo field) {
        DynamicMethod method = new("SetField", null, [typeof(object), typeof(TField)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (type.IsValueType)
            il.Emit(SOpCodes.Unbox, type);
        else
            il.Emit(SOpCodes.Castclass, type);
        il.Emit(SOpCodes.Ldarg_1);
        il.Emit(SOpCodes.Stfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Action<object, TField>>();
    }
    public static Action<T, object?> GetSetFieldDelegate<T>(FieldInfo field) where T : class {
        Type type = typeof(T);
        DynamicMethod method = new("SetField", null, [type, typeof(object)], true);
        var il = method.GetILGenerator();
        // if (type.IsValueType)
        //     il.Emit(SOpCodes.Ldarga, 0);
        // else
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Ldarg_1);
        if (field.FieldType.IsValueType) {
            il.Emit(SOpCodes.Unbox_Any, field.FieldType);
        }
        else {
            il.Emit(SOpCodes.Castclass, field.FieldType);
        }
        il.Emit(SOpCodes.Stfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Action<T, object?>>();
    }
    public static Action<object, object?> GetSetFieldDelegate(Type type, FieldInfo field) {
        DynamicMethod method = new("SetField", null, [typeof(object), typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (type.IsValueType)
            il.Emit(SOpCodes.Unbox, type);
        else
            il.Emit(SOpCodes.Castclass, type);
        il.Emit(SOpCodes.Ldarg_1);
        if (field.FieldType.IsValueType) {
            il.Emit(SOpCodes.Unbox_Any, field.FieldType);
        }
        else {
            il.Emit(SOpCodes.Castclass, field.FieldType);
        }
        il.Emit(SOpCodes.Stfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Action<object, object?>>();
    }
    public static RefSetMemberDelegate<T, TField> GetSetFieldDelegateN<T, TField>(FieldInfo field) where T : struct {
        Type type = typeof(T);
        DynamicMethod method = new("SetField", null, [type.MakeByRefType(), typeof(TField)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Ldarg_1);
        il.Emit(SOpCodes.Stfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<RefSetMemberDelegate<T, TField>>();
    }
    public static RefSetMemberDelegate<T> GetSetFieldDelegateN<T>(FieldInfo field) where T : struct {
        Type type = typeof(T);
        DynamicMethod method = new("SetField", null, [type.MakeByRefType(), typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Ldarg_1);
        if (field.FieldType.IsValueType) {
            il.Emit(SOpCodes.Unbox_Any, field.FieldType);
        }
        else {
            il.Emit(SOpCodes.Castclass, field.FieldType);
        }
        il.Emit(SOpCodes.Stfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<RefSetMemberDelegate<T>>();
    }

    public static Action<object, object?> GetSetFieldDelegate(FieldInfo field)
        => GetSetFieldDelegate(field.DeclaringType!, field);
    public static Action<object, TField> GetSetFieldFromObjectDelegate<TField>(FieldInfo field)
        => GetSetFieldDelegate<TField>(field.DeclaringType!, field);
    public static Action<T, object?> GetSetFieldToObjectDelegate<T>(FieldInfo field) where T : class
        => GetSetFieldDelegate<T>(field);
    public static RefSetMemberDelegate<T> GetSetFieldToObjectDelegateN<T>(FieldInfo field) where T : struct
        => GetSetFieldDelegateN<T>(field);
    #endregion

    #region 设置静态字段的委托
    public static Action<TField> GetSetStaticFieldDelegate<T, TField>(string fieldName, BindingFlags flags = BFS)
        => GetSetStaticFieldDelegate<TField>(typeof(T), fieldName, flags);
    public static Action<TField> GetSetStaticFieldDelegate<TField>(Type type, string fieldName, BindingFlags flags = BFS)
        => GetSetStaticFieldDelegate<TField>(type.GetField(fieldName, flags)!);
    public static Action<object?> GetSetStaticFieldDelegate<T>(string fieldName, BindingFlags flags = BFS)
        => GetSetStaticFieldDelegate(typeof(T), fieldName, flags);
    public static Action<object?> GetSetStaticFieldDelegate(Type type, string fieldName, BindingFlags flags = BFS)
        => GetSetStaticFieldDelegate(type.GetField(fieldName, flags)!);

    public static Action<TField> GetSetStaticFieldDelegate<TField>(FieldInfo field) {
        DynamicMethod method = new("SetField", null, [typeof(TField)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Stsfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Action<TField>>();
    }
    public static Action<object?> GetSetStaticFieldDelegate(FieldInfo field) {
        DynamicMethod method = new("SetField", null, [typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (field.FieldType.IsValueType) {
            il.Emit(SOpCodes.Unbox_Any, field.FieldType);
        }
        else {
            il.Emit(SOpCodes.Castclass, field.FieldType);
        }
        il.Emit(SOpCodes.Stsfld, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Action<object?>>();
    }
    #endregion

    #endregion
    #region 获得引用字段的委托
    public delegate ref TValue RefInstanceMemberGetter<T, TValue>(T self);

    #region 获取实例引用字段的委托
    public static RefInstanceMemberGetter<T, TField> GetGetRefFieldDelegate<T, TField>(string fieldName, BindingFlags flags = BFI)
        => GetGetRefFieldDelegate<T, TField>(typeof(T).GetField(fieldName, flags)!);
    public static RefInstanceMemberGetter<object, TField> GetGetRefFieldDelegate<TField>(Type type, string fieldName, BindingFlags flags = BFI)
        => GetGetRefFieldDelegate<TField>(type, type.GetField(fieldName, flags)!);

    public static RefInstanceMemberGetter<T, TField> GetGetRefFieldDelegate<T, TField>(FieldInfo field) {
        DynamicMethod method = new("GetRefField", typeof(TField).MakeByRefType(), [typeof(T)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Ldflda, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<RefInstanceMemberGetter<T, TField>>();
    }
    public static RefInstanceMemberGetter<object, TField> GetGetRefFieldDelegate<TField>(Type type, FieldInfo field) {
        DynamicMethod method = new("GetRefField", typeof(TField).MakeByRefType(), [typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (type.IsValueType) {
            il.Emit(SOpCodes.Unbox, type);
        }
        else {
            il.Emit(SOpCodes.Castclass, type);
        }
        il.Emit(SOpCodes.Ldflda, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<RefInstanceMemberGetter<object, TField>>();
    }
    public static RefInstanceMemberGetter<object, TField> GetGetRefFieldDelegate<TField>(FieldInfo field)
        => GetGetRefFieldDelegate<TField>(field.DeclaringType!, field);
    public static RefInstanceMemberGetter<object, TField> GetGetRefFieldFromObjectDelegate<TField>(FieldInfo field)
        => GetGetRefFieldDelegate<TField>(field.DeclaringType!, field);
    // 必须指定 TField, 否则不能使用 Ref
    #endregion

    #region 获取静态引用字段的委托

    public delegate ref TValue RefStaticMemberGetter<TValue>();

    public static RefStaticMemberGetter<TField> GetGetStaticRefFieldDelegate<T, TField>(string fieldName, BindingFlags flags = BFS)
        => GetGetStaticRefFieldDelegate<TField>(typeof(T).GetField(fieldName, flags)!);
    public static RefStaticMemberGetter<TField> GetGetStaticRefFieldDelegate<TField>(Type type, string fieldName, BindingFlags flags = BFS)
        => GetGetStaticRefFieldDelegate<TField>(type.GetField(fieldName, flags)!);

    public static RefStaticMemberGetter<TField> GetGetStaticRefFieldDelegate<TField>(FieldInfo field) {
        DynamicMethod method = new("GetStaticRefField", typeof(TField).MakeByRefType(), [], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldsflda, field);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<RefStaticMemberGetter<TField>>();
    }
    // 必须指定 TField, 否则不能使用 Ref
    #endregion

    #endregion
    #region 获取属性的委托

    #region 获取实例属性的委托
    public static Func<T, TProperty> GetGetPropertyDelegate<T, TProperty>(string propertyName, BindingFlags flags = BFI)
        => GetGetPropertyDelegate<T, TProperty>(typeof(T).GetProperty(propertyName, flags)!);
    public static Func<object, TProperty> GetGetPropertyDelegate<TProperty>(Type type, string propertyName, BindingFlags flags = BFI)
        => GetGetPropertyDelegate<TProperty>(type, type.GetProperty(propertyName, flags)!);
    public static Func<T, object?> GetGetPropertyDelegate<T>(string propertyName, BindingFlags flags = BFI)
        => GetGetPropertyDelegate<T>(typeof(T).GetProperty(propertyName, flags)!);
    public static Func<object, object?> GetGetPropertyDelegate(Type type, string propertyName, BindingFlags flags = BFI)
        => GetGetPropertyDelegate(type, type.GetProperty(propertyName, flags)!);
    
    public static Func<object, TProperty> GetGetPropertyFromObjectDelegate<TProperty>(Type type, string propertyName, BindingFlags flags = BFI)
        => GetGetPropertyDelegate<TProperty>(type, propertyName, flags);
    public static Func<T, object?> GetGetPropertyToObjectDelegate<T>(Type type, string propertyName, BindingFlags flags = BFI)
        => GetGetPropertyDelegate<T>(type.GetProperty(propertyName, flags)!);

    public static Func<T, TProperty> GetGetPropertyDelegate<T, TProperty>(PropertyInfo property) {
        // return property.GetMethod!.CreateDelegate<Func<T, TProperty>>(); // <- 对于 T 是值类型时不行
        DynamicMethod method = new("GetProperty", typeof(TProperty), [typeof(T)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Callvirt, property.GetMethod!);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<T, TProperty>>();
    }
    public static Func<object, TProperty> GetGetPropertyDelegate<TProperty>(Type type, PropertyInfo property) {
        DynamicMethod method = new("GetProperty", typeof(TProperty), [typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (type.IsValueType) {
            il.Emit(SOpCodes.Unbox, type);
        }
        else {
            il.Emit(SOpCodes.Castclass, type);
        }
        il.Emit(SOpCodes.Callvirt, property.GetMethod!);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<object, TProperty>>();
    }
    public static Func<T, object?> GetGetPropertyDelegate<T>(PropertyInfo property) {
        DynamicMethod method = new("GetProperty", typeof(object), [typeof(T)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Callvirt, property.GetMethod!);
        if (property.PropertyType.IsValueType) {
            il.Emit(SOpCodes.Box, property.PropertyType);
        }
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<T, object?>>();
    }
    public static Func<object, object?> GetGetPropertyDelegate(Type type, PropertyInfo property) {
        DynamicMethod method = new("GetProperty", typeof(object), [typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (type.IsValueType) {
            il.Emit(SOpCodes.Unbox, type);
        }
        else {
            il.Emit(SOpCodes.Castclass, type);
        }
        il.Emit(SOpCodes.Callvirt, property.GetMethod!);
        if (property.PropertyType.IsValueType) {
            il.Emit(SOpCodes.Box, property.PropertyType);
        }
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<object, object?>>();
    }

    public static Func<object, object?> GetGetPropertyDelegate(PropertyInfo property)
        => GetGetPropertyDelegate(property.DeclaringType!, property);
    public static Func<object, TProperty> GetGetPropertyFromObjectDelegate<TProperty>(PropertyInfo property)
        => GetGetPropertyDelegate<TProperty>(property.DeclaringType!, property);
    public static Func<T, object?> GetGetPropertyToObjectDelegate<T>(PropertyInfo property)
        => GetGetPropertyDelegate<T>(property);
    #endregion

    #region 获取静态属性的委托
    public static Func<TProperty> GetGetStaticPropertyDelegate<T, TProperty>(string propertyName, BindingFlags flags = BFS)
        => GetGetStaticPropertyDelegate<TProperty>(typeof(T), propertyName, flags);
    public static Func<TProperty> GetGetStaticPropertyDelegate<TProperty>(Type type, string propertyName, BindingFlags flags = BFS)
        => GetGetStaticPropertyDelegate<TProperty>(type.GetProperty(propertyName, flags)!);
    public static Func<object?> GetGetStaticPropertyDelegate<T>(string propertyName, BindingFlags flags = BFS)
        => GetGetStaticPropertyDelegate(typeof(T), propertyName, flags);
    public static Func<object?> GetGetStaticPropertyDelegate(Type type, string propertyName, BindingFlags flags = BFS)
        => GetGetStaticPropertyDelegate(type.GetProperty(propertyName, flags)!);

    public static Func<TProperty> GetGetStaticPropertyDelegate<TProperty>(PropertyInfo property) {
        return property.GetMethod!.CreateDelegate<Func<TProperty>>();
    }
    public static Func<object?> GetGetStaticPropertyDelegate(PropertyInfo property) {
        DynamicMethod method = new("GetProperty", typeof(object), null, true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Call, property.GetMethod!);
        if (property.PropertyType.IsValueType) {
            il.Emit(SOpCodes.Box, property.PropertyType);
        }
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Func<object>>();
    }
    #endregion

    #endregion
    #region 设置属性的委托

    #region 设置实例属性的委托
    public static Action<T, TProperty> GetSetPropertyDelegate<T, TProperty>(string propertyName, BindingFlags flags = BFI) where T : class
        => GetSetPropertyDelegate<T, TProperty>(typeof(T).GetProperty(propertyName, flags)!);
    public static Action<object, TProperty> GetSetPropertyDelegate<TProperty>(Type type, string propertyName, BindingFlags flags = BFI)
        => GetSetPropertyDelegate<TProperty>(type, type.GetProperty(propertyName, flags)!);
    public static Action<T, object?> GetSetPropertyDelegate<T>(string propertyName, BindingFlags flags = BFI) where T : class
        => GetSetPropertyToObjectDelegate<T>(typeof(T), propertyName, flags);
    public static Action<object, object?> GetSetPropertyDelegate(Type type, string propertyName, BindingFlags flags = BFI)
        => GetSetPropertyDelegate(type, type.GetProperty(propertyName, flags)!);
    public static RefSetMemberDelegate<T, TProperty> GetSetPropertyDelegateN<T, TProperty>(Type type, string propertyName, BindingFlags flags = BFI) where T : struct
        => GetSetPropertyDelegateN<T, TProperty>(type.GetProperty(propertyName, flags)!);
    public static RefSetMemberDelegate<T> GetSetPropertyDelegateN<T>(Type type, string propertyName, BindingFlags flags = BFI) where T : struct
        => GetSetPropertyDelegateN<T>(type.GetProperty(propertyName, flags)!);

    public static Action<object, TProperty> GetSetPropertyFromObjectDelegate<TProperty>(Type type, string propertyName, BindingFlags flags = BFI)
        => GetSetPropertyDelegate<TProperty>(type, propertyName, flags);
    public static Action<T, object?> GetSetPropertyToObjectDelegate<T>(Type type, string propertyName, BindingFlags flags = BFI) where T : class
        => GetSetPropertyDelegate<T>(type.GetProperty(propertyName, flags)!);
    public static RefSetMemberDelegate<T> GetSetPropertyToObjectDelegateN<T>(Type type, string propertyName, BindingFlags flags = BFI) where T : struct
        => GetSetPropertyDelegateN<T>(type.GetProperty(propertyName, flags)!);

    public static Action<T, TProperty> GetSetPropertyDelegate<T, TProperty>(PropertyInfo property) where T : class {
        return property.SetMethod!.CreateDelegate<Action<T, TProperty>>();
    }
    public static Action<object, TProperty> GetSetPropertyDelegate<TProperty>(Type type, PropertyInfo property) {
        DynamicMethod method = new("SetProperty", null, [typeof(object), typeof(TProperty)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (type.IsValueType)
            il.Emit(SOpCodes.Unbox, type);
        else
            il.Emit(SOpCodes.Castclass, type);
        il.Emit(SOpCodes.Ldarg_1);
        il.Emit(SOpCodes.Callvirt, property.SetMethod!);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Action<object, TProperty>>();
    }
    public static Action<T, object?> GetSetPropertyDelegate<T>(PropertyInfo property) where T : class {
        Type type = typeof(T);
        DynamicMethod method = new("SetProperty", null, [type, typeof(object)], true);
        var il = method.GetILGenerator();
        // if (type.IsValueType)
        //     il.Emit(SOpCodes.Ldarga, 0);
        // else
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Ldarg_1);
        if (property.PropertyType.IsValueType) {
            il.Emit(SOpCodes.Unbox_Any, property.PropertyType);
        }
        else {
            il.Emit(SOpCodes.Castclass, property.PropertyType);
        }
        il.Emit(SOpCodes.Callvirt, property.SetMethod!);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Action<T, object?>>();
    }
    public static Action<object, object?> GetSetPropertyDelegate(Type type, PropertyInfo property) {
        DynamicMethod method = new("SetProperty", null, [typeof(object), typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (type.IsValueType)
            il.Emit(SOpCodes.Unbox, type);
        else
            il.Emit(SOpCodes.Castclass, type);
        il.Emit(SOpCodes.Ldarg_1);
        if (property.PropertyType.IsValueType) {
            il.Emit(SOpCodes.Unbox_Any, property.PropertyType);
        }
        else {
            il.Emit(SOpCodes.Castclass, property.PropertyType);
        }
        il.Emit(SOpCodes.Callvirt, property.SetMethod!);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Action<object, object?>>();
    }
    public static RefSetMemberDelegate<T, TProperty> GetSetPropertyDelegateN<T, TProperty>(PropertyInfo property) where T : struct {
        Type type = typeof(T);
        DynamicMethod method = new("SetProperty", null, [type.MakeByRefType(), typeof(TProperty)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Ldarg_1);
        il.Emit(SOpCodes.Callvirt, property.SetMethod!);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<RefSetMemberDelegate<T, TProperty>>();
    }
    public static RefSetMemberDelegate<T> GetSetPropertyDelegateN<T>(PropertyInfo property) where T : struct {
        Type type = typeof(T);
        DynamicMethod method = new("SetProperty", null, [type.MakeByRefType(), typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        il.Emit(SOpCodes.Ldarg_1);
        if (property.PropertyType.IsValueType) {
            il.Emit(SOpCodes.Unbox_Any, property.PropertyType);
        }
        else {
            il.Emit(SOpCodes.Castclass, property.PropertyType);
        }
        il.Emit(SOpCodes.Callvirt, property.SetMethod!);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<RefSetMemberDelegate<T>>();
    }

    public static Action<object, object?> GetSetPropertyDelegate(PropertyInfo property)
        => GetSetPropertyDelegate(property.DeclaringType!, property);
    public static Action<object, TProperty> GetSetPropertyFromObjectDelegate<TProperty>(PropertyInfo property)
        => GetSetPropertyDelegate<TProperty>(property.DeclaringType!, property);
    public static Action<T, object?> GetSetPropertyToObjectDelegate<T>(PropertyInfo property) where T : class
        => GetSetPropertyDelegate<T>(property);
    public static RefSetMemberDelegate<T> GetSetPropertyToObjectDelegateN<T>(PropertyInfo property) where T : struct
        => GetSetPropertyDelegateN<T>(property);
    #endregion

    #region 设置静态属性的委托
    public static Action<TProperty> GetSetStaticPropertyDelegate<T, TProperty>(string propertyName, BindingFlags flags = BFS)
        => GetSetStaticPropertyDelegate<TProperty>(typeof(T), propertyName, flags);
    public static Action<TProperty> GetSetStaticPropertyDelegate<TProperty>(Type type, string propertyName, BindingFlags flags = BFS)
        => GetSetStaticPropertyDelegate<TProperty>(type.GetProperty(propertyName, flags)!);
    public static Action<object?> GetSetStaticPropertyDelegate<T>(string propertyName, BindingFlags flags = BFS)
        => GetSetStaticPropertyDelegate(typeof(T), propertyName, flags);
    public static Action<object?> GetSetStaticPropertyDelegate(Type type, string propertyName, BindingFlags flags = BFS)
        => GetSetStaticPropertyDelegate(type.GetProperty(propertyName, flags)!);

    public static Action<TProperty> GetSetStaticPropertyDelegate<TProperty>(PropertyInfo property) {
        return property.SetMethod!.CreateDelegate<Action<TProperty>>();
    }
    public static Action<object?> GetSetStaticPropertyDelegate(PropertyInfo property) {
        DynamicMethod method = new("SetProperty", null, [typeof(object)], true);
        var il = method.GetILGenerator();
        il.Emit(SOpCodes.Ldarg_0);
        if (property.PropertyType.IsValueType) {
            il.Emit(SOpCodes.Unbox_Any, property.PropertyType);
        }
        else {
            il.Emit(SOpCodes.Castclass, property.PropertyType);
        }
        il.Emit(SOpCodes.Call, property.SetMethod!);
        il.Emit(SOpCodes.Ret);
        return method.CreateDelegate<Action<object?>>();
    }
    #endregion

    #endregion
    #endregion
    #region 创建方法 CreateMethod
    public static TDelegate CreateMethod<TDelegate>(string name, Action<ILGenerator> generate) where TDelegate : Delegate {
        var invoke = typeof(TDelegate).GetMethod("Invoke", BFI)
            ?? throw new ArgumentException("TDelegate must have exact one instance \"Invoke\" method");
        DynamicMethod method = new(name, invoke.ReturnType, invoke.GetParameters().Select(p => p.ParameterType).ToArray(), true);
        var il = method.GetILGenerator();
        generate(il);
        return method.CreateDelegate<TDelegate>();
    }
    public static TDelegate CreateMethod<TDelegate>(string name, IEnumerable<(SOpCode opCode, object? operand)> instrs) where TDelegate : Delegate {
        return CreateMethod<TDelegate>(name, il => {
            foreach (var instr in instrs) {
                var opCode = instr.opCode;
                switch (instr.operand) {
                case null:
                    il.Emit(opCode);
                    break;
                case Type cls:
                    il.Emit(opCode, cls);
                    break;
                case string str:
                    il.Emit(opCode, str);
                    break;
                case float arg:
                    il.Emit(opCode, arg);
                    break;
                case sbyte arg:
                    il.Emit(opCode, arg);
                    break;
                case MethodInfo meth:
                    il.Emit(opCode, meth);
                    break;
                case FieldInfo field:
                    il.Emit(opCode, field);
                    break;
                case Label[] labels:
                    il.Emit(opCode, labels);
                    break;
                case SignatureHelper signature:
                    il.Emit(opCode, signature);
                    break;
                case LocalBuilder local:
                    il.Emit(opCode, local);
                    break;
                case ConstructorInfo con:
                    il.Emit(opCode, con);
                    break;
                case long arg:
                    il.Emit(opCode, arg);
                    break;
                case int arg:
                    il.Emit(opCode, arg);
                    break;
                case short arg:
                    il.Emit(opCode, arg);
                    break;
                case double arg:
                    il.Emit(opCode, arg);
                    break;
                case byte arg:
                    il.Emit(opCode, arg);
                    break;
                case Label label:
                    il.Emit(opCode, label);
                    break;
                default:
                    throw new ArgumentException("Not supported type of operand: " + instr.operand.GetType());
                }
            }
        });
    }
    public static TDelegate CreateMethod<TDelegate>(string name, params (SOpCode opCode, object? operand)[] instrs) where TDelegate : Delegate {
        return CreateMethod<TDelegate>(name, (IEnumerable<(SOpCode opCode, object? operand)>)instrs);
    }
    #endregion
    #region Clone
    private static Lazy<Func<object, object>> memberwiseCloneFunc = new(() => GetMethodDelegate<object, Func<object, object>>("MemberwiseClone"));
    public static T ShallowClone<T>(T obj) where T : notnull {
        if (obj is ICloneable cloneable) {
            return (T)cloneable.Clone();
        }
        return (T)memberwiseCloneFunc.Value(obj);
    }
    #endregion
    #region Null 相关
    public static T ThrowIfNull<T>([NotNull] T? self, string? message = null) {
        if (self == null) {
            throw new NullReferenceException(message);
        }
        return self;
    }
#pragma warning disable CS8777 // 退出时，参数必须具有非 null 值。
    // 用来跟踪哪里使用了 xxx!
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNull<T>([NotNull] T? self) => self!;
#pragma warning restore CS8777 // 退出时，参数必须具有非 null 值。
    #endregion
    #region 杂项
    public static void Swap<T>(ref T left, ref T right) => (left, right) = (right, left);
    /// <summary>
    /// Undo, Dispose and set to null
    /// </summary>
    public static void FullyUndoILHook(ref ILHook? ilHook) {
        if (ilHook == null) {
            return;
        }
        ilHook.Undo();
        ilHook.Dispose();
        ilHook = null;
    }
    /// <summary>
    /// Undo, Dispose and set to null
    /// </summary>
    public static void FullyUndoHook(ref Hook? hook) {
        if (hook == null) {
            return;
        }
        hook.Undo();
        hook.Dispose();
        hook = null;
    }
    public static T Instance<T>() => StaticInstance<T>.Value;
    public static void SetInstance<T>(T value) => StaticInstance<T>.Set(value);
    public static KeyValuePair<TKey, TValue> NewPair<TKey, TValue>(TKey key, TValue value) => new(key, value);
    public static ValueHolder<T> NewHolder<T>(T value) => new(value);
    public static Existable<T> NewExistable<T>(T value) => new(value);
    public static IEqualityComparer<T> NewEqualityComparer<T>(Func<T?, T?, bool> equals, Func<T, int> getHashCode) {
        return new CustomEqualityComparer<T>(equals, getHashCode);
    }
    public static IComparer<T> NewComparer<T>(Func<T?, T?, int> compare) => new CustomComparer<T>(compare);
    public static int ToInt(bool @bool) => @bool ? 1 : 0;
    /// <summary>
    /// 保证类型 <typeparamref name="T"/> 的静态构造已被执行, 如果有的话 (静态构造只会执行一次)
    /// </summary>
    public static void InvokeStaticConstructor<T>() => InvokeStaticConstructor(typeof(T));
    /// <summary>
    /// 保证类型 <paramref name="type"/> 的静态构造已被执行, 如果有的话 (静态构造只会执行一次)
    /// </summary>
    public static void InvokeStaticConstructor(Type type) => type.TypeInitializer?.Invoke(null, null);
    #endregion
}

public static partial class TigerClasses {
    public class ValueHolder<T>(T value) {
        public T Value = value;
        public static implicit operator T(ValueHolder<T> holder) => holder.Value;
        public static implicit operator ValueHolder<T>(T value) => new(value);
    }
    // 对标 Nullable, 但是适用于任意类型, 暂时没有处理 Equals 相关的内容
    public readonly struct Existable<T> {
        public Existable(T value) {
            this.value = value;
            hasValue = true;
        }
        public Existable() { }
        private readonly bool hasValue;
        private readonly T? value;
        public readonly bool HasValue => hasValue;
        public readonly T Value {
            get {
                if (!hasValue) {
                    throw new InvalidOperationException("No value in Existable!");
                }
                return value!;
            }
        }
        public readonly T? GetValueOrDefault() => value;
        [return: NotNullIfNotNull(nameof(defaultValue))]
        public readonly T? GetValueOrDefault(T? defaultValue) {
            return !hasValue ? defaultValue : value;
        }
        public override string ToString() {
            if (!hasValue) {
                return "";
            }
            return value?.ToString() ?? "";
        }
    }
    /// <summary>
    /// Value that is defaulted when got
    /// </summary>
    [Obsolete($"线程不安全, 使用{nameof(Lazy<T>)}代替")]
    public class ValueDG<T>(Func<T> getDefaultValue) {
        private T? value;
        private bool got;
        public T Value {
            get {
                if (got) {
                    return value!;
                }
                got = true;
                value = getDefaultValue();
                return value;
            }
        }
        public static implicit operator T(ValueDG<T> self) => self.Value;
    }
    public struct UncheckedUlongTime(ulong value) : IEquatable<UncheckedUlongTime>, IComparable, IComparable<UncheckedUlongTime> {
        private static ulong fps = 60;
        public static ulong FPS {
            get => fps;
            set {
                if (fps == value) {
                    return;
                }
                fps = value;
                second.Value = fps;
                minite.Value = fps * 60;
            }
        }
        private static UncheckedUlongTime second = new(fps);
        public static UncheckedUlongTime Second => second;
        private static UncheckedUlongTime minite = new(fps * 60);
        public static UncheckedUlongTime Minite => minite;

        public ulong Value { readonly get; set; } = value;
        #region +-*/%
        public static UncheckedUlongTime operator +(UncheckedUlongTime left, UncheckedUlongTime right) {
            unchecked {
                return new(left.Value + right.Value);
            }
        }
        public static UncheckedUlongTime operator +(UncheckedUlongTime left, ulong right) {
            unchecked {
                return new(left.Value + right);
            }
        }
        public static UncheckedUlongTime operator -(UncheckedUlongTime left, UncheckedUlongTime right) {
            unchecked {
                return new(left.Value - right.Value);
            }
        }
        public static UncheckedUlongTime operator -(UncheckedUlongTime left, ulong right) {
            unchecked {
                return new(left.Value - right);
            }
        }
        public static UncheckedUlongTime operator *(UncheckedUlongTime left, UncheckedUlongTime right) {
            unchecked {
                return new(left.Value * right.Value);
            }
        }
        public static UncheckedUlongTime operator *(UncheckedUlongTime left, ulong right) {
            unchecked {
                return new(left.Value * right);
            }
        }
        public static UncheckedUlongTime operator /(UncheckedUlongTime left, UncheckedUlongTime right) {
            unchecked {
                return new(left.Value / right.Value);
            }
        }
        public static UncheckedUlongTime operator /(UncheckedUlongTime left, ulong right) {
            unchecked {
                return new(left.Value / right);
            }
        }
        public static UncheckedUlongTime operator %(UncheckedUlongTime left, UncheckedUlongTime right) {
            unchecked {
                return new(left.Value % right.Value);
            }
        }
        public static UncheckedUlongTime operator %(UncheckedUlongTime left, ulong right) {
            unchecked {
                return new(left.Value % right);
            }
        }
        #endregion
        #region 类型转换
        public static explicit operator ulong(UncheckedUlongTime self) {
            return self.Value;
        }
        public static explicit operator long(UncheckedUlongTime self) {
            unchecked {
                return (long)self.Value;
            }
        }
        public static implicit operator UncheckedUlongTime(ulong self) {
            return new(self);
        }
        public static implicit operator UncheckedUlongTime(long self) {
            unchecked {
                return new((ulong)self);
            }
        }
        #endregion
        #region 比较
        public static bool operator ==(UncheckedUlongTime left, UncheckedUlongTime right) {
            return left.Value == right.Value;
        }
        public static bool operator !=(UncheckedUlongTime left, UncheckedUlongTime right) {
            return left.Value != right.Value;
        }
        public static bool operator <(UncheckedUlongTime left, UncheckedUlongTime right) {
            unchecked {
                return left.Value - right.Value > long.MaxValue;
            }
        }
        public static bool operator >(UncheckedUlongTime left, UncheckedUlongTime right) {
            return right < left;
        }
        public static bool operator <=(UncheckedUlongTime left, UncheckedUlongTime right) {
            return !(right < left);
        }
        public static bool operator >=(UncheckedUlongTime left, UncheckedUlongTime right) {
            return !(left < right);
        }

        public readonly bool Equals(UncheckedUlongTime other) {
            return Value == other.Value;
        }
        public override readonly bool Equals(object? obj) {
            if (obj == null) {
                return false;
            }
            return obj is UncheckedUlongTime time && Equals(time) || obj.Equals(Value);
        }
        public override readonly int GetHashCode() {
            return HashCode.Combine(Value);
        }
        public readonly int CompareTo(object? obj) {
            if (obj is UncheckedUlongTime time) {
                return Value.CompareTo(time.Value);
            }
            return Value.CompareTo(obj);
        }
        public readonly int CompareTo(UncheckedUlongTime other) {
            return Value.CompareTo(other.Value);
        }
        #endregion
        public override readonly string ToString() {
            return Value.ToString();
        }
    }
    /// <summary>
    /// 空类, 用以做标识
    /// </summary>
    public class Identifier { }
    public static class StaticInstance<T> {
        private static bool hasValue;
        private static T? value;
        public static T Value {
            get {
                if (hasValue) {
                    return value!;
                }
                value = (T)Activator.CreateInstance(typeof(T))!;
                if (value == null) {
                    throw new Exception("cannot create instance of type: " + typeof(T).FullName);
                }
                hasValue = true;
                return value;
            }
        }
        public static void Set(T value) => StaticInstance<T>.value = value;
        public static void Clear(T? defaultValue = default) {
            hasValue = false;
            value = defaultValue;
        }
    }
    #region Delegate
    public delegate T Alter<T>(T source);
    public delegate TResult Alter<TSource, TResult>(TSource source);
    public delegate void RefAction<T>(ref T arg);
    #region out
    public delegate TResult OutDelegate<TOut, TResult>(out TOut @out);
    public delegate TResult Out1Delegate<TOut, TResult>(out TOut @out);
    public delegate TResult OutDelegate<T1, TOut, TResult>(T1 t1, out TOut @out);
    public delegate TResult Out1Delegate<TOut, T2, TResult>(out TOut @out, T2 t2);
    public delegate TResult Out2Delegate<T1, TOut, TResult>(T1 t1, out TOut @out);
    public delegate TResult OutDelegate<T1, T2, TOut, TResult>(T1 t1, T2 t2, out TOut @out);
    public delegate TResult Out1Delegate<TOut, T2, T3, TResult>(out TOut @out, T2 t2, T3 t3);
    public delegate TResult Out2Delegate<T1, TOut, T3, TResult>(T1 t1, out TOut @out, T3 t3);
    public delegate TResult Out3Delegate<T1, T2, TOut, TResult>(T1 t1, T2 t2, out TOut @out);
    public delegate TResult OutDelegate<T1, T2, T3, TOut, TResult>(T1 t1, T2 t2, T3 t3, out TOut @out);
    public delegate TResult Out1Delegate<TOut, T2, T3, T4, TResult>(out TOut @out, T2 t2, T3 t3, T4 t4);
    public delegate TResult Out2Delegate<T1, TOut, T3, T4, TResult>(T1 t1, out TOut @out, T3 t3, T4 t4);
    public delegate TResult Out3Delegate<T1, T2, TOut, T4, TResult>(T1 t1, T2 t2, out TOut @out, T4 t4);
    public delegate TResult Out4Delegate<T1, T2, T3, TOut, TResult>(T1 t1, T2 t2, T3 t3, out TOut @out);
    
    public delegate void VoidOutDelegate<TOut>(out TOut @out);
    public delegate void VoidOut1Delegate<TOut>(out TOut @out);
    public delegate void VoidOutDelegate<T1, TOut>(T1 t1, out TOut @out);
    public delegate void VoidOut1Delegate<TOut, T2>(out TOut @out, T2 t2);
    public delegate void VoidOut2Delegate<T1, TOut>(T1 t1, out TOut @out);
    public delegate void VoidOutDelegate<T1, T2, TOut>(T1 t1, T2 t2, out TOut @out);
    public delegate void VoidOut1Delegate<TOut, T2, T3>(out TOut @out, T2 t2, T3 t3);
    public delegate void VoidOut2Delegate<T1, TOut, T3>(T1 t1, out TOut @out, T3 t3);
    public delegate void VoidOut3Delegate<T1, T2, TOut>(T1 t1, T2 t2, out TOut @out);
    public delegate void VoidOutDelegate<T1, T2, T3, TOut>(T1 t1, T2 t2, T3 t3, out TOut @out);
    public delegate void VoidOut1Delegate<TOut, T2, T3, T4>(out TOut @out, T2 t2, T3 t3, T4 t4);
    public delegate void VoidOut2Delegate<T1, TOut, T3, T4>(T1 t1, out TOut @out, T3 t3, T4 t4);
    public delegate void VoidOut3Delegate<T1, T2, TOut, T4>(T1 t1, T2 t2, out TOut @out, T4 t4);
    public delegate void VoidOut4Delegate<T1, T2, T3, TOut>(T1 t1, T2 t2, T3 t3, out TOut @out);
    #endregion
    #endregion
    #region 几何
    public struct DirectedLineD1(float start, float end) {
        public DirectedLineD1(float end) : this(0, end) { }
        public float Start { readonly get => start; set => start = value; }
        public float End { readonly get => end; set => end = value; }
        public float Delta { readonly get => end - start; set => end = start + value; }
        public readonly bool Collide(float point) {
            GetRange(out float left, out float right);
            return left <= point && point <= right;
        }
        public readonly bool CollideI(float point) {
            GetRange(out float left, out float right);
            return left < point && point < right;
        }
        /// <summary>
        /// 自动处理 <paramref name="start"/> 和 <paramref name="end"/> 的大小关系
        /// </summary>
        public readonly bool Collide(float start, float end) {
            GetRange(out float left, out float right);
            if (start < left && end < left) {
                return false;
            }
            if (start > right && end > right) {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 自动处理 <paramref name="start"/> 和 <paramref name="end"/> 的大小关系
        /// </summary>
        public readonly bool CollideI(float start, float end) {
            GetRange(out float left, out float right);
            if (start <= left && end <= left) {
                return false;
            }
            if (start >= right && end >= right) {
                return false;
            }
            return true;
        }
        public readonly bool Collide(DirectedLineD1 line) => Collide(line.Start, line.End);
        public readonly bool CollideI(DirectedLineD1 line) => CollideI(line.Start, line.End);
        public readonly DirectedLineD1? GetCollideRange(DirectedLineD1 line) {
            GetRange(out float left, out float right);
            line.GetRange(out float lineLeft, out float lineRight);
            if (lineRight < left || lineLeft > right) {
                return null;
            }
            return new(Math.Max(left, lineLeft), Math.Min(right, lineRight));
        }
        public readonly float? GetCollidePosition(DirectedLineD1 line) {
            GetRange(out float left, out float right, out bool reverse);
            line.GetRange(out float lineLeft, out float lineRight);
            if (lineRight < left || lineLeft > right) {
                return null;
            }
            return reverse ? Math.Min(right, lineRight) : Math.Max(left, lineLeft);
        }
        public void MakePositive() {
            if (end < start) {
                (end, start) = (start, end);
            }
        }
        public readonly void GetRange(out float left, out float right) {
            if (start <= end) {
                left = start;
                right = end;
            }
            else {
                left = end;
                right = start;
            }
        }
        public readonly void GetRange(out float left, out float right, out bool reverse) {
            if (start <= end) {
                left = start;
                right = end;
                reverse = false;
            }
            else {
                left = end;
                right = start;
                reverse = true;
            }
        }
        /// <summary>
        /// <br/>返回两个线段之间的距离
        /// <br/>如果重合会返回 0
        /// </summary>
        public readonly float Distance(DirectedLineD1 line) {
            GetRange(out float left, out float right);
            line.GetRange(out float lineLeft, out float lineRight);
            if (right <= lineLeft) {
                return lineLeft - right;
            }
            if (lineRight < left) {
                return left - lineRight;
            }
            return 0;
        }
        public readonly float Distance(float point) {
            GetRange(out float left, out float right);
            if (point <= left) {
                return left - point;
            }
            if (point >= right) {
                return point - right;
            }
            return 0;
        }
    }
    public struct DirectedLine(Vector2 start, Vector2 end) {
        public DirectedLine(Vector2 end) : this(Vector2.Zero, end) { }
        public DirectedLine(float startX, float startY, float endX, float endY) : this(new Vector2(startX, startY), new Vector2(endX, endY)) { }
        public Vector2 Start { readonly get => start; set => start = value; }
        public Vector2 End { readonly get => end; set => end = value; }
        public Vector2 Delta { readonly get => end - start; set => end = start + value; }

        public readonly bool Collide(Rect rect) {
            DirectedLine? cutX = CutByX(rect.Left, rect.Right);
            if (cutX == null)
                return false;
            DirectedLineD1 line = new(cutX.Value.Start.Y, cutX.Value.End.Y);
            return line.Collide(rect.Top, rect.Bottom);
        }
        public readonly bool CollideI(Rect rect) {
            DirectedLine? cutX = CutByX(rect.Left, rect.Right);
            if (cutX == null)
                return false;
            DirectedLineD1 line = new(cutX.Value.Start.Y, cutX.Value.End.Y);
            return line.CollideI(rect.Top, rect.Bottom);
        }
        public readonly bool Collide(Circle circle) {
            if (circle.Contains(Start) || circle.Contains(End)) {
                return true;
            }
            // 线段为 (x1, y1) - (x2, y2), 圆心为 (xc, yc), 半径 r
            // 线段方程为 P(t) = (x1, y1) + t⋅(x2 - x1, y2 - y1), 0 <= t <= 1
            // 圆方程为 (x - xc)^2 + (y - yc)^2 = r^2
            // 带入得 ((x1 - xc) + t⋅(x2 - x1))^2 + ((y1 - yc) + t⋅(y2 - y1))^2 = r^2
            // 展开得到 A⋅t^2 + B⋅t + C = 0
            // 其中
            // A = (x2 - x1)^2 + (y2 - y1)^2
            // B = 2 ⋅ ((x2 - x1)(x1 - xc) + (y2 - y1)(y1 - yc))
            // C = (x1 - xc)^2 + (y1 - yc)^2 - r^2
            // 只要至少有一个解在 [0, 1] 中则圆和线段相交
            float dx = End.X - Start.X;
            float dy = End.Y - Start.Y;
            float fx = Start.X - circle.Center.X;
            float fy = Start.Y - circle.Center.Y;
            float a = dx * dx + dy * dy;
            float b = 2 * (fx * dx + fy * dy);
            float c = fx * fx + fy + fy - circle.Radius * circle.Radius;
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return false;
            float sqrtDisc = MathF.Sqrt(discriminant);
            float t1 = (-b - sqrtDisc) / (2 * a);
            float t2 = (-b + sqrtDisc) / (2 * a);
            return t1 >= 0 && t1 <= 1 || t2 >= 0 && t2 <= 1;
        }
        public readonly bool CollideI(Circle circle) {
            if (circle.ContainsI(Start) || circle.ContainsI(End)) {
                return true;
            }
            float dx = End.X - Start.X;
            float dy = End.Y - Start.Y;
            float fx = Start.X - circle.Center.X;
            float fy = Start.Y - circle.Center.Y;
            float a = dx * dx + dy * dy;
            float b = 2 * (fx * dx + fy * dy);
            float c = fx * fx + fy + fy - circle.Radius * circle.Radius;
            float discriminant = b * b - 4 * a * c;
            if (discriminant <= 0)
                return false;
            float sqrtDisc = MathF.Sqrt(discriminant);
            float t1 = (-b - sqrtDisc) / (2 * a);
            float t2 = (-b + sqrtDisc) / (2 * a);
            return t1 > 0 && t1 < 1 || t2 > 0 && t2 < 1;
        }
        public readonly bool CollideO(Circle circle) {
            float dx = End.X - Start.X;
            float dy = End.Y - Start.Y;
            float fx = Start.X - circle.Center.X;
            float fy = Start.Y - circle.Center.Y;
            float a = dx * dx + dy * dy;
            float b = 2 * (fx * dx + fy * dy);
            float c = fx * fx + fy + fy - circle.Radius * circle.Radius;
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return false;
            float sqrtDisc = MathF.Sqrt(discriminant);
            float t1 = (-b - sqrtDisc) / (2 * a);
            float t2 = (-b + sqrtDisc) / (2 * a);
            return t1 >= 0 && t1 <= 1 || t2 >= 0 && t2 <= 1;
        }
        public readonly bool CollideOI(Circle circle) {
            float dx = End.X - Start.X;
            float dy = End.Y - Start.Y;
            float fx = Start.X - circle.Center.X;
            float fy = Start.Y - circle.Center.Y;
            float a = dx * dx + dy * dy;
            float b = 2 * (fx * dx + fy * dy);
            float c = fx * fx + fy + fy - circle.Radius * circle.Radius;
            float discriminant = b * b - 4 * a * c;
            if (discriminant <= 0)
                return false;
            float sqrtDisc = MathF.Sqrt(discriminant);
            float t1 = (-b - sqrtDisc) / (2 * a);
            float t2 = (-b + sqrtDisc) / (2 * a);
            return t1 > 0 && t1 < 1 || t2 > 0 && t2 < 1;
        }

        /// <summary>
        /// 得到有向线段与一个矩形碰撞的结果
        /// 若返回 null 则代表没有碰撞
        /// </summary>
        public readonly Vector2? GetCollidePosition(Rect rect) {
            DirectedLine? cutQ = CutByX(rect.Left, rect.Right);
            if (cutQ == null) {
                return null;
            }
            DirectedLine cut = cutQ.Value;
            DirectedLineD1 rectRangeY = new(rect.Top, rect.Bottom);
            float? collideYQ = new DirectedLineD1(cut.Start.Y, cut.End.Y).GetCollidePosition(rectRangeY);
            if (collideYQ == null) {
                return null;
            }
            float collideY = collideYQ.Value;
            if (cut.Start.Y == cut.End.Y) {
                return cut.Start;
            }
            return new Vector2(GetXOnLineByYF(collideY), collideY);
        }

        public readonly Rect Range() {
            Rect result = new(start, Delta);
            result.MakePositive();
            return result;
        }

        /// <summary>
        /// <br/>获取截取自一定 x 范围内的有向线段
        /// <br/>与 <paramref name="range"/> 的方向无关
        /// </summary>
        public readonly DirectedLine? CutByX(DirectedLineD1 range) {
            range.GetRange(out float rangeLeft, out float rangeRight);
            return CutByXF(rangeLeft, rangeRight);
        }
        /// <summary>
        /// <br/>获取截取自一定 x 范围内的有向线段
        /// <br/>自动处理 <paramref name="rangeLeft"/> 与 <paramref name="rangeRight"/> 的大小关系
        /// </summary>
        public readonly DirectedLine? CutByX(float rangeLeft, float rangeRight) {
            return rangeLeft <= rangeRight ? CutByXF(rangeLeft, rangeRight) : CutByXF(rangeRight, rangeLeft);
        }
        /// <summary>
        /// <br/>获取截取自一定 x 范围内的有向线段
        /// <br/>需要 <paramref name="rangeLeft"/> &lt;= <paramref name="rangeRight"/>
        /// </summary>
        public readonly DirectedLine? CutByXF(float rangeLeft, float rangeRight) {
            new DirectedLineD1(start.X, end.X).GetRange(out float left, out float right, out bool reverse);
            if (rangeRight < left || rangeLeft > right) {
                return null;
            }
            if (left == right) {
                return this;
            }
            bool leftOver = rangeLeft <= left;
            bool rightOver = rangeRight >= right;
            if (leftOver) {
                if (rightOver) {
                    return this;
                }
                float middleY = GetYOnLineByXF(rangeRight);
                Vector2 middle = new(rangeRight, middleY);
                return reverse ? new DirectedLine(middle, end) : new DirectedLine(start, middle);
            }
            if (rightOver) {
                float middleY = GetYOnLineByXF(rangeLeft);
                Vector2 middle = new(rangeLeft, middleY);
                return reverse ? new DirectedLine(start, middle) : new DirectedLine(middle, end);
            }
            float middleY1 = GetYOnLineByXF(rangeLeft);
            float middleY2 = GetYOnLineByXF(rangeRight);
            Vector2 middle1 = new(rangeLeft, middleY1);
            Vector2 middle2 = new(rangeRight, middleY2);
            return reverse ? new DirectedLine(middle2, middle1) : new DirectedLine(middle1, middle2);
        }
        /// <summary>
        /// <br/>获取截取自一定 y 范围内的有向线段
        /// <br/>与 <paramref name="range"/> 的方向无关
        /// </summary>
        public readonly DirectedLine? CutByY(DirectedLineD1 range) {
            range.GetRange(out float rangeTop, out float rangeBottom);
            return CutByYF(rangeTop, rangeBottom);
        }
        /// <summary>
        /// <br/>获取截取自一定 y 范围内的有向线段
        /// <br/>自动处理 <paramref name="rangeTop"/> 与 <paramref name="rangeBottom"/> 的大小关系
        /// </summary>
        public readonly DirectedLine? CutByY(float rangeTop, float rangeBottom) {
            return rangeTop <= rangeBottom ? CutByYF(rangeTop, rangeBottom) : CutByYF(rangeBottom, rangeTop);
        }
        /// <summary>
        /// <br/>获取截取自一定 y 范围内的有向线段
        /// <br/>需要 <paramref name="rangeTop"/> &lt;= <paramref name="rangeBottom"/>
        /// </summary>
        public readonly DirectedLine? CutByYF(float rangeTop, float rangeBottom) {
            new DirectedLineD1(start.Y, end.Y).GetRange(out float top, out float bottom, out bool reverse);
            if (rangeBottom < top || rangeTop > bottom) {
                return null;
            }
            if (top == bottom) {
                return this;
            }
            bool topOver = rangeTop <= top;
            bool bottomOver = rangeBottom >= bottom;
            if (topOver) {
                if (bottomOver) {
                    return this;
                }
                float middleX = GetXOnLineByYF(rangeBottom);
                Vector2 middle = new(middleX, rangeBottom);
                return reverse ? new DirectedLine(middle, end) : new DirectedLine(start, middle);
            }
            if (topOver) {
                float middleX = GetXOnLineByYF(rangeTop);
                Vector2 middle = new(middleX, rangeTop);
                return reverse ? new DirectedLine(start, middle) : new DirectedLine(middle, end);
            }
            float middleX1 = GetXOnLineByYF(rangeTop);
            float middleX2 = GetXOnLineByYF(rangeBottom);
            Vector2 middle1 = new(middleX1, rangeTop);
            Vector2 middle2 = new(middleX2, rangeBottom);
            return reverse ? new DirectedLine(middle2, middle1) : new DirectedLine(middle1, middle2);
        }
        /// <summary>
        /// <br/>获取截取自一定范围内的有向线段
        /// <br/>与 <paramref name="rect"/> 的方向性无关
        /// </summary>
        public readonly DirectedLine? CutByRect(Rect rect) {
            return CutByX(rect.Left, rect.Right)?.CutByY(rect.Top, rect.Bottom);
        }

        /// <summary>
        /// 在所属直线上寻找 x 对应的 y 值
        /// 若直线竖直则返回 null
        /// </summary>
        public readonly float? GetYOnLineByX(float x) {
            if (start.X == end.X) {
                return null;
            }
            return GetYOnLineByXF(x);
        }
        /// <summary>
        /// 在所属直线上寻找 x 对应的 y 值
        /// 需确保直线不竖直
        /// </summary>
        public readonly float GetYOnLineByXF(float x) {
            float k = (end.Y - start.Y) / (end.X - start.X);
            return k * (x - start.X) + start.Y;
        }
        /// <summary>
        /// 在所属直线上寻找 y 对应的 x 值
        /// 若直线水平则返回 null
        /// </summary>
        public readonly float? GetXOnLineByY(float y) {
            if (start.Y == end.Y) {
                return null;
            }
            return GetXOnLineByY(y);
        }
        /// <summary>
        /// 在所属直线上寻找 y 对应的 x 值
        /// 需确保直线不水平
        /// </summary>
        public readonly float GetXOnLineByYF(float y) {
            float k = (end.X - start.X) / (end.Y - start.Y);
            return k * (y - start.Y) + start.X;
        }

        public static implicit operator DirectedLine((Vector2, Vector2) tuple) {
            return new(tuple.Item1, tuple.Item2);
        }
    }
    /// <summary>
    /// 代表一个矩形, 允许长和宽为负数
    /// </summary>
    public struct Rect(float x, float y, float width, float height) {
        public Rect(Vector2 position, Vector2 size) : this(position.X, position.Y, size.X, size.Y) { }
        public Vector2 Position { readonly get => new(x, y); set => (x, y) = value; }
        public Vector2 Size { readonly get => new(width, height); set => (width, height) = value; }
        public float X { readonly get => x; set => x = value; }
        public float Y { readonly get => y; set => y = value; }
        public float Width { readonly get => width; set => width = value; }
        public float Height { readonly get => height; set => height = value; }
        public float Left { readonly get => x; set => x = value; }
        public float Right { readonly get => x + width; set => x = value - width; }
        public float Top { readonly get => y; set => y = value; }
        public float Bottom { readonly get => y + height; set => y = value - height; }
        public Vector2 RealSize { readonly get => new(RealWidth, RealHeight); set => (RealWidth, RealHeight) = value; }
        public float RealWidth { readonly get => MathF.Abs(width); set => width = ToInt(width >= 0) * value; }
        public float RealHeight { readonly get => MathF.Abs(height); set => height = ToInt(height >= 0) * value; }
        public float RealLeft { readonly get => width >= 0 ? x : (x + width); set => x = width >= 0 ? value : (value - width); }
        public float RealRight { readonly get => width <= 0 ? x : (x + width); set => x = width <= 0 ? value : (value - width); }
        public float RealTop { readonly get => height >= 0 ? y : (y + height); set => y = height >= 0 ? value : (value - height); }
        public float RealBottom { readonly get => height <= 0 ? y : (y + height); set => y = height <= 0 ? value : (value - height); }

        public Vector2 Center { readonly get => new(x + width / 2, y + height / 2); set => (x, y) = (value.X - width / 2, value.Y - height / 2); }

        /// <summary>
        /// 让 <see cref="Width"/> 与 <see cref="Height"/> 保持非负
        /// </summary>
        public void MakePositive() {
            if (Width < 0) {
                X += Width;
                Width = -Width;
            }
            if (Height < 0) {
                Y += Height;
                Height = -Height;
            }
        }
        public Rect MakePositiveL() {
            MakePositive();
            return this;
        }
        public readonly void GetRange(out float left, out float right, out float top, out float bottom) {
            if (Width >= 0) {
                left = Left;
                right = Right;
            }
            else {
                left = Right;
                right = Left;
            }
            if (Height >= 0) {
                top = Top;
                bottom = Bottom;
            }
            else {
                top = Bottom;
                bottom = top;
            }
        }
        public readonly bool Collide(Vector2 point) {
            GetRange(out float left, out float right, out float top, out float bottom);
            return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
        }
        public readonly bool CollideI(Vector2 point) {
            GetRange(out float left, out float right, out float top, out float bottom);
            return point.X > left && point.X < right && point.Y > top && point.Y < bottom;
        }
        public readonly bool Collide(DirectedLine line) => line.Collide(this);
        public readonly bool CollideI(DirectedLine line) => line.CollideI(this);
        /// <summary>
        /// 返回两个 <see cref="Rect"/> 是否有重合部分, 包括边界
        /// </summary>
        public readonly bool Collide(Rect rect) {
            var positionDelta = rect.Position - Position;

            static bool Check(float delta, float x1, float x2) {
                // delta 为一维空间中两线段起点的差值
                // x1 为第一条线段的长度
                // x2 为第二条线段的长度
                // 返回两线段是否有重合区域
                if (delta < 0) {
                    delta = -delta;
                    x1 = -x1;
                }
                else if (delta > 0) {
                    x2 = -x2;
                }
                else {
                    return true;
                }
                if (x1 <= 0) {
                    return x2 >= delta;
                }
                if (x2 <= 0) {
                    return x1 >= delta;
                }
                return x1 + x2 >= delta;
            }
            return Check(positionDelta.X, Width, rect.Width) && Check(positionDelta.Y, Height, rect.Height);
        }
        public readonly bool CollideI(Rect rect) {
            var positionDelta = rect.Position - Position;

            static bool CheckI(float delta, float x1, float x2) {
                // delta 为一维空间中两线段起点的差值
                // x1 为第一条线段的长度
                // x2 为第二条线段的长度
                // 返回两线段是否有重合区域
                if (delta < 0) {
                    delta = -delta;
                    x1 = -x1;
                }
                else if (delta > 0) {
                    x2 = -x2;
                }
                if (x1 <= 0) {
                    return x2 > delta;
                }
                if (x2 <= 0) {
                    return x1 > delta;
                }
                return x1 + x2 > delta;
            }
            return CheckI(positionDelta.X, Width, rect.Width) && CheckI(positionDelta.Y, Height, rect.Height);
        }
        public readonly bool Collide(Circle circle) {
            var xDistance = new DirectedLineD1(Left, Right).Distance(circle.Center.X);
            var yDistance = new DirectedLineD1(Top, Bottom).Distance(circle.Center.Y);
            if (xDistance == 0) {
                return yDistance <= circle.Radius;
            }
            if (yDistance == 0) {
                return xDistance <= circle.Radius;
            }
            return xDistance * xDistance + yDistance * yDistance <= circle.Radius * circle.Radius;
        }
        public readonly bool CollideI(Circle circle) {
            if (circle.Radius <= 0) {
                return CollideI(circle.Center);
            }
            var xDistance = new DirectedLineD1(Left, Right).Distance(circle.Center.X);
            var yDistance = new DirectedLineD1(Top, Bottom).Distance(circle.Center.Y);
            if (xDistance == 0) {
                return yDistance < circle.Radius;
            }
            if (yDistance == 0) {
                return xDistance < circle.Radius;
            }
            return xDistance * xDistance + yDistance * yDistance < circle.Radius * circle.Radius;
        }
        #region Distance
        public readonly float Distance(Vector2 point) {
            var xDistance = new DirectedLineD1(Left, Right).Distance(point.X);
            var yDistance = new DirectedLineD1(Top, Bottom).Distance(point.Y);
            if (xDistance == 0) {
                return yDistance;
            }
            if (yDistance == 0) {
                return xDistance;
            }
            return MathF.Sqrt(xDistance * xDistance + yDistance * yDistance);
        }
        public readonly float DistanceSquared(Vector2 point) {
            var xDistance = new DirectedLineD1(Left, Right).Distance(point.X);
            var yDistance = new DirectedLineD1(Top, Bottom).Distance(point.Y);
            if (xDistance == 0) {
                return yDistance * yDistance;
            }
            if (yDistance == 0) {
                return xDistance * xDistance;
            }
            return xDistance * xDistance + yDistance * yDistance;
        }

        /// <summary>
        /// <br/>返回两个 <see cref="Rect"/> 之间的距离
        /// <br/>如果重合, 会返回 0
        /// </summary>
        public readonly float Distance(Rect rect) {
            var xDistance = new DirectedLineD1(Left, Right).Distance(new DirectedLineD1(rect.Left, rect.Right));
            var yDistance = new DirectedLineD1(Top, Bottom).Distance(new DirectedLineD1(rect.Top, rect.Bottom));
            if (xDistance == 0) {
                return yDistance;
            }
            if (yDistance == 0) {
                return xDistance;
            }
            return MathF.Sqrt(xDistance * xDistance + yDistance * yDistance);
        }
        public readonly float DistanceSquared(Rect rect) {
            var xDistance = new DirectedLineD1(Left, Right).Distance(new DirectedLineD1(rect.Left, rect.Right));
            var yDistance = new DirectedLineD1(Top, Bottom).Distance(new DirectedLineD1(rect.Top, rect.Bottom));
            if (xDistance == 0) {
                return yDistance * yDistance;
            }
            if (yDistance == 0) {
                return xDistance * xDistance;
            }
            return xDistance * xDistance + yDistance * yDistance;
        }

        public readonly float Distance(Circle circle) {
            return (Distance(circle.Center) - circle.Radius).WithMin(0);
        }
        #endregion

        public static implicit operator Rectangle(Rect rect) => NewRectangle(rect.X, rect.Y, rect.Width, rect.Height);
        public static implicit operator Rect(Rectangle rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }
    public struct Circle {
        public float CenterX { readonly get; set; }
        public float CenterY { readonly get; set; }
        public Vector2 Center { readonly get => new(CenterX, CenterY); set => (CenterX, CenterY) = value; }
        private float _radius;
        public float Radius {
            readonly get => _radius;
            set {
                if (value < 0) {
                    throw new ArgumentException("radius should not be below 0", nameof(value));
                }
                _radius = value;
            }
        }
        public Circle(float centerX, float centerY, float radius) {
            if (radius < 0) {
                throw new ArgumentException("radius should not be below 0", nameof(radius));
            }
            CenterX = centerX;
            CenterY = centerY;
            _radius = radius;
        }
        public Circle(Vector2 center, float radius) : this(center.X, center.Y, radius) { }
        /// <summary>
        /// 外切矩形
        /// </summary>
        public readonly Rect EnclosingRect => new(CenterX - Radius, CenterY - Radius, 2 * Radius, 2 * Radius);
        #region Collide
        public readonly bool Collide(Vector2 point) {
            return Vector2.DistanceSquared(Center, point) <= Radius * Radius;
        }
        public readonly bool CollideI(Vector2 point) {
            return Vector2.DistanceSquared(Center, point) < Radius * Radius;
        }
        public readonly bool CollideO(Vector2 point) {
            return Vector2.DistanceSquared(Center, point) == Radius * Radius;
        }
        public readonly bool Collide(DirectedLine line) => line.Collide(this);
        public readonly bool CollideI(DirectedLine line) => line.CollideI(this);
        public readonly bool CollideO(DirectedLine line) => line.CollideO(this);
        public readonly bool CollideOI(DirectedLine line) => line.CollideOI(this);
        public readonly bool Collide(Circle circle) {
            var radiusSum = Radius + circle.Radius;
            return Vector2.DistanceSquared(Center, circle.Center) <= radiusSum * radiusSum;
        }
        public readonly bool CollideI(Circle circle) {
            var radiusSum = Radius + circle.Radius;
            return Vector2.DistanceSquared(Center, circle.Center) < radiusSum * radiusSum;
        }
        public readonly bool Collide(Rect rect) => rect.Collide(this);
        public readonly bool CollideI(Rect rect) => rect.CollideI(this);
        public readonly bool Contains(Vector2 point) {
            return Vector2.DistanceSquared(Center, point) <= Radius * Radius;
        }
        public readonly bool ContainsI(Vector2 point) {
            return Vector2.DistanceSquared(Center, point) < Radius * Radius;
        }
        #endregion
        #region Distance
        public readonly float Distance(Vector2 point) {
            return (Vector2.Distance(Center, point) - Radius).WithMin(0);
        }
        public readonly float Distance(Circle circle) {
            return (Vector2.Distance(Center, circle.Center) - Radius - circle.Radius).WithMin(0);
        }
        public readonly float Distance(Rect rect) {
            return rect.Distance(this);
        }
        #endregion
    }
    #endregion
    #region 杂项
    public class CustomEqualityComparer<T>(Func<T?, T?, bool> equals, Func<T, int> getHashCode) : IEqualityComparer<T> {
        public bool Equals(T? x, T? y) => equals(x, y);
        public int GetHashCode([DisallowNull] T obj) => getHashCode(obj);
    }
    public class CustomComparer<T>(Func<T?, T?, int> compare) : IComparer<T> {
        public int Compare(T? x, T? y) => compare(x, y);
    }
    public class ComparisonComparer<T>(Comparison<T> comparison) : IComparer<T> {
        private readonly Comparison<T> _comparison = comparison;
        public int Compare(T? x, T? y) => _comparison(NotNull(x), NotNull(y));
    }
    #endregion
}

public static class TigerStatics {
    #region BindingFlags
    public const BindingFlags BFALL = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    public const BindingFlags BFP   = BindingFlags.Public                          | BindingFlags.Static | BindingFlags.Instance;
    public const BindingFlags BFN   =                       BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    public const BindingFlags BFS   = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static                        ;
    public const BindingFlags BFI   = BindingFlags.Public | BindingFlags.NonPublic |                       BindingFlags.Instance;
    public const BindingFlags BFPS  = BindingFlags.Public                          | BindingFlags.Static                        ;
    public const BindingFlags BFNS  =                       BindingFlags.NonPublic | BindingFlags.Static                        ;
    public const BindingFlags BFPI  = BindingFlags.Public                          |                       BindingFlags.Instance;
    public const BindingFlags BFNI  =                       BindingFlags.NonPublic |                       BindingFlags.Instance;
    #endregion
}

public static partial class TigerExtensions {
    #region Lua的 And / Or 体系
    /// <summary>
    /// 若<paramref name="i"/>判定为真则返回<paramref name="i"/>, 否则返回<paramref name="o"/>
    /// </summary>
    public static T LuaOr<T>(this T i, T o) {
        return Convert.ToBoolean(i) ? i : o;
    }
    /// <summary>
    /// 若<paramref name="i"/>判定为假则返回<paramref name="i"/>, 否则返回<paramref name="o"/>
    /// </summary>
    public static T LuaAnd<T>(this T i, T o) {
        return Convert.ToBoolean(i) ? o : i;
    }
    /// <summary>
    /// 若i判定为假则将o赋值给i
    /// 对于引用类型, 一般相当于 ??=
    /// </summary>
    public static T LuaOrAssignFrom<T>(this ref T i, T o) where T : struct {
        if (!Convert.ToBoolean(i)) {
            i = o;
        }
        return i;
    }
    /// <summary>
    /// 若i判定为假则将o赋值给i
    /// </summary>
    public static T LuaAndAssignFrom<T>(this ref T i, T o) where T : struct {
        if (Convert.ToBoolean(i)) {
            i = o;
        }
        return i;
    }
    #endregion
    #region Clamp
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>最好要保证<paramref name="left"/> 不大于 <paramref name="right"/>, 
    /// <br/>否则最好使用<see cref="ClampS{T}(T, T, T)"/>
    /// <br/>优先保证不小于<paramref name="left"/>
    /// </summary>
    public static T Clamp<T>(this T self, T left, T right) where T : IComparable<T>
        => self.CompareTo(left) < 0 ? left : self.CompareTo(right) > 0 ? right : self;
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>最好要保证<paramref name="left"/> 不大于 <paramref name="right"/>, 
    /// <br/>否则最好使用<see cref="ClampToS{T}(ref T, T, T)"/>
    /// <br/>优先保证不小于<paramref name="left"/>
    /// </summary>
    public static ref T ClampTo<T>(ref this T self, T left, T right) where T : struct, IComparable<T>
        => ref self.Assign(self.CompareTo(left) < 0 ? left : self.CompareTo(right) > 0 ? right : self);
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>自动判断<paramref name="left"/>和<paramref name="right"/>的大小关系
    /// </summary>
    public static T ClampS<T>(this T self, T left, T right) where T : IComparable<T>
        => left.CompareTo(right) > 0 ? self.Clamp(right, left) : self.Clamp(left, right);
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>自动判断<paramref name="left"/>和<paramref name="right"/>的大小关系
    /// </summary>
    public static ref T ClampToS<T>(ref this T self, T left, T right) where T : struct, IComparable<T>
        => ref left.CompareTo(right) > 0 ? ref self.ClampTo(right, left) : ref self.ClampTo(left, right);
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>最好要保证<paramref name="left"/> 不大于 <paramref name="right"/>, 
    /// <br/>否则最好使用<see cref="ClampS{T}(T, T, T)"/>
    /// <br/>优先保证不大于<paramref name="right"/>
    /// </summary>
    public static T ClampR<T>(this T self, T left, T right) where T : IComparable<T>
        => self.CompareTo(right) > 0 ? right : self.CompareTo(left) < 0 ? left : self;
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>最好要保证<paramref name="left"/> 不大于 <paramref name="right"/>, 
    /// <br/>否则最好使用<see cref="ClampToS{T}(ref T, T, T)"/>
    /// <br/>优先保证不大于<paramref name="right"/>
    /// </summary>
    public static ref T ClampToR<T>(ref this T self, T left, T right) where T : struct, IComparable<T>
        => ref self.Assign(self.CompareTo(right) > 0 ? right : self.CompareTo(left) < 0 ? left : self);
    public static T ClampMin<T>(this T self, T min) where T : IComparable<T>
        => self.CompareTo(min) < 0 ? min : self;
    public static ref T ClampMinTo<T>(ref this T self, T min) where T : struct, IComparable<T>
        => ref self.CompareTo(min) > 0 ? ref self : ref self.Assign(min);
    public static T ClampMax<T>(this T self, T max) where T : IComparable<T>
        => self.CompareTo(max) > 0 ? max : self;
    public static ref T ClampMaxTo<T>(ref this T self, T max) where T : struct, IComparable<T>
        => ref self.CompareTo(max) < 0 ? ref self : ref self.Assign(max);
    /// <summary>
    /// <br/>比较平缓的Clamp方式, 当<paramref name="self"/>在<paramref name="left"/>和<paramref name="right"/>正中间时不变
    /// <br/>在两边时会逐渐趋向两边的值, 但不会达到
    /// <br/>不需要注意<paramref name="left"/>和<paramref name="right"/>的大小关系
    /// </summary>
    /// <param name="width">
    /// 代表变化的缓度, 为1时当<paramref name="self"/>到达<paramref name="left"/>或<paramref name="right"/>时,
    /// 实际得到的值还差25%左右, 当此值越小, 相差的值越小
    /// <br/>与<paramref name="self"/>在<paramref name="left"/>和<paramref name="right"/>正中间的斜率成反比
    /// </param>
    public static double ClampWithTanh(this double self, double left, double right, double width = 1) {
        if (left == right) {
            return left;
        }
        double halfDelta = (right - left) / 2;
        double middle = left + halfDelta;
        return middle + halfDelta * Math.Tanh((self - middle) / halfDelta / width);
    }
    /// <summary>
    /// <br/>比较平缓的Clamp方式, 当<paramref name="self"/>在<paramref name="left"/>和<paramref name="right"/>正中间时不变
    /// <br/>在两边时会逐渐趋向两边的值, 但不会达到
    /// <br/>不需要注意<paramref name="left"/>和<paramref name="right"/>的大小关系
    /// </summary>
    /// <param name="width">
    /// 代表变化的缓度, 为1时当<paramref name="self"/>到达<paramref name="left"/>或<paramref name="right"/>时,
    /// 实际得到的值还差25%左右, 当此值越小, 相差的值越小
    /// <br/>与<paramref name="self"/>在<paramref name="left"/>和<paramref name="right"/>正中间的斜率互为倒数
    /// </param>
    public static ref double ClampWithTanhTo(ref this double self, double left, double right, double width) {
        if (left == right) {
            self = left;
            return ref self;
        }
        double halfDelta = (right - left) / 2;
        double middle = left + halfDelta;
        self = middle + halfDelta * Math.Tanh((self - middle) / halfDelta / width);
        return ref self;
    }

    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>最好要保证<paramref name="left"/> 不大于 <paramref name="right"/>, 
    /// <br/>否则最好使用<see cref="ClampToS{T}(ref T?, T, T)"/>
    /// <br/>优先保证不小于<paramref name="left"/>
    /// <br/>当自身为空时返回空
    /// </summary>
    public static ref T? ClampTo<T>(ref this T? self, T left, T right) where T : struct, IComparable<T>
        => ref self.HasValue ? ref self.Assign(self.Value.CompareTo(left) < 0 ? left : self.Value.CompareTo(right) > 0 ? right : self) : ref self;
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>自动判断<paramref name="left"/>和<paramref name="right"/>的大小关系
    /// <br/>当自身为空时返回空
    /// </summary>
    public static ref T? ClampToS<T>(ref this T? self, T left, T right) where T : struct, IComparable<T>
        => ref left.CompareTo(right) > 0 ? ref self.ClampTo(right, left) : ref self.ClampTo(left, right);
    /// <summary>
    /// <br/>得到自身被限制在<paramref name="left"/>和<paramref name="right"/>之间的大小
    /// <br/>最好要保证<paramref name="left"/> 不大于 <paramref name="right"/>, 
    /// <br/>否则最好使用<see cref="ClampToS{T}(ref T?, T, T)"/>
    /// <br/>优先保证不大于<paramref name="right"/>
    /// <br/>当自身为空时返回空
    /// </summary>
    public static ref T? ClampToR<T>(ref this T? self, T left, T right) where T : struct, IComparable<T>
        => ref self.HasValue ? ref self.Assign(self.Value.CompareTo(right) > 0 ? right : self.Value.CompareTo(left) < 0 ? left : self) : ref self;
    public static ref T? ClampMinTo<T>(ref this T? self, T min) where T : struct, IComparable<T>
        => ref self.HasValue ? ref self.Value.CompareTo(min) > 0 ? ref self : ref self.Assign(min) : ref self;
    public static ref T? ClampMaxTo<T>(ref this T? self, T max) where T : struct, IComparable<T>
        => ref self.HasValue ? ref self.Value.CompareTo(max) < 0 ? ref self : ref self.Assign(max) : ref self;
    #endregion
    #region With Min / Max
    public static T WithMin<T>(this T self, T min) where T : IComparable<T> => self.CompareTo(min) < 0 ? min : self;
    public static T WithMax<T>(this T self, T max) where T : IComparable<T> => self.CompareTo(max) > 0 ? max : self;
    #endregion
    #region WithAction
    public static T WithAction<T>(this T self, Action<T> action) {
        action(self);
        return self;
    }
    public static ref T WithAction<T>(ref this T self, RefAction<T> action) where T : struct {
        action(ref self);
        return ref self;
    }
    #endregion
    #region IsBetween
    /// <summary>
    /// 返回 <paramref name="left"/> &lt;= <paramref name="self"/> &amp;&amp; <paramref name="self"/> &lt; <paramref name="right"/>
    /// </summary>
    public static bool IsBetween<T>(this T self, T left, T right) where T : IComparable<T>
        => self.CompareTo(left) >= 0 && self.CompareTo(right) < 0;
    /// <summary>
    /// 返回 <paramref name="left"/> &lt; <paramref name="self"/> &amp;&amp; <paramref name="self"/> &lt; <paramref name="right"/>
    /// </summary>
    public static bool IsBetweenI<T>(this T self, T left, T right) where T : IComparable<T>
        => self.CompareTo(left) > 0 && self.CompareTo(right) < 0;
    /// <summary>
    /// <br/>返回 <paramref name="self"/> 是否处于 <paramref name="left"/> 和 <paramref name="right"/> 构成的开区间中
    /// <br/>自动比较 <paramref name="left"/> 和 <paramref name="right"/> 的大小
    /// </summary>
    public static bool IsBetweenIS<T>(this T self, T left, T right) where T : IComparable<T>
        => left.CompareTo(right) < 0
        ? self.CompareTo(left) > 0 && self.CompareTo(right) < 0
        : self.CompareTo(right) > 0 && self.CompareTo(left) < 0;
    /// <summary>
    /// 返回 <paramref name="left"/> &lt;= <paramref name="self"/> &amp;&amp; <paramref name="self"/> &lt;= <paramref name="right"/>
    /// </summary>
    public static bool IsBetweenO<T>(this T self, T left, T right) where T : IComparable<T>
        => self.CompareTo(left) >= 0 && self.CompareTo(right) <= 0;
    #endregion

    #region 反射
#if TIGER_REFLECTION_EXTENSIONS
    /// <summary>
    /// 常用flags:
    /// <see cref="BindingFlags.Public"/>
    /// <see cref="BindingFlags.NonPublic"/>
    /// <see cref="BindingFlags.Instance"/>
    /// <see cref="BindingFlags.Static"/>
    /// </summary>
    public static object GetField(this object self, string fieldName, BindingFlags flags)
        => self.GetType().GetField(fieldName, flags).GetValue(self);
    public static T GetField<T>(this object self, string fieldName, BindingFlags flags)
        => (T)self.GetType().GetField(fieldName, flags).GetValue(self);
    public static void GetField<T>(this object self, out T field, string fieldName, BindingFlags flags)
        => field = (T)self.GetType().GetField(fieldName, flags).GetValue(self);
    public static object GetField(this object self, FieldInfo fieldInfo)
        => fieldInfo.GetValue(self);
    public static T GetField<T>(this object self, FieldInfo fieldInfo)
        => (T)fieldInfo.GetValue(self);
    public static void GetField<T>(this object self, out T field, FieldInfo fieldInfo)
        => field = (T)fieldInfo.GetValue(self);

    public static void SetField(this object self, string fieldName, BindingFlags flags, object value)
        => self.GetType().GetField(fieldName, flags).SetValue(self, value);
    public static void SetField(this object self, FieldInfo fieldInfo, object value)
        => fieldInfo.SetValue(self, value);

    public static object InvokeMethod(this object self, string methodName, BindingFlags flags, params object[] parameters)
        => self.GetType().GetMethod(methodName, flags)?.Invoke(self, parameters);
    public static T InvokeMethod<T>(this object self, string methodName, BindingFlags flags, params object[] parameters)
        => (T)self.GetType().GetMethod(methodName, flags)?.Invoke(self, parameters);
    public static object InvokeMethod(this object self, MethodInfo methodInfo, params object[] parameters)
        => methodInfo.Invoke(self, parameters);
    public static T InvokeMethod<T>(this object self, MethodInfo methodInfo, params object[] parameters)
        => (T)methodInfo.Invoke(self, parameters);

    public static FieldInfo GetFieldInfo(this object self, string fieldName, BindingFlags flags)
        => self.GetType().GetField(fieldName, flags);
    public static FieldInfo GetFieldInfo<T>(string fieldName, BindingFlags flags)
        => typeof(T).GetField(fieldName, flags);

    public static MethodInfo GetMethodInfo(this object self, string methodName, BindingFlags flags)
        => self.GetType().GetMethod(methodName, flags);
    public static MethodInfo GetMethodInfo<T>(string methodName, BindingFlags flags)
        => typeof(T).GetMethod(methodName, flags);
#endif
    #region DeclareLocal
    public static VariableDefinition DeclareLocal<T>(this ILContext il) => DeclareLocal(il, typeof(T));
    public static VariableDefinition DeclareLocal(this ILContext il, Type type)
         => new VariableDefinition(il.Method.DeclaringType.Module.ImportReference(type)).WithAction(il.Body.Variables.Add);
    #endregion
    #region type.GetMethod
    public static MethodInfo? GetMethod<TDelegate>(this Type type, string name, BindingFlags bindingAttr = BFALL, bool matchGeneric = false) => GetMethod(type, name, typeof(TDelegate), bindingAttr, matchGeneric);
    public static MethodInfo? GetMethod(this Type type, string name, Type delegateType, BindingFlags bindingAttr = BFALL, bool matchGeneric = false) {

        if (delegateType.ContainsGenericParameters) {
            throw new ArgumentException("delegateType should not contains generic parameters", nameof(delegateType));
        }
        var invoke = delegateType.GetMethod("Invoke", BFI)
                ?? throw new ArgumentException("delegateType must have exact one Invoke method");
        var parameters = invoke.GetParameters();
        ParameterModifier modifier = new(parameters.Length);
        for (int i = 0; i < parameters.Length; ++i) {
            var parameter = parameters[i];
            if (parameter.IsIn || parameter.IsOut || parameter.IsRetval) {
                modifier[i] = true;
            }
        }
        var result = type.GetMethod(name, bindingAttr, binder: null, invoke.GetParameters().Select(p => p.ParameterType).ToArray(), modifiers: [modifier]);
        if (result != null || !matchGeneric) {
            return result;
        }
        var genericTypes = delegateType.GenericTypeArguments;
        foreach (var method in type.GetMethods(bindingAttr)) {
            // 跳过名字不同的方法.
            if (method.Name != name) {
                continue;
            }
            // 对于方法和方法所在类都不是泛型时在上面已经作出了判断, 直接跳过.
            if (!method.ContainsGenericParameters) // 包括所在类有泛型和此方法本身有泛型
            {
                continue;
            }
            // 获取方法的参数列表
            var methodParameters = method.GetParameters();
            // 若方法参数个数和委托的对不上, 则跳过
            if ((method.IsStatic ? methodParameters.Length : methodParameters.Length + 1) != parameters.Length) {
                continue;
            }
            var realTypeGenericTypes = type.GetGenericArguments();
            var realMethodGenericTypes = method.GetGenericArguments();
            var typeGenericTypes = type.IsGenericTypeDefinition ? realTypeGenericTypes : []; // <- 这会处理嵌套类
            var methodGenericTypes = method.IsGenericMethodDefinition ? realMethodGenericTypes : [];
            bool mismatched = false;
            Type?[] matchedTypeGenericTypes = new Type?[typeGenericTypes.Length];
            Type?[] matchedMethodGenericTypes = new Type?[methodGenericTypes.Length];
            for (int i = 0; i < methodParameters.Length; ++i) {
                var methodParameterType = methodParameters[i].ParameterType;
                var parameterType = parameters[i].ParameterType;
                // 如果和委托的参数匹配, 则继续判断下一个参数.
                if (methodParameterType == parameterType) {
                    continue;
                }
                // 如果不包含泛型参数, 则判断为不匹配, 直接跳出循环.
                if (!methodParameterType.ContainsGenericParameters) {
                    mismatched = true;
                    break;
                }
                // 如果是纯粹的泛型类型 (比如 T, 而不是 List<T> 等)...
                if (methodParameterType.IsGenericParameter) {
                    Type? nullRef = null;
                    ref Type? matchedType = ref nullRef;
                    // 首先在类型的泛型参数中寻找.
                    var index = typeGenericTypes.FindIndexOf(t => t == methodParameterType);
                    // 如果找到了, 那么对应就类型的泛型参数.
                    if (index >= 0) {
                        matchedType = ref matchedTypeGenericTypes[index];
                    }
                    // 如果在类型的泛型参数中未找到...
                    else {
                        // 那么在方法的泛型参数中寻找.
                        index = methodGenericTypes.FindIndexOf(t => t == methodParameterType);
                        // 如果未找到, 那么无法匹配此泛型 (理应不应该出现这种情况), 直接判断为不匹配并跳出循环.
                        if (index < 0) {
                            mismatched = true;
                            break;
                        }
                        // 找到的话则对应的方法的泛型参数.
                        matchedType = ref matchedMethodGenericTypes[index];
                    }
                    // 如果对应参数已存在...
                    if (matchedType != null) {
                        // 匹配则继续.
                        if (matchedType == parameterType) {
                            continue;
                        }
                        // 不匹配则直接结束 (不应该出现).
                        mismatched = true;
                        break;
                    }
                    // 如果还没有对应参数, 则填上
                    matchedType = parameterType;
                    continue;
                }
                // TODO: 像是 List<T> 这种东西的处理
            }
            // 如果有参数无法匹配, 则继续判断下一个方法
            if (mismatched) {
                continue;
            }

            Type newType;
            // 如果已完全匹配, 则按匹配的参数填入泛型中, 并返回填好参数的方法
            if (matchedTypeGenericTypes.All(t => t != null) && matchedMethodGenericTypes.All(t => t != null)) {
                if (type.IsGenericTypeDefinition) {
                    newType = type.MakeGenericType(matchedTypeGenericTypes!);
                    return GetMethod(newType, name, delegateType, bindingAttr, true);
                }
                return method.MakeGenericMethod(matchedMethodGenericTypes!);
            }

            // 如果不能完全匹配, 则转为尝试使用委托类型的泛型参数
            var delegateGenericTypeArguments = delegateType.GenericTypeArguments; // <- 这个只会获取填实了的类型参数, 而 GetGenericArguments 则不管填不填实都会获取到, 虽然在前面限制了 !delegateType.ContainsGenericParameters 让它们在这里没有区别就是了

            // 尝试将委托类型的泛型参数按对应长度填入类型和方法的泛型参数中, 如果不能则继续判断下一个方法
            if (type.IsGenericTypeDefinition) {
                if (method.IsGenericMethodDefinition) {
                    if (realTypeGenericTypes.Length + realMethodGenericTypes.Length != delegateGenericTypeArguments.Length) {
                        continue;
                    }
                    newType = type.MakeGenericType(delegateGenericTypeArguments[..realTypeGenericTypes.Length]);
                    return GetMethod(newType, name, delegateType, bindingAttr, true);
                }
                if (realTypeGenericTypes.Length == delegateGenericTypeArguments.Length) {
                    newType = type.MakeGenericType(delegateGenericTypeArguments);
                    return GetMethod(newType, name, delegateType, bindingAttr, true);
                }
                if (realTypeGenericTypes.Length + realMethodGenericTypes.Length == delegateGenericTypeArguments.Length) {
                    newType = type.MakeGenericType(delegateGenericTypeArguments[..realTypeGenericTypes.Length]);
                    return GetMethod(newType, name, delegateType, bindingAttr, true);
                }
                continue;
            }
            if (!method.IsGenericMethodDefinition) {
                continue;
            }
            if (realMethodGenericTypes.Length == delegateGenericTypeArguments.Length) {
                result = method.MakeGenericMethod(delegateGenericTypeArguments);
            }
            else if (realTypeGenericTypes.Length + realMethodGenericTypes.Length == delegateGenericTypeArguments.Length) {
                result = method.MakeGenericMethod(delegateGenericTypeArguments[realTypeGenericTypes.Length..]);
            }
            else {
                continue;
            }

            // 检查填好类型参数的结果中的参数是否可以和委托中的参数对上, 如果完全对的上则返回结果, 否则继续判断下一个方法
            var resultParameters = result.GetParameters();
            if (resultParameters.Length != parameters.Length) {
                continue;
            }
            for (int i = 0; i < resultParameters.Length; ++i) {
                if (resultParameters[i].ParameterType != parameters[i].ParameterType) {
                    continue;
                }
            }
            return result;
        }
        return null;
    }
    #endregion
    #endregion

    #region Vector2  拓展
    public static Vector2 ClampDistance(this Vector2 self, Vector2 origin, float distance) {
        return distance <= 0 ? origin :
            Vector2.DistanceSquared(self, origin) <= distance * distance ? self :
            origin + (self - origin).SafeNormalize(Vector2.Zero) * distance;
    }
    private static Vector2 SafeNormalize(this Vector2 self, Vector2 defaultValue = default) {
        return self == Vector2.Zero || self.HasNaNs() ? defaultValue : Vector2.Normalize(self);
    }
    private static bool HasNaNs(this Vector2 vec) => float.IsNaN(vec.X) || float.IsNaN(vec.Y);
    #endregion
    #region IEnumerable拓展
    #region Foreach
    /// <summary>
    /// returns false when action or condition is null, else returns true
    /// </summary>
    public static bool ForeachDo<T>(this IEnumerable<T> enumerable, Action<T> action) {
        if (enumerable == null || action == null) {
            return false;
        }
        foreach (T t in enumerable) {
            action(t);
        }
        return true;
    }
    /// <summary>
    /// returns true when break out, else returns false.
    /// same as <see cref="Enumerable.Any{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>
    /// </summary>
    /// <param name="action">break out when get true</param>
    public static bool ForeachDoB<T>(this IEnumerable<T> enumerable, Func<T, bool> action) {
        if (enumerable == null || action == null) {
            return false;
        }
        foreach (T t in enumerable) {
            if (action(t)) {
                return true;
            }
        }
        return false;
    }
    public static TResult? ForeachGet<TSource, TResult>(IEnumerable<TSource> enumerable, Func<TSource, (bool succeeded, TResult value)> supplier, TResult? defaultValue = default) {
        foreach (TSource t in enumerable) {
            var (succeeded, value) = supplier(t);
            if (succeeded) {
                return value;
            }
        }
        return defaultValue;
    }
    public static bool ForeachGet<TSource, TResult>(IEnumerable<TSource> enumerable, Func<TSource, (bool succeeded, TResult value)> supplier, out TResult? value, TResult? defaultValue = default) {
        foreach (TSource t in enumerable) {
            var (succeeded, getValue) = supplier(t);
            if (succeeded) {
                value = getValue;
                return true;
            }
        }
        value = defaultValue;
        return false;
    }
    #endregion
    #region out Exception
    public static IEnumerable<(T, Exception?)> WithException<T>(this IEnumerable<T> enumerable) {
        foreach (T t in enumerable) {
            yield return (t, null);
        }
    }
    public delegate TResult ConverterWithException<TSource, TResult>(TSource source, out Exception exception);
    public delegate bool PredicateWithException<T>(T source, out Exception exception);
    public delegate void ActionWithException<T>(T source, out Exception exception);
    public static IEnumerable<(TResult?, Exception?)> Select<TSource, TResult>(this IEnumerable<(TSource, Exception)> source, ConverterWithException<TSource, TResult> selector) {
        foreach ((TSource element, Exception e) in source) {
            if (e != null) {
                yield return (default, e);
                yield break;
            }
            TResult result = selector(element, out Exception exception);
            if (exception != null) {
                yield return (default, exception);
                yield break;
            }
            yield return (result, null);
        }
    }
    public static bool Any<TSource>(this IEnumerable<(TSource, Exception)> source, PredicateWithException<TSource> predicate, out Exception? exception) {
        exception = null;
        foreach ((TSource element, Exception e) in source) {
            if (e != null) {
                exception = e;
                return false;
            }
            bool result = predicate(element, out exception);
            if (exception != null) {
                return false;
            }
            if (predicate(element, out exception)) {
                return true;
            }
        }

        return false;
    }
    public static List<TSource>? ToList<TSource>(this IEnumerable<(TSource, Exception)> source, out Exception? exception) {
        exception = null;
        List<TSource> list = [];
        foreach ((TSource element, Exception e) in source) {
            if (e != null) {
                return null;
            }
            list.Add(element);
        }
        return list;
    }
    public static TSource[]? ToArray<TSource>(this IEnumerable<(TSource, Exception)> source, out Exception? exception) {
        var list = source.ToList(out exception);
        return exception == null ? list?.ToArray() : null;
    }
    public static List<TResult>? ConvertAll<TSource, TResult>(this List<TSource> source, ConverterWithException<TSource, TResult> converter, out Exception? e) {
        e = null;
        List<TResult> list = new(source.Count);
        for (int i = 0; i < source.Count; i++) {
            TResult element = converter(source[i], out e);
            if (e != null)
                return default;
            list.Add(element);
        }
        return list;
    }
    public static T? Find<T>(this IEnumerable<(T, Exception)> source, PredicateWithException<T> match, out Exception? exception) {
        exception = null;
        foreach ((T element, Exception e) in source) {
            if (e != null) {
                exception = e;
                return default;
            }
            bool result = match(element, out exception);
            if (exception != null)
                return default;
            if (result)
                return element;
        }
        return default;
    }
    public static void ForEach<T>(this IEnumerable<(T, Exception)> source, ActionWithException<T> action, out Exception? exception) {
        exception = null;
        foreach ((T element, Exception e) in source) {
            if (e != null) {
                exception = e;
                return;
            }
            action(element, out exception);
            if (exception != null)
                return;
        }
    }
    #endregion
    /// <returns>(序号, 迭代值) 其中序号从0开始</returns>
    public static IEnumerable<(int index, T value)> WithIndex<T>(this IEnumerable<T> enumerable) {
        int index = 0;
        foreach (T t in enumerable) {
            yield return (index++, t);
        }
    }
    public static IEnumerable<TResult> Filter<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, Existable<TResult>> filter) {
        foreach (var t in enumerable) {
            var existable = filter(t);
            if (existable.HasValue) {
                yield return existable.Value;
            }
        }
    }
    public static IEnumerable<TResult> SelectWhere<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, ValueHolder<TResult>?> selector) {
        foreach (TSource t in enumerable) {
            var value = selector(t);
            if (value != null) {
                yield return value;
            }
        }
    }
    /// <summary>
    /// <br/> 相比于<see cref="SelectWhere"/>,
    /// <br/> 它会剔除空值
    /// </summary>
    public static IEnumerable<TResult> SelectWhereN<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, ValueHolder<TResult?>?> selector) {
        foreach (TSource t in enumerable) {
            var value = selector(t);
            if (value != null && value.Value is not null) {
                yield return value.Value;
            }
        }
    }
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) {
        foreach (T? t in enumerable) {
            if (t is not null) {
                yield return t;
            }
        }
    }
    public static IEnumerable<T> WithAction<T>(this IEnumerable<T> enumerable, Action<T> action) {
        foreach (T t in enumerable) {
            action(t);
            yield return t;
        }
    }
    public static IEnumerator<int> GetEnumerator(this int i)
        => Range(i).GetEnumerator();
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TResult? SelectFirstOrDefault<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector, TResult? defaultValue = default) {
        foreach (var t in enumerable) {
            return selector(t);
        }
        return defaultValue;
    }
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TResult? SelectFirstOrDefault<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, ValueHolder<TResult>?> selector, TResult? defaultValue = default) {
        foreach (var t in enumerable) {
            var value = selector(t);
            if (value != null) {
                return value;
            }
        }
        return defaultValue;
    }
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TResult? SelectFirstNotNullOrDefault<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult?> selector, TResult? defaultValue = default) {
        foreach (var t in enumerable) {
            var value = selector(t);
            if (value != null) {
                return value;
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// 找到了则返回索引, 否则返回 -1
    /// </summary>
    public static int FindIndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) {
        int i = 0;
        foreach (var item in enumerable) {
            if (predicate(item)) {
                return i;
            }
            i += 1;
        }
        return -1;
    }
    public static int FindIndexOf<T>(this IEnumerable<T> enumerable, T item) {
        int i = 0;
        foreach (T c in enumerable) {
            if (object.Equals(c, item)) {
                return i;
            }
            i += 1;
        }
        return -1;
    }

    public static IEnumerable SelectMany<TSource>(this IEnumerable<TSource> enumerable, Func<TSource, IEnumerable> selector) {
        foreach (var t in enumerable) {
            foreach (var o in selector(t)) {
                yield return o;
            }
        }
    }
    #endregion
    #region 数组和列表相关
    #region 打乱数组/列表
    /// <summary>
    /// 直接在此列表上打乱整个列表
    /// </summary>
    public static List<T> Shuffle<T>(this List<T> list, Random? rd = null) {
        T tmp;
        if (list.Count == 0) {
            return list;
        }
        rd ??= new();
        foreach (int i in Range(list.Count - 1, 0, RangeType.Negative)) {
            int randint = rd.Next(0, i + 1);
            tmp = list[randint];
            list[randint] = list[i];
            list[i] = tmp;
        }
        return list;
    }
    /// <summary>
    /// 直接在此数组上打乱整个数组
    /// </summary>
    public static T[] Shuffle<T>(this T[] array, Random? rd = null) {
        T tmp;
        rd ??= new();
        foreach (int i in Range(array.Length - 1, 0, RangeType.Negative)) {
            int randint = rd.Next(0, i + 1);
            tmp = array[randint];
            array[randint] = array[i];
            array[i] = tmp;
        }
        return array;
    }
    /// <summary>
    /// 返回一个打乱了的列表, 原列表不变
    /// </summary>
    public static List<T> Shuffled<T>(this List<T> list) where T : ICloneable {
        List<T> ret = [];
        foreach (T t in list) {
            ret.Add((T)t.Clone());
        }
        return ret.Shuffle();
    }
    /// <summary>
    /// 返回一个打乱了的数组, 原数组不变
    /// </summary>
    public static T[] Shuffled<T>(this T[] array) where T : ICloneable {
        T[] ret = new T[array.Length];
        foreach (int i in Range(array.Length)) {
            ret[i] = (T)array.Clone();
        }
        return ret.Shuffle();
    }
    #endregion
    #region IList的Index和Range拓展
    private static int GetIndex<T>(IList<T> list, Index index) {
        return index.IsFromEnd ? list.Count - index.Value : index.Value;
    }
    private static void GetRange<T>(IList<T> list, Range range, out int start, out int end) {
        start = GetIndex(list, range.Start);
        end = GetIndex(list, range.End);
        if (start > end) {
            start ^= end;
            end ^= start;
            start ^= end;
        }
    }
    #endregion
    #region 删除
    /// <summary>
    /// 删除第一个符合条件的元素
    /// </summary>
    /// <param name="list">列表</param>
    /// <param name="predicate">条件, 符合此条件的第一个元素将被删除</param>
    /// <returns>是否删除了元素</returns>
    public static bool Remove<T>(this IList<T> list, Func<T, bool> predicate) {
        for (int i = list.Count - 1; i >= 0; --i) {
            if (predicate(list[i])) {
                list.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 删除所有符合条件的元素
    /// </summary>
    /// <param name="list">列表</param>
    /// <param name="predicate">条件, 符合此条件的所有元素将被删除</param>
    /// <returns>删除的元素个数</returns>
    public static int RemoveAll<T>(this IList<T> list, Func<T, bool> predicate) {
        int result = 0;
        for (int i = 0; i < list.Count; ++i) {
            if (predicate(list[i])) {
                result += 1;
                continue;
            }
            if (result != 0) {
                list[i - result] = list[i];
            }
        }
        if (result > 0) {
            list.RemoveRange(list.Count - result, result);
        }
        return result;
    }
    public static void RemoveAt<T>(this IList<T> list, Index index) {
        list.RemoveAt(GetIndex(list, index));
    }
    #region RemoveRange
    public static void RemoveRange<T>(this IList<T> list, Range range) {
        GetRange(list, range, out int start, out int end);
        list.RemoveRange(start, end - start);
    }
    public static void RemoveRangeS<T>(this IList<T> list, Range range) {
        GetRange(list, range, out int start, out int end);
        list.RemoveRangeS(start, end - start);
    }
    public static void RemoveRange<T>(this IList<T> list, int index, int count) {
        if (list is List<T> realList) {
            realList.RemoveRange(index, count);
            return;
        }
        for (int i = index + count; i < list.Count; ++i) {
            list[i - count] = list[i];
        }
        var resultCount = list.Count - count;
        for (int i = list.Count - 1; i >= resultCount; --i) {
            list.RemoveAt(i);
        }
    }
    public static void RemoveRangeS<T>(this IList<T> list, int index, int count) {
        if (index < 0) {
            count += index;
            index = 0;
        }
        if (index >= list.Count) {
            return;
        }
        count.ClampMaxTo(list.Count - index);
        list.RemoveRange(index, count);
    }
    #endregion
    #endregion
    #region 快排 (快速排序)
    public static void QuickSort<T>(this IList<T> list) where T : IComparable<T> => QuickSortInner(list, 0, list.Count);
    /// <summary>
    /// 包含左边界但不包含右边界
    /// </summary>
    private static void QuickSortInner<T>(IList<T> list, int left, int right) where T : IComparable<T> {
        if (left >= right - 1)
            return;
        int p = QuickSort_Partition(list, left, right);
        QuickSortInner(list, left, p);
        QuickSortInner(list, p + 1, right);
    }
    private static int QuickSort_Partition<T>(IList<T> list, int left, int right) where T : IComparable<T> {
        int middle = (left + right) / 2;
        T pivotValue = list[middle];
        int newRight = right - 1;
        for (; newRight > middle; newRight--) {
            if (list[newRight].CompareTo(pivotValue) < 0) {
                (list[middle], list[newRight]) = (list[newRight], list[middle]);
                break;
            }
        }
        int p = left;
        for (; p < newRight; p++) {
            if (list[p].CompareTo(pivotValue) > 0) {
                break;
            }
        }
        for (int i = p + 1; i < newRight; i++) {
            if (list[i].CompareTo(pivotValue) < 0) {
                (list[i], list[p]) = (list[p], list[i]);
                p += 1;
            }
        }
        if (p < newRight) {
            (list[p], list[newRight]) = (list[newRight], list[p]);
        }
        return p;
    }
    #endregion
    #region 堆排序
    // 分 List, Array, IList 三个类型, 以及使用 IComparer / 自定义 comparer 方法 / IComparable, 总共 3x3 共 9 个
    public static List<T> HeapSort<T>(this List<T> list, IComparer<T> comparer) => HeapSort(list, comparer.Compare);
    public static T[] HeapSort<T>(T[] list, IComparer<T> comparer) => HeapSort(list, comparer.Compare);
    public static IList<T> HeapSort<T>(this IList<T> list, IComparer<T> comparer) => HeapSort(list, comparer.Compare);

    public static List<T> HeapSort<T>(this List<T> list, Func<T, T, int> comparer) {
        HeapSortInner(list, comparer, 0, list.Count, null);
        return list;
    }
    private static void HeapSortInner<T>(this List<T> list, Func<T, T, int> comparer, int left, int right, T[]? lefts) {
        int length = right - left;
        if (length <= 1) {
            return;
        }
        if (length == 2) {
            if (comparer(list[left], list[left + 1]) > 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
            }
            return;
        }
        if (length == 3) {
            if (comparer(list[left], list[left + 1]) <= 0) {
                if (comparer(list[left + 1], list[left + 2]) <= 0) {
                    return;
                }
                if (comparer(list[left], list[left + 2]) <= 0) {
                    (list[left + 2], list[left + 1]) = (list[left + 1], list[left + 2]);
                    return;
                }
                (list[left], list[left + 1], list[left + 2]) = (list[left + 2], list[left], list[left + 1]);
                return;
            }
            if (comparer(list[left], list[left + 2]) <= 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
                return;
            }
            if (comparer(list[left + 1], list[left + 2]) > 0) {
                (list[left], list[left + 2]) = (list[left + 2], list[left]);
                return;
            }
            (list[left], list[left + 1], list[left + 2]) = (list[left + 1], list[left + 2], list[left]);
            return;
        }
        int middle = (left + right) / 2;
        lefts ??= new T[middle - left];
        HeapSortInner(list, comparer, left, middle, lefts);
        HeapSortInner(list, comparer, middle, right, lefts);
        if (comparer(list[middle - 1], list[middle]) <= 0) {
            return;
        }
        list.CopyTo(left, lefts, 0, middle - left);
        int indexLeft = 0, indexRight = middle;
        bool rightMoved = false;
        for (int index = left; index < right; ++index) {
            if (comparer(lefts[indexLeft], list[indexRight]) <= 0) {
                if (rightMoved) {
                    list[index] = lefts[indexLeft];
                }
                indexLeft += 1;
                if (indexLeft == middle - left) {
                    return;
                }
                continue;
            }
            rightMoved = true;
            list[index] = list[indexRight];
            indexRight += 1;
            if (indexRight == right) {
                for (; indexLeft < middle - left; ++indexLeft) {
                    list[++index] = lefts[indexLeft];
                }
                return;
            }
        }
    }
    public static T[] HeapSort<T>(this T[] list, Func<T, T, int> comparer) {
        HeapSortInner(list, comparer, 0, list.Length, null);
        return list;
    }
    private static void HeapSortInner<T>(this T[] list, Func<T, T, int> comparer, int left, int right, T[]? lefts) {
        int length = right - left;
        if (length <= 1) {
            return;
        }
        if (length == 2) {
            if (comparer(list[left], list[left + 1]) > 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
            }
            return;
        }
        if (length == 3) {
            if (comparer(list[left], list[left + 1]) <= 0) {
                if (comparer(list[left + 1], list[left + 2]) <= 0) {
                    return;
                }
                if (comparer(list[left], list[left + 2]) <= 0) {
                    (list[left + 2], list[left + 1]) = (list[left + 1], list[left + 2]);
                    return;
                }
                (list[left], list[left + 1], list[left + 2]) = (list[left + 2], list[left], list[left + 1]);
                return;
            }
            if (comparer(list[left], list[left + 2]) <= 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
                return;
            }
            if (comparer(list[left + 1], list[left + 2]) > 0) {
                (list[left], list[left + 2]) = (list[left + 2], list[left]);
                return;
            }
            (list[left], list[left + 1], list[left + 2]) = (list[left + 1], list[left + 2], list[left]);
            return;
        }
        int middle = (left + right) / 2;
        lefts ??= new T[middle - left];
        HeapSortInner(list, comparer, left, middle, lefts);
        HeapSortInner(list, comparer, middle, right, lefts);
        if (comparer(list[middle - 1], list[middle]) <= 0) {
            return;
        }
        Array.Copy(list, left, lefts, 0, middle - left);
        int indexLeft = 0, indexRight = middle;
        bool rightMoved = false;
        for (int index = left; index < right; ++index) {
            if (comparer(lefts[indexLeft], list[indexRight]) <= 0) {
                if (rightMoved) {
                    list[index] = lefts[indexLeft];
                }
                indexLeft += 1;
                if (indexLeft == middle - left) {
                    return;
                }
                continue;
            }
            rightMoved = true;
            list[index] = list[indexRight];
            indexRight += 1;
            if (indexRight == right) {
                for (; indexLeft < middle - left; ++indexLeft) {
                    list[++index] = lefts[indexLeft];
                }
                return;
            }
        }
    }
    public static IList<T> HeapSort<T>(this IList<T> list, Func<T, T, int> comparer) {
        HeapSortInner(list, comparer, 0, list.Count, null);
        return list;
    }
    private static void HeapSortInner<T>(this IList<T> list, Func<T, T, int> comparer, int left, int right, T[]? lefts) {
        int length = right - left;
        if (length <= 1) {
            return;
        }
        if (length == 2) {
            if (comparer(list[left], list[left + 1]) > 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
            }
            return;
        }
        if (length == 3) {
            if (comparer(list[left], list[left + 1]) <= 0) {
                if (comparer(list[left + 1], list[left + 2]) <= 0) {
                    return;
                }
                if (comparer(list[left], list[left + 2]) <= 0) {
                    (list[left + 2], list[left + 1]) = (list[left + 1], list[left + 2]);
                    return;
                }
                (list[left], list[left + 1], list[left + 2]) = (list[left + 2], list[left], list[left + 1]);
                return;
            }
            if (comparer(list[left], list[left + 2]) <= 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
                return;
            }
            if (comparer(list[left + 1], list[left + 2]) > 0) {
                (list[left], list[left + 2]) = (list[left + 2], list[left]);
                return;
            }
            (list[left], list[left + 1], list[left + 2]) = (list[left + 1], list[left + 2], list[left]);
            return;
        }
        int middle = (left + right) / 2;
        lefts ??= new T[middle - left];
        HeapSortInner(list, comparer, left, middle, lefts);
        HeapSortInner(list, comparer, middle, right, lefts);
        if (comparer(list[middle - 1], list[middle]) <= 0) {
            return;
        }
        #region Copy
        for (int i = 0; i < middle - left; ++i) {
            lefts[i] = list[i + left];
        }
        #endregion
        int indexLeft = 0, indexRight = middle;
        bool rightMoved = false;
        for (int index = left; index < right; ++index) {
            if (comparer(lefts[indexLeft], list[indexRight]) <= 0) {
                if (rightMoved) {
                    list[index] = lefts[indexLeft];
                }
                indexLeft += 1;
                if (indexLeft == middle - left) {
                    return;
                }
                continue;
            }
            rightMoved = true;
            list[index] = list[indexRight];
            indexRight += 1;
            if (indexRight == right) {
                for (; indexLeft < middle - left; ++indexLeft) {
                    list[++index] = lefts[indexLeft];
                }
                return;
            }
        }
    }

    public static List<T> HeapSort<T>(this List<T> list) where T : IComparable<T> {
        HeapSortInner(list, 0, list.Count, null);
        return list;
    }
    private static void HeapSortInner<T>(this List<T> list, int left, int right, T[]? lefts) where T : IComparable<T> {
        int length = right - left;
        if (length <= 1) {
            return;
        }
        if (length == 2) {
            if (list[left].CompareTo(list[left + 1]) > 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
            }
            return;
        }
        if (length == 3) {
            if (list[left].CompareTo(list[left + 1]) <= 0) {
                if (list[left + 1].CompareTo(list[left + 2]) <= 0) {
                    return;
                }
                if (list[left].CompareTo(list[left + 2]) <= 0) {
                    (list[left + 2], list[left + 1]) = (list[left + 1], list[left + 2]);
                    return;
                }
                (list[left], list[left + 1], list[left + 2]) = (list[left + 2], list[left], list[left + 1]);
                return;
            }
            if (list[left].CompareTo(list[left + 2]) <= 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
                return;
            }
            if (list[left + 1].CompareTo(list[left + 2]) > 0) {
                (list[left], list[left + 2]) = (list[left + 2], list[left]);
                return;
            }
            (list[left], list[left + 1], list[left + 2]) = (list[left + 1], list[left + 2], list[left]);
            return;
        }
        int middle = (left + right) / 2;
        lefts ??= new T[middle - left];
        HeapSortInner(list, left, middle, null);
        HeapSortInner(list, middle, right, null);
        if (list[middle - 1].CompareTo(list[middle]) <= 0) {
            return;
        }
        list.CopyTo(left, lefts, 0, middle - left);
        int indexLeft = 0, indexRight = middle;
        bool rightMoved = false;
        for (int index = left; index < right; ++index) {
            if (lefts[indexLeft].CompareTo(list[indexRight]) <= 0) {
                if (rightMoved) {
                    list[index] = lefts[indexLeft];
                }
                indexLeft += 1;
                if (indexLeft == middle - left) {
                    return;
                }
                continue;
            }
            rightMoved = true;
            list[index] = list[indexRight];
            indexRight += 1;
            if (indexRight == right) {
                for (; indexLeft < middle - left; ++indexLeft) {
                    list[++index] = lefts[indexLeft];
                }
                return;
            }
        }
    }
    public static T[] HeapSort<T>(this T[] list) where T : IComparable<T> {
        HeapSortInner(list, 0, list.Length, null);
        return list;
    }
    private static void HeapSortInner<T>(this T[] list, int left, int right, T[]? lefts) where T : IComparable<T> {
        int length = right - left;
        if (length <= 1) {
            return;
        }
        if (length == 2) {
            if (list[left].CompareTo(list[left + 1]) > 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
            }
            return;
        }
        if (length == 3) {
            if (list[left].CompareTo(list[left + 1]) <= 0) {
                if (list[left + 1].CompareTo(list[left + 2]) <= 0) {
                    return;
                }
                if (list[left].CompareTo(list[left + 2]) <= 0) {
                    (list[left + 2], list[left + 1]) = (list[left + 1], list[left + 2]);
                    return;
                }
                (list[left], list[left + 1], list[left + 2]) = (list[left + 2], list[left], list[left + 1]);
                return;
            }
            if (list[left].CompareTo(list[left + 2]) <= 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
                return;
            }
            if (list[left + 1].CompareTo(list[left + 2]) > 0) {
                (list[left], list[left + 2]) = (list[left + 2], list[left]);
                return;
            }
            (list[left], list[left + 1], list[left + 2]) = (list[left + 1], list[left + 2], list[left]);
            return;
        }
        int middle = (left + right) / 2;
        lefts ??= new T[middle - left];
        HeapSortInner(list, left, middle, null);
        HeapSortInner(list, middle, right, null);
        if (list[middle - 1].CompareTo(list[middle]) <= 0) {
            return;
        }
        Array.Copy(list, left, lefts, 0, middle - left);
        int indexLeft = 0, indexRight = middle;
        bool rightMoved = false;
        for (int index = left; index < right; ++index) {
            if (lefts[indexLeft].CompareTo(list[indexRight]) <= 0) {
                if (rightMoved) {
                    list[index] = lefts[indexLeft];
                }
                indexLeft += 1;
                if (indexLeft == middle - left) {
                    return;
                }
                continue;
            }
            rightMoved = true;
            list[index] = list[indexRight];
            indexRight += 1;
            if (indexRight == right) {
                for (; indexLeft < middle - left; ++indexLeft) {
                    list[++index] = lefts[indexLeft];
                }
                return;
            }
        }
    }
    public static IList<T> HeapSort<T>(this IList<T> list) where T : IComparable<T> {
        HeapSortInner(list, 0, list.Count, null);
        return list;
    }
    private static void HeapSortInner<T>(this IList<T> list, int left, int right, T[]? lefts) where T : IComparable<T> {
        int length = right - left;
        if (length <= 1) {
            return;
        }
        if (length == 2) {
            if (list[left].CompareTo(list[left + 1]) > 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
            }
            return;
        }
        if (length == 3) {
            if (list[left].CompareTo(list[left + 1]) <= 0) {
                if (list[left + 1].CompareTo(list[left + 2]) <= 0) {
                    return;
                }
                if (list[left].CompareTo(list[left + 2]) <= 0) {
                    (list[left + 2], list[left + 1]) = (list[left + 1], list[left + 2]);
                    return;
                }
                (list[left], list[left + 1], list[left + 2]) = (list[left + 2], list[left], list[left + 1]);
                return;
            }
            if (list[left].CompareTo(list[left + 2]) <= 0) {
                (list[left], list[left + 1]) = (list[left + 1], list[left]);
                return;
            }
            if (list[left + 1].CompareTo(list[left + 2]) > 0) {
                (list[left], list[left + 2]) = (list[left + 2], list[left]);
                return;
            }
            (list[left], list[left + 1], list[left + 2]) = (list[left + 1], list[left + 2], list[left]);
            return;
        }
        int middle = (left + right) / 2;
        lefts ??= new T[middle - left];
        HeapSortInner(list, left, middle, null);
        HeapSortInner(list, middle, right, null);
        if (list[middle - 1].CompareTo(list[middle]) <= 0) {
            return;
        }
        #region Copy
        for (int i = 0; i < middle - left; ++i) {
            lefts[i] = list[i + left];
        }
        #endregion
        int indexLeft = 0, indexRight = middle;
        bool rightMoved = false;
        for (int index = left; index < right; ++index) {
            if (lefts[indexLeft].CompareTo(list[indexRight]) <= 0) {
                if (rightMoved) {
                    list[index] = lefts[indexLeft];
                }
                indexLeft += 1;
                if (indexLeft == middle - left) {
                    return;
                }
                continue;
            }
            rightMoved = true;
            list[index] = list[indexRight];
            indexRight += 1;
            if (indexRight == right) {
                for (; indexLeft < middle - left; ++indexLeft) {
                    list[++index] = lefts[indexLeft];
                }
                return;
            }
        }
    }
    #endregion
    #region BitArray拓展
    [Obsolete("使用 HasAllSet 和 HasAnySet")]
    public static bool CheckAll(this BitArray bitArray, bool value = true) {
        for (int i = 0; i < bitArray.Length; i++) {
            if (bitArray[i] != value) {
                return false;
            }
        }
        return true;
    }
    public static int[] ToIntArray(this BitArray bitArray, int offset = 0, params bool[] prefix) {
        if (bitArray.Count - prefix.Length <= 0 && prefix.Length == 0) {
            return [];
        }
        int[] result = new int[((bitArray.Count - offset).WithMin(0) + prefix.Length - 1) / 32 + 1];
        int i = 0;
        for (; i < prefix.Length; ++i) {
            if (prefix[i]) {
                result[i / 32] |= 1 << i % 32;
            }
        }
        int addonLength = (bitArray.Count - offset).WithMin(0) + prefix.Length;
        for (; i < addonLength; ++i) {
            if (bitArray[i + offset - prefix.Length]) {
                result[i / 32] |= 1 << i % 32;
            }
        }
        return result;
    }
    public static byte[] ToByteArray(this BitArray bitArray, int offset = 0, params bool[] prefix) {
        if (bitArray.Count - prefix.Length <= 0 && prefix.Length == 0) {
            return [];
        }
        byte[] result = new byte[((bitArray.Count - offset).WithMin(0) + prefix.Length - 1) / 8 + 1];
        int i = 0;
        for (; i < prefix.Length; ++i) {
            if (prefix[i]) {
                result[i / 8] |= (byte)(1 << i % 8);
            }
        }
        int addonLength = (bitArray.Count - offset).WithMin(0) + prefix.Length;
        for (; i < addonLength; ++i) {
            if (bitArray[i + offset - prefix.Length]) {
                result[i / 8] |= (byte)(1 << i % 8);
            }
        }
        return result;
    }
    public static int[] ToIntArray(this bool[] bitArray, int offset = 0, params bool[] prefix) {
        if (bitArray.Length - prefix.Length <= 0 && prefix.Length == 0) {
            return [];
        }
        int[] result = new int[((bitArray.Length - offset).WithMin(0) + prefix.Length - 1) / 32 + 1];
        int i = 0;
        for (; i < prefix.Length; ++i) {
            if (prefix[i]) {
                result[i / 32] |= 1 << i % 32;
            }
        }
        int addonLength = (bitArray.Length - offset).WithMin(0) + prefix.Length;
        for (; i < addonLength; ++i) {
            if (bitArray[i + offset - prefix.Length]) {
                result[i / 32] |= 1 << i % 32;
            }
        }
        return result;
    }
    public static byte[] ToByteArray(this bool[] bitArray, int offset = 0, params bool[] prefix) {
        if (bitArray.Length - prefix.Length <= 0 && prefix.Length == 0) {
            return [];
        }
        byte[] result = new byte[((bitArray.Length - offset).WithMin(0) + prefix.Length - 1) / 8 + 1];
        int i = 0;
        for (; i < prefix.Length; ++i) {
            if (prefix[i]) {
                result[i / 8] |= (byte)(1 << i % 8);
            }
        }
        int addonLength = (bitArray.Length - offset).WithMin(0) + prefix.Length;
        for (; i < addonLength; ++i) {
            if (bitArray[i + offset - prefix.Length]) {
                result[i / 8] |= (byte)(1 << i % 8);
            }
        }
        return result;
    }

    public static void ToBoolArray(this int[] intArray, bool[] boolArray, int offset = 0, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolArray.Length, prefix.Length))) {
            boolArray[i] = prefix[i];
        }
        foreach (int i in Range(offset, Math.Min(boolArray.Length + offset - prefix.Length, intArray.Length * 32))) {
            boolArray[i - offset + prefix.Length] = (intArray[i / 32] & 1 << i % 32) != 0;
        }
    }
    public static void ToBoolArray(this byte[] byteArray, bool[] boolArray, int offset = 0, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolArray.Length, prefix.Length))) {
            boolArray[i] = prefix[i];
        }
        foreach (int i in Range(offset, Math.Min(boolArray.Length + offset - prefix.Length, byteArray.Length * 8))) {
            boolArray[i - offset + prefix.Length] = (byteArray[i / 8] & 1 << i % 8) != 0;
        }
    }
    public static void ToBoolArray(this IList<int> intList, bool[] boolArray, int offset = 0, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolArray.Length, prefix.Length))) {
            boolArray[i] = prefix[i];
        }
        foreach (int i in Range(offset, Math.Min(boolArray.Length + offset - prefix.Length, intList.Count * 32))) {
            boolArray[i - offset + prefix.Length] = (intList[i / 32] & 1 << i % 32) != 0;
        }
    }
    public static void ToBoolArray(this IList<byte> byteList, bool[] boolArray, int offset = 0, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolArray.Length, prefix.Length))) {
            boolArray[i] = prefix[i];
        }
        foreach (int i in Range(offset, Math.Min(boolArray.Length + offset - prefix.Length, byteList.Count * 8))) {
            boolArray[i - offset + prefix.Length] = (byteList[i / 8] & 1 << i % 8) != 0;
        }
    }
    public static void ToBoolArray(this IEnumerable<int> ints, bool[] boolArray, int offset = 0, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolArray.Length, prefix.Length))) {
            boolArray[i] = prefix[i];
        }
        int boolArrayLength = boolArray.Length;
        if (boolArrayLength <= 0) {
            return;
        }
        var enumerator = ints.GetEnumerator();
        while (offset >= 32) {
            if (!enumerator.MoveNext()) {
                return;
            }
            offset -= 32;
        }
        int index = prefix.Length - offset;
        while (enumerator.MoveNext()) {
            do {
                boolArray[index + offset] = (enumerator.Current & 1 << offset++ % 32) != 0;
            } while (index < boolArrayLength && offset % 32 != 0);
        }
    }
    public static void ToBoolArray(this IEnumerable<byte> bytes, bool[] boolArray, int offset = 0, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolArray.Length, prefix.Length))) {
            boolArray[i] = prefix[i];
        }
        int boolArrayLength = boolArray.Length;
        if (boolArrayLength <= 0) {
            return;
        }
        var enumerator = bytes.GetEnumerator();
        while (offset >= 8) {
            if (!enumerator.MoveNext()) {
                return;
            }
            offset -= 8;
        }
        int index = prefix.Length - offset;
        while (enumerator.MoveNext()) {
            do {
                boolArray[index + offset] = (enumerator.Current & 1 << offset++ % 8) != 0;
            } while (index < boolArrayLength && offset % 8 != 0);
        }
    }
    public static bool[] ToBoolArray(this int[] intArray, int offset = 0, params bool[] prefix)
        => new bool[(intArray.Length * 32 - offset).WithMin(0) + prefix.Length].WithAction(ba => intArray.ToBoolArray(ba, offset, prefix));
    public static bool[] ToBoolArray(this byte[] byteArray, int offset = 0, params bool[] prefix)
        => new bool[(byteArray.Length * 8 - offset).WithMin(0) + prefix.Length].WithAction(ba => byteArray.ToBoolArray(ba, offset, prefix));
    public static bool[] ToBoolArray(this IList<int> intList, int offset = 0, params bool[] prefix)
        => new bool[(intList.Count * 8 - offset.WithMin(0) + prefix.Length)].WithAction(ba => intList.ToBoolArray(ba, offset, prefix));
    public static bool[] ToBoolArray(this IList<byte> byteList, int offset = 0, params bool[] prefix)
        => new bool[(byteList.Count * 8 - offset).WithMin(0) + prefix.Length].WithAction(ba => byteList.ToBoolArray(ba, offset, prefix));
    public static void ToBoolList(this int[] intArray, IList<bool> boolList, int offset, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolList.Count, prefix.Length))) {
            boolList[i] = prefix[i];
        }
        foreach (int i in Range(offset, Math.Min(boolList.Count + offset - prefix.Length, intArray.Length * 32))) {
            boolList[i - offset + prefix.Length] = (intArray[i / 32] & 1 << i % 32) != 0;
        }
    }
    public static void ToBoolList(this byte[] byteArray, IList<bool> boolList, int offset, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolList.Count, prefix.Length))) {
            boolList[i] = prefix[i];
        }
        foreach (int i in Range(offset, Math.Min(boolList.Count + offset - prefix.Length, byteArray.Length * 8))) {
            boolList[i - offset + prefix.Length] = (byteArray[i / 8] & 1 << i % 8) != 0;
        }
    }
    public static void ToBoolList(this IList<int> intList, IList<bool> boolList, int offset = 0, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolList.Count, prefix.Length))) {
            boolList[i] = prefix[i];
        }
        foreach (int i in Range(offset, Math.Min(boolList.Count + offset - prefix.Length, intList.Count * 32))) {
            boolList[i - offset + prefix.Length] = (intList[i / 32] & 1 << i % 32) != 0;
        }
    }
    public static void ToBoolList(this IList<byte> byteList, IList<bool> boolList, int offset = 0, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolList.Count, prefix.Length))) {
            boolList[i] = prefix[i];
        }
        foreach (int i in Range(offset, Math.Min(boolList.Count + offset - prefix.Length, byteList.Count * 8))) {
            boolList[i - offset + prefix.Length] = (byteList[i / 8] & 1 << i % 8) != 0;
        }
    }
    public static void ToBoolList(this IEnumerable<int> ints, IList<bool> boolList, int offset = 0, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolList.Count, prefix.Length))) {
            boolList[i] = prefix[i];
        }
        int boolListCount = boolList.Count;
        if (boolListCount <= 0) {
            return;
        }
        var enumerator = ints.GetEnumerator();
        while (offset >= 32) {
            if (!enumerator.MoveNext()) {
                return;
            }
            offset -= 32;
        }
        int index = prefix.Length - offset;
        while (enumerator.MoveNext()) {
            do {
                boolList[index + offset] = (enumerator.Current & 1 << offset++ % 32) != 0;
            } while (index < boolListCount && offset % 32 != 0);
        }
    }
    public static void ToBoolList(this IEnumerable<byte> bytes, IList<bool> boolList, int offset = 0, params bool[] prefix) {
        foreach (int i in Range(Math.Min(boolList.Count, prefix.Length))) {
            boolList[i] = prefix[i];
        }
        int boolListCount = boolList.Count;
        if (boolListCount <= 0) {
            return;
        }
        var enumerator = bytes.GetEnumerator();
        while (offset >= 8) {
            if (!enumerator.MoveNext()) {
                return;
            }
            offset -= 8;
        }
        int index = prefix.Length - offset;
        while (enumerator.MoveNext()) {
            do {
                boolList[index + offset] = (enumerator.Current & 1 << offset++ % 8) != 0;
            } while (index < boolListCount && offset % 8 != 0);
        }
    }
    public static List<bool> ToBoolList(this int[] intArray, int offset = 0, params bool[] prefix) {
        List<bool> boolList = new((intArray.Length * 32 - offset).WithMin(0) + prefix.Length);
        foreach (bool b in prefix) {
            boolList.Add(b);
        }
        foreach (int i in Range(offset, intArray.Length * 32)) {
            boolList.Add((intArray[i / 32] & 1 << i % 32) != 0);
        };
        return boolList;
    }
    public static List<bool> ToBoolList(this byte[] byteArray, int offset = 0, params bool[] prefix) {
        List<bool> boolList = new((byteArray.Length * 8 - offset).WithMin(0) + prefix.Length);
        foreach (bool b in prefix) {
            boolList.Add(b);
        }
        foreach (int i in Range(offset, byteArray.Length * 8)) {
            boolList.Add((byteArray[i / 8] & 1 << i % 8) != 0);
        };
        return boolList;
    }
    public static List<bool> ToBoolList(this IList<int> intList, int offset = 0, params bool[] prefix) {
        List<bool> boolList = new((intList.Count * 32 - offset).WithMin(0) + prefix.Length);
        foreach (bool b in prefix) {
            boolList.Add(b);
        }
        foreach (int i in Range(offset, intList.Count * 32)) {
            boolList.Add((intList[i / 32] & 1 << i % 32) != 0);
        };
        return boolList;
    }
    public static List<bool> ToBoolList(this IList<byte> byteList, int offset = 0, params bool[] prefix) {
        List<bool> boolList = new((byteList.Count * 8 - offset).WithMin(0) + prefix.Length);
        foreach (bool b in prefix) {
            boolList.Add(b);
        }
        foreach (int i in Range(offset, byteList.Count * 8)) {
            boolList.Add((byteList[i / 8] & 1 << i % 8) != 0);
        };
        return boolList;
    }
    public static List<bool> ToBoolList(this IEnumerable<int> ints, int offset = 0, params bool[] prefix) {
        List<bool> boolList = [.. prefix];
        foreach (int i in ints) {
            if (offset >= 32) {
                offset -= 32;
                continue;
            }
            do {
                boolList.Add((i & 1 << offset++) != 0);
            } while (offset < 32);
            offset = 0;
        }
        return boolList;
    }
    public static List<bool> ToBoolList(this IEnumerable<byte> bytes, int offset = 0, params bool[] prefix) {
        List<bool> boolList = [.. prefix];
        foreach (int i in bytes) {
            if (offset >= 8) {
                offset -= 8;
                continue;
            }
            do {
                boolList.Add((i & 1 << offset++) != 0);
            } while (offset < 8);
            offset = 0;
        }
        return boolList;
    }
    #endregion
    #region Fill
    /// <summary>
    /// 用<paramref name="value"/>填充<paramref name="list"/>
    /// </summary>
    public static void Fill<T>(this IList<T> list, T value) {
        foreach (int i in list.Count) {
            list[i] = value;
        }
    }
    /// <summary>
    /// <br/>用<paramref name="value"/>填充<paramref name="list"/>
    /// <br/>从<paramref name="startIndex"/>开始填充, 共填充<paramref name="count"/>个
    /// <br/>不检查<paramref name="startIndex"/>和<paramref name="count"/>的安全性
    /// </summary>
    public static void Fill<T>(this IList<T> list, T value, int startIndex, int count) {
        foreach (int i in Range(startIndex, startIndex + count)) {
            list[i] = value;
        }
    }
    public static void Fill<T>(this T[] array, T value) => Array.Fill(array, value);
    public static void Fill<T>(this T[] array, T value, int startIndex, int count) => Array.Fill(array, value, startIndex, count);
    #endregion
    #region 添加元素( Add... )和删除元素
    /// <summary>
    /// 返回是否成功添加
    /// </summary>
    public static bool AddIf<T>(this ICollection<T> list, bool condition, T element) {
        if (condition) {
            list.Add(element);
        }
        return condition;
    }
    /// <summary>
    /// 返回是否成功添加
    /// </summary>
    public static bool AddIf<T>(this ICollection<T> list, bool condition, Func<T> getElement) {
        if (condition) {
            list.Add(getElement());
        }
        return condition;
    }
    /// <summary>
    /// 返回是否成功添加
    /// </summary>
    public static bool AddIfNotNull<T>(this ICollection<T> list, T? element) {
        if (element is not null) {
            list.Add(element);
            return true;
        }
        return false;
    }
    public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> elements) {
        foreach (var e in elements) {
            list.Add(e);
        }
    }

    /// <summary>
    /// 如果 <paramref name="element"/> 不在 <paramref name="list"/> 中则添加它, 否则移除它
    /// </summary>
    public static void AddOrRemove<T>(this ICollection<T> list, T element) {
        if (!list.Remove(element)) {
            list.Add(element);
        }
    }

    public static void RemoveAll<T>(this ICollection<T> list, Func<T, bool> predicate) {
        List<T> toRemove = [];
        foreach (var item in list) {
            if (predicate(item)) {
                toRemove.Add(item);
            }
        }
        foreach (var r in toRemove) {
            list.Remove(r);
        }
    }
    #endregion
    #region 特殊列表操作
    #region 元组列表
    public static void Add<T1, T2>(this List<(T1, T2)> list, T1 item1, T2 item2) {
        list.Add((item1, item2));
    }
    public static void Add<T1, T2>(this ICollection<(T1, T2)> list, T1 item1, T2 item2) {
        list.Add((item1, item2));
    }
    #endregion
    #endregion
    #region Length相关
    #region ClampLength
    /// <summary>
    /// 限制 <paramref name="list"/> 的最大长度为 <paramref name="length"/>
    /// </summary>
    public static void ClampLength<T>(this IList<T> list, int length) {
        if (list.Count <= length) {
            return;
        }
        list.RemoveRange(length, list.Count - length);
    }
    /// <summary>
    /// <br/>限制 <paramref name="list"/> 的最大长度为 <paramref name="length"/>
    /// <br/>需要 <paramref name="list"/> 的长度大于 <paramref name="length"/>
    /// </summary>
    public static void ClampLengthF<T>(this IList<T> list, int length) {
        list.RemoveRange(length, list.Count - length);
    }
    /// <summary>
    /// <br/>限制 <paramref name="list"/> 的最大长度为 <paramref name="length"/>
    /// <br/>在移除每一项前调用 <paramref name="onRemove"/>
    /// </summary>
    public static void ClampLength<T>(this IList<T> list, int length, Action<T> onRemove) {
        for (int i = list.Count - 1; i >= length; --i) {
            onRemove(list[i]);
            list.RemoveAt(i);
        }
    }
    /// <inheritdoc cref="ClampLength{T}(IList{T}, int, Action{T})"/>
    public static void ClampLength<T>(this IList<T> list, int length, Action<int, T> onRemove) {
        for (int i = list.Count - 1; i >= length; --i) {
            onRemove(i, list[i]);
            list.RemoveAt(i);
        }
    }
    /// <summary>
    /// <br/>限制 <paramref name="list"/> 的最大长度为 <paramref name="length"/>
    /// <br/>在移除每一项前调用 <paramref name="onRemove"/>
    /// <br/>需要 <paramref name="list"/> 的长度大于 <paramref name="length"/>
    /// </summary>
    public static void ClampLengthF<T>(this IList<T> list, int length, Action<T> onRemove) {
        for (int i = list.Count - 1; i >= length; --i) {
            onRemove(list[i]);
        }
        list.RemoveRange(length, list.Count - length);
    }
    /// <inheritdoc cref="ClampLengthF{T}(IList{T}, int, Action{T})"/>
    public static void ClampLengthF<T>(this IList<T> list, int length, Action<int, T> onRemove) {
        for (int i = list.Count - 1; i >= length; --i) {
            onRemove(i, list[i]);
        }
        list.RemoveRange(length, list.Count - length);
    }
    #endregion
    #region EnsureLength
    /// <summary>
    /// 保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用默认值填充
    /// </summary>
    public static void EnsureLength<T>(this IList<T?> list, int length) => list.EnsureLength(length, default(T));
    /// <summary>
    /// 保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用 <paramref name="fillValue"/> 填充
    /// </summary>
    public static void EnsureLength<T>(this IList<T> list, int length, T fillValue) {
        for (int i = list.Count; i < length; ++i) {
            list.Add(fillValue);
        }
    }
    /// <summary>
    /// 保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用 <paramref name="fillValueGetter"/> 填充
    /// </summary>
    public static void EnsureLength<T>(this IList<T> list, int length, Func<T> fillValueGetter) {
        for (int i = list.Count; i < length; ++i) {
            list.Add(fillValueGetter());
        }
    }
    /// <inheritdoc cref="EnsureLength{T}(IList{T}, int, Func{T})"/>
    public static void EnsureLength<T>(this IList<T> list, int length, Func<int, T> fillValueGetter) {
        for (int i = list.Count; i < length; ++i) {
            list.Add(fillValueGetter(i));
        }
    }
    #endregion
    #region SetLength
    #region 没有 onRemove
    /// <summary>
    /// 保证 <paramref name="list"/> 的长度为 <paramref name="length"/>, 不足使用默认值填充
    /// </summary>
    public static void SetLength<T>(this IList<T?> list, int length) => list.SetLength(length, default(T));
    /// <summary>
    /// 保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用 <paramref name="fillValue"/> 填充
    /// </summary>
    public static void SetLength<T>(this IList<T> list, int length, T fillValue) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValue);
        }
        else if (count > length) {
            list.ClampLength(length);
        }
    }
    /// <summary>
    /// 保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用 <paramref name="fillValueGetter"/> 填充
    /// </summary>
    public static void SetLength<T>(this IList<T> list, int length, Func<T> fillValueGetter) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValueGetter);
        }
        else if (count > length) {
            list.ClampLength(length);
        }
    }
    /// <inheritdoc cref="SetLength{T}(IList{T}, int, Func{T})"/>
    public static void SetLength<T>(this IList<T> list, int length, Func<int, T> fillValueGetter) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValueGetter);
        }
        else if (count > length) {
            list.ClampLength(length);
        }
    }
    #endregion
    #region 带有 onRemove, 使用默认值填充
    /// <summary>
    /// <br/>保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用默认值填充
    /// <br/>在移除每一项前调用 <paramref name="onRemove"/>
    /// </summary>
    public static void SetLength<T>(this IList<T?> list, int length, Action<T?> onRemove) => list.SetLength(length, default(T), onRemove);
    /// <summary>
    /// <br/>保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用默认值填充
    /// <br/>在移除每一个非空项前调用 <paramref name="onRemove"/>
    /// </summary>
    public static void SetLengthS<T>(this IList<T?> list, int length, Action<T> onRemove) => list.SetLength(length, default(T), t => {
        if (t != null)
            onRemove(t);
    });
    /// <inheritdoc cref="SetLength{T}(IList{T}, int, Action{T})"/>
    public static void SetLength<T>(this IList<T?> list, int length, Action<int, T?> onRemove) => list.SetLength(length, default(T), onRemove);
    /// <inheritdoc cref="SetLengthS{T}(IList{T}, int, Action{T})"/>
    public static void SetLengthS<T>(this IList<T?> list, int length, Action<int, T> onRemove) => list.SetLength(length, default(T), (i, t) => {
        if (t != null)
            onRemove(i, t);
    });
    #endregion
    #region 带有 onRemove, 使用固定值填充
    /// <summary>
    /// 保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用 <paramref name="fillValue"/> 填充
    /// <br/>在移除每一项前调用 <paramref name="onRemove"/>
    /// </summary>
    public static void SetLength<T>(this IList<T> list, int length, T fillValue, Action<T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValue);
        }
        else if (count > length) {
            list.ClampLength(length, onRemove);
        }
    }
    /// <summary>
    /// 保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用 <paramref name="fillValue"/> 填充
    /// <br/>在移除前对每一个要移除的项调用 <paramref name="onRemove"/>, 随后再全部移除
    /// </summary>
    public static void SetLengthF<T>(this IList<T> list, int length, T fillValue, Action<T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValue);
        }
        else if (count > length) {
            list.ClampLengthF(length, onRemove);
        }
    }
    /// <inheritdoc cref="SetLength{T}(IList{T}, int, T, Action{T})"/>
    public static void SetLength<T>(this IList<T> list, int length, T fillValue, Action<int, T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValue);
        }
        else if (count > length) {
            list.ClampLength(length, onRemove);
        }
    }
    /// <inheritdoc cref="SetLengthF{T}(IList{T}, int, T, Action{T})"/>
    public static void SetLengthF<T>(this IList<T> list, int length, T fillValue, Action<int, T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValue);
        }
        else if (count > length) {
            list.ClampLengthF(length, onRemove);
        }
    }
    #endregion
    #region 带有 onRemove, 使用方法填充
    /// <summary>
    /// 保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用 <paramref name="fillValueGetter"/> 填充
    /// <br/>在移除每一项前调用 <paramref name="onRemove"/>
    /// </summary>
    public static void SetLength<T>(this IList<T> list, int length, Func<T> fillValueGetter, Action<T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValueGetter);
        }
        else if (count > length) {
            list.ClampLength(length, onRemove);
        }
    }
    /// <inheritdoc cref="SetLength{T}(IList{T}, int, Func{T}, Action{T})"/>
    public static void SetLength<T>(this IList<T> list, int length, Func<int, T> fillValueGetter, Action<T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValueGetter);
        }
        else if (count > length) {
            list.ClampLength(length, onRemove);
        }
    }
    /// <summary>
    /// 保证 <paramref name="list"/> 的最小长度为 <paramref name="length"/>, 不足使用 <paramref name="fillValueGetter"/> 填充
    /// <br/>在移除前对每一个要移除的项调用 <paramref name="onRemove"/>, 随后再全部移除
    /// </summary>
    public static void SetLengthF<T>(this IList<T> list, int length, Func<T> fillValueGetter, Action<T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValueGetter);
        }
        else if (count > length) {
            list.ClampLengthF(length, onRemove);
        }
    }
    /// <inheritdoc cref="SetLengthF{T}(IList{T}, int, Func{T}, Action{T})"/>
    public static void SetLengthF<T>(this IList<T> list, int length, Func<int, T> fillValueGetter, Action<T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValueGetter);
        }
        else if (count > length) {
            list.ClampLengthF(length, onRemove);
        }
    }
    /// <inheritdoc cref="SetLength{T}(IList{T}, int, Func{T}, Action{T})"/>
    public static void SetLength<T>(this IList<T> list, int length, Func<T> fillValueGetter, Action<int, T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValueGetter);
        }
        else if (count > length) {
            list.ClampLength(length, onRemove);
        }
    }
    /// <inheritdoc cref="SetLength{T}(IList{T}, int, Func{int, T}, Action{T})"/>
    public static void SetLength<T>(this IList<T> list, int length, Func<int, T> fillValueGetter, Action<int, T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValueGetter);
        }
        else if (count > length) {
            list.ClampLength(length, onRemove);
        }
    }
    /// <inheritdoc cref="SetLengthF{T}(IList{T}, int, Func{T}, Action{T})"/>
    public static void SetLengthF<T>(this IList<T> list, int length, Func<T> fillValueGetter, Action<int, T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValueGetter);
        }
        else if (count > length) {
            list.ClampLengthF(length, onRemove);
        }
    }
    /// <inheritdoc cref="SetLengthF{T}(IList{T}, int, Func{int, T}, Action{T})"/>
    public static void SetLengthF<T>(this IList<T> list, int length, Func<int, T> fillValueGetter, Action<int, T> onRemove) {
        int count = list.Count;
        if (count < length) {
            list.EnsureLength(length, fillValueGetter);
        }
        else if (count > length) {
            list.ClampLengthF(length, onRemove);
        }
    }
    #endregion
    #endregion
    #endregion
    #region 获得数组的元素
    #region Get<T>
    public static T? Get<T>(this object?[] array, int index) => (index >= array.Length || index < 0) ? default : (T?)array[index];
    public static bool Get<T>(this object?[] array, int index, out T? value) {
        if (index >= array.Length || index < 0) {
            value = default;
            return false;
        }
        value = (T?)array[index];
        return true;
    }
    public static T? GetF<T>(this object?[] array, int index) => (T?)array[index];
    public static void GetF<T>(this object?[] array, int index, out T? value) => value = (T?)array[index];
    #endregion
    /// <summary>
    /// <br/>获取列表中某个下标对应的值
    /// <br/>若超界则返回默认值
    /// </summary>
    public static T? GetS<T>(this IList<T> list, int index) => index < 0 || index >= list.Count ? default : list[index];
    /// <summary>
    /// <br/>获取列表中某个下标对应的值
    /// <br/>若超界则获得默认值
    /// <br/>获得的值由 out 参数带出
    /// <br/>返回是否获取成功
    /// </summary>
    public static bool GetS<T>(this IList<T> list, int index, out T? value) {
        if (index < 0 || index >= list.Count) {
            value = default;
            return false;
        }
        value = list[index];
        return true;
    }

    public static T? ElementAtS<T>(this IList<T> list, int index) => index < 0 || index >= list.Count ? default : list[index];
    public static void ElementAsS<T>(this IList<T> list, int index, out T? value) => value = index < 0 || index >= list.Count ? default : list[index];
    public static T GetS<T>(this IList<T> list, int index, T defaultValue) => index < 0 || index >= list.Count ? defaultValue : list[index];
    public static void GetS<T>(this IList<T> list, int index, out T value, T defaultValue) => value = index < 0 || index >= list.Count ? defaultValue : list[index];
    public static T ElementAtS<T>(this IList<T> list, int index, T defaultValue) => index < 0 || index >= list.Count ? defaultValue : list[index];
    public static void ElementAsS<T>(this IList<T> list, int index, out T value, T defaultValue) => value = index < 0 || index >= list.Count ? defaultValue : list[index];
    #endregion
    #region 设置数组元素
    /// <summary>
    /// <br/>保证设置值
    /// <br/>若 <paramref name="index"/> &lt; 0 则在末尾插入
    /// <br/>若 <paramref name="index"/> 过大, 则将 <paramref name="list"/> 的长度扩充到 <paramref name="index"/> + 1 再设置
    /// </summary>
    public static void SetFS<T>(this IList<T?> list, int index, T? value) {
        if (index < 0) {
            list.Add(value);
        }
        list.EnsureLength(index + 1);
        list[index] = value;
    }
    #endregion
    #region ToSpan
    public static Span<T> ToSpan<T>(this T[]? array) => array;
    public static Span<T> ToSpan<T>(this T[]? array, Range range) {
        if (array == null) {
            return default;
        }
        var (offset, length) = range.GetOffsetAndLengthSafe(array.Length);
        return new(array, offset, length);
    }
    public static Span<T> ToSpan<T>(this List<T>? list) {
        if (list is null) {
            return default;
        }
        var items = RList.GetItems(list);
        var size = RList.GetSize(list);
        Debug.Assert(items is not null);
        Debug.Assert((uint)size <= (uint)items.Length);
        Debug.Assert(items.GetType() == typeof(T[]));
        return new(items, 0, size);
    }
    public static Span<T> ToSpan<T>(this List<T> list, Range range) {
        if (list is null) {
            return default;
        }
        var items = RList.GetItems(list);
        var size = RList.GetSize(list);
        Debug.Assert(items is not null);
        Debug.Assert((uint)size <= (uint)items.Length);
        Debug.Assert(items.GetType() == typeof(T[]));
        var (offset, length) = range.GetOffsetAndLengthSafe(size);
        return new(items, offset, length);
    }
    #endregion
    #region 杂项
    public static void ReverseSelf<T>(this IList<T> list) {
        int m = list.Count / 2;
        for (int i = 0; i < m; ++i) {
            (list[i], list[list.Count - i - 1]) = (list[list.Count - i - 1], list[i]);
        }
    }
    #endregion
    #endregion
    #region 字典拓展
    public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, (TKey Key, TValue Value) pair) where TKey : notnull => dictionary.Add(pair.Key, pair.Value);
    public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> pair) where TKey : notnull => dictionary.Add(pair.Key, pair.Value);
    public static void AddCount<T>(this Dictionary<T, int> dictionary, T item, int count = 1) where T : notnull {
        if (!dictionary.TryAdd(item, count)) {
            dictionary[item] += count;
        }
    }
    public static void RemoveCount<T>(this Dictionary<T, int> dictionary, T item, int count = 1) where T : notnull {
        if (dictionary.TryGetValue(item, out int value)) {
            value -= count;
            if (value <= 0) {
                dictionary.Remove(item);
            }
        }
    }
    #region AddElement
    public static void AddElement<TKey, TElement>(this IDictionary<TKey, List<TElement>> dictionary, TKey key, TElement element) where TKey : notnull {
        if (dictionary.TryGetValue(key, out List<TElement>? value)) {
            value.Add(element);
        }
        else {
            dictionary.Add(key, [element]);
        }
    }
    public static void AddElementRange<TKey, TElement>(this Dictionary<TKey, List<TElement>> dictionary, TKey key, IEnumerable<TElement> elements) where TKey : notnull {
        if (dictionary.TryGetValue(key, out List<TElement>? value)) {
            value.AddRange(elements);
        }
        else {
            dictionary.Add(key, [.. elements]);
        }
    }
    public static void AddElementRange<TKey, TElement>(this Dictionary<TKey, List<TElement>> dictionary, TKey key, List<TElement> elementList) where TKey : notnull {
        if (dictionary.TryGetValue(key, out List<TElement>? value)) {
            value.AddRange(elementList);
        }
        else {
            dictionary.Add(key, elementList);
        }
    }
    public static void AddElementRange<TKey, TElement>(this IDictionary<TKey, List<TElement>> dictionary, TKey key, IEnumerable<TElement> elements) where TKey : notnull {
        if (dictionary.TryGetValue(key, out List<TElement>? value)) {
            value.AddRange(elements);
        }
        else {
            dictionary.Add(key, [.. elements]);
        }
    }
    public static void AddElementRange<TKey, TElement>(this IDictionary<TKey, List<TElement>> dictionary, TKey key, List<TElement> elementList) where TKey : notnull {
        if (dictionary.TryGetValue(key, out List<TElement>? value)) {
            value.AddRange(elementList);
        }
        else {
            dictionary.Add(key, elementList);
        }
    }
    
    public static void AddElement<TKey, TList, TElement>(this IDictionary<TKey, TList> dictionary, TKey key, TElement element) where TKey : notnull where TList : IList<TElement>, new() {
        if (dictionary.TryGetValue(key, out TList? value)) {
            value.Add(element);
        }
        else {
            dictionary.Add(key, [element]);
        }
    }
    public static void AddElementRange<TKey, TList, TElement>(this Dictionary<TKey, TList> dictionary, TKey key, IEnumerable<TElement> elements) where TKey : notnull where TList : IList<TElement>, new() {
        if (dictionary.TryGetValue(key, out TList? value)) {
            value.AddRange(elements);
        }
        else {
            dictionary.Add(key, [.. elements]);
        }
    }
    public static void AddElementRange<TKey, TList, TElement>(this Dictionary<TKey, TList> dictionary, TKey key, TList elementList) where TKey : notnull where TList : IList<TElement>, new() {
        if (dictionary.TryGetValue(key, out TList? value)) {
            value.AddRange(elementList);
        }
        else {
            dictionary.Add(key, elementList);
        }
    }
    public static void AddElementRange<TKey, TList, TElement>(this IDictionary<TKey, TList> dictionary, TKey key, IEnumerable<TElement> elements) where TKey : notnull where TList : IList<TElement>, new() {
        if (dictionary.TryGetValue(key, out TList? value)) {
            value.AddRange(elements);
        }
        else {
            dictionary.Add(key, [.. elements]);
        }
    }
    public static void AddElementRange<TKey, TList, TElement>(this IDictionary<TKey, TList> dictionary, TKey key, TList elementList) where TKey : notnull where TList : IList<TElement>, new() {
        if (dictionary.TryGetValue(key, out TList? value)) {
            value.AddRange(elementList);
        }
        else {
            dictionary.Add(key, elementList);
        }
    }
    
    public static void AddItem<TKey, TCollection, TItem>(this IDictionary<TKey, TCollection> dictionary, TKey key, TItem item) where TKey : notnull where TCollection : ICollection<TItem>, new() {
        if (dictionary.TryGetValue(key, out TCollection? value)) {
            value.Add(item);
        }
        else {
            dictionary.Add(key, [item]);
        }
    }
    public static void AddItemRange<TKey, TCollection, TItem>(this Dictionary<TKey, TCollection> dictionary, TKey key, IEnumerable<TItem> items) where TKey : notnull where TCollection : ICollection<TItem>, new() {
        if (dictionary.TryGetValue(key, out TCollection? value)) {
            value.AddRange(items);
        }
        else {
            dictionary.Add(key, [.. items]);
        }
    }
    public static void AddItemRange<TKey, TCollection, TItem>(this Dictionary<TKey, TCollection> dictionary, TKey key, TCollection itemCollection) where TKey : notnull where TCollection : ICollection<TItem>, new() {
        if (dictionary.TryGetValue(key, out TCollection? value)) {
            value.AddRange(itemCollection);
        }
        else {
            dictionary.Add(key, itemCollection);
        }
    }
    public static void AddItemRange<TKey, TCollection, TItem>(this IDictionary<TKey, TCollection> dictionary, TKey key, IEnumerable<TItem> items) where TKey : notnull where TCollection : ICollection<TItem>, new() {
        if (dictionary.TryGetValue(key, out TCollection? value)) {
            value.AddRange(items);
        }
        else {
            dictionary.Add(key, [.. items]);
        }
    }
    public static void AddItemRange<TKey, TCollection, TItem>(this IDictionary<TKey, TCollection> dictionary, TKey key, TCollection itemCollection) where TKey : notnull where TCollection : ICollection<TItem>, new() {
        if (dictionary.TryGetValue(key, out TCollection? value)) {
            value.AddRange(itemCollection);
        }
        else {
            dictionary.Add(key, itemCollection);
        }
    }
    #endregion
    public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<TKey, TValue, bool> predicate) where TKey : notnull {
        List<TKey> toRemove = [];
        foreach (var (key, value) in dictionary) {
            if (predicate(key, value)) {
                toRemove.Add(key);
            }
        }
        foreach (var keyToRemove in toRemove) {
            dictionary.Remove(keyToRemove);
        }
    }
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue valueToAdd) where TKey : notnull {
        if (dictionary.TryGetValue(key, out var value)) {
            return value;
        }
        dictionary.Add(key, valueToAdd);
        return valueToAdd;
    }
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> getValueToAdd) where TKey : notnull {
        if (dictionary.TryGetValue(key, out var value)) {
            return value;
        }
        var  valueToAdd = getValueToAdd();
        dictionary.Add(key, valueToAdd);
        return valueToAdd;
    }
    #region 字典的序号相关
    private static class DictionaryIndexMethodExtendHelper<TKey, TValue> where TKey : notnull {
        public static Func<Dictionary<TKey, TValue>, int, TKey> GetKeyByIndex;
        public static Func<Dictionary<TKey, TValue>, int, TValue> GetValueByIndex;
        public static Func<Dictionary<TKey, TValue>, TKey, int> GetIndexByKey;
        static DictionaryIndexMethodExtendHelper() {
            #region 反射
            var dictionaryType = typeof(Dictionary<TKey, TValue>);
            var iEqualityComparerType = typeof(IEqualityComparer<TKey>);
            var entriesField = dictionaryType.GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var entriesType = entriesField.FieldType;
            var entryType = entriesType.GetElementType()!;
            var bucketsField = dictionaryType.GetField("_buckets", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var comparerField = dictionaryType.GetField("_comparer", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var objectGetHashCode = typeof(object).GetMethod(nameof(GetHashCode))!;
            var iEqualityComparerGetHashCodeMethod = iEqualityComparerType.GetMethod(nameof(IEqualityComparer<TKey>.GetHashCode), [typeof(TKey)])!;
            var iEqualityComparerEqualMethod = iEqualityComparerType.GetMethod(nameof(IEqualityComparer<TKey>.Equals), [typeof(TKey?), typeof(TKey?)])!;
            var equalityComparerGetDefaultMethod = typeof(EqualityComparer<TKey>).GetProperty(nameof(EqualityComparer<TKey>.Default), BindingFlags.Static | BindingFlags.Public)!.GetGetMethod()!;
            var getBucketMethod = dictionaryType.GetMethod("GetBucket", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var entryHashCodeField = entryType.GetField("hashCode")!;
            var entryKeyField = entryType.GetField("key")!;
            var entryValueField = entryType.GetField("value")!;
            var entryNextField = entryType.GetField("next")!;
            var invalidOperationExceptionConstructor = typeof(InvalidOperationException).GetConstructor([typeof(string)])!;
            #endregion

            ILGenerator il;

            #region GetIndexByKey

            DynamicMethod getIndexByKeyDynamicMethod = new("GetDictionaryIndexByKey", typeof(int), [dictionaryType, typeof(TKey)], true);
            il = getIndexByKeyDynamicMethod.GetILGenerator();

            #region locals
            var i_local = il.DeclareLocal(typeof(int));
            var comparer_local = il.DeclareLocal(iEqualityComparerType);
            var hashCode_local = il.DeclareLocal(typeof(uint));
            var entries_local = il.DeclareLocal(entriesType);
            var collisionCount_local = il.DeclareLocal(typeof(uint));
            var entry_local = il.DeclareLocal(entryType);
            #endregion

            #region labels
            var returnFound_label = il.DefineLabel();
            var returnNotFound_label = il.DefineLabel();
            var comparerIsNotNull_label = il.DefineLabel();
            var afterComparerIsNotNull_label = il.DefineLabel();
            var loopStart_label = il.DefineLabel();
            var testEntryNext_label = il.DefineLabel();
            #endregion

            /*
            int GetIndexOfKey(TKey key) {
                int i;
                if (_buckets == null) {
                    goto ReturnNotFound;
                }
                IEqualityComparer<TKey>? comparer = _comparer;
                uint hashCode;
                if (comparer == null) {
                    hashCode = (uint)key.GetHashCode();
                    comparer = EqualityComparer<TKey>.Default;
                }
                else {
                    hashCode = (uint)comparer.GetHashCode(key);
                }
                i = GetBucket(hashCode);
                Entry[]? entries = _entries;
                uint collisionCount = 0;
                i--;
                do {
                    if ((uint)i >= (uint)entries.Length) {
                        goto ReturnNotFound;
                    }

                    var entry = entries[i];
                    if (entry.hashCode == hashCode && comparer.Equals(entry.key, key)) {
                        goto ReturnFound;
                    }

                    i = entry.next;

                    collisionCount++;
                } while (collisionCount <= (uint)entries.Length);
                throw new InvalidOperationException("Concurrent operations not supported");

            ReturnFound:
                return i;
            ReturnNotFound:
                return -1;
            }
            */

            #region IL
            // if (_buckets == null) goto ReturnNotFound;
            il.Emit(SOpCodes.Ldarg_0);
            il.Emit(SOpCodes.Ldfld, bucketsField);
            il.Emit(SOpCodes.Brfalse, returnNotFound_label);

            // IEqualityComparer<TKey> comparer = _comparer;
            il.Emit(SOpCodes.Ldarg_0);
            il.Emit(SOpCodes.Ldfld, comparerField);
            il.Emit(SOpCodes.Stloc, comparer_local);

            // if (comparer == null) {
            il.Emit(SOpCodes.Ldloc, comparer_local);
            il.Emit(SOpCodes.Brtrue, comparerIsNotNull_label);

            // hashCode = (uint)key.GetHashCode();
            il.Emit(SOpCodes.Ldarga_S, 1);
            il.Emit(SOpCodes.Constrained, typeof(TKey));
            il.Emit(SOpCodes.Callvirt, objectGetHashCode);
            il.Emit(SOpCodes.Stloc, hashCode_local);

            // comparer = EqualityComparer<TKey>.Default;
            il.Emit(SOpCodes.Call, equalityComparerGetDefaultMethod);
            il.Emit(SOpCodes.Stloc, comparer_local);
            il.Emit(SOpCodes.Br, afterComparerIsNotNull_label);

            // } else {
            il.MarkLabel(comparerIsNotNull_label);

            // hashCode = (uint)comparer.GetHashCode(key);
            il.Emit(SOpCodes.Ldloc, comparer_local);
            il.Emit(SOpCodes.Ldarg_1);
            il.Emit(SOpCodes.Callvirt, iEqualityComparerGetHashCodeMethod);
            il.Emit(SOpCodes.Stloc, hashCode_local);

            // }
            il.MarkLabel(afterComparerIsNotNull_label);

            // i = GetBucket(hashCode);
            il.Emit(SOpCodes.Ldarg_0);
            il.Emit(SOpCodes.Ldloc, hashCode_local);
            il.Emit(SOpCodes.Call, getBucketMethod);
            il.Emit(SOpCodes.Ldind_I4);
            il.Emit(SOpCodes.Stloc, i_local);

            // entries = _entries;
            il.Emit(SOpCodes.Ldarg_0);
            il.Emit(SOpCodes.Ldfld, entriesField);
            il.Emit(SOpCodes.Stloc, entries_local);

            // uint collisionCount = 0;
            il.Emit(SOpCodes.Ldc_I4_0);
            il.Emit(SOpCodes.Stloc, collisionCount_local);

            // i--;
            il.Emit(SOpCodes.Ldloc, i_local);
            il.Emit(SOpCodes.Ldc_I4_1);
            il.Emit(SOpCodes.Sub);
            il.Emit(SOpCodes.Stloc, i_local);

            // do {
            il.MarkLabel(loopStart_label);

            // if ((uint)i >= (uint)entries.Length) { goto ReturnNotFound; }
            il.Emit(SOpCodes.Ldloc, i_local);
            il.Emit(SOpCodes.Ldloc, entries_local);
            il.Emit(SOpCodes.Ldlen);
            il.Emit(SOpCodes.Conv_I4);
            il.Emit(SOpCodes.Bge_Un, returnNotFound_label);

            // entry = entries[i];
            il.Emit(SOpCodes.Ldloc, entries_local);
            il.Emit(SOpCodes.Ldloc, i_local);
            il.Emit(SOpCodes.Ldelem, entryType);
            il.Emit(SOpCodes.Stloc, entry_local);

            // if (entry.hashCode != hashCode) goto TestEntryNext;
            il.Emit(SOpCodes.Ldloc, entry_local);
            il.Emit(SOpCodes.Ldfld, entryHashCodeField);
            il.Emit(SOpCodes.Ldloc, hashCode_local);
            il.Emit(SOpCodes.Bne_Un, testEntryNext_label);

            // if (comparer.Equals(entry.key, key)) goto ReturnFound;
            il.Emit(SOpCodes.Ldloc, comparer_local);
            il.Emit(SOpCodes.Ldloc, entry_local);
            il.Emit(SOpCodes.Ldfld, entryKeyField);
            il.Emit(SOpCodes.Ldarg_1);
            il.Emit(SOpCodes.Callvirt, iEqualityComparerEqualMethod);
            il.Emit(SOpCodes.Brtrue, returnFound_label);

            // i = entry.next;
            il.MarkLabel(testEntryNext_label);
            il.Emit(SOpCodes.Ldloc, entry_local);
            il.Emit(SOpCodes.Ldfld, entryNextField);
            il.Emit(SOpCodes.Stloc, i_local);

            // collisionCount++;
            il.Emit(SOpCodes.Ldloc, collisionCount_local);
            il.Emit(SOpCodes.Ldc_I4_1);
            il.Emit(SOpCodes.Add);
            il.Emit(SOpCodes.Stloc, collisionCount_local);

            // } while (collisionCount <= (uint)entries.Length);
            il.Emit(SOpCodes.Ldloc, collisionCount_local);
            il.Emit(SOpCodes.Ldloc, entries_local);
            il.Emit(SOpCodes.Ldlen);
            il.Emit(SOpCodes.Conv_I4);
            il.Emit(SOpCodes.Ble_Un, loopStart_label);

            // throw new InvalidOperationException("Concurrent operations not supported");
            il.Emit(SOpCodes.Ldstr, "Concurrent operations not supported");
            il.Emit(SOpCodes.Newobj, invalidOperationExceptionConstructor);
            il.Emit(SOpCodes.Throw);

            // ReturnFound: return i;
            il.MarkLabel(returnFound_label);
            il.Emit(SOpCodes.Ldloc, i_local);
            il.Emit(SOpCodes.Ret);

            // ReturnNotFound: return -1;
            il.MarkLabel(returnNotFound_label);
            il.Emit(SOpCodes.Ldc_I4_M1);
            il.Emit(SOpCodes.Ret);
            #endregion

            GetIndexByKey = getIndexByKeyDynamicMethod.CreateDelegate<Func<Dictionary<TKey, TValue>, TKey, int>>();

            #endregion

            #region GetKeyByIndex

            DynamicMethod getKeyByIndexDynamicMethod = new("GetDictionaryKeyByIndex", typeof(TKey), [dictionaryType, typeof(int)], true);
            il = getKeyByIndexDynamicMethod.GetILGenerator();

            il.Emit(SOpCodes.Ldarg_0);
            il.Emit(SOpCodes.Ldfld, entriesField);
            il.Emit(SOpCodes.Ldarg_1);
            il.Emit(SOpCodes.Ldelema, entryType);
            il.Emit(SOpCodes.Ldfld, entryKeyField);
            il.Emit(SOpCodes.Ret);

            GetKeyByIndex = getKeyByIndexDynamicMethod.CreateDelegate<Func<Dictionary<TKey, TValue>, int, TKey>>();

            #endregion

            #region GetValueByIndex

            DynamicMethod getValueByIndexDynamicMethod = new("GetDictionaryValueByIndex", typeof(TValue), [dictionaryType, typeof(int)], true);
            il = getValueByIndexDynamicMethod.GetILGenerator();

            il.Emit(SOpCodes.Ldarg_0);
            il.Emit(SOpCodes.Ldfld, entriesField);
            il.Emit(SOpCodes.Ldarg_1);
            il.Emit(SOpCodes.Ldelema, entryType);
            il.Emit(SOpCodes.Ldfld, entryValueField);
            il.Emit(SOpCodes.Ret);

            GetValueByIndex = getValueByIndexDynamicMethod.CreateDelegate<Func<Dictionary<TKey, TValue>, int, TValue>>();

            #endregion
        }
    }
    /// <summary>
    /// 返回索引对应的键, 若超界则报错
    /// </summary>
    public static TKey GetKeyByIndex<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index) where TKey : notnull {
        return DictionaryIndexMethodExtendHelper<TKey, TValue>.GetKeyByIndex(dictionary, index);
    }
    /// <summary>
    /// 返回索引对应的值, 若超界则报错
    /// </summary>
    public static TValue GetValueByIndex<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index) where TKey : notnull {
        return DictionaryIndexMethodExtendHelper<TKey, TValue>.GetValueByIndex(dictionary, index);
    }
    /// <summary>
    /// 返回索引对应的键值对, 若超界则报错
    /// </summary>
    public static KeyValuePair<TKey, TValue> GetPairByIndex<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index) where TKey : notnull {
        return new(GetKeyByIndex(dictionary, index), GetValueByIndex(dictionary, index));
    }
    /// <summary>
    /// 返回索引对应的键, 若超界则返回默认值
    /// </summary>
    public static TKey? GetKeyByIndexS<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index) where TKey : notnull {
        return index.IsBetween(0, dictionary.Count) ? DictionaryIndexMethodExtendHelper<TKey, TValue>.GetKeyByIndex(dictionary, index) : default;
    }
    /// <summary>
    /// 返回索引对应的值, 若超界则返回默认值
    /// </summary>
    public static TValue? GetValueByIndexS<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index) where TKey : notnull {
        return index.IsBetween(0, dictionary.Count) ? DictionaryIndexMethodExtendHelper<TKey, TValue>.GetValueByIndex(dictionary, index) : default;
    }
    /// <summary>
    /// 返回索引对应的键值对, 若超界则返回默认值
    /// </summary>
    public static KeyValuePair<TKey?, TValue?> GetPairByIndexS<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index) where TKey : notnull {
        return index.IsBetween(0, dictionary.Count) ? new(GetKeyByIndex(dictionary, index), GetValueByIndex(dictionary, index)) : default;
    }
    /// <summary>
    /// 返回索引对应的键, 若超界则返回默认值
    /// </summary>
    public static TKey GetKeyByIndexS<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index, TKey defaultValue) where TKey : notnull {
        return index.IsBetween(0, dictionary.Count) ? DictionaryIndexMethodExtendHelper<TKey, TValue>.GetKeyByIndex(dictionary, index) : defaultValue;
    }
    /// <summary>
    /// 返回索引对应的值, 若超界则返回默认值
    /// </summary>
    public static TValue GetValueByIndexS<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index, TValue defaultValue) where TKey : notnull {
        return index.IsBetween(0, dictionary.Count) ? DictionaryIndexMethodExtendHelper<TKey, TValue>.GetValueByIndex(dictionary, index) : defaultValue;
    }
    /// <summary>
    /// 返回键对应的索引, 若键不在字典中则返回 -1
    /// </summary>
    public static int GetIndexByKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull {
        return DictionaryIndexMethodExtendHelper<TKey, TValue>.GetIndexByKey(dictionary, key);
    }
    #endregion
    public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value, bool replace = true) where TKey : notnull {
        if (replace) {
            dictionary[key] = value;
        }
        else {
            dictionary.TryAdd(key, value);
        }
    }
    public static Action<TValue> AddFP<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull => v => dictionary.Add(key, v);
    public static Action<TKey> AddFPR<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value) where TKey : notnull => k => dictionary.Add(k, value);
    #endregion
    #region System.Range 拓展
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int Offset, int Length) GetOffsetAndLengthSafe(this Range range, int length) {
        int left = range.Start.GetOffset(length);
        int right = range.End.GetOffset(length);
        left.ClampTo(0, length);
        right.ClampTo(left, length);
        return (left, right - left);
    }
    #endregion
    #region ref相关拓展
    //ref拓展不知道为什么只能给值类型用
    /// <summary>
    /// 对<paramref name="self"/>执行<paramref name="action"/>
    /// </summary>
    /// <returns><paramref name="self"/>的引用</returns>
    public static ref T Do<T>(ref this T self, Action<T> action) where T : struct {
        action(self);
        return ref self;
    }
    /// <summary>
    /// 将<paramref name="other"/>的值赋给<paramref name="self"/>
    /// </summary>
    /// <returns><paramref name="self"/>的引用</returns>
    public static ref T Assign<T>(ref this T self, T other) where T : struct {
        self = other;
        return ref self;
    }
    public static ref T AssignIf<T>(ref this T self, bool condition, T other) where T : struct {
        if (condition) {
            self = other;
        }
        return ref self;
    }
    public static ref T AssignIfNotNull<T>(ref this T self, T? other) where T : struct {
        if (other.HasValue) {
            self = other.Value;
        }
        return ref self;
    }

    public static ref T? Assign<T>(ref this T? self, T? other) where T : struct {
        self = other;
        return ref self;
    }
    public static ref T? AssignIf<T>(ref this T? self, bool condition, T other) where T : struct {
        if (condition) {
            self = other;
        }
        return ref self;
    }
    public static ref T? AssignIfNotNull<T>(ref this T? self, T? other) where T : struct {
        if (other.HasValue) {
            self = other;
        }
        return ref self;
    }
    #endregion
    #region 解构拓展
    public static void Deconstruct(this Vector2 vector2, out float x, out float y) {
        x = vector2.X;
        y = vector2.Y;
    }
    public static void Deconstruct(this Vector3 vector3, out float x, out float y, out float z) {
        x = vector3.X;
        y = vector3.Y;
        z = vector3.Z;
    }
    public static void Deconstruct(this Rectangle rectangle, out int x, out int y, out int width, out int height) {
        x = rectangle.X;
        y = rectangle.Y;
        width = rectangle.Width;
        height = rectangle.Height;
    }
    #region 数组解构
    public static void Deconstruct<T>(this IList<T> list, out T v0) {
        v0 = list[0];
    }
    public static void Deconstruct<T>(this IList<T> list, out T v0, out T v1) {
        v0 = list[0];
        v1 = list[1];
    }
    public static void Deconstruct<T>(this IList<T> list, out T v0, out T v1, out T v2) {
        v0 = list[0];
        v1 = list[1];
        v2 = list[2];
    }
    public static void Deconstruct<T>(this IList<T> list, out T v0, out T v1, out T v2, out T v3) {
        v0 = list[0];
        v1 = list[1];
        v2 = list[2];
        v3 = list[3];
    }
    public static void Deconstruct<T>(this IList<T> list, out T v0, out T v1, out T v2, out T v3, out T v4) {
        v0 = list[0];
        v1 = list[1];
        v2 = list[2];
        v3 = list[3];
        v4 = list[4];
    }
    public static void Deconstruct<T>(this IList<T> list, out T v0, out T v1, out T v2, out T v3, out T v4, out T v5) {
        v0 = list[0];
        v1 = list[1];
        v2 = list[2];
        v3 = list[3];
        v4 = list[4];
        v5 = list[5];
    }
    public static void Deconstruct<T>(this IList<T> list, out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6) {
        v0 = list[0];
        v1 = list[1];
        v2 = list[2];
        v3 = list[3];
        v4 = list[4];
        v5 = list[5];
        v6 = list[6];
    }
    public static void Deconstruct<T>(this IList<T> list, out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7) {
        v0 = list[0];
        v1 = list[1];
        v2 = list[2];
        v3 = list[3];
        v4 = list[4];
        v5 = list[5];
        v6 = list[6];
        v7 = list[7];
    }
    public static void Deconstruct<T>(this IList<T> list, out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8) {
        v0 = list[0];
        v1 = list[1];
        v2 = list[2];
        v3 = list[3];
        v4 = list[4];
        v5 = list[5];
        v6 = list[6];
        v7 = list[7];
        v8 = list[8];
    }
    public static void Deconstruct<T>(this IList<T> list, out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8, out T v9) {
        v0 = list[0];
        v1 = list[1];
        v2 = list[2];
        v3 = list[3];
        v4 = list[4];
        v5 = list[5];
        v6 = list[6];
        v7 = list[7];
        v8 = list[8];
        v9 = list[9];
    }
    #endregion
    #region IEnumerable 解构
    public static void Deconstruct<T>(this IEnumerable<T> list, out T v0) {
        var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        v0 = enumerator.Current;
    }
    public static void Deconstruct<T>(this IEnumerable<T> list, out T v0, out T v1) {
        var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        v0 = enumerator.Current;
        enumerator.MoveNext();
        v1 = enumerator.Current;
    }
    public static void Deconstruct<T>(this IEnumerable<T> list, out T v0, out T v1, out T v2) {
        var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        v0 = enumerator.Current;
        enumerator.MoveNext();
        v1 = enumerator.Current;
        enumerator.MoveNext();
        v2 = enumerator.Current;
    }
    public static void Deconstruct<T>(this IEnumerable<T> list, out T v0, out T v1, out T v2, out T v3) {
        var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        v0 = enumerator.Current;
        enumerator.MoveNext();
        v1 = enumerator.Current;
        enumerator.MoveNext();
        v2 = enumerator.Current;
        enumerator.MoveNext();
        v3 = enumerator.Current;
    }
    public static void Deconstruct<T>(this IEnumerable<T> list, out T v0, out T v1, out T v2, out T v3, out T v4) {
        var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        v0 = enumerator.Current;
        enumerator.MoveNext();
        v1 = enumerator.Current;
        enumerator.MoveNext();
        v2 = enumerator.Current;
        enumerator.MoveNext();
        v3 = enumerator.Current;
        enumerator.MoveNext();
        v4 = enumerator.Current;
    }
    public static void Deconstruct<T>(this IEnumerable<T> list, out T v0, out T v1, out T v2, out T v3, out T v4, out T v5) {
        var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        v0 = enumerator.Current;
        enumerator.MoveNext();
        v1 = enumerator.Current;
        enumerator.MoveNext();
        v2 = enumerator.Current;
        enumerator.MoveNext();
        v3 = enumerator.Current;
        enumerator.MoveNext();
        v4 = enumerator.Current;
        enumerator.MoveNext();
        v5 = enumerator.Current;
    }
    public static void Deconstruct<T>(this IEnumerable<T> list, out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6) {
        var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        v0 = enumerator.Current;
        enumerator.MoveNext();
        v1 = enumerator.Current;
        enumerator.MoveNext();
        v2 = enumerator.Current;
        enumerator.MoveNext();
        v3 = enumerator.Current;
        enumerator.MoveNext();
        v4 = enumerator.Current;
        enumerator.MoveNext();
        v5 = enumerator.Current;
        enumerator.MoveNext();
        v6 = enumerator.Current;
    }
    public static void Deconstruct<T>(this IEnumerable<T> list, out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7) {
        var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        v0 = enumerator.Current;
        enumerator.MoveNext();
        v1 = enumerator.Current;
        enumerator.MoveNext();
        v2 = enumerator.Current;
        enumerator.MoveNext();
        v3 = enumerator.Current;
        enumerator.MoveNext();
        v4 = enumerator.Current;
        enumerator.MoveNext();
        v5 = enumerator.Current;
        enumerator.MoveNext();
        v6 = enumerator.Current;
        enumerator.MoveNext();
        v7 = enumerator.Current;
    }
    public static void Deconstruct<T>(this IEnumerable<T> list, out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8) {
        var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        v0 = enumerator.Current;
        enumerator.MoveNext();
        v1 = enumerator.Current;
        enumerator.MoveNext();
        v2 = enumerator.Current;
        enumerator.MoveNext();
        v3 = enumerator.Current;
        enumerator.MoveNext();
        v4 = enumerator.Current;
        enumerator.MoveNext();
        v5 = enumerator.Current;
        enumerator.MoveNext();
        v6 = enumerator.Current;
        enumerator.MoveNext();
        v7 = enumerator.Current;
        enumerator.MoveNext();
        v8 = enumerator.Current;
    }
    public static void Deconstruct<T>(this IEnumerable<T> list, out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8, out T v9) {
        var enumerator = list.GetEnumerator();
        enumerator.MoveNext();
        v0 = enumerator.Current;
        enumerator.MoveNext();
        v1 = enumerator.Current;
        enumerator.MoveNext();
        v2 = enumerator.Current;
        enumerator.MoveNext();
        v3 = enumerator.Current;
        enumerator.MoveNext();
        v4 = enumerator.Current;
        enumerator.MoveNext();
        v5 = enumerator.Current;
        enumerator.MoveNext();
        v6 = enumerator.Current;
        enumerator.MoveNext();
        v7 = enumerator.Current;
        enumerator.MoveNext();
        v8 = enumerator.Current;
        enumerator.MoveNext();
        v9 = enumerator.Current;
    }
    #endregion
    #endregion
    #region 其他
    public static int Get7BitEncodedLength(this int self) {
        if (self < 0) {
            return 5;
        }
        if (self < 1 << 7) {
            return 1;
        }
        if (self < 1 << 14) {
            return 2;
        }
        if (self < 1 << 21) {
            return 3;
        }
        if (self < 1 << 28) {
            return 4;
        }
        return 5;
    }
    public static double Get7BitEncodedAverageLength(this int self) {
        double total = 0;
        uint uself = (uint)self;
        if (uself < 1 << 7) {
            return 1;
        }
        total += 1 << 7;
        if (uself < 1 << 14) {
            total += (uself - (1 << 7)) * 2;
            return total / uself;
        }
        total += 2 * ((1 << 14) - (1 << 7));
        if (uself < 1 << 21) {
            total += (uself - (1 << 14)) * 3;
            return total / uself;
        }
        total += 3 * ((1 << 21) - (1 << 14));
        if (uself < 1 << 28) {
            total += (uself - (1 << 21)) * 4;
            return total / uself;
        }
        total += 4 * ((1 << 28) - (1 << 21));
        total += (double)(uself - (1 << 28)) * 5;
        return total / uself;
    }
    public static int ToSign(this bool self, bool reverse = false) => self ^ reverse ? 1 : -1;
    public static int LosslessToInt(this float self) {
        unsafe {
            return *(int*)&self;
        }
    }
    public static float LosslessToFloat(this int self) {
        unsafe {
            return *(float*)&self;
        }
    }
    public static TResult Transfer<TSource, TResult>(this TSource source, Func<TSource, TResult> transfer) => transfer(source);
    public static void Do<T>(this T self, Action<T> action) => action(self);
    #endregion
}
