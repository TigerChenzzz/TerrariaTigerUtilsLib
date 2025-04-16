using System;

#if XNA
using XNAVector2 = Microsoft.Xna.Framework.Vector2;
using XNAVector3 = Microsoft.Xna.Framework.Vector3;
using XNAVector4 = Microsoft.Xna.Framework.Vector4;
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

partial class TigerExtensions {
    #region ClampDistance
#if XNA
    public static XNAVector2 ClampDistance(this XNAVector2 self, XNAVector2 origin, float distance) {
        return distance <= 0 ? origin : XNAVector2.DistanceSquared(self, origin) <= distance * distance ? self
            : origin + (self - origin).SafeNormalized(XNAVector2.Zero) * distance;
    }
    public static XNAVector3 ClampDistance(this XNAVector3 self, XNAVector3 origin, float distance) {
        return distance <= 0 ? origin : XNAVector3.DistanceSquared(self, origin) <= distance * distance ? self
            : origin + (self - origin).SafeNormalized(XNAVector3.Zero) * distance;
    }
    public static XNAVector4 ClampDistance(this XNAVector4 self, XNAVector4 origin, float distance) {
        return distance <= 0 ? origin : XNAVector4.DistanceSquared(self, origin) <= distance * distance ? self
            : origin + (self - origin).SafeNormalized(XNAVector4.Zero) * distance;
    }
#endif
#if GODOT
    public static GDVector2 ClampDistance(this GDVector2 self, GDVector2 origin, float distance) {
        return distance <= 0 ? origin : self.DistanceSquaredTo(origin) <= distance * distance ? self
            : origin + (self - origin).Normalized() * distance;
    }
    public static GDVector3 ClampDistance(this GDVector3 self, GDVector3 origin, float distance) {
        return distance <= 0 ? origin : self.DistanceSquaredTo(origin) <= distance * distance ? self
            : origin + (self - origin).Normalized() * distance;
    }
    public static GDVector4 ClampDistance(this GDVector4 self, GDVector4 origin, float distance) {
        return distance <= 0 ? origin : self.DistanceSquaredTo(origin) <= distance * distance ? self
            : origin + (self - origin).Normalized() * distance;
    }
#endif
    #endregion
    #region Normalized
#if XNA
    public static XNAVector2 Normalized(this XNAVector2 self) => XNAVector2.Normalize(self);
    public static XNAVector3 Normalized(this XNAVector3 self) => XNAVector3.Normalize(self);
    public static XNAVector4 Normalized(this XNAVector4 self) => XNAVector4.Normalize(self);
#endif
    // GDVector 自带 Normalized
    #endregion
    #region SafeNormalize
#if XNA
#if TERRARIA
    internal
#else
    public
#endif
    static XNAVector2 SafeNormalized(this XNAVector2 self, XNAVector2 defaultValue = default) {
        return self == XNAVector2.Zero || !self.IsFinite() ? defaultValue : self.Normalized();
    }
    public static XNAVector3 SafeNormalized(this XNAVector3 self, XNAVector3 defaultValue = default) {
        return self == XNAVector3.Zero || !self.IsFinite() ? defaultValue : self.Normalized();
    }
    public static XNAVector4 SafeNormalized(this XNAVector4 self, XNAVector4 defaultValue = default) {
        return self == XNAVector4.Zero || !self.IsFinite() ? defaultValue : self.Normalized();
    }
#endif
#if GODOT
    public static GDVector2 SafeNormalized(this GDVector2 self, GDVector2 defaultValue = default) {
        return self == GDVector2.Zero || !self.IsFinite() ? defaultValue : self.Normalized();
    }
    public static GDVector3 SafeNormalized(this GDVector3 self, GDVector3 defaultValue = default) {
        return self == GDVector3.Zero || !self.IsFinite() ? defaultValue : self.Normalized();
    }
    public static GDVector4 SafeNormalized(this GDVector4 self, GDVector4 defaultValue = default) {
        return self == GDVector4.Zero || !self.IsFinite() ? defaultValue : self.Normalized();
    }
#endif
    #endregion
    #region IsFinite
#if XNA
    public static bool IsFinite(this XNAVector2 vec) => float.IsFinite(vec.X) && float.IsFinite(vec.Y);
    public static bool IsFinite(this XNAVector3 vec) => float.IsFinite(vec.X) && float.IsFinite(vec.Y) && float.IsFinite(vec.Z);
    public static bool IsFinite(this XNAVector4 vec) => float.IsFinite(vec.X) && float.IsFinite(vec.Y) && float.IsFinite(vec.Z) && float.IsFinite(vec.W);
#endif
    // GDVector 自带 IsFinite
    #endregion
    #region HasNaNs
#if XNA
#if TERRARIA
    internal
#else
    public
#endif
    static bool HasNaNs(this XNAVector2 vec) => float.IsNaN(vec.X) || float.IsNaN(vec.Y);
    public static bool HasNaNs(this XNAVector3 vec) => float.IsNaN(vec.X) || float.IsNaN(vec.Y) || float.IsNaN(vec.Z);
    public static bool HasNaNs(this XNAVector4 vec) => float.IsNaN(vec.X) || float.IsNaN(vec.Y) || float.IsNaN(vec.Z) || float.IsNaN(vec.W);
#endif
#if GODOT
    public static bool HasNaNs(this GDVector2 vec) => float.IsNaN(vec.X) || float.IsNaN(vec.Y);
    public static bool HasNaNs(this GDVector3 vec) => float.IsNaN(vec.X) || float.IsNaN(vec.Y) || float.IsNaN(vec.Z);
    public static bool HasNaNs(this GDVector4 vec) => float.IsNaN(vec.X) || float.IsNaN(vec.Y) || float.IsNaN(vec.Z) || float.IsNaN(vec.W);
#endif
    #endregion
    #region Floor
#if XNA
#if TERRARIA
    internal
#else
    public
#endif
    static XNAVector2 Floor(this XNAVector2 v) => new(MathF.Floor(v.X), MathF.Floor(v.Y));
    public static XNAVector3 Floor(this XNAVector3 v) => new(MathF.Floor(v.X), MathF.Floor(v.Y), MathF.Floor(v.Z));
    public static XNAVector4 Floor(this XNAVector4 v) => new(MathF.Floor(v.X), MathF.Floor(v.Y), MathF.Floor(v.Z), MathF.Floor(v.W));
    public static AnyVector2I FloorI(this XNAVector2 v) => new((int)MathF.Floor(v.X), (int)MathF.Floor(v.Y));
    public static AnyVector3I FloorI(this XNAVector3 v) => new((int)MathF.Floor(v.X), (int)MathF.Floor(v.Y), (int)MathF.Floor(v.Z));
    public static AnyVector4I FloorI(this XNAVector4 v) => new((int)MathF.Floor(v.X), (int)MathF.Floor(v.Y), (int)MathF.Floor(v.Z), (int)MathF.Floor(v.W));
#endif
#if GODOT
    // GDVector 自带 Floor
    public static GDVector2I FloorI(this GDVector2 v) => new((int)MathF.Floor(v.X), (int)MathF.Floor(v.Y));
    public static GDVector3I FloorI(this GDVector3 v) => new((int)MathF.Floor(v.X), (int)MathF.Floor(v.Y), (int)MathF.Floor(v.Z));
    public static GDVector4I FloorI(this GDVector4 v) => new((int)MathF.Floor(v.X), (int)MathF.Floor(v.Y), (int)MathF.Floor(v.Z), (int)MathF.Floor(v.W));
#endif
    #endregion
    #region IsBetween
#if XNA
    public static bool IsBetweenO(this XNAVector2 v, XNAVector2 min, XNAVector2 max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y);
    public static bool IsBetweenO(this XNAVector3 v, XNAVector3 min, XNAVector3 max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y) && v.Z.IsBetweenO(min.Z, max.Z);
    public static bool IsBetweenO(this XNAVector4 v, XNAVector4 min, XNAVector4 max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y) && v.Z.IsBetweenO(min.Z, max.Z) && v.W.IsBetweenO(min.W, max.W);
    public static bool IsBetweenI(this XNAVector2 v, XNAVector2 min, XNAVector2 max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y);
    public static bool IsBetweenI(this XNAVector3 v, XNAVector3 min, XNAVector3 max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y) && v.Z.IsBetweenI(min.Z, max.Z);
    public static bool IsBetweenI(this XNAVector4 v, XNAVector4 min, XNAVector4 max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y) && v.Z.IsBetweenI(min.Z, max.Z) && v.W.IsBetweenI(min.W, max.W);
#endif
#if GODOT
    public static bool IsBetweenO(this GDVector2 v, GDVector2 min, GDVector2 max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y);
    public static bool IsBetweenO(this GDVector3 v, GDVector3 min, GDVector3 max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y) && v.Z.IsBetweenO(min.Z, max.Z);
    public static bool IsBetweenO(this GDVector4 v, GDVector4 min, GDVector4 max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y) && v.Z.IsBetweenO(min.Z, max.Z) && v.W.IsBetweenO(min.W, max.W);
    public static bool IsBetweenI(this GDVector2 v, GDVector2 min, GDVector2 max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y);
    public static bool IsBetweenI(this GDVector3 v, GDVector3 min, GDVector3 max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y) && v.Z.IsBetweenI(min.Z, max.Z);
    public static bool IsBetweenI(this GDVector4 v, GDVector4 min, GDVector4 max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y) && v.Z.IsBetweenI(min.Z, max.Z) && v.W.IsBetweenI(min.W, max.W);
    public static bool IsBetweenO(this GDVector2I v, GDVector2I min, GDVector2I max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y);
    public static bool IsBetweenO(this GDVector3I v, GDVector3I min, GDVector3I max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y) && v.Z.IsBetweenO(min.Z, max.Z);
    public static bool IsBetweenO(this GDVector4I v, GDVector4I min, GDVector4I max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y) && v.Z.IsBetweenO(min.Z, max.Z) && v.W.IsBetweenO(min.W, max.W);
    public static bool IsBetweenI(this GDVector2I v, GDVector2I min, GDVector2I max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y);
    public static bool IsBetweenI(this GDVector3I v, GDVector3I min, GDVector3I max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y) && v.Z.IsBetweenI(min.Z, max.Z);
    public static bool IsBetweenI(this GDVector4I v, GDVector4I min, GDVector4I max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y) && v.Z.IsBetweenI(min.Z, max.Z) && v.W.IsBetweenI(min.W, max.W);
#endif
    #endregion
    #region Distance & DistanceSquared
#if XNA
#if TERRARIA
    internal
#else
    public
#endif
    static float DistanceTo(this XNAVector2 origin, XNAVector2 target) => XNAVector2.Distance(origin, target);
    public static float DistanceTo(this XNAVector3 origin, XNAVector3 target) => XNAVector3.Distance(origin, target);
    public static float DistanceTo(this XNAVector4 origin, XNAVector4 target) => XNAVector4.Distance(origin, target);
    public static float DistanceSquaredTo(this XNAVector2 origin, XNAVector2 target) => XNAVector2.DistanceSquared(origin, target);
    public static float DistanceSquaredTo(this XNAVector3 origin, XNAVector3 target) => XNAVector3.DistanceSquared(origin, target);
    public static float DistanceSquaredTo(this XNAVector4 origin, XNAVector4 target) => XNAVector4.DistanceSquared(origin, target);
#endif
    // GDVector[I] 自带 DistanceTo 和 DistanceSquaredTo
    #endregion
    #region Vector2 旋转相关
    public static AnyVector2 ToRotationAnyVector2(this float f) => new(MathF.Cos(f), MathF.Sin(f));
#if XNA
#if TERRARIA
    internal
#else
    public
#endif
    static float ToRotation(XNAVector2 v) => MathF.Atan2(v.Y, v.X);
    public static XNAVector2 ToRotationXNAVector2(this float f) => new(MathF.Cos(f), MathF.Sin(f));
    public static XNAVector2 Rotated(this XNAVector2 spinningPoint, float radians, XNAVector2 center) => spinningPoint.Rotate(radians, center);
    public static XNAVector2 Rotated(this XNAVector2 spinningPoint, float radians) => spinningPoint.Rotate(radians);
    public static ref XNAVector2 Rotate(ref this XNAVector2 spinningPoint, float radians, XNAVector2 center) {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        XNAVector2 delta = spinningPoint - center;
        spinningPoint = center + new XNAVector2(delta.X * cos - delta.Y * sin, delta.X * sin + delta.Y * cos);
        return ref spinningPoint;
    }
    public static ref XNAVector2 Rotate(ref this XNAVector2 spinningPoint, float radians) {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        spinningPoint = new(spinningPoint.X * cos - spinningPoint.Y * sin, spinningPoint.X * sin + spinningPoint.Y * cos);
        return ref spinningPoint;
    }
#endif
#if GODOT
    public static float ToRotation(GDVector2 v) => v.Angle();
    public static GDVector2 ToRotationGDVector2(this float f) => new(MathF.Cos(f), MathF.Sin(f));
    public static GDVector2 Rotated(this GDVector2 spinningPoint, float radians, GDVector2 center) => spinningPoint.Rotate(radians, center);
    // GDVector 自带 Rotated(angle)
    public static ref GDVector2 Rotate(ref this GDVector2 spinningPoint, float radians, GDVector2 center) {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        GDVector2 delta = spinningPoint - center;
        spinningPoint = center + new GDVector2(delta.X * cos - delta.Y * sin, delta.X * sin + delta.Y * cos);
        return ref spinningPoint;
    }
    public static ref GDVector2 Rotate(ref this GDVector2 spinningPoint, float radians) {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        spinningPoint = new(spinningPoint.X * cos - spinningPoint.Y * sin, spinningPoint.X * sin + spinningPoint.Y * cos);
        return ref spinningPoint;
    }
#endif
    #endregion
    #region 解构拓展
#if XNA
    public static void Deconstruct(this XNAVector2 vector2, out float x, out float y) => (x, y) = (vector2.X, vector2.Y);
    public static void Deconstruct(this XNAVector3 vector3, out float x, out float y, out float z) => (x, y, z) = (vector3.X, vector3.Y, vector3.Z);
    public static void Deconstruct(this XNAVector4 vector3, out float x, out float y, out float z, out float w) => (x, y, z, w) = (vector3.X, vector3.Y, vector3.Z, vector3.W);
#endif
    // GDVector[I] 自带 Deconstruct
    #endregion
}
