using System;

#if XNA
using XNAVector2 = Microsoft.Xna.Framework.Vector2;
using XNAVector3 = Microsoft.Xna.Framework.Vector3;
using XNAVector4 = Microsoft.Xna.Framework.Vector4;
#endif

namespace TigerUtilsLib;

partial class TigerExtensions {
    #region ClampDistance
#if XNA
    public static XNAVector2 ClampDistance(this XNAVector2 self, XNAVector2 origin, float distance) {
        return distance <= 0 ? origin :
            XNAVector2.DistanceSquared(self, origin) <= distance * distance ? self :
            origin + (self - origin).SafeNormalized(XNAVector2.Zero) * distance;
    }
    public static XNAVector3 ClampDistance(this XNAVector3 self, XNAVector3 origin, float distance) {
        return distance <= 0 ? origin :
            XNAVector3.DistanceSquared(self, origin) <= distance * distance ? self :
            origin + (self - origin).SafeNormalized(XNAVector3.Zero) * distance;
    }
    public static XNAVector4 ClampDistance(this XNAVector4 self, XNAVector4 origin, float distance) {
        return distance <= 0 ? origin :
            XNAVector4.DistanceSquared(self, origin) <= distance * distance ? self :
            origin + (self - origin).SafeNormalized(XNAVector4.Zero) * distance;
    }
#endif
    #endregion
    #region Normalized
#if XNA
    public static XNAVector2 Normalized(this XNAVector2 self) => XNAVector2.Normalize(self);
    public static XNAVector3 Normalized(this XNAVector3 self) => XNAVector3.Normalize(self);
    public static XNAVector4 Normalized(this XNAVector4 self) => XNAVector4.Normalize(self);
#endif
    #endregion
    #region SafeNormalize
#if XNA
#if TERRARIA
    internal
#else
    public
#endif
    static XNAVector2 SafeNormalized(this XNAVector2 self, XNAVector2 defaultValue = default) {
        return self == XNAVector2.Zero || self.HasNaNs() ? defaultValue : self.Normalized();
    }
    public static XNAVector3 SafeNormalized(this XNAVector3 self, XNAVector3 defaultValue = default) {
        return self == XNAVector3.Zero || self.HasNaNs() ? defaultValue : self.Normalized();
    }
    public static XNAVector4 SafeNormalized(this XNAVector4 self, XNAVector4 defaultValue = default) {
        return self == XNAVector4.Zero || self.HasNaNs() ? defaultValue : self.Normalized();
    }
#endif
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
#endif
    #endregion
    #region Between
#if XNA
    public static bool BetweenO(this XNAVector2 v, XNAVector2 min, XNAVector2 max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y);
    public static bool BetweenO(this XNAVector3 v, XNAVector3 min, XNAVector3 max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y) && v.Z.IsBetweenO(min.Z, max.Z);
    public static bool BetweenO(this XNAVector4 v, XNAVector4 min, XNAVector4 max) => v.X.IsBetweenO(min.X, max.X) && v.Y.IsBetweenO(min.Y, max.Y) && v.Z.IsBetweenO(min.Z, max.Z) && v.W.IsBetweenO(min.W, max.W);
    public static bool BetweenI(this XNAVector2 v, XNAVector2 min, XNAVector2 max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y);
    public static bool BetweenI(this XNAVector3 v, XNAVector3 min, XNAVector3 max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y) && v.Z.IsBetweenI(min.Z, max.Z);
    public static bool BetweenI(this XNAVector4 v, XNAVector4 min, XNAVector4 max) => v.X.IsBetweenI(min.X, max.X) && v.Y.IsBetweenI(min.Y, max.Y) && v.Z.IsBetweenI(min.Z, max.Z) && v.W.IsBetweenI(min.W, max.W);
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
    #endregion
    #region Vector2 旋转相关
#if XNA
#if TERRARIA
    internal
#else
    public
#endif
    static float ToRotation(XNAVector2 v) => MathF.Atan2(v.Y, v.X);
    public static XNAVector2 ToRotationXNAVector2(this float f) => new(MathF.Cos(f), MathF.Sin(f));
    public static XNAVector2 Rotated(this XNAVector2 spinningPoint, float radians, XNAVector2 center = default) => spinningPoint.Rotate(radians, center);
    public static ref XNAVector2 Rotate(ref this XNAVector2 spinningPoint, float radians, XNAVector2 center = default) {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        XNAVector2 delta = spinningPoint - center;
        spinningPoint = center + new XNAVector2(delta.X * cos - delta.Y * sin, delta.X * sin + delta.Y * cos);
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
    #endregion
}
