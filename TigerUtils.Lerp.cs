#nullable enable

using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using static TigerUtilsLib.TigerClasses.LerpModels;

namespace TigerUtilsLib {
    partial class TigerUtils {
        public enum LerpType {
            Linear,
            Quadratic,
            Cubic,
            CubicByK,
            Sin,
            Stay,
        }
        public static float NewLerpValue(float val, bool clamped, LerpType type, params float[] pars) => type switch {
            LerpType.Linear => FloatLerpModelByLinear.StaticNewLerpValue(val, clamped),
            LerpType.Quadratic => FloatLerpModelByQuadratic.StaticNewLerpValue(val, clamped, pars),
            LerpType.Cubic => FloatLerpModelByCubic.StaticNewLerpValue(val, clamped, pars),
            LerpType.CubicByK => FloatLerpModelByCubic.StaticNewLerpValue(val, clamped, pars),
            LerpType.Sin => FloatLerpModelByCubic.StaticNewLerpValue(val, clamped, pars),
            LerpType.Stay => FloatLerpModelByStay.StaticNewLerpValue(val),
            _ => FloatLerpModelByLinear.StaticNewLerpValue(val, clamped),
        };
        public static double NewLerpValue(double val, bool clamped, LerpType type, params double[] pars) => type switch {
            LerpType.Linear => DoubleLerpModelByLinear.StaticNewLerpValue(val, clamped),
            LerpType.Quadratic => DoubleLerpModelByQuadratic.StaticNewLerpValue(val, clamped, pars),
            LerpType.Cubic => DoubleLerpModelByCubic.StaticNewLerpValue(val, clamped, pars),
            LerpType.CubicByK => DoubleLerpModelByCubic.StaticNewLerpValue(val, clamped, pars),
            LerpType.Sin => DoubleLerpModelByCubic.StaticNewLerpValue(val, clamped, pars),
            LerpType.Stay => DoubleLerpModelByStay.StaticNewLerpValue(val),
            _ => DoubleLerpModelByLinear.StaticNewLerpValue(val, clamped),
        };

        #region LerpF
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpF(float left, float right, float val) => left + val * (right - left);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpFC(float left, float right, float val) => val <= 0 ? left : val >= 1 ? right : left + val * (right - left);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpF(float left, float right, double val) => (float)(left + val * (right - left));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpFC(float left, float right, double val) => (float)(val <= 0 ? left : val >= 1 ? right : left + val * (right - left));
        #endregion
        #region LerpWithLerpType
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
            return new(LerpF(left.X, right.X, val), LerpF(left.Y, right.Y, val));
        }
        public static Vector3 Lerp(Vector3 left, Vector3 right, double val, bool clamped = false, LerpType type = LerpType.Linear, params double[] pars) {
            val = NewLerpValue(val, clamped, type, pars);
            return new(LerpF(left.X, right.X, val), LerpF(left.Y, right.Y, val), LerpF(left.Z, right.Z, val));
        }
        public static Vector4 Lerp(Vector4 left, Vector4 right, double val, bool clamped = false, LerpType type = LerpType.Linear, params double[] pars) {
            val = NewLerpValue(val, clamped, type, pars);
            return new(LerpF(left.X, right.X, val), LerpF(left.Y, right.Y, val), LerpF(left.Z, right.Z, val), LerpF(left.W, right.W, val));
        }
        #endregion
        #region LerpWithLerpModel
        public static float Lerp(float left, float right, float val, bool clamped, IFloatLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return left * (1 - val) + right * val;
        }
        public static int Lerp(int left, int right, float val, bool clamped, IFloatLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return (int)(left * (1 - val) + right * val);
        }
        public static Vector2 Lerp(Vector2 left, Vector2 right, float val, bool clamped, IFloatLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return left * (1 - val) + right * val;
        }
        public static Vector3 Lerp(Vector3 left, Vector3 right, float val, bool clamped, IFloatLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return left * (1 - val) + right * val;
        }
        public static Vector4 Lerp(Vector4 left, Vector4 right, float val, bool clamped, IFloatLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return left * (1 - val) + right * val;
        }

        public static double Lerp(double left, double right, double val, bool clamped, IDoubleLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return left * (1 - val) + right * val;
        }
        public static float Lerp(float left, float right, double val, bool clamped, IDoubleLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return (float)(left * (1 - val) + right * val);
        }
        public static int Lerp(int left, int right, double val, bool clamped, IDoubleLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return (int)(left * (1 - val) + right * val);
        }
        public static Vector2 Lerp(Vector2 left, Vector2 right, double val, bool clamped, IDoubleLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return new(LerpF(left.X, right.X, val), LerpF(left.Y, right.Y, val));
        }
        public static Vector3 Lerp(Vector3 left, Vector3 right, double val, bool clamped, IDoubleLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return new(LerpF(left.X, right.X, val), LerpF(left.Y, right.Y, val), LerpF(left.Z, right.Z, val));
        }
        public static Vector4 Lerp(Vector4 left, Vector4 right, double val, bool clamped, IDoubleLerpModel model) {
            val = model.NewLerpValue(val, clamped);
            return new(LerpF(left.X, right.X, val), LerpF(left.Y, right.Y, val), LerpF(left.Z, right.Z, val), LerpF(left.W, right.W, val));
        }
        #endregion
    }

    partial class TigerClasses {
        public static class LerpModels {
            #region FloatLerpModel
            public interface IFloatLerpModel {
                public float NewLerpValue(float value, bool clamped);
                public static IFloatLerpModel Default { get; } = new FloatLerpModelByLinear();
            }
            public class FloatLerpModelByStay : IFloatLerpModel {
                public static FloatLerpModelByStay Shared { get; } = new();
                public static IFloatLerpModel Create() => Shared;
                public float NewLerpValue(float value, bool clamped) => value < 1 ? 0 : 1;
                public static float StaticNewLerpValue(float value) => value < 1 ? 0 : 1;
            }
            public abstract class BaseFloatLerpModel : IFloatLerpModel {
                public float NewLerpValue(float value, bool clamped) {
                    if (HandleClamped(ref value, clamped)) {
                        return value;
                    }
                    return NewLerpValue(value);
                }
                protected abstract float NewLerpValue(float value);
                public static bool HandleClamped(ref float value, bool clamped) {
                    if (clamped) {
                        if (value <= 0) {
                            value = 0;
                            return true;
                        }
                        if (value >= 1) {
                            value = 1;
                            return true;
                        }
                    }
                    if (value == 0) {
                        value = 0;
                        return true;
                    }
                    if (value == 1) {
                        value = 1;
                        return true;
                    }
                    return false;
                }
            }
            public class FloatLerpModelByLinear : IFloatLerpModel {
                public static FloatLerpModelByLinear Shared { get; } = new();
                public static IFloatLerpModel Create() => Shared;
                public float NewLerpValue(float value, bool clamped) {
                    BaseFloatLerpModel.HandleClamped(ref value, clamped);
                    return value;
                }
                public static float StaticNewLerpValue(float value, bool clamped) {
                    BaseFloatLerpModel.HandleClamped(ref value, clamped);
                    return value;
                }
            }
            public class FloatLerpModelByQuadratic : BaseFloatLerpModel {
                public static FloatLerpModelByQuadratic Shared { get; } = new();
                public static IFloatLerpModel Create(float pole) => pole == 0.5f ? FloatLerpModelByStay.Shared : new FloatLerpModelByQuadraticFast(pole);
                public FloatLerpModelByQuadratic(float pole) => Pole = pole;
                public FloatLerpModelByQuadratic() { }
                public float Pole { get => poleM2 / 2; set => poleM2 = value * 2; }
                private float poleM2;
                protected override float NewLerpValue(float value) {
                    if (poleM2 == 1) {
                        return value < 1 ? 0 : 1;
                    }
                    return value * (value - poleM2) / (1 - poleM2);
                }
                public static float StaticNewLerpValue(float value, bool clamped, float pole) {
                    if (HandleClamped(ref value, clamped)) {
                        return value;
                    }
                    var poleM2 = pole * 2;
                    if (poleM2 == 1) {
                        return value < 1 ? 0 : 1;
                    }
                    return value * (value - poleM2) / (1 - poleM2);
                }
                internal static float StaticNewLerpValue(float value, bool clamped, float[] pars) {
                    if (pars.Length <= 0) {
                        throw new TargetParameterCountException("pars count not enough");
                    }
                    return StaticNewLerpValue(value, clamped, pars[0]);
                }
            }
            private class FloatLerpModelByQuadraticFast : BaseFloatLerpModel {
                public FloatLerpModelByQuadraticFast(float pole) => denominator = 1 - (poleM2 = pole * 2);
                public FloatLerpModelByQuadraticFast(float poleM2, float denominator) => (this.poleM2, this.denominator) = (poleM2, denominator);
                private readonly float poleM2;
                private readonly float denominator;
                protected override float NewLerpValue(float value) => value * (value - poleM2) / denominator;
            }
            public class FloatLerpModelByCubic : BaseFloatLerpModel {
                public static FloatLerpModelByCubic Shared { get; } = new();
                public static IFloatLerpModel Create(float pole1, float pole2) {
                    var addM3D2 = (pole1 + pole2) * 1.5f;
                    var mulM3 = pole1 * pole2 * 3;
                    var denominator = 1 - addM3D2 + mulM3;
                    return denominator == 0 ? FloatLerpModelByStay.Shared : new FloatLerpModelByCubicFast(addM3D2, mulM3, denominator);
                }
                public FloatLerpModelByCubic(float pole1, float pole2) => (Pole1, Pole2) = (pole1, pole2);
                public FloatLerpModelByCubic() { }
                public float Pole1;
                public float Pole2;
                protected override float NewLerpValue(float value) {
                    var addM3D2 = (Pole1 + Pole2) * 1.5f;
                    var mulM3 = Pole1 * Pole2 * 3;
                    var denominator = 1 - addM3D2 + mulM3;
                    if (denominator == 0) {
                        return value < 1 ? 0 : 1;
                    }
                    return ((value - addM3D2) * value + mulM3) * value / denominator;
                }
                public static float StaticNewLerpValue(float value, bool clamped, float pole1, float pole2) {
                    if (HandleClamped(ref value, clamped)) {
                        return value;
                    }
                    var addM3D2 = (pole1 + pole2) * 1.5f;
                    var mulM3 = pole1 * pole2 * 3;
                    var denominator = 1 - addM3D2 + mulM3;
                    if (denominator == 0) {
                        return value < 1 ? 0 : 1;
                    }
                    return ((value - addM3D2) * value + mulM3) * value / denominator;
                }
                internal static float StaticNewLerpValue(float value, bool clamped, float[] pars) {
                    if (pars.Length <= 1) {
                        throw new TargetParameterCountException("pars count not enough");
                    }
                    return StaticNewLerpValue(value, clamped, pars[0], pars[1]);
                }
            }
            private class FloatLerpModelByCubicFast(float addM3D2, float mulM3, float denomicator) : BaseFloatLerpModel {
                protected override float NewLerpValue(float value)
                    => ((value - addM3D2) * value + mulM3) * value / denomicator;
            }
            public class FloatLerpModelByCubicByK : BaseFloatLerpModel {
                public static FloatLerpModelByCubicByK Shared { get; } = new();
                public static IFloatLerpModel Create(float k0, float k1) => new FloatLerpModelByCubicByK(k0, k1);
                public static IFloatLerpModel Create(float k0, float k1, float width)
                    => width == 0 ? FloatLerpModelByStay.Shared : new FloatLerpModelByCubicByK(k0 / width, k1 / width);
                public static IFloatLerpModel Create(float k0, float k1, float width, float height) => Create(k0 * height, k1 * height, width);
                public FloatLerpModelByCubicByK(float k0, float k1) => SetK(k0, k1);
                public FloatLerpModelByCubicByK() => (mda, mdb) = (-2, 3);

                public void SetK(float k0, float k1) {
                    this.k0 = k0;
                    mda = k0 + k1 - 2;
                    mdb = 1 - k0 - mda;
                }
                public float K0 {
                    get => k0;
                    set {
                        if (value != k0) {
                            var k1 = K1;
                            k0 = value;
                            mda = k0 + k1 - 2;
                            mdb = 1 - k0 - mda;
                        }
                    }
                }
                public float K1 {
                    get => mda - k0 + 2;
                    set {
                        mda = k0 + value - 2;
                        mdb = 1 - k0 - mda;
                    }
                }

                private float k0;
                // k1 = mda - k0 + 2
                private float mda; // = k0 + k1 - 2
                private float mdb; // = -2 * k0 - k1 + 3 = -2 * k0 - (2 - k0 + mda) + 3 = 1 - k0 - mda
                protected override float NewLerpValue(float value) {
                    // k0 * (x^3 - 2x^2 + x) + k1 * (x^3 - x^2) + (-2x^3 + 3x^2)
                    // 图像见 https://zuotu.91maths.com/#W3sidHlwZSI6MCwiZXEiOiIwKih4XjMtMip4XjIreCkrMCooeF4zLXheMiktMip4XjMrMyp4XjIiLCJjb2xvciI6IiMwMDAwMDAifSx7InR5cGUiOjEwMDAsImdyaWQiOlsiMSIsIjEiXX1d
                    return ((mda * value + mdb) * value + k0) * value;
                }
                public static float StaticNewLerpValue(float value, bool clamped, float k0, float k1) {
                    if (HandleClamped(ref value, clamped)) {
                        return value;
                    }
                    var mda = k0 + k1 - 2;
                    var mdb = 1 - k0 - mda;
                    return ((mda * value + mdb) * value + k0) * value;
                }
                public static float StaticNewLerpValue(float value, bool clamped, float k0, float k1, float width) {
                    if (width == 0) {
                        return value < 1 ? 0 : 1;
                    }
                    return StaticNewLerpValue(value, clamped, k0 / width, k1 / width);
                }
                public static float StaticNewLerpValue(float value, bool clamped, float k0, float k1, float width, float height)
                    => StaticNewLerpValue(value, clamped, k0 * height, k1 * height, width);
                internal static float StaticNewLerpValue(float value, bool clamped, float[] pars) => pars.Length switch {
                    <= 1 => throw new TargetParameterCountException("pars count not enough"),
                    >= 4 => StaticNewLerpValue(value, clamped, pars[0], pars[1], pars[2], pars[3]),
                    >= 3 => StaticNewLerpValue(value, clamped, pars[0], pars[1], pars[2]),
                    _ => StaticNewLerpValue(value, clamped, pars[0], pars[1]),
                };
            }
            public class FloatLerpModelBySin : BaseFloatLerpModel {
                public static FloatLerpModelBySin Shared { get; } = new();
                public static IFloatLerpModel Create(float phase1ByQuarterPeriod, float phase2ByQuarterPeriod)
                    => CreateByPhase(MathF.PI / 2 * phase1ByQuarterPeriod, MathF.PI / 2 * phase2ByQuarterPeriod);
                public static IFloatLerpModel CreateByPhase(float phase1, float phase2) {
                    var y1 = MathF.Sin(phase1);
                    var denomicator = MathF.Sin(phase2) - y1;
                    if (denomicator == 0) {
                        return FloatLerpModelByStay.Shared;
                    }
                    return new FloatLerpModelBySinFast(phase1, phase2 - phase1, y1, denomicator);
                }
                public FloatLerpModelBySin(float phase1ByQuarterPeriod, float phase2ByQuarterPeriod)
                    => SetPhaseByQuarterPeriod(phase1ByQuarterPeriod, phase2ByQuarterPeriod);
                private FloatLerpModelBySin() { } // all 0
                public void SetPhaseByQuarterPeriod(float phase1ByQuarterPeriod, float phase2ByQuarterPeriod)
                    => SetPhase(2 / MathF.PI * phase1ByQuarterPeriod, 2 / MathF.PI * phase2ByQuarterPeriod);
                public float Phase1ByQuarterPeriod {
                    get => 2 / MathF.PI * Phase1;
                    set => Phase2 = MathF.PI / 2 * value;
                }
                public float Phase2ByQuarterPeriod {
                    get => 2 / MathF.PI * Phase2;
                    set => Phase2 = MathF.PI / 2 * value;
                }
                public void SetPhase(float phase1, float phase2) {
                    this.phase1 = phase1;
                    phaseDelta = phase2 - phase1;
                    y1 = MathF.Sin(phase1);
                    denomicator = MathF.Sin(phase2) - y1;
                }
                public float Phase1 {
                    get => phase1;
                    set {
                        if (value == phase1) {
                            return;
                        }
                        var phase2 = Phase2;
                        phase1 = value;
                        Phase2 = phase2;
                    }
                }
                public float Phase2 {
                    get => phaseDelta + phase1;
                    set {
                        if (value == Phase2) {
                            return;
                        }
                        phaseDelta = value - phase1;
                        y1 = MathF.Sin(Phase1);
                        denomicator = MathF.Sin(value) - y1;
                    }
                }
                private float phase1;
                private float phaseDelta; // = phase2 - phase1
                private float y1; // = sin(phase1)
                private float denomicator; // = sin(phase2) - sin(phase1)
                protected override float NewLerpValue(float value) {
                    if (denomicator == 0) {
                        return value < 1 ? 0 : 1;
                    }
                    return (MathF.Sin(phase1 + value + phaseDelta) - y1) / denomicator;
                }
                public static float StaticNewLerpValue(float value, bool clamped, float phase1ByQuarterPeriod, float phase2ByQuarterPeriod) {
                    if (HandleClamped(ref value, clamped)) {
                        return value;
                    }
                    float phase1 = MathF.PI / 2 * phase1ByQuarterPeriod, phase2 = MathF.PI / 2 * phase2ByQuarterPeriod, phase = Lerp(phase1, phase2, value);
                    float y1 = MathF.Sin(phase1), y2 = MathF.Sin(phase2), y = (float)Math.Sin(phase);
                    if (y1 == y2) {
                        return value < 1 ? 1 : 0;
                    }
                    return (y - y1) / (y2 - y1);
                }
                internal static float StaticNewLerpValue(float value, bool clamped, float[] pars) {
                    if (pars.Length <= 1) {
                        throw new TargetParameterCountException("pars count not enough");
                    }
                    return StaticNewLerpValue(value, clamped, pars[0], pars[1]);
                }
            }
            private class FloatLerpModelBySinFast(float phase1, float phaseDelta, float y1, float denomicator) : BaseFloatLerpModel {
                protected override float NewLerpValue(float value)
                    => (MathF.Sin(phase1 + value * phaseDelta) - y1) / denomicator;
            }
            #endregion
            #region DoubleLerpModel
            public interface IDoubleLerpModel {
                public double NewLerpValue(double value, bool clamped);
                public static IDoubleLerpModel Default { get; } = new DoubleLerpModelByLinear();
            }
            public class DoubleLerpModelByStay : IDoubleLerpModel {
                public static DoubleLerpModelByStay Shared { get; } = new();
                public static IDoubleLerpModel Create() => Shared;
                public double NewLerpValue(double value, bool clamped) => value < 1 ? 0 : 1;
                public static double StaticNewLerpValue(double value) => value < 1 ? 0 : 1;
            }
            public abstract class BaseDoubleLerpModel : IDoubleLerpModel {
                public double NewLerpValue(double value, bool clamped) {
                    if (HandleClamped(ref value, clamped)) {
                        return value;
                    }
                    return NewLerpValue(value);
                }
                protected abstract double NewLerpValue(double value);
                public static bool HandleClamped(ref double value, bool clamped) {
                    if (clamped) {
                        if (value <= 0) {
                            value = 0;
                            return true;
                        }
                        if (value >= 1) {
                            value = 1;
                            return true;
                        }
                    }
                    if (value == 0) {
                        value = 0;
                        return true;
                    }
                    if (value == 1) {
                        value = 1;
                        return true;
                    }
                    return false;
                }
            }
            public class DoubleLerpModelByLinear : IDoubleLerpModel {
                public static DoubleLerpModelByLinear Shared { get; } = new();
                public static IDoubleLerpModel Create() => Shared;
                public double NewLerpValue(double value, bool clamped) {
                    BaseDoubleLerpModel.HandleClamped(ref value, clamped);
                    return value;
                }
                public static double StaticNewLerpValue(double value, bool clamped) {
                    BaseDoubleLerpModel.HandleClamped(ref value, clamped);
                    return value;
                }
            }
            public class DoubleLerpModelByQuadratic : BaseDoubleLerpModel {
                public static DoubleLerpModelByQuadratic Shared { get; } = new();
                public static IDoubleLerpModel Create(double pole) => pole == 0.5 ? DoubleLerpModelByStay.Shared : new DoubleLerpModelByQuadraticFast(pole);
                public DoubleLerpModelByQuadratic(double pole) => Pole = pole;
                public DoubleLerpModelByQuadratic() { }
                public double Pole { get => poleM2 / 2; set => poleM2 = value * 2; }
                private double poleM2;
                protected override double NewLerpValue(double value) {
                    if (poleM2 == 1) {
                        return value < 1 ? 0 : 1;
                    }
                    return value * (value - poleM2) / (1 - poleM2);
                }
                public static double StaticNewLerpValue(double value, bool clamped, double pole) {
                    if (HandleClamped(ref value, clamped)) {
                        return value;
                    }
                    var poleM2 = pole * 2;
                    if (poleM2 == 1) {
                        return value < 1 ? 0 : 1;
                    }
                    return value * (value - poleM2) / (1 - poleM2);
                }
                internal static double StaticNewLerpValue(double value, bool clamped, double[] pars) {
                    if (pars.Length <= 0) {
                        throw new TargetParameterCountException("pars count not enough");
                    }
                    return StaticNewLerpValue(value, clamped, pars[0]);
                }
            }
            private class DoubleLerpModelByQuadraticFast : BaseDoubleLerpModel {
                public DoubleLerpModelByQuadraticFast(double pole) => denominator = 1 - (poleM2 = pole * 2);
                public DoubleLerpModelByQuadraticFast(double poleM2, double denominator) => (this.poleM2, this.denominator) = (poleM2, denominator);
                private readonly double poleM2;
                private readonly double denominator;
                protected override double NewLerpValue(double value) => value * (value - poleM2) / denominator;
            }
            public class DoubleLerpModelByCubic : BaseDoubleLerpModel {
                public static DoubleLerpModelByCubic Shared { get; } = new();
                public static IDoubleLerpModel Create(double pole1, double pole2) {
                    var addM3D2 = (pole1 + pole2) * 1.5;
                    var mulM3 = pole1 * pole2 * 3;
                    var denominator = 1 - addM3D2 + mulM3;
                    return denominator == 0 ? DoubleLerpModelByStay.Shared : new DoubleLerpModelByCubicFast(addM3D2, mulM3, denominator);
                }
                public DoubleLerpModelByCubic(double pole1, double pole2) => (Pole1, Pole2) = (pole1, pole2);
                public DoubleLerpModelByCubic() { }
                public double Pole1;
                public double Pole2;
                protected override double NewLerpValue(double value) {
                    var addM3D2 = (Pole1 + Pole2) * 1.5;
                    var mulM3 = Pole1 * Pole2 * 3;
                    var denominator = 1 - addM3D2 + mulM3;
                    if (denominator == 0) {
                        return value < 1 ? 0 : 1;
                    }
                    return ((value - addM3D2) * value + mulM3) * value / denominator;
                }
                public static double StaticNewLerpValue(double value, bool clamped, double pole1, double pole2) {
                    if (HandleClamped(ref value, clamped)) {
                        return value;
                    }
                    var addM3D2 = (pole1 + pole2) * 1.5;
                    var mulM3 = pole1 * pole2 * 3;
                    var denominator = 1 - addM3D2 + mulM3;
                    if (denominator == 0) {
                        return value < 1 ? 0 : 1;
                    }
                    return ((value - addM3D2) * value + mulM3) * value / denominator;
                }
                internal static double StaticNewLerpValue(double value, bool clamped, double[] pars) {
                    if (pars.Length <= 1) {
                        throw new TargetParameterCountException("pars count not enough");
                    }
                    return StaticNewLerpValue(value, clamped, pars[0], pars[1]);
                }
            }
            private class DoubleLerpModelByCubicFast(double addM3D2, double mulM3, double denomicator) : BaseDoubleLerpModel {
                protected override double NewLerpValue(double value)
                    => ((value - addM3D2) * value + mulM3) * value / denomicator;
            }
            public class DoubleLerpModelByCubicByK : BaseDoubleLerpModel {
                public static DoubleLerpModelByCubicByK Shared { get; } = new();
                public static IDoubleLerpModel Create(double k0, double k1) => new DoubleLerpModelByCubicByK(k0, k1);
                public static IDoubleLerpModel Create(double k0, double k1, double width)
                    => width == 0 ? DoubleLerpModelByStay.Shared : new DoubleLerpModelByCubicByK(k0 / width, k1 / width);
                public static IDoubleLerpModel Create(double k0, double k1, double width, double height) => Create(k0 * height, k1 * height, width);
                public DoubleLerpModelByCubicByK(double k0, double k1) => SetK(k0, k1);
                public DoubleLerpModelByCubicByK() => (mda, mdb) = (-2, 3);

                public void SetK(double k0, double k1) {
                    this.k0 = k0;
                    mda = k0 + k1 - 2;
                    mdb = 1 - k0 - mda;
                }
                public double K0 {
                    get => k0;
                    set {
                        if (value != k0) {
                            var k1 = K1;
                            k0 = value;
                            mda = k0 + k1 - 2;
                            mdb = 1 - k0 - mda;
                        }
                    }
                }
                public double K1 {
                    get => mda - k0 + 2;
                    set {
                        mda = k0 + value - 2;
                        mdb = 1 - k0 - mda;
                    }
                }

                private double k0;
                // k1 = mda - k0 + 2
                private double mda; // = k0 + k1 - 2
                private double mdb; // = -2 * k0 - k1 + 3 = -2 * k0 - (2 - k0 + mda) + 3 = 1 - k0 - mda
                protected override double NewLerpValue(double value) {
                    // k0 * (x^3 - 2x^2 + x) + k1 * (x^3 - x^2) + (-2x^3 + 3x^2)
                    // 图像见 https://zuotu.91maths.com/#W3sidHlwZSI6MCwiZXEiOiIwKih4XjMtMip4XjIreCkrMCooeF4zLXheMiktMip4XjMrMyp4XjIiLCJjb2xvciI6IiMwMDAwMDAifSx7InR5cGUiOjEwMDAsImdyaWQiOlsiMSIsIjEiXX1d
                    return ((mda * value + mdb) * value + k0) * value;
                }
                public static double StaticNewLerpValue(double value, bool clamped, double k0, double k1) {
                    if (HandleClamped(ref value, clamped)) {
                        return value;
                    }
                    var mda = k0 + k1 - 2;
                    var mdb = 1 - k0 - mda;
                    return ((mda * value + mdb) * value + k0) * value;
                }
                public static double StaticNewLerpValue(double value, bool clamped, double k0, double k1, double width) {
                    if (width == 0) {
                        return value < 1 ? 0 : 1;
                    }
                    return StaticNewLerpValue(value, clamped, k0 / width, k1 / width);
                }
                public static double StaticNewLerpValue(double value, bool clamped, double k0, double k1, double width, double height)
                    => StaticNewLerpValue(value, clamped, k0 * height, k1 * height, width);
                internal static double StaticNewLerpValue(double value, bool clamped, double[] pars) => pars.Length switch {
                    <= 1 => throw new TargetParameterCountException("pars count not enough"),
                    >= 4 => StaticNewLerpValue(value, clamped, pars[0], pars[1], pars[2], pars[3]),
                    >= 3 => StaticNewLerpValue(value, clamped, pars[0], pars[1], pars[2]),
                    _ => StaticNewLerpValue(value, clamped, pars[0], pars[1]),
                };
            }
            public class DoubleLerpModelBySin : BaseDoubleLerpModel {
                public static DoubleLerpModelBySin Shared { get; } = new();
                public static IDoubleLerpModel Create(double phase1ByQuarterPeriod, double phase2ByQuarterPeriod)
                    => CreateByPhase(Math.PI / 2 * phase1ByQuarterPeriod, Math.PI / 2 * phase2ByQuarterPeriod);
                public static IDoubleLerpModel CreateByPhase(double phase1, double phase2) {
                    var y1 = Math.Sin(phase1);
                    var denomicator = Math.Sin(phase2) - y1;
                    if (denomicator == 0) {
                        return DoubleLerpModelByStay.Shared;
                    }
                    return new DoubleLerpModelBySinFast(phase1, phase2 - phase1, y1, denomicator);
                }
                public DoubleLerpModelBySin(double phase1ByQuarterPeriod, double phase2ByQuarterPeriod)
                    => SetPhaseByQuarterPeriod(phase1ByQuarterPeriod, phase2ByQuarterPeriod);
                private DoubleLerpModelBySin() { } // all 0
                public void SetPhaseByQuarterPeriod(double phase1ByQuarterPeriod, double phase2ByQuarterPeriod)
                    => SetPhase(2 / Math.PI * phase1ByQuarterPeriod, 2 / Math.PI * phase2ByQuarterPeriod);
                public double Phase1ByQuarterPeriod {
                    get => 2 / Math.PI * Phase1;
                    set => Phase2 = Math.PI / 2 * value;
                }
                public double Phase2ByQuarterPeriod {
                    get => 2 / Math.PI * Phase2;
                    set => Phase2 = Math.PI / 2 * value;
                }
                public void SetPhase(double phase1, double phase2) {
                    this.phase1 = phase1;
                    phaseDelta = phase2 - phase1;
                    y1 = Math.Sin(phase1);
                    denomicator = Math.Sin(phase2) - y1;
                }
                public double Phase1 {
                    get => phase1;
                    set {
                        if (value == phase1) {
                            return;
                        }
                        var phase2 = Phase2;
                        phase1 = value;
                        Phase2 = phase2;
                    }
                }
                public double Phase2 {
                    get => phaseDelta + phase1;
                    set {
                        if (value == Phase2) {
                            return;
                        }
                        phaseDelta = value - phase1;
                        y1 = Math.Sin(Phase1);
                        denomicator = Math.Sin(value) - y1;
                    }
                }
                private double phase1;
                private double phaseDelta; // = phase2 - phase1
                private double y1; // = sin(phase1)
                private double denomicator; // = sin(phase2) - sin(phase1)
                protected override double NewLerpValue(double value) {
                    if (denomicator == 0) {
                        return value < 1 ? 0 : 1;
                    }
                    return (Math.Sin(phase1 + value + phaseDelta) - y1) / denomicator;
                }
                public static double StaticNewLerpValue(double value, bool clamped, double phase1ByQuarterPeriod, double phase2ByQuarterPeriod) {
                    if (HandleClamped(ref value, clamped)) {
                        return value;
                    }
                    double phase1 = Math.PI / 2 * phase1ByQuarterPeriod, phase2 = Math.PI / 2 * phase2ByQuarterPeriod, phase = Lerp(phase1, phase2, value);
                    double y1 = Math.Sin(phase1), y2 = Math.Sin(phase2), y = (double)Math.Sin(phase);
                    if (y1 == y2) {
                        return value < 1 ? 1 : 0;
                    }
                    return (y - y1) / (y2 - y1);
                }
                internal static double StaticNewLerpValue(double value, bool clamped, double[] pars) {
                    if (pars.Length <= 1) {
                        throw new TargetParameterCountException("pars count not enough");
                    }
                    return StaticNewLerpValue(value, clamped, pars[0], pars[1]);
                }
            }
            private class DoubleLerpModelBySinFast(double phase1, double phaseDelta, double y1, double denomicator) : BaseDoubleLerpModel {
                protected override double NewLerpValue(double value)
                    => (Math.Sin(phase1 + value * phaseDelta) - y1) / denomicator;
            }
            #endregion
        }
    }
}
