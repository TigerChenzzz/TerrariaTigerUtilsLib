using System;
using System.Diagnostics.CodeAnalysis;

#if XNA
using XNAVector2 = Microsoft.Xna.Framework.Vector2;
using XNAVector3 = Microsoft.Xna.Framework.Vector3;
using XNAVector4 = Microsoft.Xna.Framework.Vector4;
using XNAVector2I = Microsoft.Xna.Framework.Point;
#endif
#if GODOT
using GDVector2 = Godot.Vector2;
using GDVector3 = Godot.Vector3;
using GDVector4 = Godot.Vector4;
using GDVector2I = Godot.Vector2I;
using GDVector3I = Godot.Vector3I;
using GDVector4I = Godot.Vector4I;
#endif

namespace TigerUtilsLib;

partial class TigerClasses {
    public struct AnyVector2 : IEquatable<AnyVector2> {
        #region 字段与属性
        public float X;
        public float Y;
        public float this[int index] {
            readonly get {
                return index switch {
                    0 => X,
                    1 => Y,
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }
            set {
                switch (index) {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #region XX
        public readonly AnyVector2 XX => new(X, X);
        public AnyVector2 XY { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2 YX { readonly get => new(Y, X); set => (Y, X) = value; }
        public readonly AnyVector2 YY => new(Y, Y);
        #endregion
        #region XXX
        public readonly AnyVector3 XXX => new(X, X, X);
        public readonly AnyVector3 XXY => new(X, X, Y);
        public readonly AnyVector3 XYX => new(X, Y, X);
        public readonly AnyVector3 XYY => new(X, Y, Y);

        public readonly AnyVector3 YXX => new(Y, X, X);
        public readonly AnyVector3 YXY => new(Y, X, Y);
        public readonly AnyVector3 YYX => new(Y, Y, X);
        public readonly AnyVector3 YYY => new(Y, Y, Y);
        #endregion
        #region XXXX
        public readonly AnyVector4 XXXX => new(X, X, X, X);
        public readonly AnyVector4 XXXY => new(X, X, X, Y);
        public readonly AnyVector4 XXYX => new(X, X, Y, X);
        public readonly AnyVector4 XXYY => new(X, X, Y, Y);
        public readonly AnyVector4 XYXX => new(X, Y, X, X);
        public readonly AnyVector4 XYXY => new(X, Y, X, Y);
        public readonly AnyVector4 XYYX => new(X, Y, Y, X);
        public readonly AnyVector4 XYYY => new(X, Y, Y, Y);

        public readonly AnyVector4 YXXX => new(Y, X, X, X);
        public readonly AnyVector4 YXXY => new(Y, X, X, Y);
        public readonly AnyVector4 YXYX => new(Y, X, Y, X);
        public readonly AnyVector4 YXYY => new(Y, X, Y, Y);
        public readonly AnyVector4 YYXX => new(Y, Y, X, X);
        public readonly AnyVector4 YYXY => new(Y, Y, X, Y);
        public readonly AnyVector4 YYYX => new(Y, Y, Y, X);
        public readonly AnyVector4 YYYY => new(Y, Y, Y, Y);
        #endregion
        #endregion
        #region 构造
        public AnyVector2(float x, float y) => (X, Y) = (x, y);
        public AnyVector2(float a) => (X, Y) = (a, a);
        public AnyVector2(AnyVector2 other) => (X, Y) = (other.X, other.Y);
        #endregion
        #region statics
        private static readonly AnyVector2 _zero = new(0, 0);
        private static readonly AnyVector2 _one = new(1, 1);
        private static readonly AnyVector2 _inf = new(float.PositiveInfinity, float.PositiveInfinity);
        private static readonly AnyVector2 _unitX = new(1, 0);
        private static readonly AnyVector2 _unitY = new(0, 1);
        public static AnyVector2 Zero => _zero;
        public static AnyVector2 One => _one;
        public static AnyVector2 Inf => _inf;
        public static AnyVector2 UnitX => _unitX;
        public static AnyVector2 UnitY => _unitY;
        #endregion
        #region 方法
        public readonly void Deconstruct(out float x, out float y) => (x, y) = (X, Y);
        #region 特殊判断
        public readonly bool IsFinite() => float.IsFinite(X) && float.IsFinite(Y);
        public readonly bool HasNaNs() => float.IsNaN(X) || float.IsNaN(Y);
        public readonly bool IsNormalized() => LengthSquared() == 1;
        #endregion
        #region 长度相关 (长度, 距离, 标准化)
        public readonly float LengthSquared() => X * X + Y * Y;
        public readonly float Length() => MathF.Sqrt(X * X + Y * Y);
        public readonly float DistanceSquaredTo(AnyVector2 vector) => (this - vector).LengthSquared();
        public readonly float DistanceTo(AnyVector2 vector) => (this - vector).Length();
        public void ClampDistance(AnyVector2 origin, float distance) {
            if (distance <= 0) {
                this = origin;
            }
            if (DistanceSquaredTo(origin) > distance * distance) {
                this = origin + (this - origin).Normalized() * distance;
            }
        }
        public readonly AnyVector2 ClampedDistance(AnyVector2 origin, float distance)
            => distance <= 0 ? origin : DistanceSquaredTo(origin) <= distance * distance ? this
                : origin + (this - origin).Normalized() * distance;
        public void Normalize() {
            var lengthSquared = LengthSquared();
            if (lengthSquared == 0) {
                X = Y = 0;
                return;
            }
            var length = MathF.Sqrt(lengthSquared);
            this /= length;
        }
        public void SafeNormalize(AnyVector2 defaultValue = default) {
            if (!IsFinite()) {
                this = defaultValue;
                return;
            }
            var lengthSquared = LengthSquared();
            if (lengthSquared == 0) {
                this = defaultValue;
                return;
            }
            var length = MathF.Sqrt(lengthSquared);
            this /= length;
        }
        public readonly AnyVector2 Normalized() {
            var result = this;
            result.Normalize();
            return result;
        }
        public readonly AnyVector2 SafeNormalized(AnyVector2 defaultValue = default) {
            var result = this;
            result.SafeNormalize(defaultValue);
            return result;
        }
        public static float DistanceSquared(AnyVector2 left, AnyVector2 right) => left.DistanceSquaredTo(right);
        public static float Distance(AnyVector2 left, AnyVector2 right) => left.DistanceTo(right);
        public static AnyVector2 Normalized(AnyVector2 vec) => vec.Normalized();
        public static AnyVector2 SafeNormalized(AnyVector2 vec, AnyVector2 defaultValue = default) => vec.SafeNormalized(defaultValue);
        #endregion
        #region 旋转
        public readonly float ToRotation() => MathF.Atan2(Y, X);
        public void Rotate(float radians, AnyVector2 center) {
            var rotation = radians.ToRotationAnyVector2();
            var delta = this - center;
            this = center + new AnyVector2(delta.Cross(rotation), delta.Dot(rotation));
        }
        public void Rotate(float radians) {
            var rotation = radians.ToRotationAnyVector2();
            this = new(Cross(rotation), Dot(rotation));
        }
        public AnyVector2 Rotated(float radians, AnyVector2 center) {
            var result = this;
            result.Rotated(radians, center);
            return result;
        }
        public AnyVector2 Rotated(float radians) {
            var result = this;
            result.Rotated(radians);
            return result;
        }
        #endregion
        #region 转化
        public readonly AnyVector2 Abs() => new(MathF.Abs(X), MathF.Abs(Y));
        public readonly AnyVector2 Ceil() => new(MathF.Ceiling(X), MathF.Ceiling(Y));
        public readonly AnyVector2 Floor() => new(MathF.Floor(X), MathF.Floor(Y));
        public readonly AnyVector2 Round() => new(MathF.Round(X), MathF.Round(Y));
        public readonly AnyVector2 Sign() => new(MathF.Sign(X), MathF.Sign(Y));
        public readonly AnyVector2I CeilI() => new((int)MathF.Ceiling(X), (int)MathF.Ceiling(Y));
        public readonly AnyVector2I FloorI() => new((int)MathF.Floor(X), (int)MathF.Floor(Y));
        public readonly AnyVector2I RoundI() => new((int)MathF.Round(X), (int)MathF.Round(Y));
        public readonly AnyVector2I SignI() => new(MathF.Sign(X), MathF.Sign(Y));
        public readonly AnyVector2 CopySign(float sign) => new(MathF.CopySign(X, sign), MathF.CopySign(Y, sign));
        public readonly AnyVector2 CopySign(AnyVector2 signv) => new(MathF.CopySign(X, signv.X), MathF.CopySign(Y, signv.Y));
        #endregion
        #region 限制
        public readonly bool IsBetweenO(AnyVector2 min, AnyVector2 max) => X.IsBetweenO(min.X, max.X) && Y.IsBetweenO(min.Y, max.Y);
        public readonly bool IsBetweenI(AnyVector2 min, AnyVector2 max) => X.IsBetweenI(min.X, max.X) && Y.IsBetweenI(min.Y, max.Y);
        public void ClampTo(AnyVector2 min, AnyVector2 max) { X.ClampTo(min.X, max.X); Y.ClampTo(min.Y, max.Y); }
        public readonly AnyVector2 Clamp(AnyVector2 min, AnyVector2 max) => new(X.Clamp(min.X, max.X), Y.Clamp(min.Y, max.Y));
        #endregion
        public readonly float Dot(AnyVector2 other) => X * other.X + Y * other.Y;
        public readonly float Cross(AnyVector2 other) => X * other.Y - Y * other.X;
        #endregion
        #region 运算
        #region 相等
        public static bool operator ==(AnyVector2 left, AnyVector2 right) => left.Equals(right);
        public static bool operator !=(AnyVector2 left, AnyVector2 right) => !left.Equals(right);
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is AnyVector2 other && Equals(other);
        public readonly bool Equals(AnyVector2 other) => X == other.X && Y == other.Y;
        public override readonly int GetHashCode() => HashCode.Combine(X, Y);
        #endregion
        #region operators
        public static AnyVector2 operator +(AnyVector2 left, AnyVector2 right) => new(left.X + right.X, left.Y + right.Y);
        public static AnyVector2 operator +(AnyVector2 vec) => vec;
        public static AnyVector2 operator -(AnyVector2 left, AnyVector2 right) => new(left.X - right.X, left.Y - right.Y);
        public static AnyVector2 operator -(AnyVector2 vec) => new(-vec.X, -vec.Y);
        public static AnyVector2 operator *(AnyVector2 vec, float scale) => new(vec.X * scale, vec.Y * scale);
        public static AnyVector2 operator *(float scale, AnyVector2 vec) => new(vec.X * scale, vec.Y * scale);
        public static AnyVector2 operator *(AnyVector2 left, AnyVector2 right) => new(left.X * right.X, left.Y * right.Y);
        public static AnyVector2 operator /(AnyVector2 vec, float divisor) => new(vec.X / divisor, vec.Y / divisor);
        public static AnyVector2 operator /(AnyVector2 vec, AnyVector2 divisorv) => new(vec.X / divisorv.X, vec.Y / divisorv.Y);
        public static AnyVector2 operator %(AnyVector2 vec, float divisor) => new(vec.X % divisor, vec.Y % divisor);
        public static AnyVector2 operator %(AnyVector2 vec, AnyVector2 divisorv) => new(vec.X % divisorv.X, vec.Y % divisorv.Y);
        public static bool operator <(AnyVector2 left, AnyVector2 right) => left.X != right.X ? left.X < right.X : left.Y < right.Y;
        public static bool operator >(AnyVector2 left, AnyVector2 right) => left.X != right.X ? left.X > right.X : left.Y > right.Y;
        public static bool operator <=(AnyVector2 left, AnyVector2 right) => left.X != right.X ? left.X < right.X : left.Y <= right.Y;
        public static bool operator >=(AnyVector2 left, AnyVector2 right) => left.X != right.X ? left.X > right.X : left.Y >= right.Y;
        #endregion
        #endregion
        #region 类型转换
        public override readonly string ToString() => $"({X}, {Y})";
        public readonly string ToString(string? format) => $"({X.ToString(format)}, {Y.ToString(format)})";
        public static implicit operator (float, float)(AnyVector2 vector) => (vector.X, vector.Y);
        public static implicit operator AnyVector2((float, float) tuple) => new(tuple.Item1, tuple.Item2);
#if XNA
        public static implicit operator XNAVector2(AnyVector2 vector) => new(vector.X, vector.Y);
        public static implicit operator AnyVector2(XNAVector2 vector) => new(vector.X, vector.Y);
        public static explicit operator XNAVector3(AnyVector2 vector) => new(vector.X, vector.Y, 0);
        public static explicit operator AnyVector2(XNAVector3 vector) => new(vector.X, vector.Y);
        public static explicit operator XNAVector4(AnyVector2 vector) => new(vector.X, vector.Y, 0, 0);
        public static explicit operator AnyVector2(XNAVector4 vector) => new(vector.X, vector.Y);
        public static explicit operator XNAVector2I(AnyVector2 vector) => new((int)vector.X, (int)vector.Y);
        public static implicit operator AnyVector2(XNAVector2I vector) => new(vector.X, vector.Y);
#endif
#if GODOT
        public static implicit operator GDVector2(AnyVector2 vector) => new(vector.X, vector.Y);
        public static implicit operator AnyVector2(GDVector2 vector) => new(vector.X, vector.Y);
        public static explicit operator GDVector3(AnyVector2 vector) => new(vector.X, vector.Y, 0);
        public static explicit operator AnyVector2(GDVector3 vector) => new(vector.X, vector.Y);
        public static explicit operator GDVector4(AnyVector2 vector) => new(vector.X, vector.Y, 0, 0);
        public static explicit operator AnyVector2(GDVector4 vector) => new(vector.X, vector.Y);
        public static explicit operator GDVector2I(AnyVector2 vector) => new((int)vector.X, (int)vector.Y);
        public static implicit operator AnyVector2(GDVector2I vector) => new(vector.X, vector.Y);
        public static explicit operator GDVector3I(AnyVector2 vector) => new((int)vector.X, (int)vector.Y, 0);
        public static explicit operator AnyVector2(GDVector3I vector) => new(vector.X, vector.Y);
        public static explicit operator GDVector4I(AnyVector2 vector) => new((int)vector.X, (int)vector.Y, 0, 0);
        public static explicit operator AnyVector2(GDVector4I vector) => new(vector.X, vector.Y);
#endif
        #endregion
    }
    public struct AnyVector2I : IEquatable<AnyVector2I> {
        #region 字段与属性
        public int X;
        public int Y;
        public int this[int index] {
            readonly get {
                return index switch {
                    0 => X,
                    1 => Y,
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }
            set {
                switch (index) {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #region XX
        public readonly AnyVector2I XX => new(X, X);
        public AnyVector2I XY { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2I YX { readonly get => new(Y, X); set => (Y, X) = value; }
        public readonly AnyVector2I YY => new(Y, Y);
        #endregion
        #region XXX
        public readonly AnyVector3I XXX => new(X, X, X);
        public readonly AnyVector3I XXY => new(X, X, Y);
        public readonly AnyVector3I XYX => new(X, Y, X);
        public readonly AnyVector3I XYY => new(X, Y, Y);

        public readonly AnyVector3I YXX => new(Y, X, X);
        public readonly AnyVector3I YXY => new(Y, X, Y);
        public readonly AnyVector3I YYX => new(Y, Y, X);
        public readonly AnyVector3I YYY => new(Y, Y, Y);
        #endregion
        #region XXXX
        public readonly AnyVector4I XXXX => new(X, X, X, X);
        public readonly AnyVector4I XXXY => new(X, X, X, Y);
        public readonly AnyVector4I XXYX => new(X, X, Y, X);
        public readonly AnyVector4I XXYY => new(X, X, Y, Y);
        public readonly AnyVector4I XYXX => new(X, Y, X, X);
        public readonly AnyVector4I XYXY => new(X, Y, X, Y);
        public readonly AnyVector4I XYYX => new(X, Y, Y, X);
        public readonly AnyVector4I XYYY => new(X, Y, Y, Y);

        public readonly AnyVector4I YXXX => new(Y, X, X, X);
        public readonly AnyVector4I YXXY => new(Y, X, X, Y);
        public readonly AnyVector4I YXYX => new(Y, X, Y, X);
        public readonly AnyVector4I YXYY => new(Y, X, Y, Y);
        public readonly AnyVector4I YYXX => new(Y, Y, X, X);
        public readonly AnyVector4I YYXY => new(Y, Y, X, Y);
        public readonly AnyVector4I YYYX => new(Y, Y, Y, X);
        public readonly AnyVector4I YYYY => new(Y, Y, Y, Y);
        #endregion
        #endregion
        #region 构造
        public AnyVector2I(int x, int y) => (X, Y) = (x, y);
        public AnyVector2I(int a) => (X, Y) = (a, a);
        public AnyVector2I(AnyVector2I other) => (X, Y) = (other.X, other.Y);
        #endregion
        #region statics
        private static readonly AnyVector2I _zero = new(0, 0);
        private static readonly AnyVector2I _one = new(1, 1);
        private static readonly AnyVector2I _minValue = new(int.MinValue, int.MinValue);
        private static readonly AnyVector2I _maxValue = new(int.MaxValue, int.MaxValue);
        private static readonly AnyVector2I _unitX = new(1, 0);
        private static readonly AnyVector2I _unitY = new(0, 1);
        public static AnyVector2I Zero => _zero;
        public static AnyVector2I One => _one;
        public static AnyVector2I MinValue => _minValue;
        public static AnyVector2I MaxValue => _maxValue;
        public static AnyVector2I UnitX => _unitX;
        public static AnyVector2I UnitY => _unitY;
        #endregion
        #region 方法
        public readonly void Deconstruct(out int x, out int y) => (x, y) = (X, Y);
        #region 长度相关 (长度, 距离)
        public readonly int LengthSquared() => X * X + Y * Y;
        public readonly long LongLengthSquared() => (long)X * X + (long)Y * Y;
        public readonly float Length() => MathF.Sqrt(X * X + Y * Y);
        public readonly float LongLength() => MathF.Sqrt((long)X * X + (long)Y * Y);
        public readonly int DistanceSquaredTo(AnyVector2I vector) => (this - vector).LengthSquared();
        public readonly long LongDistanceSquaredTo(AnyVector2I vector) => (this - vector).LongLengthSquared();
        public readonly float DistanceTo(AnyVector2I vector) => (this - vector).Length();
        public readonly float LongDistanceTo(AnyVector2I vector) => (this - vector).LongLength();
        public static int DistanceSquared(AnyVector2I left, AnyVector2I right) => left.DistanceSquaredTo(right);
        public static long LongDistanceSquared(AnyVector2I left, AnyVector2I right) => left.LongDistanceSquaredTo(right);
        public static float Distance(AnyVector2I left, AnyVector2I right) => left.DistanceTo(right);
        public static float LongDistance(AnyVector2I left, AnyVector2I right) => left.LongDistanceTo(right);
        #endregion
        #region 转化
        public readonly AnyVector2I Abs() => new(int.Abs(X), int.Abs(Y));
        public readonly AnyVector2I Sign() => new(int.Sign(X), int.Sign(Y));
        public readonly AnyVector2I CopySign(int sign) => new(int.CopySign(X, sign), int.CopySign(Y, sign));
        public readonly AnyVector2I CopySign(AnyVector2I signv) => new(int.CopySign(X, signv.X), int.CopySign(Y, signv.Y));
        #endregion
        #region 限制
        public readonly bool IsBetweenO(AnyVector2I min, AnyVector2I max) => X.IsBetweenO(min.X, max.X) && Y.IsBetweenO(min.Y, max.Y);
        public readonly bool IsBetweenI(AnyVector2I min, AnyVector2I max) => X.IsBetweenI(min.X, max.X) && Y.IsBetweenI(min.Y, max.Y);
        public void ClampTo(AnyVector2I min, AnyVector2I max) { X.ClampTo(min.X, max.X); Y.ClampTo(min.Y, max.Y); }
        public readonly AnyVector2I Clamp(AnyVector2I min, AnyVector2I max) => new(X.Clamp(min.X, max.X), Y.Clamp(min.Y, max.Y));
        #endregion
        public readonly int Dot(AnyVector2I other) => X * other.X + Y * other.Y;
        public readonly long LongDot(AnyVector2I other) => (long)X * other.X + (long)Y * other.Y;
        public readonly int Cross(AnyVector2I other) => X * other.Y - Y * other.X;
        public readonly long LongCross(AnyVector2I other) => (long)X * other.Y - (long)Y * other.X;
        #endregion
        #region 运算
        #region 相等
        public static bool operator ==(AnyVector2I left, AnyVector2I right) => left.Equals(right);
        public static bool operator !=(AnyVector2I left, AnyVector2I right) => !left.Equals(right);
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is AnyVector2I other && Equals(other);
        public readonly bool Equals(AnyVector2I other) => X == other.X && Y == other.Y;
        public override readonly int GetHashCode() => HashCode.Combine(X, Y);
        #endregion
        #region operators
        public static AnyVector2I operator +(AnyVector2I left, AnyVector2I right) => new(left.X + right.X, left.Y + right.Y);
        public static AnyVector2I operator +(AnyVector2I vec) => vec;
        public static AnyVector2I operator -(AnyVector2I left, AnyVector2I right) => new(left.X - right.X, left.Y - right.Y);
        public static AnyVector2I operator -(AnyVector2I vec) => new(-vec.X, -vec.Y);
        public static AnyVector2I operator *(AnyVector2I vec, int scale) => new(vec.X * scale, vec.Y * scale);
        public static AnyVector2I operator *(int scale, AnyVector2I vec) => new(vec.X * scale, vec.Y * scale);
        public static AnyVector2I operator *(AnyVector2I left, AnyVector2I right) => new(left.X * right.X, left.Y * right.Y);
        public static AnyVector2I operator /(AnyVector2I vec, int divisor) => new(vec.X / divisor, vec.Y / divisor);
        public static AnyVector2I operator /(AnyVector2I vec, AnyVector2I divisorv) => new(vec.X / divisorv.X, vec.Y / divisorv.Y);
        public static AnyVector2I operator %(AnyVector2I vec, int divisor) => new(vec.X % divisor, vec.Y % divisor);
        public static AnyVector2I operator %(AnyVector2I vec, AnyVector2I divisorv) => new(vec.X % divisorv.X, vec.Y % divisorv.Y);
        public static bool operator <(AnyVector2I left, AnyVector2I right) => left.X != right.X ? left.X < right.X : left.Y < right.Y;
        public static bool operator >(AnyVector2I left, AnyVector2I right) => left.X != right.X ? left.X > right.X : left.Y > right.Y;
        public static bool operator <=(AnyVector2I left, AnyVector2I right) => left.X != right.X ? left.X < right.X : left.Y <= right.Y;
        public static bool operator >=(AnyVector2I left, AnyVector2I right) => left.X != right.X ? left.X > right.X : left.Y >= right.Y;
        #endregion
        #endregion
        #region 类型转换
        public override readonly string ToString() => $"({X}, {Y})";
        public readonly string ToString(string? format) => $"({X.ToString(format)}, {Y.ToString(format)})";
        public static implicit operator (int, int)(AnyVector2I vector) => (vector.X, vector.Y);
        public static implicit operator AnyVector2I((int, int) tuple) => new(tuple.Item1, tuple.Item2);
        public static implicit operator AnyVector2(AnyVector2I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector2I(AnyVector2 vector) => new((int)vector.X, (int)vector.Y);
#if XNA
        public static implicit operator XNAVector2(AnyVector2I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector2I(XNAVector2 vector) => new((int)vector.X, (int)vector.Y);
        public static explicit operator XNAVector3(AnyVector2I vector) => new(vector.X, vector.Y, 0);
        public static explicit operator AnyVector2I(XNAVector3 vector) => new((int)vector.X, (int)vector.Y);
        public static explicit operator XNAVector4(AnyVector2I vector) => new(vector.X, vector.Y, 0, 0);
        public static explicit operator AnyVector2I(XNAVector4 vector) => new((int)vector.X, (int)vector.Y);
        public static implicit operator XNAVector2I(AnyVector2I vector) => new(vector.X, vector.Y);
        public static implicit operator AnyVector2I(XNAVector2I vector) => new(vector.X, vector.Y);
#endif
#if GODOT
        public static implicit operator GDVector2(AnyVector2I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector2I(GDVector2 vector) => new((int)vector.X);
        public static explicit operator GDVector3(AnyVector2I vector) => new(vector.X, vector.Y, 0);
        public static explicit operator AnyVector2I(GDVector3 vector) => new((int)vector.X, (int)vector.Y);
        public static explicit operator GDVector4(AnyVector2I vector) => new(vector.X, vector.Y, 0, 0);
        public static explicit operator AnyVector2I(GDVector4 vector) => new((int)vector.X, (int)vector.Y);
        public static implicit operator GDVector2I(AnyVector2I vector) => new(vector.X, vector.Y);
        public static implicit operator AnyVector2I(GDVector2I vector) => new(vector.X, vector.Y);
        public static explicit operator GDVector3I(AnyVector2I vector) => new(vector.X, vector.Y, 0);
        public static explicit operator AnyVector2I(GDVector3I vector) => new(vector.X, vector.Y);
        public static explicit operator GDVector4I(AnyVector2I vector) => new(vector.X, vector.Y, 0, 0);
        public static explicit operator AnyVector2I(GDVector4I vector) => new(vector.X, vector.Y);
#endif
        #endregion
    }
    public struct AnyVector3 : IEquatable<AnyVector3> {
        #region 字段与属性
        public float X;
        public float Y;
        public float Z;
        public float this[int index] {
            readonly get {
                return index switch {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }
            set {
                switch (index) {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #region XX
        public readonly AnyVector2 XX => new(X, X);
        public AnyVector2 XY { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2 XZ { readonly get => new(X, Z); set => (X, Z) = value; }
        public AnyVector2 YX { readonly get => new(Y, X); set => (Y, X) = value; }
        public readonly AnyVector2 YY => new(Y, Y);
        public AnyVector2 YZ { readonly get => new(Y, Z); set => (Y, Z) = value; }
        public AnyVector2 ZX { readonly get => new(Z, X); set => (Z, X) = value; }
        public AnyVector2 ZY { readonly get => new(Z, Y); set => (Z, Y) = value; }
        public readonly AnyVector2 ZZ => new(Z, Z);
        #endregion
        #region XXX
        public readonly AnyVector3 XXX => new(X, X, X);
        public readonly AnyVector3 XXY => new(X, X, Y);
        public readonly AnyVector3 XXZ => new(X, X, Z);
        public readonly AnyVector3 XYX => new(X, Y, X);
        public readonly AnyVector3 XYY => new(X, Y, Y);
        public AnyVector3 XYZ { readonly get => new(X, Y, Z); set => (X, Y, Z) = value; }
        public readonly AnyVector3 XZX => new(X, Z, X);
        public AnyVector3 XZY { readonly get => new(X, Z, Y); set => (X, Z, Y) = value; }
        public readonly AnyVector3 XZZ => new(X, Z, Z);
        
        public readonly AnyVector3 YXX => new(Y, X, X);
        public readonly AnyVector3 YXY => new(Y, X, Y);
        public AnyVector3 YXZ { readonly get => new(Y, X, Z); set => (Y, X, Z) = value; }
        public readonly AnyVector3 YYX => new(Y, Y, X);
        public readonly AnyVector3 YYY => new(Y, Y, Y);
        public readonly AnyVector3 YYZ => new(Y, Y, Z);
        public AnyVector3 YZX { readonly get => new(Y, Z, X); set => (Y, Z, X) = value; }
        public readonly AnyVector3 YZY => new(Y, Z, Y);
        public readonly AnyVector3 YZZ => new(Y, Z, Z);
        
        public readonly AnyVector3 ZXX => new(Z, X, X);
        public AnyVector3 ZXY { readonly get => new(Z, X, Y); set => (Z, X, Y) = value; }
        public readonly AnyVector3 ZXZ => new(Z, X, Z);
        public AnyVector3 ZYX { readonly get => new(Z, Y, X); set => (Z, Y, X) = value; }
        public readonly AnyVector3 ZYY => new(Z, Y, Y);
        public readonly AnyVector3 ZYZ => new(Z, Y, Z);
        public readonly AnyVector3 ZZX => new(Z, Z, X);
        public readonly AnyVector3 ZZY => new(Z, Z, Y);
        public readonly AnyVector3 ZZZ => new(Z, Z, Z);
        #endregion
        #region XXXX
        public readonly AnyVector4 XXXX => new(X, X, X, X);
        public readonly AnyVector4 XXXY => new(X, X, X, Y);
        public readonly AnyVector4 XXXZ => new(X, X, X, Z);
        public readonly AnyVector4 XXYX => new(X, X, Y, X);
        public readonly AnyVector4 XXYY => new(X, X, Y, Y);
        public readonly AnyVector4 XXYZ => new(X, X, Y, Z);
        public readonly AnyVector4 XXZX => new(X, X, Z, X);
        public readonly AnyVector4 XXZY => new(X, X, Z, Y);
        public readonly AnyVector4 XXZZ => new(X, X, Z, Z);
        public readonly AnyVector4 XYXX => new(X, Y, X, X);
        public readonly AnyVector4 XYXY => new(X, Y, X, Y);
        public readonly AnyVector4 XYXZ => new(X, Y, X, Z);
        public readonly AnyVector4 XYYX => new(X, Y, Y, X);
        public readonly AnyVector4 XYYY => new(X, Y, Y, Y);
        public readonly AnyVector4 XYYZ => new(X, Y, Y, Z);
        public readonly AnyVector4 XYZX => new(X, Y, Z, X);
        public readonly AnyVector4 XYZY => new(X, Y, Z, Y);
        public readonly AnyVector4 XYZZ => new(X, Y, Z, Z);
        public readonly AnyVector4 XZXX => new(X, Z, X, X);
        public readonly AnyVector4 XZXY => new(X, Z, X, Y);
        public readonly AnyVector4 XZXZ => new(X, Z, X, Z);
        public readonly AnyVector4 XZYX => new(X, Z, Y, X);
        public readonly AnyVector4 XZYY => new(X, Z, Y, Y);
        public readonly AnyVector4 XZYZ => new(X, Z, Y, Z);
        public readonly AnyVector4 XZZX => new(X, Z, Z, X);
        public readonly AnyVector4 XZZY => new(X, Z, Z, Y);
        public readonly AnyVector4 XZZZ => new(X, Z, Z, Z);
        
        public readonly AnyVector4 YXXX => new(Y, X, X, X);
        public readonly AnyVector4 YXXY => new(Y, X, X, Y);
        public readonly AnyVector4 YXXZ => new(Y, X, X, Z);
        public readonly AnyVector4 YXYX => new(Y, X, Y, X);
        public readonly AnyVector4 YXYY => new(Y, X, Y, Y);
        public readonly AnyVector4 YXYZ => new(Y, X, Y, Z);
        public readonly AnyVector4 YXZX => new(Y, X, Z, X);
        public readonly AnyVector4 YXZY => new(Y, X, Z, Y);
        public readonly AnyVector4 YXZZ => new(Y, X, Z, Z);
        public readonly AnyVector4 YYXX => new(Y, Y, X, X);
        public readonly AnyVector4 YYXY => new(Y, Y, X, Y);
        public readonly AnyVector4 YYXZ => new(Y, Y, X, Z);
        public readonly AnyVector4 YYYX => new(Y, Y, Y, X);
        public readonly AnyVector4 YYYY => new(Y, Y, Y, Y);
        public readonly AnyVector4 YYYZ => new(Y, Y, Y, Z);
        public readonly AnyVector4 YYZX => new(Y, Y, Z, X);
        public readonly AnyVector4 YYZY => new(Y, Y, Z, Y);
        public readonly AnyVector4 YYZZ => new(Y, Y, Z, Z);
        public readonly AnyVector4 YZXX => new(Y, Z, X, X);
        public readonly AnyVector4 YZXY => new(Y, Z, X, Y);
        public readonly AnyVector4 YZXZ => new(Y, Z, X, Z);
        public readonly AnyVector4 YZYX => new(Y, Z, Y, X);
        public readonly AnyVector4 YZYY => new(Y, Z, Y, Y);
        public readonly AnyVector4 YZYZ => new(Y, Z, Y, Z);
        public readonly AnyVector4 YZZX => new(Y, Z, Z, X);
        public readonly AnyVector4 YZZY => new(Y, Z, Z, Y);
        public readonly AnyVector4 YZZZ => new(Y, Z, Z, Z);
        
        public readonly AnyVector4 ZXXX => new(Z, X, X, X);
        public readonly AnyVector4 ZXXY => new(Z, X, X, Y);
        public readonly AnyVector4 ZXXZ => new(Z, X, X, Z);
        public readonly AnyVector4 ZXYX => new(Z, X, Y, X);
        public readonly AnyVector4 ZXYY => new(Z, X, Y, Y);
        public readonly AnyVector4 ZXYZ => new(Z, X, Y, Z);
        public readonly AnyVector4 ZXZX => new(Z, X, Z, X);
        public readonly AnyVector4 ZXZY => new(Z, X, Z, Y);
        public readonly AnyVector4 ZXZZ => new(Z, X, Z, Z);
        public readonly AnyVector4 ZYXX => new(Z, Y, X, X);
        public readonly AnyVector4 ZYXY => new(Z, Y, X, Y);
        public readonly AnyVector4 ZYXZ => new(Z, Y, X, Z);
        public readonly AnyVector4 ZYYX => new(Z, Y, Y, X);
        public readonly AnyVector4 ZYYY => new(Z, Y, Y, Y);
        public readonly AnyVector4 ZYYZ => new(Z, Y, Y, Z);
        public readonly AnyVector4 ZYZX => new(Z, Y, Z, X);
        public readonly AnyVector4 ZYZY => new(Z, Y, Z, Y);
        public readonly AnyVector4 ZYZZ => new(Z, Y, Z, Z);
        public readonly AnyVector4 ZZXX => new(Z, Z, X, X);
        public readonly AnyVector4 ZZXY => new(Z, Z, X, Y);
        public readonly AnyVector4 ZZXZ => new(Z, Z, X, Z);
        public readonly AnyVector4 ZZYX => new(Z, Z, Y, X);
        public readonly AnyVector4 ZZYY => new(Z, Z, Y, Y);
        public readonly AnyVector4 ZZYZ => new(Z, Z, Y, Z);
        public readonly AnyVector4 ZZZX => new(Z, Z, Z, X);
        public readonly AnyVector4 ZZZY => new(Z, Z, Z, Y);
        public readonly AnyVector4 ZZZZ => new(Z, Z, Z, Z);
        #endregion
        #endregion
        #region 构造
        public AnyVector3(float x, float y, float z) => (X, Y, Z) = (x, y, z);
        public AnyVector3(float a) => (X, Y, Z) = (a, a, a);
        public AnyVector3(AnyVector3 other) => (X, Y, Z) = (other.X, other.Y, other.Z);
        public AnyVector3(float x, AnyVector2 yz) => (X, Y, Z) = (x, yz.X, yz.Y);
        public AnyVector3(AnyVector2 xy, float z) => (X, Y, Z) = (xy.X, xy.Y, z);
        #endregion
        #region statics
        private static readonly AnyVector3 _zero = new(0, 0, 0);
        private static readonly AnyVector3 _one = new(1, 1, 1);
        private static readonly AnyVector3 _inf = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        private static readonly AnyVector3 _unitX = new(1, 0, 0);
        private static readonly AnyVector3 _unitY = new(0, 1, 0);
        private static readonly AnyVector3 _unitZ = new(0, 0, 1);
        public static AnyVector3 Zero => _zero;
        public static AnyVector3 One => _one;
        public static AnyVector3 Inf => _inf;
        public static AnyVector3 UnitX => _unitX;
        public static AnyVector3 UnitY => _unitY;
        public static AnyVector3 UnitZ => _unitZ;
        #endregion
        #region 方法
        public readonly void Deconstruct(out float x, out float y, out float z) => (x, y, z) = (X, Y, Z);
        #region 特殊判断
        public readonly bool IsFinite() => float.IsFinite(X) && float.IsFinite(Y) && float.IsFinite(Z);
        public readonly bool HasNaNs() => float.IsNaN(X) || float.IsNaN(Y) || float.IsNaN(Z);
        public readonly bool IsNormalized() => LengthSquared() == 1;
        #endregion
        #region 长度相关 (长度, 距离, 标准化)
        public readonly float LengthSquared() => X * X + Y * Y + Z * Z;
        public readonly float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z);
        public readonly float DistanceSquaredTo(AnyVector3 vector) => (this - vector).LengthSquared();
        public readonly float DistanceTo(AnyVector3 vector) => (this - vector).Length();
        public void ClampDistance(AnyVector3 origin, float distance) {
            if (distance <= 0) {
                this = origin;
            }
            if (DistanceSquaredTo(origin) > distance * distance) {
                this = origin + (this - origin).Normalized() * distance;
            }
        }
        public readonly AnyVector3 ClampedDistance(AnyVector3 origin, float distance)
            => distance <= 0 ? origin : DistanceSquaredTo(origin) <= distance * distance ? this
                : origin + (this - origin).Normalized() * distance;
        public void Normalize() {
            var lengthSquared = LengthSquared();
            if (lengthSquared == 0) {
                X = Y = 0;
                return;
            }
            var length = MathF.Sqrt(lengthSquared);
            this /= length;
        }
        public void SafeNormalize(AnyVector3 defaultValue = default) {
            if (!IsFinite()) {
                this = defaultValue;
                return;
            }
            var lengthSquared = LengthSquared();
            if (lengthSquared == 0) {
                this = defaultValue;
                return;
            }
            var length = MathF.Sqrt(lengthSquared);
            this /= length;
        }
        public readonly AnyVector3 Normalized() {
            var result = this;
            result.Normalize();
            return result;
        }
        public readonly AnyVector3 SafeNormalized(AnyVector3 defaultValue = default) {
            var result = this;
            result.SafeNormalize(defaultValue);
            return result;
        }
        public static float DistanceSquared(AnyVector3 left, AnyVector3 right) => left.DistanceSquaredTo(right);
        public static float Distance(AnyVector3 left, AnyVector3 right) => left.DistanceTo(right);
        public static AnyVector3 Normalized(AnyVector3 vec) => vec.Normalized();
        public static AnyVector3 SafeNormalized(AnyVector3 vec, AnyVector3 defaultValue = default) => vec.SafeNormalized(defaultValue);
        #endregion
        #region 转化
        public readonly AnyVector3 Abs() => new(MathF.Abs(X), MathF.Abs(Y), MathF.Abs(Z));
        public readonly AnyVector3 Ceil() => new(MathF.Ceiling(X), MathF.Ceiling(Y), MathF.Ceiling(Z));
        public readonly AnyVector3 Floor() => new(MathF.Floor(X), MathF.Floor(Y), MathF.Floor(Z));
        public readonly AnyVector3 Round() => new(MathF.Round(X), MathF.Round(Y), MathF.Round(Z));
        public readonly AnyVector3 Sign() => new(MathF.Sign(X), MathF.Sign(Y), MathF.Sign(Z));
        public readonly AnyVector3I CeilI() => new((int)MathF.Ceiling(X), (int)MathF.Ceiling(Y), (int)MathF.Ceiling(Z));
        public readonly AnyVector3I FloorI() => new((int)MathF.Floor(X), (int)MathF.Floor(Y), (int)MathF.Floor(Z));
        public readonly AnyVector3I RoundI() => new((int)MathF.Round(X), (int)MathF.Round(Y), (int)MathF.Round(Z));
        public readonly AnyVector3I SignI() => new(MathF.Sign(X), MathF.Sign(Y), MathF.Sign(Z));
        public readonly AnyVector3 CopySign(float sign) => new(MathF.CopySign(X, sign), MathF.CopySign(Y, sign), MathF.CopySign(Z, sign));
        public readonly AnyVector3 CopySign(AnyVector3 signv) => new(MathF.CopySign(X, signv.X), MathF.CopySign(Y, signv.Y), MathF.CopySign(Z, signv.Z));
        #endregion
        #region 限制
        public readonly bool IsBetweenO(AnyVector3 min, AnyVector3 max) => X.IsBetweenO(min.X, max.X) && Y.IsBetweenO(min.Y, max.Y) && Z.IsBetweenO(min.Z, max.Z);
        public readonly bool IsBetweenI(AnyVector3 min, AnyVector3 max) => X.IsBetweenI(min.X, max.X) && Y.IsBetweenI(min.Y, max.Y) && Z.IsBetweenI(min.Z, max.Z);
        public void ClampTo(AnyVector3 min, AnyVector3 max) { X.ClampTo(min.X, max.X); Y.ClampTo(min.Y, max.Y); Z.ClampTo(min.Z, max.Z); }
        public readonly AnyVector3 Clamp(AnyVector3 min, AnyVector3 max) => new(X.Clamp(min.X, max.X), Y.Clamp(min.Y, max.Y), Z.Clamp(min.Z, max.Z));
        #endregion
        public readonly float Dot(AnyVector3 other) => X * other.X + Y * other.Y + Z * other.Z;
        public readonly AnyVector3 Cross(AnyVector3 other) => new(Y * other.Z - Z * other.Y, Z * other.X - X * other.Y, X * other.Y - Y * other.X);
        #endregion
        #region 运算
        #region 相等
        public static bool operator ==(AnyVector3 left, AnyVector3 right) => left.Equals(right);
        public static bool operator !=(AnyVector3 left, AnyVector3 right) => !left.Equals(right);
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is AnyVector3 other && Equals(other);
        public readonly bool Equals(AnyVector3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);
        #endregion
        #region operators
        public static AnyVector3 operator +(AnyVector3 left, AnyVector3 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        public static AnyVector3 operator +(AnyVector3 vec) => vec;
        public static AnyVector3 operator -(AnyVector3 left, AnyVector3 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        public static AnyVector3 operator -(AnyVector3 vec) => new(-vec.X, -vec.Y, -vec.Z);
        public static AnyVector3 operator *(AnyVector3 vec, float scale) => new(vec.X * scale, vec.Y * scale, vec.Z * scale);
        public static AnyVector3 operator *(float scale, AnyVector3 vec) => new(vec.X * scale, vec.Y * scale, vec.Z * scale);
        public static AnyVector3 operator *(AnyVector3 left, AnyVector3 right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        public static AnyVector3 operator /(AnyVector3 vec, float divisor) => new(vec.X / divisor, vec.Y / divisor, vec.Z / divisor);
        public static AnyVector3 operator /(AnyVector3 vec, AnyVector3 divisorv) => new(vec.X / divisorv.X, vec.Y / divisorv.Y, vec.Z / divisorv.Z);
        public static AnyVector3 operator %(AnyVector3 vec, float divisor) => new(vec.X % divisor, vec.Y % divisor, vec.Z % divisor);
        public static AnyVector3 operator %(AnyVector3 vec, AnyVector3 divisorv) => new(vec.X % divisorv.X, vec.Y % divisorv.Y, vec.Z % divisorv.Z);
        public static bool operator <(AnyVector3 left, AnyVector3 right) => left.X != right.X ? left.X < right.X : left.Y != right.Y ? left.Y < right.Y : left.Z < right.Z;
        public static bool operator >(AnyVector3 left, AnyVector3 right) => left.X != right.X ? left.X > right.X : left.Y != right.Y ? left.Y > right.Y : left.Z > right.Z;
        public static bool operator <=(AnyVector3 left, AnyVector3 right) => left.X != right.X ? left.X < right.X : left.Y != right.Y ? left.Y < right.Y : left.Z <= right.Z;
        public static bool operator >=(AnyVector3 left, AnyVector3 right) => left.X != right.X ? left.X > right.X : left.Y != right.Y ? left.Y > right.Y : left.Z >= right.Z;
        #endregion
        #endregion
        #region 类型转换
        public override readonly string ToString() => $"({X}, {Y}, {Z})";
        public readonly string ToString(string? format) => $"({X.ToString(format)}, {Y.ToString(format)}, {Z.ToString(format)})";
        public static implicit operator (float, float, float)(AnyVector3 vector) => (vector.X, vector.Y, vector.Z);
        public static implicit operator AnyVector3((float, float, float) tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3);
        public static explicit operator AnyVector2(AnyVector3 vector4) => new(vector4.X, vector4.Y);
        public static explicit operator AnyVector3(AnyVector2 vector2) => new(vector2.X, vector2.Y, 0);
        public static explicit operator AnyVector2I(AnyVector3 vector4) => new((int)vector4.X, (int)vector4.Y);
        public static explicit operator AnyVector3(AnyVector2I vector2) => new(vector2.X, vector2.Y, 0);
#if XNA
        public static explicit operator XNAVector2(AnyVector3 vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector3(XNAVector2 vector) => new(vector.X, vector.Y, 0);
        public static implicit operator XNAVector3(AnyVector3 vector) => new(vector.X, vector.Y, vector.Z);
        public static implicit operator AnyVector3(XNAVector3 vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator XNAVector4(AnyVector3 vector) => new(vector.X, vector.Y, vector.Z, 0);
        public static explicit operator AnyVector3(XNAVector4 vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator XNAVector2I(AnyVector3 vector) => new((int)vector.X, (int)vector.Y);
        public static explicit operator AnyVector3(XNAVector2I vector) => new(vector.X, vector.Y, 0);
#endif
#if GODOT
        public static explicit operator GDVector2(AnyVector3 vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector3(GDVector2 vector) => new(vector.X, vector.Y, 0);
        public static implicit operator GDVector3(AnyVector3 vector) => new(vector.X, vector.Y, vector.Z);
        public static implicit operator AnyVector3(GDVector3 vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator GDVector4(AnyVector3 vector) => new(vector.X, vector.Y, vector.Z, 0);
        public static explicit operator AnyVector3(GDVector4 vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator GDVector2I(AnyVector3 vector) => new((int)vector.X, (int)vector.Y);
        public static explicit operator AnyVector3(GDVector2I vector) => new(vector.X, vector.Y, 0);
        public static explicit operator GDVector3I(AnyVector3 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z);
        public static implicit operator AnyVector3(GDVector3I vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator GDVector4I(AnyVector3 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z, 0);
        public static explicit operator AnyVector3(GDVector4I vector) => new(vector.X, vector.Y, vector.Z);
#endif
        #endregion
    }
    public struct AnyVector3I : IEquatable<AnyVector3I> {
        #region 字段与属性
        public int X;
        public int Y;
        public int Z;
        public int this[int index] {
            readonly get {
                return index switch {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }
            set {
                switch (index) {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #region XX
        public readonly AnyVector2I XX => new(X, X);
        public AnyVector2I XY { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2I XZ { readonly get => new(X, Z); set => (X, Z) = value; }

        public AnyVector2I YX { readonly get => new(Y, X); set => (Y, X) = value; }
        public readonly AnyVector2I YY => new(Y, Y);
        public AnyVector2I YZ { readonly get => new(Y, Z); set => (Y, Z) = value; }

        public AnyVector2I ZX { readonly get => new(Z, X); set => (Z, X) = value; }
        public AnyVector2I ZY { readonly get => new(Z, Y); set => (Z, Y) = value; }
        public readonly AnyVector2I ZZ => new(Z, Z);
        #endregion
        #region XXX
        public readonly AnyVector3I XXX => new(X, X, X);
        public readonly AnyVector3I XXY => new(X, X, Y);
        public readonly AnyVector3I XXZ => new(X, X, Z);
        public readonly AnyVector3I XYX => new(X, Y, X);
        public readonly AnyVector3I XYY => new(X, Y, Y);
        public AnyVector3I XYZ { readonly get => new(X, Y, Z); set => (X, Y, Z) = value; }
        public readonly AnyVector3I XZX => new(X, Z, X);
        public AnyVector3I XZY { readonly get => new(X, Z, Y); set => (X, Z, Y) = value; }
        public readonly AnyVector3I XZZ => new(X, Z, Z);
        
        public readonly AnyVector3I YXX => new(Y, X, X);
        public readonly AnyVector3I YXY => new(Y, X, Y);
        public AnyVector3I YXZ { readonly get => new(Y, X, Z); set => (Y, X, Z) = value; }
        public readonly AnyVector3I YYX => new(Y, Y, X);
        public readonly AnyVector3I YYY => new(Y, Y, Y);
        public readonly AnyVector3I YYZ => new(Y, Y, Z);
        public AnyVector3I YZX { readonly get => new(Y, Z, X); set => (Y, Z, X) = value; }
        public readonly AnyVector3I YZY => new(Y, Z, Y);
        public readonly AnyVector3I YZZ => new(Y, Z, Z);
        
        public readonly AnyVector3I ZXX => new(Z, X, X);
        public AnyVector3I ZXY { readonly get => new(Z, X, Y); set => (Z, X, Y) = value; }
        public readonly AnyVector3I ZXZ => new(Z, X, Z);
        public AnyVector3I ZYX { readonly get => new(Z, Y, X); set => (Z, Y, X) = value; }
        public readonly AnyVector3I ZYY => new(Z, Y, Y);
        public readonly AnyVector3I ZYZ => new(Z, Y, Z);
        public readonly AnyVector3I ZZX => new(Z, Z, X);
        public readonly AnyVector3I ZZY => new(Z, Z, Y);
        public readonly AnyVector3I ZZZ => new(Z, Z, Z);
        #endregion
        #region XXXX
        public readonly AnyVector4I XXXX => new(X, X, X, X);
        public readonly AnyVector4I XXXY => new(X, X, X, Y);
        public readonly AnyVector4I XXXZ => new(X, X, X, Z);
        public readonly AnyVector4I XXYX => new(X, X, Y, X);
        public readonly AnyVector4I XXYY => new(X, X, Y, Y);
        public readonly AnyVector4I XXYZ => new(X, X, Y, Z);
        public readonly AnyVector4I XXZX => new(X, X, Z, X);
        public readonly AnyVector4I XXZY => new(X, X, Z, Y);
        public readonly AnyVector4I XXZZ => new(X, X, Z, Z);
        public readonly AnyVector4I XYXX => new(X, Y, X, X);
        public readonly AnyVector4I XYXY => new(X, Y, X, Y);
        public readonly AnyVector4I XYXZ => new(X, Y, X, Z);
        public readonly AnyVector4I XYYX => new(X, Y, Y, X);
        public readonly AnyVector4I XYYY => new(X, Y, Y, Y);
        public readonly AnyVector4I XYYZ => new(X, Y, Y, Z);
        public readonly AnyVector4I XYZX => new(X, Y, Z, X);
        public readonly AnyVector4I XYZY => new(X, Y, Z, Y);
        public readonly AnyVector4I XYZZ => new(X, Y, Z, Z);
        public readonly AnyVector4I XZXX => new(X, Z, X, X);
        public readonly AnyVector4I XZXY => new(X, Z, X, Y);
        public readonly AnyVector4I XZXZ => new(X, Z, X, Z);
        public readonly AnyVector4I XZYX => new(X, Z, Y, X);
        public readonly AnyVector4I XZYY => new(X, Z, Y, Y);
        public readonly AnyVector4I XZYZ => new(X, Z, Y, Z);
        public readonly AnyVector4I XZZX => new(X, Z, Z, X);
        public readonly AnyVector4I XZZY => new(X, Z, Z, Y);
        public readonly AnyVector4I XZZZ => new(X, Z, Z, Z);
        
        public readonly AnyVector4I YXXX => new(Y, X, X, X);
        public readonly AnyVector4I YXXY => new(Y, X, X, Y);
        public readonly AnyVector4I YXXZ => new(Y, X, X, Z);
        public readonly AnyVector4I YXYX => new(Y, X, Y, X);
        public readonly AnyVector4I YXYY => new(Y, X, Y, Y);
        public readonly AnyVector4I YXYZ => new(Y, X, Y, Z);
        public readonly AnyVector4I YXZX => new(Y, X, Z, X);
        public readonly AnyVector4I YXZY => new(Y, X, Z, Y);
        public readonly AnyVector4I YXZZ => new(Y, X, Z, Z);
        public readonly AnyVector4I YYXX => new(Y, Y, X, X);
        public readonly AnyVector4I YYXY => new(Y, Y, X, Y);
        public readonly AnyVector4I YYXZ => new(Y, Y, X, Z);
        public readonly AnyVector4I YYYX => new(Y, Y, Y, X);
        public readonly AnyVector4I YYYY => new(Y, Y, Y, Y);
        public readonly AnyVector4I YYYZ => new(Y, Y, Y, Z);
        public readonly AnyVector4I YYZX => new(Y, Y, Z, X);
        public readonly AnyVector4I YYZY => new(Y, Y, Z, Y);
        public readonly AnyVector4I YYZZ => new(Y, Y, Z, Z);
        public readonly AnyVector4I YZXX => new(Y, Z, X, X);
        public readonly AnyVector4I YZXY => new(Y, Z, X, Y);
        public readonly AnyVector4I YZXZ => new(Y, Z, X, Z);
        public readonly AnyVector4I YZYX => new(Y, Z, Y, X);
        public readonly AnyVector4I YZYY => new(Y, Z, Y, Y);
        public readonly AnyVector4I YZYZ => new(Y, Z, Y, Z);
        public readonly AnyVector4I YZZX => new(Y, Z, Z, X);
        public readonly AnyVector4I YZZY => new(Y, Z, Z, Y);
        public readonly AnyVector4I YZZZ => new(Y, Z, Z, Z);
        
        public readonly AnyVector4I ZXXX => new(Z, X, X, X);
        public readonly AnyVector4I ZXXY => new(Z, X, X, Y);
        public readonly AnyVector4I ZXXZ => new(Z, X, X, Z);
        public readonly AnyVector4I ZXYX => new(Z, X, Y, X);
        public readonly AnyVector4I ZXYY => new(Z, X, Y, Y);
        public readonly AnyVector4I ZXYZ => new(Z, X, Y, Z);
        public readonly AnyVector4I ZXZX => new(Z, X, Z, X);
        public readonly AnyVector4I ZXZY => new(Z, X, Z, Y);
        public readonly AnyVector4I ZXZZ => new(Z, X, Z, Z);
        public readonly AnyVector4I ZYXX => new(Z, Y, X, X);
        public readonly AnyVector4I ZYXY => new(Z, Y, X, Y);
        public readonly AnyVector4I ZYXZ => new(Z, Y, X, Z);
        public readonly AnyVector4I ZYYX => new(Z, Y, Y, X);
        public readonly AnyVector4I ZYYY => new(Z, Y, Y, Y);
        public readonly AnyVector4I ZYYZ => new(Z, Y, Y, Z);
        public readonly AnyVector4I ZYZX => new(Z, Y, Z, X);
        public readonly AnyVector4I ZYZY => new(Z, Y, Z, Y);
        public readonly AnyVector4I ZYZZ => new(Z, Y, Z, Z);
        public readonly AnyVector4I ZZXX => new(Z, Z, X, X);
        public readonly AnyVector4I ZZXY => new(Z, Z, X, Y);
        public readonly AnyVector4I ZZXZ => new(Z, Z, X, Z);
        public readonly AnyVector4I ZZYX => new(Z, Z, Y, X);
        public readonly AnyVector4I ZZYY => new(Z, Z, Y, Y);
        public readonly AnyVector4I ZZYZ => new(Z, Z, Y, Z);
        public readonly AnyVector4I ZZZX => new(Z, Z, Z, X);
        public readonly AnyVector4I ZZZY => new(Z, Z, Z, Y);
        public readonly AnyVector4I ZZZZ => new(Z, Z, Z, Z);
        #endregion
        #endregion
        #region 构造
        public AnyVector3I(int x, int y, int z) => (X, Y, Z) = (x, y, z);
        public AnyVector3I(int a) => (X, Y, Z) = (a, a, a);
        public AnyVector3I(AnyVector3I other) => (X, Y, Z) = (other.X, other.Y, other.Z);
        public AnyVector3I(int x, AnyVector2I yz) => (X, Y, Z) = (x, yz.X, yz.Y);
        public AnyVector3I(AnyVector2I xy, int z) => (X, Y, Z) = (xy.X, xy.Y, z);
        #endregion
        #region statics
        private static readonly AnyVector3I _zero = new(0, 0, 0);
        private static readonly AnyVector3I _one = new(1, 1, 1);
        private static readonly AnyVector3I _minValue = new(int.MinValue, int.MinValue, int.MinValue);
        private static readonly AnyVector3I _maxValue = new(int.MaxValue, int.MaxValue, int.MaxValue);
        private static readonly AnyVector3I _unitX = new(1, 0, 0);
        private static readonly AnyVector3I _unitY = new(0, 1, 0);
        private static readonly AnyVector3I _unitZ = new(0, 0, 1);
        public static AnyVector3I Zero => _zero;
        public static AnyVector3I One => _one;
        public static AnyVector3I MinValue => _minValue;
        public static AnyVector3I MaxValue => _maxValue;
        public static AnyVector3I UnitX => _unitX;
        public static AnyVector3I UnitY => _unitY;
        public static AnyVector3I UnitZ => _unitZ;
        #endregion
        #region 方法
        public readonly void Deconstruct(out int x, out int y, out int z) => (x, y, z) = (X, Y, Z);
        #region 长度相关 (长度, 距离)
        public readonly int LengthSquared() => X * X + Y * Y + Z * Z;
        public readonly long LongLengthSquared() => (long)X * X + (long)Y * Y + (long)Z * Z;
        public readonly float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z);
        public readonly float LongLength() => MathF.Sqrt((long)X * X + (long)Y * Y + (long)Z * Z);
        public readonly int DistanceSquaredTo(AnyVector3I vector) => (this - vector).LengthSquared();
        public readonly long LongDistanceSquaredTo(AnyVector3I vector) => (this - vector).LongLengthSquared();
        public readonly float DistanceTo(AnyVector3I vector) => (this - vector).Length();
        public readonly float LongDistanceTo(AnyVector3I vector) => (this - vector).LongLength();
        public static int DistanceSquared(AnyVector3I left, AnyVector3I right) => left.DistanceSquaredTo(right);
        public static long LongDistanceSquared(AnyVector3I left, AnyVector3I right) => left.LongDistanceSquaredTo(right);
        public static float Distance(AnyVector3I left, AnyVector3I right) => left.DistanceTo(right);
        public static float LongDistance(AnyVector3I left, AnyVector3I right) => left.LongDistanceTo(right);
        #endregion
        #region 转化
        public readonly AnyVector3I Abs() => new(int.Abs(X), int.Abs(Y), int.Abs(Z));
        public readonly AnyVector3I Sign() => new(int.Sign(X), int.Sign(Y), int.Sign(Z));
        public readonly AnyVector3I CopySign(int sign) => new(int.CopySign(X, sign), int.CopySign(Y, sign), int.CopySign(Z, sign));
        public readonly AnyVector3I CopySign(AnyVector3I signv) => new(int.CopySign(X, signv.X), int.CopySign(Y, signv.Y), int.CopySign(Z, signv.Z));
        #endregion
        #region 限制
        public readonly bool IsBetweenO(AnyVector3I min, AnyVector3I max) => X.IsBetweenO(min.X, max.X) && Y.IsBetweenO(min.Y, max.Y) && Z.IsBetweenO(min.Z, max.Z);
        public readonly bool IsBetweenI(AnyVector3I min, AnyVector3I max) => X.IsBetweenI(min.X, max.X) && Y.IsBetweenI(min.Y, max.Y) && Z.IsBetweenI(min.Z, max.Z);
        public void ClampTo(AnyVector3I min, AnyVector3I max) { X.ClampTo(min.X, max.X); Y.ClampTo(min.Y, max.Y); Z.ClampTo(min.Z, max.Z); }
        public readonly AnyVector3I Clamp(AnyVector3I min, AnyVector3I max) => new(X.Clamp(min.X, max.X), Y.Clamp(min.Y, max.Y), Z.Clamp(min.Z, max.Z));
        #endregion
        public readonly int Dot(AnyVector3I other) => X * other.X + Y * other.Y + Z * other.Z;
        public readonly long LongDot(AnyVector3I other) => (long)X * other.X + (long)Y * other.Y + (long)Z * other.Z;
        public readonly AnyVector3I Cross(AnyVector3I other) => new(Y * other.Z - Z * other.Y, Z * other.X - X * other.Y, X * other.Y - Y * other.X);
        #endregion
        #region 运算
        #region 相等
        public static bool operator ==(AnyVector3I left, AnyVector3I right) => left.Equals(right);
        public static bool operator !=(AnyVector3I left, AnyVector3I right) => !left.Equals(right);
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is AnyVector3I other && Equals(other);
        public readonly bool Equals(AnyVector3I other) => X == other.X && Y == other.Y && Z == other.Z;
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);
        #endregion
        #region operators
        public static AnyVector3I operator +(AnyVector3I left, AnyVector3I right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        public static AnyVector3I operator +(AnyVector3I vec) => vec;
        public static AnyVector3I operator -(AnyVector3I left, AnyVector3I right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        public static AnyVector3I operator -(AnyVector3I vec) => new(-vec.X, -vec.Y, -vec.Z);
        public static AnyVector3I operator *(AnyVector3I vec, int scale) => new(vec.X * scale, vec.Y * scale, vec.Z * scale);
        public static AnyVector3I operator *(int scale, AnyVector3I vec) => new(vec.X * scale, vec.Y * scale, vec.Z * scale);
        public static AnyVector3I operator *(AnyVector3I left, AnyVector3I right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        public static AnyVector3I operator /(AnyVector3I vec, int divisor) => new(vec.X / divisor, vec.Y / divisor, vec.Z / divisor);
        public static AnyVector3I operator /(AnyVector3I vec, AnyVector3I divisorv) => new(vec.X / divisorv.X, vec.Y / divisorv.Y, vec.Z / divisorv.Z);
        public static AnyVector3I operator %(AnyVector3I vec, int divisor) => new(vec.X % divisor, vec.Y % divisor, vec.Z % divisor);
        public static AnyVector3I operator %(AnyVector3I vec, AnyVector3I divisorv) => new(vec.X % divisorv.X, vec.Y % divisorv.Y, vec.Z % divisorv.Z);
        public static bool operator <(AnyVector3I left, AnyVector3I right) => left.X != right.X ? left.X < right.X : left.Y != right.Y ? left.Y < right.Y : left.Z < right.Z;
        public static bool operator >(AnyVector3I left, AnyVector3I right) => left.X != right.X ? left.X > right.X : left.Y != right.Y ? left.Y > right.Y : left.Z > right.Z;
        public static bool operator <=(AnyVector3I left, AnyVector3I right) => left.X != right.X ? left.X < right.X : left.Y != right.Y ? left.Y < right.Y : left.Z <= right.Z;
        public static bool operator >=(AnyVector3I left, AnyVector3I right) => left.X != right.X ? left.X > right.X : left.Y != right.Y ? left.Y > right.Y : left.Z >= right.Z;
        #endregion
        #endregion
        #region 类型转换
        public override readonly string ToString() => $"({X}, {Y}, {Z})";
        public readonly string ToString(string? format) => $"({X.ToString(format)}, {Y.ToString(format)}, {Z.ToString(format)})";
        public static implicit operator (int, int, int)(AnyVector3I vector) => (vector.X, vector.Y, vector.Z);
        public static implicit operator AnyVector3I((int, int, int) tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3);
        public static explicit operator AnyVector2(AnyVector3I vector4) => new(vector4.X, vector4.Y);
        public static explicit operator AnyVector3I(AnyVector2 vector2) => new((int)vector2.X, (int)vector2.Y, 0);
        public static explicit operator AnyVector2I(AnyVector3I vector4) => new(vector4.X, vector4.Y);
        public static explicit operator AnyVector3I(AnyVector2I vector2) => new(vector2.X, vector2.Y, 0);
        public static implicit operator AnyVector3(AnyVector3I vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator AnyVector3I(AnyVector3 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z);
#if XNA
        public static explicit operator XNAVector2(AnyVector3I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector3I(XNAVector2 vector) => new((int)vector.X, (int)vector.Y, 0);
        public static implicit operator XNAVector3(AnyVector3I vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator AnyVector3I(XNAVector3 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z);
        public static explicit operator XNAVector4(AnyVector3I vector) => new(vector.X, vector.Y, vector.Z, 0);
        public static explicit operator AnyVector3I(XNAVector4 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z);
        public static explicit operator XNAVector2I(AnyVector3I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector3I(XNAVector2I vector) => new(vector.X, vector.Y, 0);
#endif
#if GODOT
        public static explicit operator GDVector2(AnyVector3I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector3I(GDVector2 vector) => new((int)vector.X, (int)vector.Y, 0);
        public static implicit operator GDVector3(AnyVector3I vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator AnyVector3I(GDVector3 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z);
        public static explicit operator GDVector4(AnyVector3I vector) => new(vector.X, vector.Y, vector.Z, 0);
        public static explicit operator AnyVector3I(GDVector4 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z);
        public static explicit operator GDVector2I(AnyVector3I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector3I(GDVector2I vector) => new(vector.X, vector.Y, 0);
        public static implicit operator GDVector3I(AnyVector3I vector) => new(vector.X, vector.Y, vector.Z);
        public static implicit operator AnyVector3I(GDVector3I vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator GDVector4I(AnyVector3I vector) => new(vector.X, vector.Y, vector.Z, 0);
        public static explicit operator AnyVector3I(GDVector4I vector) => new(vector.X, vector.Y, vector.Z);
#endif
        #endregion
    }
    public struct AnyVector4 : IEquatable<AnyVector4> {
        #region 字段与属性
        public float X;
        public float Y;
        public float Z;
        public float W;
        public float this[int index] {
            readonly get {
                return index switch {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    3 => W,
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }
            set {
                switch (index) {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                case 3:
                    W = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #region XX
        public readonly AnyVector2 XX => new(X, X);
        public AnyVector2 XY { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2 XZ { readonly get => new(X, Z); set => (X, Z) = value; }
        public AnyVector2 XW { readonly get => new(X, W); set => (X, W) = value; }

        public AnyVector2 YX { readonly get => new(Y, X); set => (Y, X) = value; }
        public readonly AnyVector2 YY => new(Y, Y);
        public AnyVector2 YZ { readonly get => new(Y, Z); set => (Y, Z) = value; }
        public AnyVector2 YW { readonly get => new(Y, W); set => (Y, W) = value; }

        public AnyVector2 ZX { readonly get => new(Z, X); set => (Z, X) = value; }
        public AnyVector2 ZY { readonly get => new(Z, Y); set => (Z, Y) = value; }
        public readonly AnyVector2 ZZ => new(Z, Z);
        public AnyVector2 ZW { readonly get => new(Z, W); set => (Z, W) = value; }

        public AnyVector2 WX { readonly get => new(W, X); set => (W, X) = value; }
        public AnyVector2 WY { readonly get => new(W, Y); set => (W, Y) = value; }
        public AnyVector2 WZ { readonly get => new(W, Z); set => (W, Z) = value; }
        public readonly AnyVector2 WW => new(W, W);
        #endregion
        #region XXX
        public readonly AnyVector3 XXX => new(X, X, X);
        public readonly AnyVector3 XXY => new(X, X, Y);
        public readonly AnyVector3 XXZ => new(X, X, Z);
        public readonly AnyVector3 XXW => new(X, X, W);
        public readonly AnyVector3 XYX => new(X, Y, X);
        public readonly AnyVector3 XYY => new(X, Y, Y);
        public AnyVector3 XYZ { readonly get => new(X, Y, Z); set => (X, Y, Z) = value; }
        public AnyVector3 XYW { readonly get => new(X, Y, W); set => (X, Y, W) = value; }
        public readonly AnyVector3 XZX => new(X, Z, X);
        public AnyVector3 XZY { readonly get => new(X, Z, Y); set => (X, Z, Y) = value; }
        public readonly AnyVector3 XZZ => new(X, Z, Z);
        public AnyVector3 XZW { readonly get => new(X, Z, W); set => (X, Z, W) = value; }
        public readonly AnyVector3 XWX => new(X, W, X);
        public AnyVector3 XWY { readonly get => new(X, W, Y); set => (X, W, Y) = value; }
        public AnyVector3 XWZ { readonly get => new(X, W, Z); set => (X, W, Z) = value; }
        public readonly AnyVector3 XWW => new(X, W, W);
        
        public readonly AnyVector3 YXX => new(Y, X, X);
        public readonly AnyVector3 YXY => new(Y, X, Y);
        public AnyVector3 YXZ { readonly get => new(Y, X, Z); set => (Y, X, Z) = value; }
        public AnyVector3 YXW { readonly get => new(Y, X, W); set => (Y, X, W) = value; }
        public readonly AnyVector3 YYX => new(Y, Y, X);
        public readonly AnyVector3 YYY => new(Y, Y, Y);
        public readonly AnyVector3 YYZ => new(Y, Y, Z);
        public readonly AnyVector3 YYW => new(Y, Y, W);
        public AnyVector3 YZX { readonly get => new(Y, Z, X); set => (Y, Z, X) = value; }
        public readonly AnyVector3 YZY => new(Y, Z, Y);
        public readonly AnyVector3 YZZ => new(Y, Z, Z);
        public AnyVector3 YZW { readonly get => new(Y, Z, W); set => (Y, Z, W) = value; }
        public AnyVector3 YWX { readonly get => new(Y, W, X); set => (Y, W, X) = value; }
        public readonly AnyVector3 YWY => new(Y, W, Y);
        public AnyVector3 YWZ { readonly get => new(Y, W, Z); set => (Y, W, Z) = value; }
        public readonly AnyVector3 YWW => new(Y, W, W);
        
        public readonly AnyVector3 ZXX => new(Z, X, X);
        public AnyVector3 ZXY { readonly get => new(Z, X, Y); set => (Z, X, Y) = value; }
        public readonly AnyVector3 ZXZ => new(Z, X, Z);
        public AnyVector3 ZXW { readonly get => new(Z, X, W); set => (Z, X, W) = value; }
        public AnyVector3 ZYX { readonly get => new(Z, Y, X); set => (Z, Y, X) = value; }
        public readonly AnyVector3 ZYY => new(Z, Y, Y);
        public readonly AnyVector3 ZYZ => new(Z, Y, Z);
        public AnyVector3 ZYW { readonly get => new(Z, Y, W); set => (Z, Y, W) = value; }
        public readonly AnyVector3 ZZX => new(Z, Z, X);
        public readonly AnyVector3 ZZY => new(Z, Z, Y);
        public readonly AnyVector3 ZZZ => new(Z, Z, Z);
        public readonly AnyVector3 ZZW => new(Z, Z, W);
        public AnyVector3 ZWX { readonly get => new(Z, W, X); set => (Z, W, X) = value; }
        public AnyVector3 ZWY { readonly get => new(Z, W, Y); set => (Z, W, Y) = value; }
        public readonly AnyVector3 ZWZ => new(Z, W, Z);
        public readonly AnyVector3 ZWW => new(Z, W, W);
        
        public readonly AnyVector3 WXX => new(W, X, X);
        public AnyVector3 WXY { readonly get => new(W, X, Y); set => (W, X, Y) = value; }
        public AnyVector3 WXZ { readonly get => new(W, X, Z); set => (W, X, Z) = value; }
        public readonly AnyVector3 WXW => new(W, X, W);
        public AnyVector3 WYX { readonly get => new(W, Y, X); set => (W, Y, X) = value; }
        public readonly AnyVector3 WYY => new(W, Y, Y);
        public AnyVector3 WYZ { readonly get => new(W, Y, Z); set => (W, Y, Z) = value; }
        public readonly AnyVector3 WYW => new(W, Y, W);
        public AnyVector3 WZX { readonly get => new(W, Z, X); set => (W, Z, X) = value; }
        public AnyVector3 WZY { readonly get => new(W, Z, Y); set => (W, Z, Y) = value; }
        public readonly AnyVector3 WZZ => new(W, Z, Z);
        public readonly AnyVector3 WZW => new(W, Z, W);
        public readonly AnyVector3 WWX => new(W, W, X);
        public readonly AnyVector3 WWY => new(W, W, Y);
        public readonly AnyVector3 WWZ => new(W, W, Z);
        public readonly AnyVector3 WWW => new(W, W, W);
        #endregion
        #region XXXX
        public readonly AnyVector4 XXXX => new(X, X, X, X);
        public readonly AnyVector4 XXXY => new(X, X, X, Y);
        public readonly AnyVector4 XXXZ => new(X, X, X, Z);
        public readonly AnyVector4 XXXW => new(X, X, X, W);
        public readonly AnyVector4 XXYX => new(X, X, Y, X);
        public readonly AnyVector4 XXYY => new(X, X, Y, Y);
        public readonly AnyVector4 XXYZ => new(X, X, Y, Z);
        public readonly AnyVector4 XXYW => new(X, X, Y, W);
        public readonly AnyVector4 XXZX => new(X, X, Z, X);
        public readonly AnyVector4 XXZY => new(X, X, Z, Y);
        public readonly AnyVector4 XXZZ => new(X, X, Z, Z);
        public readonly AnyVector4 XXZW => new(X, X, Z, W);
        public readonly AnyVector4 XXWX => new(X, X, W, X);
        public readonly AnyVector4 XXWY => new(X, X, W, Y);
        public readonly AnyVector4 XXWZ => new(X, X, W, Z);
        public readonly AnyVector4 XXWW => new(X, X, W, W);
        public readonly AnyVector4 XYXX => new(X, Y, X, X);
        public readonly AnyVector4 XYXY => new(X, Y, X, Y);
        public readonly AnyVector4 XYXZ => new(X, Y, X, Z);
        public readonly AnyVector4 XYXW => new(X, Y, X, W);
        public readonly AnyVector4 XYYX => new(X, Y, Y, X);
        public readonly AnyVector4 XYYY => new(X, Y, Y, Y);
        public readonly AnyVector4 XYYZ => new(X, Y, Y, Z);
        public readonly AnyVector4 XYYW => new(X, Y, Y, W);
        public readonly AnyVector4 XYZX => new(X, Y, Z, X);
        public readonly AnyVector4 XYZY => new(X, Y, Z, Y);
        public AnyVector4 XYZZ { readonly get => new(X, Y, Z, Z); set => (X, Y, Z, Z) = value; }
        public AnyVector4 XYZW { readonly get => new(X, Y, Z, W); set => (X, Y, Z, W) = value; }
        public readonly AnyVector4 XYWX => new(X, Y, W, X);
        public readonly AnyVector4 XYWY => new(X, Y, W, Y);
        public readonly AnyVector4 XYWZ => new(X, Y, W, Z);
        public readonly AnyVector4 XYWW => new(X, Y, W, W);
        public readonly AnyVector4 XZXX => new(X, Z, X, X);
        public readonly AnyVector4 XZXY => new(X, Z, X, Y);
        public readonly AnyVector4 XZXZ => new(X, Z, X, Z);
        public readonly AnyVector4 XZXW => new(X, Z, X, W);
        public readonly AnyVector4 XZYX => new(X, Z, Y, X);
        public readonly AnyVector4 XZYY => new(X, Z, Y, Y);
        public AnyVector4 XZYZ { readonly get => new(X, Z, Y, Z); set => (X, Z, Y, Z) = value; }
        public readonly AnyVector4 XZYW => new(X, Z, Y, W);
        public readonly AnyVector4 XZZX => new(X, Z, Z, X);
        public readonly AnyVector4 XZZY => new(X, Z, Z, Y);
        public readonly AnyVector4 XZZZ => new(X, Z, Z, Z);
        public readonly AnyVector4 XZZW => new(X, Z, Z, W);
        public AnyVector4 XZWX { readonly get => new(X, Z, W, X); set => (X, Z, W, X) = value; }
        public readonly AnyVector4 XZWY => new(X, Z, W, Y);
        public readonly AnyVector4 XZWZ => new(X, Z, W, Z);
        public readonly AnyVector4 XZWW => new(X, Z, W, W);
        public readonly AnyVector4 XWXX => new(X, W, X, X);
        public readonly AnyVector4 XWXY => new(X, W, X, Y);
        public readonly AnyVector4 XWXZ => new(X, W, X, Z);
        public readonly AnyVector4 XWXW => new(X, W, X, W);
        public readonly AnyVector4 XWYX => new(X, W, Y, X);
        public AnyVector4 XWYY { readonly get => new(X, W, Y, Y); set => (X, W, Y, Y) = value; }
        public readonly AnyVector4 XWYZ => new(X, W, Y, Z);
        public readonly AnyVector4 XWYW => new(X, W, Y, W);
        public AnyVector4 XWZX { readonly get => new(X, W, Z, X); set => (X, W, Z, X) = value; }
        public readonly AnyVector4 XWZY => new(X, W, Z, Y);
        public readonly AnyVector4 XWZZ => new(X, W, Z, Z);
        public readonly AnyVector4 XWZW => new(X, W, Z, W);
        public readonly AnyVector4 XWWX => new(X, W, W, X);
        public readonly AnyVector4 XWWY => new(X, W, W, Y);
        public readonly AnyVector4 XWWZ => new(X, W, W, Z);
        public readonly AnyVector4 XWWW => new(X, W, W, W);
        
        public readonly AnyVector4 YXXX => new(Y, X, X, X);
        public readonly AnyVector4 YXXY => new(Y, X, X, Y);
        public readonly AnyVector4 YXXZ => new(Y, X, X, Z);
        public readonly AnyVector4 YXXW => new(Y, X, X, W);
        public readonly AnyVector4 YXYX => new(Y, X, Y, X);
        public readonly AnyVector4 YXYY => new(Y, X, Y, Y);
        public readonly AnyVector4 YXYZ => new(Y, X, Y, Z);
        public readonly AnyVector4 YXYW => new(Y, X, Y, W);
        public readonly AnyVector4 YXZX => new(Y, X, Z, X);
        public readonly AnyVector4 YXZY => new(Y, X, Z, Y);
        public readonly AnyVector4 YXZZ => new(Y, X, Z, Z);
        public AnyVector4 YXZW { readonly get => new(Y, X, Z, W); set => (Y, X, Z, W) = value; }
        public readonly AnyVector4 YXWX => new(Y, X, W, X);
        public readonly AnyVector4 YXWY => new(Y, X, W, Y);
        public AnyVector4 YXWZ { readonly get => new(Y, X, W, Z); set => (Y, X, W, Z) = value; }
        public readonly AnyVector4 YXWW => new(Y, X, W, W);
        public readonly AnyVector4 YYXX => new(Y, Y, X, X);
        public readonly AnyVector4 YYXY => new(Y, Y, X, Y);
        public readonly AnyVector4 YYXZ => new(Y, Y, X, Z);
        public readonly AnyVector4 YYXW => new(Y, Y, X, W);
        public readonly AnyVector4 YYYX => new(Y, Y, Y, X);
        public readonly AnyVector4 YYYY => new(Y, Y, Y, Y);
        public readonly AnyVector4 YYYZ => new(Y, Y, Y, Z);
        public readonly AnyVector4 YYYW => new(Y, Y, Y, W);
        public readonly AnyVector4 YYZX => new(Y, Y, Z, X);
        public readonly AnyVector4 YYZY => new(Y, Y, Z, Y);
        public readonly AnyVector4 YYZZ => new(Y, Y, Z, Z);
        public readonly AnyVector4 YYZW => new(Y, Y, Z, W);
        public readonly AnyVector4 YYWX => new(Y, Y, W, X);
        public readonly AnyVector4 YYWY => new(Y, Y, W, Y);
        public readonly AnyVector4 YYWZ => new(Y, Y, W, Z);
        public readonly AnyVector4 YYWW => new(Y, Y, W, W);
        public readonly AnyVector4 YZXX => new(Y, Z, X, X);
        public readonly AnyVector4 YZXY => new(Y, Z, X, Y);
        public readonly AnyVector4 YZXZ => new(Y, Z, X, Z);
        public AnyVector4 YZXW { readonly get => new(Y, Z, X, W); set => (Y, Z, X, W) = value; }
        public readonly AnyVector4 YZYX => new(Y, Z, Y, X);
        public readonly AnyVector4 YZYY => new(Y, Z, Y, Y);
        public readonly AnyVector4 YZYZ => new(Y, Z, Y, Z);
        public readonly AnyVector4 YZYW => new(Y, Z, Y, W);
        public readonly AnyVector4 YZZX => new(Y, Z, Z, X);
        public readonly AnyVector4 YZZY => new(Y, Z, Z, Y);
        public readonly AnyVector4 YZZZ => new(Y, Z, Z, Z);
        public readonly AnyVector4 YZZW => new(Y, Z, Z, W);
        public AnyVector4 YZWX { readonly get => new(Y, Z, W, X); set => (Y, Z, W, X) = value; }
        public readonly AnyVector4 YZWY => new(Y, Z, W, Y);
        public readonly AnyVector4 YZWZ => new(Y, Z, W, Z);
        public readonly AnyVector4 YZWW => new(Y, Z, W, W);
        public readonly AnyVector4 YWXX => new(Y, W, X, X);
        public readonly AnyVector4 YWXY => new(Y, W, X, Y);
        public AnyVector4 YWXZ { readonly get => new(Y, W, X, Z); set => (Y, W, X, Z) = value; }
        public readonly AnyVector4 YWXW => new(Y, W, X, W);
        public readonly AnyVector4 YWYX => new(Y, W, Y, X);
        public readonly AnyVector4 YWYY => new(Y, W, Y, Y);
        public readonly AnyVector4 YWYZ => new(Y, W, Y, Z);
        public readonly AnyVector4 YWYW => new(Y, W, Y, W);
        public AnyVector4 YWZX { readonly get => new(Y, W, Z, X); set => (Y, W, Z, X) = value; }
        public readonly AnyVector4 YWZY => new(Y, W, Z, Y);
        public readonly AnyVector4 YWZZ => new(Y, W, Z, Z);
        public readonly AnyVector4 YWZW => new(Y, W, Z, W);
        public readonly AnyVector4 YWWX => new(Y, W, W, X);
        public readonly AnyVector4 YWWY => new(Y, W, W, Y);
        public readonly AnyVector4 YWWZ => new(Y, W, W, Z);
        public readonly AnyVector4 YWWW => new(Y, W, W, W);
        
        public readonly AnyVector4 ZXXX => new(Z, X, X, X);
        public readonly AnyVector4 ZXXY => new(Z, X, X, Y);
        public readonly AnyVector4 ZXXZ => new(Z, X, X, Z);
        public readonly AnyVector4 ZXXW => new(Z, X, X, W);
        public readonly AnyVector4 ZXYX => new(Z, X, Y, X);
        public readonly AnyVector4 ZXYY => new(Z, X, Y, Y);
        public readonly AnyVector4 ZXYZ => new(Z, X, Y, Z);
        public AnyVector4 ZXYW { readonly get => new(Z, X, Y, W); set => (Z, X, Y, W) = value; }
        public readonly AnyVector4 ZXZX => new(Z, X, Z, X);
        public readonly AnyVector4 ZXZY => new(Z, X, Z, Y);
        public readonly AnyVector4 ZXZZ => new(Z, X, Z, Z);
        public readonly AnyVector4 ZXZW => new(Z, X, Z, W);
        public readonly AnyVector4 ZXWX => new(Z, X, W, X);
        public AnyVector4 ZXWY { readonly get => new(Z, X, W, Y); set => (Z, X, W, Y) = value; }
        public readonly AnyVector4 ZXWZ => new(Z, X, W, Z);
        public readonly AnyVector4 ZXWW => new(Z, X, W, W);
        public readonly AnyVector4 ZYXX => new(Z, Y, X, X);
        public readonly AnyVector4 ZYXY => new(Z, Y, X, Y);
        public readonly AnyVector4 ZYXZ => new(Z, Y, X, Z);
        public AnyVector4 ZYXW { readonly get => new(Z, Y, X, W); set => (Z, Y, X, W) = value; }
        public readonly AnyVector4 ZYYX => new(Z, Y, Y, X);
        public readonly AnyVector4 ZYYY => new(Z, Y, Y, Y);
        public readonly AnyVector4 ZYYZ => new(Z, Y, Y, Z);
        public readonly AnyVector4 ZYYW => new(Z, Y, Y, W);
        public readonly AnyVector4 ZYZX => new(Z, Y, Z, X);
        public readonly AnyVector4 ZYZY => new(Z, Y, Z, Y);
        public readonly AnyVector4 ZYZZ => new(Z, Y, Z, Z);
        public readonly AnyVector4 ZYZW => new(Z, Y, Z, W);
        public AnyVector4 ZYWX { readonly get => new(Z, Y, W, X); set => (Z, Y, W, X) = value; }
        public readonly AnyVector4 ZYWY => new(Z, Y, W, Y);
        public readonly AnyVector4 ZYWZ => new(Z, Y, W, Z);
        public readonly AnyVector4 ZYWW => new(Z, Y, W, W);
        public readonly AnyVector4 ZZXX => new(Z, Z, X, X);
        public readonly AnyVector4 ZZXY => new(Z, Z, X, Y);
        public readonly AnyVector4 ZZXZ => new(Z, Z, X, Z);
        public readonly AnyVector4 ZZXW => new(Z, Z, X, W);
        public readonly AnyVector4 ZZYX => new(Z, Z, Y, X);
        public readonly AnyVector4 ZZYY => new(Z, Z, Y, Y);
        public readonly AnyVector4 ZZYZ => new(Z, Z, Y, Z);
        public readonly AnyVector4 ZZYW => new(Z, Z, Y, W);
        public readonly AnyVector4 ZZZX => new(Z, Z, Z, X);
        public readonly AnyVector4 ZZZY => new(Z, Z, Z, Y);
        public readonly AnyVector4 ZZZZ => new(Z, Z, Z, Z);
        public readonly AnyVector4 ZZZW => new(Z, Z, Z, W);
        public readonly AnyVector4 ZZWX => new(Z, Z, W, X);
        public readonly AnyVector4 ZZWY => new(Z, Z, W, Y);
        public readonly AnyVector4 ZZWZ => new(Z, Z, W, Z);
        public readonly AnyVector4 ZZWW => new(Z, Z, W, W);
        public readonly AnyVector4 ZWXX => new(Z, W, X, X);
        public AnyVector4 ZWXY { readonly get => new(Z, W, X, Y); set => (Z, W, X, Y) = value; }
        public readonly AnyVector4 ZWXZ => new(Z, W, X, Z);
        public readonly AnyVector4 ZWXW => new(Z, W, X, W);
        public AnyVector4 ZWYX { readonly get => new(Z, W, Y, X); set => (Z, W, Y, X) = value; }
        public readonly AnyVector4 ZWYY => new(Z, W, Y, Y);
        public readonly AnyVector4 ZWYZ => new(Z, W, Y, Z);
        public readonly AnyVector4 ZWYW => new(Z, W, Y, W);
        public readonly AnyVector4 ZWZX => new(Z, W, Z, X);
        public readonly AnyVector4 ZWZY => new(Z, W, Z, Y);
        public readonly AnyVector4 ZWZZ => new(Z, W, Z, Z);
        public readonly AnyVector4 ZWZW => new(Z, W, Z, W);
        public readonly AnyVector4 ZWWX => new(Z, W, W, X);
        public readonly AnyVector4 ZWWY => new(Z, W, W, Y);
        public readonly AnyVector4 ZWWZ => new(Z, W, W, Z);
        public readonly AnyVector4 ZWWW => new(Z, W, W, W);
        
        public readonly AnyVector4 WXXX => new(W, X, X, X);
        public readonly AnyVector4 WXXY => new(W, X, X, Y);
        public readonly AnyVector4 WXXZ => new(W, X, X, Z);
        public readonly AnyVector4 WXXW => new(W, X, X, W);
        public readonly AnyVector4 WXYX => new(W, X, Y, X);
        public readonly AnyVector4 WXYY => new(W, X, Y, Y);
        public AnyVector4 WXYZ { readonly get => new(W, X, Y, Z); set => (W, X, Y, Z) = value; }
        public readonly AnyVector4 WXYW => new(W, X, Y, W);
        public readonly AnyVector4 WXZX => new(W, X, Z, X);
        public AnyVector4 WXZY { readonly get => new(W, X, Z, Y); set => (W, X, Z, Y) = value; }
        public readonly AnyVector4 WXZZ => new(W, X, Z, Z);
        public readonly AnyVector4 WXZW => new(W, X, Z, W);
        public readonly AnyVector4 WXWX => new(W, X, W, X);
        public readonly AnyVector4 WXWY => new(W, X, W, Y);
        public readonly AnyVector4 WXWZ => new(W, X, W, Z);
        public readonly AnyVector4 WXWW => new(W, X, W, W);
        public readonly AnyVector4 WYXX => new(W, Y, X, X);
        public readonly AnyVector4 WYXY => new(W, Y, X, Y);
        public AnyVector4 WYXZ { readonly get => new(W, Y, X, Z); set => (W, Y, X, Z) = value; }
        public readonly AnyVector4 WYXW => new(W, Y, X, W);
        public readonly AnyVector4 WYYX => new(W, Y, Y, X);
        public readonly AnyVector4 WYYY => new(W, Y, Y, Y);
        public readonly AnyVector4 WYYZ => new(W, Y, Y, Z);
        public readonly AnyVector4 WYYW => new(W, Y, Y, W);
        public AnyVector4 WYZX { readonly get => new(W, Y, Z, X); set => (W, Y, Z, X) = value; }
        public readonly AnyVector4 WYZY => new(W, Y, Z, Y);
        public readonly AnyVector4 WYZZ => new(W, Y, Z, Z);
        public readonly AnyVector4 WYZW => new(W, Y, Z, W);
        public readonly AnyVector4 WYWX => new(W, Y, W, X);
        public readonly AnyVector4 WYWY => new(W, Y, W, Y);
        public readonly AnyVector4 WYWZ => new(W, Y, W, Z);
        public readonly AnyVector4 WYWW => new(W, Y, W, W);
        public readonly AnyVector4 WZXX => new(W, Z, X, X);
        public AnyVector4 WZXY { readonly get => new(W, Z, X, Y); set => (W, Z, X, Y) = value; }
        public readonly AnyVector4 WZXZ => new(W, Z, X, Z);
        public readonly AnyVector4 WZXW => new(W, Z, X, W);
        public AnyVector4 WZYX { readonly get => new(W, Z, Y, X); set => (W, Z, Y, X) = value; }
        public readonly AnyVector4 WZYY => new(W, Z, Y, Y);
        public readonly AnyVector4 WZYZ => new(W, Z, Y, Z);
        public readonly AnyVector4 WZYW => new(W, Z, Y, W);
        public readonly AnyVector4 WZZX => new(W, Z, Z, X);
        public readonly AnyVector4 WZZY => new(W, Z, Z, Y);
        public readonly AnyVector4 WZZZ => new(W, Z, Z, Z);
        public readonly AnyVector4 WZZW => new(W, Z, Z, W);
        public readonly AnyVector4 WZWX => new(W, Z, W, X);
        public readonly AnyVector4 WZWY => new(W, Z, W, Y);
        public readonly AnyVector4 WZWZ => new(W, Z, W, Z);
        public readonly AnyVector4 WZWW => new(W, Z, W, W);
        public readonly AnyVector4 WWXX => new(W, W, X, X);
        public readonly AnyVector4 WWXY => new(W, W, X, Y);
        public readonly AnyVector4 WWXZ => new(W, W, X, Z);
        public readonly AnyVector4 WWXW => new(W, W, X, W);
        public readonly AnyVector4 WWYX => new(W, W, Y, X);
        public readonly AnyVector4 WWYY => new(W, W, Y, Y);
        public readonly AnyVector4 WWYZ => new(W, W, Y, Z);
        public readonly AnyVector4 WWYW => new(W, W, Y, W);
        public readonly AnyVector4 WWZX => new(W, W, Z, X);
        public readonly AnyVector4 WWZY => new(W, W, Z, Y);
        public readonly AnyVector4 WWZZ => new(W, W, Z, Z);
        public readonly AnyVector4 WWZW => new(W, W, Z, W);
        public readonly AnyVector4 WWWX => new(W, W, W, X);
        public readonly AnyVector4 WWWY => new(W, W, W, Y);
        public readonly AnyVector4 WWWZ => new(W, W, W, Z);
        public readonly AnyVector4 WWWW => new(W, W, W, W);
        #endregion
        #endregion
        #region 构造
        public AnyVector4(float x, float y, float z, float w) => (X, Y, Z, W) = (x, y, z, w);
        public AnyVector4(float a) => (X, Y, Z, W) = (a, a, a, a);
        public AnyVector4(AnyVector4 other) => (X, Y, Z, W) = (other.X, other.Y, other.Z, other.W);
        public AnyVector4(float x, float y, AnyVector2 zw) => (X, Y, Z, W) = (x, y, zw.X, zw.Y);
        public AnyVector4(float x, AnyVector2 yz, float w) => (X, Y, Z, W) = (x, yz.X, yz.Y, w);
        public AnyVector4(AnyVector2 xy, float z, float w) => (X, Y, Z, W) = (xy.X, xy.Y, z, w);
        public AnyVector4(AnyVector2 xy, AnyVector2 zw) => (X, Y, Z, W) = (xy.X, xy.Y, zw.X, zw.Y);
        public AnyVector4(float x, AnyVector3 yzw) => (X, Y, Z, W) = (x, yzw.X, yzw.Y, yzw.Z);
        public AnyVector4(AnyVector3 xyz, float w) => (X, Y, Z, W) = (xyz.X, xyz.Y, xyz.Z, w);
        #endregion
        #region statics
        private static readonly AnyVector4 _zero = new(0, 0, 0, 0);
        private static readonly AnyVector4 _one = new(1, 1, 1, 1);
        private static readonly AnyVector4 _inf = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        private static readonly AnyVector4 _unitX = new(1, 0, 0, 0);
        private static readonly AnyVector4 _unitY = new(0, 1, 0, 0);
        private static readonly AnyVector4 _unitZ = new(0, 0, 1, 0);
        private static readonly AnyVector4 _unitW = new(0, 0, 0, 1);
        public static AnyVector4 Zero => _zero;
        public static AnyVector4 One => _one;
        public static AnyVector4 Inf => _inf;
        public static AnyVector4 UnitX => _unitX;
        public static AnyVector4 UnitY => _unitY;
        public static AnyVector4 UnitZ => _unitZ;
        public static AnyVector4 UnitW => _unitW;
        #endregion
        #region 方法
        public readonly void Deconstruct(out float x, out float y, out float z, out float w) => (x, y, z, w) = (X, Y, Z, W);
        #region 特殊判断
        public readonly bool IsFinite() => float.IsFinite(X) && float.IsFinite(Y) && float.IsFinite(Z) && float.IsFinite(W);
        public readonly bool HasNaNs() => float.IsNaN(X) || float.IsNaN(Y) || float.IsNaN(Z) || float.IsNaN(W);
        public readonly bool IsNormalized() => LengthSquared() == 1;
        #endregion
        #region 长度相关 (长度, 距离, 标准化)
        public readonly float LengthSquared() => X * X + Y * Y + Z * Z + W * W;
        public readonly float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);
        public readonly float DistanceSquaredTo(AnyVector4 vector) => (this - vector).LengthSquared();
        public readonly float DistanceTo(AnyVector4 vector) => (this - vector).Length();
        public void ClampDistance(AnyVector4 origin, float distance) {
            if (distance <= 0) {
                this = origin;
            }
            if (DistanceSquaredTo(origin) > distance * distance) {
                this = origin + (this - origin).Normalized() * distance;
            }
        }
        public readonly AnyVector4 ClampedDistance(AnyVector4 origin, float distance)
            => distance <= 0 ? origin : DistanceSquaredTo(origin) <= distance * distance ? this
                : origin + (this - origin).Normalized() * distance;
        public void Normalize() {
            var lengthSquared = LengthSquared();
            if (lengthSquared == 0) {
                X = Y = 0;
                return;
            }
            var length = MathF.Sqrt(lengthSquared);
            this /= length;
        }
        public void SafeNormalize(AnyVector4 defaultValue = default) {
            if (!IsFinite()) {
                this = defaultValue;
                return;
            }
            var lengthSquared = LengthSquared();
            if (lengthSquared == 0) {
                this = defaultValue;
                return;
            }
            var length = MathF.Sqrt(lengthSquared);
            this /= length;
        }
        public readonly AnyVector4 Normalized() {
            var result = this;
            result.Normalize();
            return result;
        }
        public readonly AnyVector4 SafeNormalized(AnyVector4 defaultValue = default) {
            var result = this;
            result.SafeNormalize(defaultValue);
            return result;
        }
        public static float DistanceSquared(AnyVector4 left, AnyVector4 right) => left.DistanceSquaredTo(right);
        public static float Distance(AnyVector4 left, AnyVector4 right) => left.DistanceTo(right);
        public static AnyVector4 Normalized(AnyVector4 vec) => vec.Normalized();
        public static AnyVector4 SafeNormalized(AnyVector4 vec, AnyVector4 defaultValue = default) => vec.SafeNormalized(defaultValue);
        #endregion
        #region 转化
        public readonly AnyVector4 Abs() => new(MathF.Abs(X), MathF.Abs(Y), MathF.Abs(Z), MathF.Abs(W));
        public readonly AnyVector4 Ceil() => new(MathF.Ceiling(X), MathF.Ceiling(Y), MathF.Ceiling(Z), MathF.Ceiling(W));
        public readonly AnyVector4 Floor() => new(MathF.Floor(X), MathF.Floor(Y), MathF.Floor(Z), MathF.Floor(W));
        public readonly AnyVector4 Round() => new(MathF.Round(X), MathF.Round(Y), MathF.Round(Z), MathF.Round(W));
        public readonly AnyVector4 Sign() => new(MathF.Sign(X), MathF.Sign(Y), MathF.Sign(Z), MathF.Sign(W));
        public readonly AnyVector4I CeilI() => new((int)MathF.Ceiling(X), (int)MathF.Ceiling(Y), (int)MathF.Ceiling(Z), (int)MathF.Ceiling(W));
        public readonly AnyVector4I FloorI() => new((int)MathF.Floor(X), (int)MathF.Floor(Y), (int)MathF.Floor(Z), (int)MathF.Floor(W));
        public readonly AnyVector4I RoundI() => new((int)MathF.Round(X), (int)MathF.Round(Y), (int)MathF.Round(Z), (int)MathF.Round(W));
        public readonly AnyVector4I SignI() => new(MathF.Sign(X), MathF.Sign(Y), MathF.Sign(Z), MathF.Sign(W));
        public readonly AnyVector4 CopySign(float sign) => new(MathF.CopySign(X, sign), MathF.CopySign(Y, sign), MathF.CopySign(Z, sign), MathF.CopySign(W, sign));
        public readonly AnyVector4 CopySign(AnyVector4 signv) => new(MathF.CopySign(X, signv.X), MathF.CopySign(Y, signv.Y), MathF.CopySign(Z, signv.Z), MathF.CopySign(W, signv.W));
        #endregion
        #region 限制
        public readonly bool IsBetweenO(AnyVector4 min, AnyVector4 max) => X.IsBetweenO(min.X, max.X) && Y.IsBetweenO(min.Y, max.Y) && Z.IsBetweenO(min.Z, max.Z) && W.IsBetweenO(min.W, max.W);
        public readonly bool IsBetweenI(AnyVector4 min, AnyVector4 max) => X.IsBetweenI(min.X, max.X) && Y.IsBetweenI(min.Y, max.Y) && Z.IsBetweenI(min.Z, max.Z) && W.IsBetweenI(min.W, max.W);
        public void ClampTo(AnyVector4 min, AnyVector4 max) { X.ClampTo(min.X, max.X); Y.ClampTo(min.Y, max.Y); Z.ClampTo(min.Z, max.Z); W.ClampTo(min.W, max.W); }
        public readonly AnyVector4 Clamp(AnyVector4 min, AnyVector4 max) => new(X.Clamp(min.X, max.X), Y.Clamp(min.Y, max.Y), Z.Clamp(min.Z, max.Z), W.Clamp(min.W, max.W));
        #endregion
        public readonly float Dot(AnyVector4 other) => X * other.X + Y * other.Y + Z * other.Z + W * other.W;
        #endregion
        #region 运算
        #region 相等
        public static bool operator ==(AnyVector4 left, AnyVector4 right) => left.Equals(right);
        public static bool operator !=(AnyVector4 left, AnyVector4 right) => !left.Equals(right);
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is AnyVector4 other && Equals(other);
        public readonly bool Equals(AnyVector4 other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);
        #endregion
        #region operators
        public static AnyVector4 operator +(AnyVector4 left, AnyVector4 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        public static AnyVector4 operator +(AnyVector4 vec) => vec;
        public static AnyVector4 operator -(AnyVector4 left, AnyVector4 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        public static AnyVector4 operator -(AnyVector4 vec) => new(-vec.X, -vec.Y, -vec.Z, -vec.W);
        public static AnyVector4 operator *(AnyVector4 vec, float scale) => new(vec.X * scale, vec.Y * scale, vec.Z * scale, vec.W * scale);
        public static AnyVector4 operator *(float scale, AnyVector4 vec) => new(vec.X * scale, vec.Y * scale, vec.Z * scale, vec.W * scale);
        public static AnyVector4 operator *(AnyVector4 left, AnyVector4 right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        public static AnyVector4 operator /(AnyVector4 vec, float divisor) => new(vec.X / divisor, vec.Y / divisor, vec.Z / divisor, vec.W / divisor);
        public static AnyVector4 operator /(AnyVector4 vec, AnyVector4 divisorv) => new(vec.X / divisorv.X, vec.Y / divisorv.Y, vec.Z / divisorv.Z, vec.W / divisorv.W);
        public static AnyVector4 operator %(AnyVector4 vec, float divisor) => new(vec.X % divisor, vec.Y % divisor, vec.Z % divisor, vec.W % divisor);
        public static AnyVector4 operator %(AnyVector4 vec, AnyVector4 divisorv) => new(vec.X % divisorv.X, vec.Y % divisorv.Y, vec.Z % divisorv.Z, vec.W % divisorv.W);
        public static bool operator <(AnyVector4 left, AnyVector4 right) => left.X != right.X ? left.X < right.X : left.Y != right.Y ? left.Y < right.Y : left.Z != right.Z ? left.Z < right.Z : left.W < right.W;
        public static bool operator >(AnyVector4 left, AnyVector4 right) => left.X != right.X ? left.X > right.X : left.Y != right.Y ? left.Y > right.Y : left.Z != right.Z ? left.Z > right.Z : left.W > right.W;
        public static bool operator <=(AnyVector4 left, AnyVector4 right) => left.X != right.X ? left.X < right.X : left.Y != right.Y ? left.Y < right.Y : left.Z != right.Z ? left.Z < right.Z : left.W <= right.W;
        public static bool operator >=(AnyVector4 left, AnyVector4 right) => left.X != right.X ? left.X > right.X : left.Y != right.Y ? left.Y > right.Y : left.Z != right.Z ? left.Z > right.Z : left.W >= right.W;
        #endregion
        #endregion
        #region 类型转换
        public override readonly string ToString() => $"({X}, {Y}, {Z}, {W})";
        public readonly string ToString(string? format) => $"({X.ToString(format)}, {Y.ToString(format)}, {Z.ToString(format)}, {W.ToString(format)})";
        public static implicit operator (float, float, float, float)(AnyVector4 vector) => (vector.X, vector.Y, vector.Z, vector.W);
        public static implicit operator AnyVector4((float, float, float, float) tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
        public static explicit operator AnyVector2(AnyVector4 vector4) => new(vector4.X, vector4.Y);
        public static explicit operator AnyVector4(AnyVector2 vector2) => new(vector2.X, vector2.Y, 0, 0);
        public static explicit operator AnyVector2I(AnyVector4 vector4) => new((int)vector4.X, (int)vector4.Y);
        public static explicit operator AnyVector4(AnyVector2I vector2) => new(vector2.X, vector2.Y, 0, 0);
        public static explicit operator AnyVector3(AnyVector4 vector4) => new(vector4.X, vector4.Y, vector4.Z);
        public static explicit operator AnyVector4(AnyVector3 vector3) => new(vector3.X, vector3.Y, vector3.Z, 0);
        public static explicit operator AnyVector3I(AnyVector4 vector4) => new((int)vector4.X, (int)vector4.Y, (int)vector4.Z);
        public static explicit operator AnyVector4(AnyVector3I vector3) => new(vector3.X, vector3.Y, vector3.Z, 0);
#if XNA
        public static explicit operator XNAVector2(AnyVector4 vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector4(XNAVector2 vector) => new(vector.X, vector.Y, 0, 0);
        public static explicit operator XNAVector3(AnyVector4 vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator AnyVector4(XNAVector3 vector) => new(vector.X, vector.Y, vector.Z, 0);
        public static implicit operator XNAVector4(AnyVector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        public static implicit operator AnyVector4(XNAVector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        public static explicit operator XNAVector2I(AnyVector4 vector) => new((int)vector.X, (int)vector.Y);
        public static explicit operator AnyVector4(XNAVector2I vector) => new(vector.X, vector.Y, 0, 0);
#endif
#if GODOT
        public static explicit operator GDVector2(AnyVector4 vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector4(GDVector2 vector) => new(vector.X, vector.Y, 0, 0);
        public static explicit operator GDVector3(AnyVector4 vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator AnyVector4(GDVector3 vector) => new(vector.X, vector.Y, vector.Z, 0);
        public static implicit operator GDVector4(AnyVector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        public static implicit operator AnyVector4(GDVector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        public static explicit operator GDVector2I(AnyVector4 vector) => new((int)vector.X, (int)vector.Y);
        public static explicit operator AnyVector4(GDVector2I vector) => new(vector.X, vector.Y, 0, 0);
        public static explicit operator GDVector3I(AnyVector4 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z);
        public static explicit operator AnyVector4(GDVector3I vector) => new(vector.X, vector.Y, vector.Z, 0);
        public static explicit operator GDVector4I(AnyVector4 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z, (int)vector.W);
        public static implicit operator AnyVector4(GDVector4I vector) => new(vector.X, vector.Y, vector.Z, vector.W);
#endif
        #endregion
    }
    public struct AnyVector4I : IEquatable<AnyVector4I> {
        #region 字段与属性
        public int X;
        public int Y;
        public int Z;
        public int W;
        public int this[int index] {
            readonly get {
                return index switch {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    3 => W,
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }
            set {
                switch (index) {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                case 3:
                    W = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #region XX
        public readonly AnyVector2I XX => new(X, X);
        public AnyVector2I XY { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2I XZ { readonly get => new(X, Z); set => (X, Z) = value; }
        public AnyVector2I XW { readonly get => new(X, W); set => (X, W) = value; }

        public AnyVector2I YX { readonly get => new(Y, X); set => (Y, X) = value; }
        public readonly AnyVector2I YY => new(Y, Y);
        public AnyVector2I YZ { readonly get => new(Y, Z); set => (Y, Z) = value; }
        public AnyVector2I YW { readonly get => new(Y, W); set => (Y, W) = value; }

        public AnyVector2I ZX { readonly get => new(Z, X); set => (Z, X) = value; }
        public AnyVector2I ZY { readonly get => new(Z, Y); set => (Z, Y) = value; }
        public readonly AnyVector2I ZZ => new(Z, Z);
        public AnyVector2I ZW { readonly get => new(Z, W); set => (Z, W) = value; }

        public AnyVector2I WX { readonly get => new(W, X); set => (W, X) = value; }
        public AnyVector2I WY { readonly get => new(W, Y); set => (W, Y) = value; }
        public AnyVector2I WZ { readonly get => new(W, Z); set => (W, Z) = value; }
        public readonly AnyVector2I WW => new(W, W);
        #endregion
        #region XXX
        public readonly AnyVector3I XXX => new(X, X, X);
        public readonly AnyVector3I XXY => new(X, X, Y);
        public readonly AnyVector3I XXZ => new(X, X, Z);
        public readonly AnyVector3I XXW => new(X, X, W);
        public readonly AnyVector3I XYX => new(X, Y, X);
        public readonly AnyVector3I XYY => new(X, Y, Y);
        public AnyVector3I XYZ { readonly get => new(X, Y, Z); set => (X, Y, Z) = value; }
        public AnyVector3I XYW { readonly get => new(X, Y, W); set => (X, Y, W) = value; }
        public readonly AnyVector3I XZX => new(X, Z, X);
        public AnyVector3I XZY { readonly get => new(X, Z, Y); set => (X, Z, Y) = value; }
        public readonly AnyVector3I XZZ => new(X, Z, Z);
        public AnyVector3I XZW { readonly get => new(X, Z, W); set => (X, Z, W) = value; }
        public readonly AnyVector3I XWX => new(X, W, X);
        public AnyVector3I XWY { readonly get => new(X, W, Y); set => (X, W, Y) = value; }
        public AnyVector3I XWZ { readonly get => new(X, W, Z); set => (X, W, Z) = value; }
        public readonly AnyVector3I XWW => new(X, W, W);
        
        public readonly AnyVector3I YXX => new(Y, X, X);
        public readonly AnyVector3I YXY => new(Y, X, Y);
        public AnyVector3I YXZ { readonly get => new(Y, X, Z); set => (Y, X, Z) = value; }
        public AnyVector3I YXW { readonly get => new(Y, X, W); set => (Y, X, W) = value; }
        public readonly AnyVector3I YYX => new(Y, Y, X);
        public readonly AnyVector3I YYY => new(Y, Y, Y);
        public readonly AnyVector3I YYZ => new(Y, Y, Z);
        public readonly AnyVector3I YYW => new(Y, Y, W);
        public AnyVector3I YZX { readonly get => new(Y, Z, X); set => (Y, Z, X) = value; }
        public readonly AnyVector3I YZY => new(Y, Z, Y);
        public readonly AnyVector3I YZZ => new(Y, Z, Z);
        public AnyVector3I YZW { readonly get => new(Y, Z, W); set => (Y, Z, W) = value; }
        public AnyVector3I YWX { readonly get => new(Y, W, X); set => (Y, W, X) = value; }
        public readonly AnyVector3I YWY => new(Y, W, Y);
        public AnyVector3I YWZ { readonly get => new(Y, W, Z); set => (Y, W, Z) = value; }
        public readonly AnyVector3I YWW => new(Y, W, W);
        
        public readonly AnyVector3I ZXX => new(Z, X, X);
        public AnyVector3I ZXY { readonly get => new(Z, X, Y); set => (Z, X, Y) = value; }
        public readonly AnyVector3I ZXZ => new(Z, X, Z);
        public AnyVector3I ZXW { readonly get => new(Z, X, W); set => (Z, X, W) = value; }
        public AnyVector3I ZYX { readonly get => new(Z, Y, X); set => (Z, Y, X) = value; }
        public readonly AnyVector3I ZYY => new(Z, Y, Y);
        public readonly AnyVector3I ZYZ => new(Z, Y, Z);
        public AnyVector3I ZYW { readonly get => new(Z, Y, W); set => (Z, Y, W) = value; }
        public readonly AnyVector3I ZZX => new(Z, Z, X);
        public readonly AnyVector3I ZZY => new(Z, Z, Y);
        public readonly AnyVector3I ZZZ => new(Z, Z, Z);
        public readonly AnyVector3I ZZW => new(Z, Z, W);
        public AnyVector3I ZWX { readonly get => new(Z, W, X); set => (Z, W, X) = value; }
        public AnyVector3I ZWY { readonly get => new(Z, W, Y); set => (Z, W, Y) = value; }
        public readonly AnyVector3I ZWZ => new(Z, W, Z);
        public readonly AnyVector3I ZWW => new(Z, W, W);
        
        public readonly AnyVector3I WXX => new(W, X, X);
        public AnyVector3I WXY { readonly get => new(W, X, Y); set => (W, X, Y) = value; }
        public AnyVector3I WXZ { readonly get => new(W, X, Z); set => (W, X, Z) = value; }
        public readonly AnyVector3I WXW => new(W, X, W);
        public AnyVector3I WYX { readonly get => new(W, Y, X); set => (W, Y, X) = value; }
        public readonly AnyVector3I WYY => new(W, Y, Y);
        public AnyVector3I WYZ { readonly get => new(W, Y, Z); set => (W, Y, Z) = value; }
        public readonly AnyVector3I WYW => new(W, Y, W);
        public AnyVector3I WZX { readonly get => new(W, Z, X); set => (W, Z, X) = value; }
        public AnyVector3I WZY { readonly get => new(W, Z, Y); set => (W, Z, Y) = value; }
        public readonly AnyVector3I WZZ => new(W, Z, Z);
        public readonly AnyVector3I WZW => new(W, Z, W);
        public readonly AnyVector3I WWX => new(W, W, X);
        public readonly AnyVector3I WWY => new(W, W, Y);
        public readonly AnyVector3I WWZ => new(W, W, Z);
        public readonly AnyVector3I WWW => new(W, W, W);
        #endregion
        #region XXXX
        public readonly AnyVector4I XXXX => new(X, X, X, X);
        public readonly AnyVector4I XXXY => new(X, X, X, Y);
        public readonly AnyVector4I XXXZ => new(X, X, X, Z);
        public readonly AnyVector4I XXXW => new(X, X, X, W);
        public readonly AnyVector4I XXYX => new(X, X, Y, X);
        public readonly AnyVector4I XXYY => new(X, X, Y, Y);
        public readonly AnyVector4I XXYZ => new(X, X, Y, Z);
        public readonly AnyVector4I XXYW => new(X, X, Y, W);
        public readonly AnyVector4I XXZX => new(X, X, Z, X);
        public readonly AnyVector4I XXZY => new(X, X, Z, Y);
        public readonly AnyVector4I XXZZ => new(X, X, Z, Z);
        public readonly AnyVector4I XXZW => new(X, X, Z, W);
        public readonly AnyVector4I XXWX => new(X, X, W, X);
        public readonly AnyVector4I XXWY => new(X, X, W, Y);
        public readonly AnyVector4I XXWZ => new(X, X, W, Z);
        public readonly AnyVector4I XXWW => new(X, X, W, W);
        public readonly AnyVector4I XYXX => new(X, Y, X, X);
        public readonly AnyVector4I XYXY => new(X, Y, X, Y);
        public readonly AnyVector4I XYXZ => new(X, Y, X, Z);
        public readonly AnyVector4I XYXW => new(X, Y, X, W);
        public readonly AnyVector4I XYYX => new(X, Y, Y, X);
        public readonly AnyVector4I XYYY => new(X, Y, Y, Y);
        public readonly AnyVector4I XYYZ => new(X, Y, Y, Z);
        public readonly AnyVector4I XYYW => new(X, Y, Y, W);
        public readonly AnyVector4I XYZX => new(X, Y, Z, X);
        public readonly AnyVector4I XYZY => new(X, Y, Z, Y);
        public AnyVector4I XYZZ { readonly get => new(X, Y, Z, Z); set => (X, Y, Z, Z) = value; }
        public AnyVector4I XYZW { readonly get => new(X, Y, Z, W); set => (X, Y, Z, W) = value; }
        public readonly AnyVector4I XYWX => new(X, Y, W, X);
        public readonly AnyVector4I XYWY => new(X, Y, W, Y);
        public readonly AnyVector4I XYWZ => new(X, Y, W, Z);
        public readonly AnyVector4I XYWW => new(X, Y, W, W);
        public readonly AnyVector4I XZXX => new(X, Z, X, X);
        public readonly AnyVector4I XZXY => new(X, Z, X, Y);
        public readonly AnyVector4I XZXZ => new(X, Z, X, Z);
        public readonly AnyVector4I XZXW => new(X, Z, X, W);
        public readonly AnyVector4I XZYX => new(X, Z, Y, X);
        public readonly AnyVector4I XZYY => new(X, Z, Y, Y);
        public AnyVector4I XZYZ { readonly get => new(X, Z, Y, Z); set => (X, Z, Y, Z) = value; }
        public readonly AnyVector4I XZYW => new(X, Z, Y, W);
        public readonly AnyVector4I XZZX => new(X, Z, Z, X);
        public readonly AnyVector4I XZZY => new(X, Z, Z, Y);
        public readonly AnyVector4I XZZZ => new(X, Z, Z, Z);
        public readonly AnyVector4I XZZW => new(X, Z, Z, W);
        public AnyVector4I XZWX { readonly get => new(X, Z, W, X); set => (X, Z, W, X) = value; }
        public readonly AnyVector4I XZWY => new(X, Z, W, Y);
        public readonly AnyVector4I XZWZ => new(X, Z, W, Z);
        public readonly AnyVector4I XZWW => new(X, Z, W, W);
        public readonly AnyVector4I XWXX => new(X, W, X, X);
        public readonly AnyVector4I XWXY => new(X, W, X, Y);
        public readonly AnyVector4I XWXZ => new(X, W, X, Z);
        public readonly AnyVector4I XWXW => new(X, W, X, W);
        public readonly AnyVector4I XWYX => new(X, W, Y, X);
        public AnyVector4I XWYY { readonly get => new(X, W, Y, Y); set => (X, W, Y, Y) = value; }
        public readonly AnyVector4I XWYZ => new(X, W, Y, Z);
        public readonly AnyVector4I XWYW => new(X, W, Y, W);
        public AnyVector4I XWZX { readonly get => new(X, W, Z, X); set => (X, W, Z, X) = value; }
        public readonly AnyVector4I XWZY => new(X, W, Z, Y);
        public readonly AnyVector4I XWZZ => new(X, W, Z, Z);
        public readonly AnyVector4I XWZW => new(X, W, Z, W);
        public readonly AnyVector4I XWWX => new(X, W, W, X);
        public readonly AnyVector4I XWWY => new(X, W, W, Y);
        public readonly AnyVector4I XWWZ => new(X, W, W, Z);
        public readonly AnyVector4I XWWW => new(X, W, W, W);
        
        public readonly AnyVector4I YXXX => new(Y, X, X, X);
        public readonly AnyVector4I YXXY => new(Y, X, X, Y);
        public readonly AnyVector4I YXXZ => new(Y, X, X, Z);
        public readonly AnyVector4I YXXW => new(Y, X, X, W);
        public readonly AnyVector4I YXYX => new(Y, X, Y, X);
        public readonly AnyVector4I YXYY => new(Y, X, Y, Y);
        public readonly AnyVector4I YXYZ => new(Y, X, Y, Z);
        public readonly AnyVector4I YXYW => new(Y, X, Y, W);
        public readonly AnyVector4I YXZX => new(Y, X, Z, X);
        public readonly AnyVector4I YXZY => new(Y, X, Z, Y);
        public readonly AnyVector4I YXZZ => new(Y, X, Z, Z);
        public AnyVector4I YXZW { readonly get => new(Y, X, Z, W); set => (Y, X, Z, W) = value; }
        public readonly AnyVector4I YXWX => new(Y, X, W, X);
        public readonly AnyVector4I YXWY => new(Y, X, W, Y);
        public AnyVector4I YXWZ { readonly get => new(Y, X, W, Z); set => (Y, X, W, Z) = value; }
        public readonly AnyVector4I YXWW => new(Y, X, W, W);
        public readonly AnyVector4I YYXX => new(Y, Y, X, X);
        public readonly AnyVector4I YYXY => new(Y, Y, X, Y);
        public readonly AnyVector4I YYXZ => new(Y, Y, X, Z);
        public readonly AnyVector4I YYXW => new(Y, Y, X, W);
        public readonly AnyVector4I YYYX => new(Y, Y, Y, X);
        public readonly AnyVector4I YYYY => new(Y, Y, Y, Y);
        public readonly AnyVector4I YYYZ => new(Y, Y, Y, Z);
        public readonly AnyVector4I YYYW => new(Y, Y, Y, W);
        public readonly AnyVector4I YYZX => new(Y, Y, Z, X);
        public readonly AnyVector4I YYZY => new(Y, Y, Z, Y);
        public readonly AnyVector4I YYZZ => new(Y, Y, Z, Z);
        public readonly AnyVector4I YYZW => new(Y, Y, Z, W);
        public readonly AnyVector4I YYWX => new(Y, Y, W, X);
        public readonly AnyVector4I YYWY => new(Y, Y, W, Y);
        public readonly AnyVector4I YYWZ => new(Y, Y, W, Z);
        public readonly AnyVector4I YYWW => new(Y, Y, W, W);
        public readonly AnyVector4I YZXX => new(Y, Z, X, X);
        public readonly AnyVector4I YZXY => new(Y, Z, X, Y);
        public readonly AnyVector4I YZXZ => new(Y, Z, X, Z);
        public AnyVector4I YZXW { readonly get => new(Y, Z, X, W); set => (Y, Z, X, W) = value; }
        public readonly AnyVector4I YZYX => new(Y, Z, Y, X);
        public readonly AnyVector4I YZYY => new(Y, Z, Y, Y);
        public readonly AnyVector4I YZYZ => new(Y, Z, Y, Z);
        public readonly AnyVector4I YZYW => new(Y, Z, Y, W);
        public readonly AnyVector4I YZZX => new(Y, Z, Z, X);
        public readonly AnyVector4I YZZY => new(Y, Z, Z, Y);
        public readonly AnyVector4I YZZZ => new(Y, Z, Z, Z);
        public readonly AnyVector4I YZZW => new(Y, Z, Z, W);
        public AnyVector4I YZWX { readonly get => new(Y, Z, W, X); set => (Y, Z, W, X) = value; }
        public readonly AnyVector4I YZWY => new(Y, Z, W, Y);
        public readonly AnyVector4I YZWZ => new(Y, Z, W, Z);
        public readonly AnyVector4I YZWW => new(Y, Z, W, W);
        public readonly AnyVector4I YWXX => new(Y, W, X, X);
        public readonly AnyVector4I YWXY => new(Y, W, X, Y);
        public AnyVector4I YWXZ { readonly get => new(Y, W, X, Z); set => (Y, W, X, Z) = value; }
        public readonly AnyVector4I YWXW => new(Y, W, X, W);
        public readonly AnyVector4I YWYX => new(Y, W, Y, X);
        public readonly AnyVector4I YWYY => new(Y, W, Y, Y);
        public readonly AnyVector4I YWYZ => new(Y, W, Y, Z);
        public readonly AnyVector4I YWYW => new(Y, W, Y, W);
        public AnyVector4I YWZX { readonly get => new(Y, W, Z, X); set => (Y, W, Z, X) = value; }
        public readonly AnyVector4I YWZY => new(Y, W, Z, Y);
        public readonly AnyVector4I YWZZ => new(Y, W, Z, Z);
        public readonly AnyVector4I YWZW => new(Y, W, Z, W);
        public readonly AnyVector4I YWWX => new(Y, W, W, X);
        public readonly AnyVector4I YWWY => new(Y, W, W, Y);
        public readonly AnyVector4I YWWZ => new(Y, W, W, Z);
        public readonly AnyVector4I YWWW => new(Y, W, W, W);
        
        public readonly AnyVector4I ZXXX => new(Z, X, X, X);
        public readonly AnyVector4I ZXXY => new(Z, X, X, Y);
        public readonly AnyVector4I ZXXZ => new(Z, X, X, Z);
        public readonly AnyVector4I ZXXW => new(Z, X, X, W);
        public readonly AnyVector4I ZXYX => new(Z, X, Y, X);
        public readonly AnyVector4I ZXYY => new(Z, X, Y, Y);
        public readonly AnyVector4I ZXYZ => new(Z, X, Y, Z);
        public AnyVector4I ZXYW { readonly get => new(Z, X, Y, W); set => (Z, X, Y, W) = value; }
        public readonly AnyVector4I ZXZX => new(Z, X, Z, X);
        public readonly AnyVector4I ZXZY => new(Z, X, Z, Y);
        public readonly AnyVector4I ZXZZ => new(Z, X, Z, Z);
        public readonly AnyVector4I ZXZW => new(Z, X, Z, W);
        public readonly AnyVector4I ZXWX => new(Z, X, W, X);
        public AnyVector4I ZXWY { readonly get => new(Z, X, W, Y); set => (Z, X, W, Y) = value; }
        public readonly AnyVector4I ZXWZ => new(Z, X, W, Z);
        public readonly AnyVector4I ZXWW => new(Z, X, W, W);
        public readonly AnyVector4I ZYXX => new(Z, Y, X, X);
        public readonly AnyVector4I ZYXY => new(Z, Y, X, Y);
        public readonly AnyVector4I ZYXZ => new(Z, Y, X, Z);
        public AnyVector4I ZYXW { readonly get => new(Z, Y, X, W); set => (Z, Y, X, W) = value; }
        public readonly AnyVector4I ZYYX => new(Z, Y, Y, X);
        public readonly AnyVector4I ZYYY => new(Z, Y, Y, Y);
        public readonly AnyVector4I ZYYZ => new(Z, Y, Y, Z);
        public readonly AnyVector4I ZYYW => new(Z, Y, Y, W);
        public readonly AnyVector4I ZYZX => new(Z, Y, Z, X);
        public readonly AnyVector4I ZYZY => new(Z, Y, Z, Y);
        public readonly AnyVector4I ZYZZ => new(Z, Y, Z, Z);
        public readonly AnyVector4I ZYZW => new(Z, Y, Z, W);
        public AnyVector4I ZYWX { readonly get => new(Z, Y, W, X); set => (Z, Y, W, X) = value; }
        public readonly AnyVector4I ZYWY => new(Z, Y, W, Y);
        public readonly AnyVector4I ZYWZ => new(Z, Y, W, Z);
        public readonly AnyVector4I ZYWW => new(Z, Y, W, W);
        public readonly AnyVector4I ZZXX => new(Z, Z, X, X);
        public readonly AnyVector4I ZZXY => new(Z, Z, X, Y);
        public readonly AnyVector4I ZZXZ => new(Z, Z, X, Z);
        public readonly AnyVector4I ZZXW => new(Z, Z, X, W);
        public readonly AnyVector4I ZZYX => new(Z, Z, Y, X);
        public readonly AnyVector4I ZZYY => new(Z, Z, Y, Y);
        public readonly AnyVector4I ZZYZ => new(Z, Z, Y, Z);
        public readonly AnyVector4I ZZYW => new(Z, Z, Y, W);
        public readonly AnyVector4I ZZZX => new(Z, Z, Z, X);
        public readonly AnyVector4I ZZZY => new(Z, Z, Z, Y);
        public readonly AnyVector4I ZZZZ => new(Z, Z, Z, Z);
        public readonly AnyVector4I ZZZW => new(Z, Z, Z, W);
        public readonly AnyVector4I ZZWX => new(Z, Z, W, X);
        public readonly AnyVector4I ZZWY => new(Z, Z, W, Y);
        public readonly AnyVector4I ZZWZ => new(Z, Z, W, Z);
        public readonly AnyVector4I ZZWW => new(Z, Z, W, W);
        public readonly AnyVector4I ZWXX => new(Z, W, X, X);
        public AnyVector4I ZWXY { readonly get => new(Z, W, X, Y); set => (Z, W, X, Y) = value; }
        public readonly AnyVector4I ZWXZ => new(Z, W, X, Z);
        public readonly AnyVector4I ZWXW => new(Z, W, X, W);
        public AnyVector4I ZWYX { readonly get => new(Z, W, Y, X); set => (Z, W, Y, X) = value; }
        public readonly AnyVector4I ZWYY => new(Z, W, Y, Y);
        public readonly AnyVector4I ZWYZ => new(Z, W, Y, Z);
        public readonly AnyVector4I ZWYW => new(Z, W, Y, W);
        public readonly AnyVector4I ZWZX => new(Z, W, Z, X);
        public readonly AnyVector4I ZWZY => new(Z, W, Z, Y);
        public readonly AnyVector4I ZWZZ => new(Z, W, Z, Z);
        public readonly AnyVector4I ZWZW => new(Z, W, Z, W);
        public readonly AnyVector4I ZWWX => new(Z, W, W, X);
        public readonly AnyVector4I ZWWY => new(Z, W, W, Y);
        public readonly AnyVector4I ZWWZ => new(Z, W, W, Z);
        public readonly AnyVector4I ZWWW => new(Z, W, W, W);
        
        public readonly AnyVector4I WXXX => new(W, X, X, X);
        public readonly AnyVector4I WXXY => new(W, X, X, Y);
        public readonly AnyVector4I WXXZ => new(W, X, X, Z);
        public readonly AnyVector4I WXXW => new(W, X, X, W);
        public readonly AnyVector4I WXYX => new(W, X, Y, X);
        public readonly AnyVector4I WXYY => new(W, X, Y, Y);
        public AnyVector4I WXYZ { readonly get => new(W, X, Y, Z); set => (W, X, Y, Z) = value; }
        public readonly AnyVector4I WXYW => new(W, X, Y, W);
        public readonly AnyVector4I WXZX => new(W, X, Z, X);
        public AnyVector4I WXZY { readonly get => new(W, X, Z, Y); set => (W, X, Z, Y) = value; }
        public readonly AnyVector4I WXZZ => new(W, X, Z, Z);
        public readonly AnyVector4I WXZW => new(W, X, Z, W);
        public readonly AnyVector4I WXWX => new(W, X, W, X);
        public readonly AnyVector4I WXWY => new(W, X, W, Y);
        public readonly AnyVector4I WXWZ => new(W, X, W, Z);
        public readonly AnyVector4I WXWW => new(W, X, W, W);
        public readonly AnyVector4I WYXX => new(W, Y, X, X);
        public readonly AnyVector4I WYXY => new(W, Y, X, Y);
        public AnyVector4I WYXZ { readonly get => new(W, Y, X, Z); set => (W, Y, X, Z) = value; }
        public readonly AnyVector4I WYXW => new(W, Y, X, W);
        public readonly AnyVector4I WYYX => new(W, Y, Y, X);
        public readonly AnyVector4I WYYY => new(W, Y, Y, Y);
        public readonly AnyVector4I WYYZ => new(W, Y, Y, Z);
        public readonly AnyVector4I WYYW => new(W, Y, Y, W);
        public AnyVector4I WYZX { readonly get => new(W, Y, Z, X); set => (W, Y, Z, X) = value; }
        public readonly AnyVector4I WYZY => new(W, Y, Z, Y);
        public readonly AnyVector4I WYZZ => new(W, Y, Z, Z);
        public readonly AnyVector4I WYZW => new(W, Y, Z, W);
        public readonly AnyVector4I WYWX => new(W, Y, W, X);
        public readonly AnyVector4I WYWY => new(W, Y, W, Y);
        public readonly AnyVector4I WYWZ => new(W, Y, W, Z);
        public readonly AnyVector4I WYWW => new(W, Y, W, W);
        public readonly AnyVector4I WZXX => new(W, Z, X, X);
        public AnyVector4I WZXY { readonly get => new(W, Z, X, Y); set => (W, Z, X, Y) = value; }
        public readonly AnyVector4I WZXZ => new(W, Z, X, Z);
        public readonly AnyVector4I WZXW => new(W, Z, X, W);
        public AnyVector4I WZYX { readonly get => new(W, Z, Y, X); set => (W, Z, Y, X) = value; }
        public readonly AnyVector4I WZYY => new(W, Z, Y, Y);
        public readonly AnyVector4I WZYZ => new(W, Z, Y, Z);
        public readonly AnyVector4I WZYW => new(W, Z, Y, W);
        public readonly AnyVector4I WZZX => new(W, Z, Z, X);
        public readonly AnyVector4I WZZY => new(W, Z, Z, Y);
        public readonly AnyVector4I WZZZ => new(W, Z, Z, Z);
        public readonly AnyVector4I WZZW => new(W, Z, Z, W);
        public readonly AnyVector4I WZWX => new(W, Z, W, X);
        public readonly AnyVector4I WZWY => new(W, Z, W, Y);
        public readonly AnyVector4I WZWZ => new(W, Z, W, Z);
        public readonly AnyVector4I WZWW => new(W, Z, W, W);
        public readonly AnyVector4I WWXX => new(W, W, X, X);
        public readonly AnyVector4I WWXY => new(W, W, X, Y);
        public readonly AnyVector4I WWXZ => new(W, W, X, Z);
        public readonly AnyVector4I WWXW => new(W, W, X, W);
        public readonly AnyVector4I WWYX => new(W, W, Y, X);
        public readonly AnyVector4I WWYY => new(W, W, Y, Y);
        public readonly AnyVector4I WWYZ => new(W, W, Y, Z);
        public readonly AnyVector4I WWYW => new(W, W, Y, W);
        public readonly AnyVector4I WWZX => new(W, W, Z, X);
        public readonly AnyVector4I WWZY => new(W, W, Z, Y);
        public readonly AnyVector4I WWZZ => new(W, W, Z, Z);
        public readonly AnyVector4I WWZW => new(W, W, Z, W);
        public readonly AnyVector4I WWWX => new(W, W, W, X);
        public readonly AnyVector4I WWWY => new(W, W, W, Y);
        public readonly AnyVector4I WWWZ => new(W, W, W, Z);
        public readonly AnyVector4I WWWW => new(W, W, W, W);
        #endregion
        #endregion
        #region 构造
        public AnyVector4I(int x, int y, int z, int w) => (X, Y, Z, W) = (x, y, z, w);
        public AnyVector4I(int a) => (X, Y, Z, W) = (a, a, a, a);
        public AnyVector4I(AnyVector4I other) => (X, Y, Z, W) = (other.X, other.Y, other.Z, other.W);
        public AnyVector4I(int x, int y, AnyVector2I zw) => (X, Y, Z, W) = (x, y, zw.X, zw.Y);
        public AnyVector4I(int x, AnyVector2I yz, int w) => (X, Y, Z, W) = (x, yz.X, yz.Y, w);
        public AnyVector4I(AnyVector2I xy, int z, int w) => (X, Y, Z, W) = (xy.X, xy.Y, z, w);
        public AnyVector4I(AnyVector2I xy, AnyVector2I zw) => (X, Y, Z, W) = (xy.X, xy.Y, zw.X, zw.Y);
        public AnyVector4I(int x, AnyVector3I yzw) => (X, Y, Z, W) = (x, yzw.X, yzw.Y, yzw.Z);
        public AnyVector4I(AnyVector3I xyz, int w) => (X, Y, Z, W) = (xyz.X, xyz.Y, xyz.Z, w);
        #endregion
        #region statics
        private static readonly AnyVector4I _zero = new(0, 0, 0, 0);
        private static readonly AnyVector4I _one = new(1, 1, 1, 1);
        private static readonly AnyVector4I _minValue = new(int.MinValue, int.MinValue, int.MinValue, int.MinValue);
        private static readonly AnyVector4I _maxValue = new(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        private static readonly AnyVector4I _unitX = new(1, 0, 0, 0);
        private static readonly AnyVector4I _unitY = new(0, 1, 0, 0);
        private static readonly AnyVector4I _unitZ = new(0, 0, 1, 0);
        private static readonly AnyVector4I _unitW = new(0, 0, 0, 1);
        public static AnyVector4I Zero => _zero;
        public static AnyVector4I One => _one;
        public static AnyVector4I MinValue => _minValue;
        public static AnyVector4I MaxValue => _maxValue;
        public static AnyVector4I UnitX => _unitX;
        public static AnyVector4I UnitY => _unitY;
        public static AnyVector4I UnitZ => _unitZ;
        public static AnyVector4I UnitW => _unitW;
        #endregion
        #region 方法
        public readonly void Deconstruct(out int x, out int y, out int z, out int w) => (x, y, z, w) = (X, Y, Z, W);
        #region 长度相关 (长度, 距离)
        public readonly int LengthSquared() => X * X + Y * Y + Z * Z + W * W;
        public readonly long LongLengthSquared() => (long)X * X + (long)Y * Y + (long)Z * Z + (long)W * W;
        public readonly float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);
        public readonly float LongLength() => MathF.Sqrt((long)X * X + (long)Y * Y + (long)Z * Z + (long)W * W);
        public readonly int DistanceSquaredTo(AnyVector4I vector) => (this - vector).LengthSquared();
        public readonly long LongDistanceSquaredTo(AnyVector4I vector) => (this - vector).LongLengthSquared();
        public readonly float DistanceTo(AnyVector4I vector) => (this - vector).Length();
        public readonly float LongDistanceTo(AnyVector4I vector) => (this - vector).LongLength();
        public static int DistanceSquared(AnyVector4I left, AnyVector4I right) => left.DistanceSquaredTo(right);
        public static long LongDistanceSquared(AnyVector4I left, AnyVector4I right) => left.LongDistanceSquaredTo(right);
        public static float Distance(AnyVector4I left, AnyVector4I right) => left.DistanceTo(right);
        public static float LongDistance(AnyVector4I left, AnyVector4I right) => left.LongDistanceTo(right);
        #endregion
        #region 转化
        public readonly AnyVector4I Abs() => new(int.Abs(X), int.Abs(Y), int.Abs(Z), int.Abs(W));
        public readonly AnyVector4I Sign() => new(int.Sign(X), int.Sign(Y), int.Sign(Z), int.Sign(W));
        public readonly AnyVector4I CopySign(int sign) => new(int.CopySign(X, sign), int.CopySign(Y, sign), int.CopySign(Z, sign), int.CopySign(W, sign));
        public readonly AnyVector4I CopySign(AnyVector4I signv) => new(int.CopySign(X, signv.X), int.CopySign(Y, signv.Y), int.CopySign(Z, signv.Z), int.CopySign(W, signv.W));
        #endregion
        #region 限制
        public readonly bool IsBetweenO(AnyVector4I min, AnyVector4I max) => X.IsBetweenO(min.X, max.X) && Y.IsBetweenO(min.Y, max.Y) && Z.IsBetweenO(min.Z, max.Z) && W.IsBetweenO(min.W, max.W);
        public readonly bool IsBetweenI(AnyVector4I min, AnyVector4I max) => X.IsBetweenI(min.X, max.X) && Y.IsBetweenI(min.Y, max.Y) && Z.IsBetweenI(min.Z, max.Z) && W.IsBetweenI(min.W, max.W);
        public void ClampTo(AnyVector4I min, AnyVector4I max) { X.ClampTo(min.X, max.X); Y.ClampTo(min.Y, max.Y); Z.ClampTo(min.Z, max.Z); W.ClampTo(min.W, max.W); }
        public readonly AnyVector4I Clamp(AnyVector4I min, AnyVector4I max) => new(X.Clamp(min.X, max.X), Y.Clamp(min.Y, max.Y), Z.Clamp(min.Z, max.Z), W.Clamp(min.W, max.W));
        #endregion
        public readonly int Dot(AnyVector4I other) => X * other.X + Y * other.Y + Z * other.Z + W * other.W;
        public readonly long LongDot(AnyVector4I other) => (long)X * other.X + (long)Y * other.Y + (long)Z * other.Z + (long)W * other.W;
        #endregion
        #region 运算
        #region 相等
        public static bool operator ==(AnyVector4I left, AnyVector4I right) => left.Equals(right);
        public static bool operator !=(AnyVector4I left, AnyVector4I right) => !left.Equals(right);
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is AnyVector4I other && Equals(other);
        public readonly bool Equals(AnyVector4I other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);
        #endregion
        #region operators
        public static AnyVector4I operator +(AnyVector4I left, AnyVector4I right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        public static AnyVector4I operator +(AnyVector4I vec) => vec;
        public static AnyVector4I operator -(AnyVector4I left, AnyVector4I right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        public static AnyVector4I operator -(AnyVector4I vec) => new(-vec.X, -vec.Y, -vec.Z, -vec.W);
        public static AnyVector4I operator *(AnyVector4I vec, int scale) => new(vec.X * scale, vec.Y * scale, vec.Z * scale, vec.W * scale);
        public static AnyVector4I operator *(int scale, AnyVector4I vec) => new(vec.X * scale, vec.Y * scale, vec.Z * scale, vec.W * scale);
        public static AnyVector4I operator *(AnyVector4I left, AnyVector4I right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        public static AnyVector4I operator /(AnyVector4I vec, int divisor) => new(vec.X / divisor, vec.Y / divisor, vec.Z / divisor, vec.W / divisor);
        public static AnyVector4I operator /(AnyVector4I vec, AnyVector4I divisorv) => new(vec.X / divisorv.X, vec.Y / divisorv.Y, vec.Z / divisorv.Z, vec.W / divisorv.W);
        public static AnyVector4I operator %(AnyVector4I vec, int divisor) => new(vec.X % divisor, vec.Y % divisor, vec.Z % divisor, vec.W % divisor);
        public static AnyVector4I operator %(AnyVector4I vec, AnyVector4I divisorv) => new(vec.X % divisorv.X, vec.Y % divisorv.Y, vec.Z % divisorv.Z, vec.W % divisorv.W);
        public static bool operator <(AnyVector4I left, AnyVector4I right) => left.X != right.X ? left.X < right.X : left.Y != right.Y ? left.Y < right.Y : left.Z != right.Z ? left.Z < right.Z : left.W < right.W;
        public static bool operator >(AnyVector4I left, AnyVector4I right) => left.X != right.X ? left.X > right.X : left.Y != right.Y ? left.Y > right.Y : left.Z != right.Z ? left.Z > right.Z : left.W > right.W;
        public static bool operator <=(AnyVector4I left, AnyVector4I right) => left.X != right.X ? left.X < right.X : left.Y != right.Y ? left.Y < right.Y : left.Z != right.Z ? left.Z < right.Z : left.W <= right.W;
        public static bool operator >=(AnyVector4I left, AnyVector4I right) => left.X != right.X ? left.X > right.X : left.Y != right.Y ? left.Y > right.Y : left.Z != right.Z ? left.Z > right.Z : left.W >= right.W;
        #endregion
        #endregion
        #region 类型转换
        public override readonly string ToString() => $"({X}, {Y}, {Z}, {W})";
        public readonly string ToString(string? format) => $"({X.ToString(format)}, {Y.ToString(format)}, {Z.ToString(format)}, {W.ToString(format)})";
        public static implicit operator (int, int, int, int)(AnyVector4I vector) => (vector.X, vector.Y, vector.Z, vector.W);
        public static implicit operator AnyVector4I((int, int, int, int) tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
        public static explicit operator AnyVector2(AnyVector4I vector4) => new(vector4.X, vector4.Y);
        public static explicit operator AnyVector4I(AnyVector2 vector2) => new((int)vector2.X, (int)vector2.Y, 0, 0);
        public static explicit operator AnyVector2I(AnyVector4I vector4) => new(vector4.X, vector4.Y);
        public static explicit operator AnyVector4I(AnyVector2I vector2) => new(vector2.X, vector2.Y, 0, 0);
        public static explicit operator AnyVector3(AnyVector4I vector4) => new(vector4.X, vector4.Y, vector4.Z);
        public static explicit operator AnyVector4I(AnyVector3 vector3) => new((int)vector3.X, (int)vector3.Y, (int)vector3.Z, 0);
        public static explicit operator AnyVector3I(AnyVector4I vector4) => new(vector4.X, vector4.Y, vector4.Z);
        public static explicit operator AnyVector4I(AnyVector3I vector3) => new(vector3.X, vector3.Y, vector3.Z, 0);
        public static implicit operator AnyVector4(AnyVector4I vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        public static explicit operator AnyVector4I(AnyVector4 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z, (int)vector.W);
#if XNA
        public static explicit operator XNAVector2(AnyVector4I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector4I(XNAVector2 vector) => new((int)vector.X, (int)vector.Y, 0, 0);
        public static explicit operator XNAVector3(AnyVector4I vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator AnyVector4I(XNAVector3 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z, 0);
        public static implicit operator XNAVector4(AnyVector4I vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        public static explicit operator AnyVector4I(XNAVector4 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z, (int)vector.W);
        public static explicit operator XNAVector2I(AnyVector4I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector4I(XNAVector2I vector) => new(vector.X, vector.Y, 0, 0);
#endif
#if GODOT
        public static explicit operator GDVector2(AnyVector4I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector4I(GDVector2 vector) => new((int)vector.X, (int)vector.Y, 0, 0);
        public static explicit operator GDVector3(AnyVector4I vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator AnyVector4I(GDVector3 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z, 0);
        public static implicit operator GDVector4(AnyVector4I vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        public static explicit operator AnyVector4I(GDVector4 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z, (int)vector.W);
        public static explicit operator GDVector2I(AnyVector4I vector) => new(vector.X, vector.Y);
        public static explicit operator AnyVector4I(GDVector2I vector) => new(vector.X, vector.Y, 0, 0);
        public static explicit operator GDVector3I(AnyVector4I vector) => new(vector.X, vector.Y, vector.Z);
        public static explicit operator AnyVector4I(GDVector3I vector) => new(vector.X, vector.Y, vector.Z, 0);
        public static implicit operator GDVector4I(AnyVector4I vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        public static implicit operator AnyVector4I(GDVector4I vector) => new(vector.X, vector.Y, vector.Z, vector.W);
#endif
        #endregion
    }
}
