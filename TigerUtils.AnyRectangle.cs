using System;
using System.Diagnostics.CodeAnalysis;

#if XNA
using XNARectangleI = Microsoft.Xna.Framework.Rectangle;
#endif
#if GODOT
using GDRectangle = Godot.Rect2;
using GDRectangleI = Godot.Rect2I;
#endif

namespace TigerUtilsLib;

partial class TigerClasses {
    public struct AnyRectangle : IEquatable<AnyRectangle> {
        #region 字段和属性
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public float this[int index] {
            readonly get {
                return index switch {
                    0 => X,
                    1 => Y,
                    2 => Width,
                    3 => Height,
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
                    Width = value;
                    break;
                case 3:
                    Height = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #region 一维
        public float Left { readonly get => X; set => X = value; }
        public float Right { readonly get => X + Width; set => X = value - Width; }
        public float CenterX { readonly get => X + Width / 2; set => X = value - Width / 2; }
        public float Top { readonly get => Y; set => Y = value; }
        public float Bottom { readonly get => Y + Height; set => Y = value - Height; }
        public float CenterY { readonly get => Y + Height / 2; set => Y = value - Height / 2; }
        #endregion
        #region 二维
        public AnyVector2 Position { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2 Size { readonly get => new(Width, Height); set => (Width, Height) = value; }
        #region 锚点
        public AnyVector2 LeftTop { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2 TopLeft { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2 CenterTop { readonly get => new(CenterX, Y); set => (CenterX, Y) = value; }
        public AnyVector2 TopCenter { readonly get => new(CenterX, Y); set => (CenterX, Y) = value; }
        public AnyVector2 RightTop { readonly get => new(Right, Y); set => (Right, Y) = value; }
        public AnyVector2 TopRight { readonly get => new(Right, Y); set => (Right, Y) = value; }

        public AnyVector2 LeftCenter { readonly get => new(X, CenterY); set => (X, CenterY) = value; }
        public AnyVector2 CenterLeft { readonly get => new(X, CenterY); set => (X, CenterY) = value; }
        public AnyVector2 Center { readonly get => new(CenterX, CenterY); set => (CenterX, CenterY) = value; }
        public AnyVector2 RightCenter { readonly get => new(Right, CenterY); set => (Right, CenterY) = value; }
        public AnyVector2 CenterRight { readonly get => new(Right, CenterY); set => (Right, CenterY) = value; }

        public AnyVector2 LeftBottom { readonly get => new(X, Bottom); set => (X, Bottom) = value; }
        public AnyVector2 BottomLeft { readonly get => new(X, Bottom); set => (X, Bottom) = value; }
        public AnyVector2 CenterBottom { readonly get => new(CenterX, Bottom); set => (CenterX, Bottom) = value; }
        public AnyVector2 BottomCenter { readonly get => new(CenterX, Bottom); set => (CenterX, Bottom) = value; }
        public AnyVector2 RightBottom { readonly get => new(Right, Bottom); set => (Right, Bottom) = value; }
        public AnyVector2 BottomRight { readonly get => new(Right, Bottom); set => (Right, Bottom) = value; }
        #endregion
        public readonly float Area => Width * Height;
        #endregion
        #endregion
        #region 构造
        public AnyRectangle(float x, float y, float width, float height) => (X, Y, Width, Height) = (x, y, width, height);
        public AnyRectangle(AnyVector2 position, AnyVector2 size) => ((X, Y), (Width, Height)) = (position, size);
        public AnyRectangle(AnyVector4 vector4) => (X, Y, Width, Height) = vector4;
        public AnyRectangle(float x, float y, AnyVector2 size) => (X, Y, (Width, Height)) = (x, y, size);
        public AnyRectangle(AnyVector2 position, float width, float height) => ((X, Y), Width, Height) = (position, width, height);
        #endregion
        #region 方法
        #region 包含
        public readonly bool Contains(float x, float y) => x >= X && y >= Y && x <= X + Width && y <= Y + Height;
        public readonly bool Contains(AnyVector2 point) => Contains(point.X, point.Y);
        public readonly bool ContainsI(float x, float y) => x > X && y > Y && x < X + Width && y < Y + Height;
        public readonly bool ContainsI(AnyVector2 point) => ContainsI(point.X, point.Y);
        #endregion
        #region Grow
        public void Grow(float by) {
            X -= by;
            Y -= by;
            Width += by * 2;
            Height += by * 2;
        }
        public void Grow(float x, float y) {
            X -= x;
            Y -= y;
            Width += x * 2;
            Height += y * 2;
        }
        public void Grow(AnyVector2 vector2) => Grow(vector2.X, vector2.Y);
        public void Grow(float left, float top, float right, float bottom) {
            X -= left;
            Y -= top;
            Width += left + right;
            Height += top + bottom;
        }
        public readonly AnyRectangle Grown(float by) {
            var result = this;
            result.Grow(by);
            return result;
        }
        public readonly AnyRectangle Grown(float x, float y) {
            var result = this;
            result.Grow(x, y);
            return result;
        }
        public readonly AnyRectangle Grown(AnyVector2 vector2) {
            var result = this;
            result.Grow(vector2);
            return result;
        }
        public readonly AnyRectangle Grown(float left, float top, float right, float bottom) {
            var result = this;
            result.Grow(left, top, right, bottom);
            return result;
        }
        #endregion
        #endregion
        #region 相等
        public static bool operator ==(AnyRectangle left, AnyRectangle right) => left.Equals(right);
        public static bool operator !=(AnyRectangle left, AnyRectangle right) => !left.Equals(right);
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is AnyRectangle other && Equals(other);
        public readonly bool Equals(AnyRectangle other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
        #endregion
        #region 类型转换
        #region ToString
        public override readonly string ToString() => $"({X}, {Y}, {Width}, {Height})";
        public readonly string ToString(string? format) => $"({X.ToString(format)}, {Y.ToString(format)}, {Width.ToString(format)}, {Height.ToString(format)})";
        public readonly string ToString(string? xFormat, string? yFormat, string? widthFormat, string? heightFormat) => $"({X.ToString(xFormat)}, {Y.ToString(yFormat)}, {Width.ToString(widthFormat)}, {Height.ToString(heightFormat)})";
        #endregion
        public static implicit operator AnyVector4(AnyRectangle rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        public static implicit operator AnyRectangle(AnyVector4 vector4) => new(vector4.X, vector4.Y, vector4.Z, vector4.W);
        public static implicit operator Rect(AnyRectangle rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        public static implicit operator AnyRectangle(Rect rect) => new(rect.X, rect.Y, rect.Width, rect.Height);
#if XNA
        public static explicit operator XNARectangleI(AnyRectangle rectangle) => NewXNARectangleI(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        public static implicit operator AnyRectangle(XNARectangleI rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
#endif
#if GODOT
        public static implicit operator GDRectangle(AnyRectangle rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        public static implicit operator AnyRectangle(GDRectangle rectangle) => new(rectangle.Position.X, rectangle.Position.Y, rectangle.Size.X, rectangle.Size.Y);
        public static explicit operator GDRectangleI(AnyRectangle rectangle) => new((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
        public static implicit operator AnyRectangle(GDRectangleI rectangle) => new(rectangle.Position.X, rectangle.Position.Y, rectangle.Size.X, rectangle.Size.Y);
#endif
        #endregion
    }
    public struct AnyRectangleI : IEquatable<AnyRectangleI> {
        #region 字段和属性
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int this[int index] {
            readonly get {
                return index switch {
                    0 => X,
                    1 => Y,
                    2 => Width,
                    3 => Height,
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
                    Width = value;
                    break;
                case 3:
                    Height = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
        #region 一维
        public int Left { readonly get => X; set => X = value; }
        public int Right { readonly get => X + Width; set => X = value - Width; }
        public int CenterX { readonly get => X + Width / 2; set => X = value - Width / 2; }
        public int Top { readonly get => Y; set => Y = value; }
        public int Bottom { readonly get => Y + Height; set => Y = value - Height; }
        public int CenterY { readonly get => Y + Height / 2; set => Y = value - Height / 2; }
        #endregion
        #region 二维
        public AnyVector2I Position { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2I Size { readonly get => new(Width, Height); set => (Width, Height) = value; }
        #region 锚点
        public AnyVector2I LeftTop { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2I TopLeft { readonly get => new(X, Y); set => (X, Y) = value; }
        public AnyVector2I CenterTop { readonly get => new(CenterX, Y); set => (CenterX, Y) = value; }
        public AnyVector2I TopCenter { readonly get => new(CenterX, Y); set => (CenterX, Y) = value; }
        public AnyVector2I RightTop { readonly get => new(Right, Y); set => (Right, Y) = value; }
        public AnyVector2I TopRight { readonly get => new(Right, Y); set => (Right, Y) = value; }

        public AnyVector2I LeftCenter { readonly get => new(X, CenterY); set => (X, CenterY) = value; }
        public AnyVector2I CenterLeft { readonly get => new(X, CenterY); set => (X, CenterY) = value; }
        public AnyVector2I Center { readonly get => new(CenterX, CenterY); set => (CenterX, CenterY) = value; }
        public AnyVector2I RightCenter { readonly get => new(Right, CenterY); set => (Right, CenterY) = value; }
        public AnyVector2I CenterRight { readonly get => new(Right, CenterY); set => (Right, CenterY) = value; }

        public AnyVector2I LeftBottom { readonly get => new(X, Bottom); set => (X, Bottom) = value; }
        public AnyVector2I BottomLeft { readonly get => new(X, Bottom); set => (X, Bottom) = value; }
        public AnyVector2I CenterBottom { readonly get => new(CenterX, Bottom); set => (CenterX, Bottom) = value; }
        public AnyVector2I BottomCenter { readonly get => new(CenterX, Bottom); set => (CenterX, Bottom) = value; }
        public AnyVector2I RightBottom { readonly get => new(Right, Bottom); set => (Right, Bottom) = value; }
        public AnyVector2I BottomRight { readonly get => new(Right, Bottom); set => (Right, Bottom) = value; }
        #endregion
        public readonly int Area => Width * Height;
        public readonly long LongArea => (long)Width * Height;
        #endregion
        #endregion
        #region 构造
        public AnyRectangleI(int x, int y, int width, int height) => (X, Y, Width, Height) = (x, y, width, height);
        public AnyRectangleI(AnyVector2I position, AnyVector2I size) => ((X, Y), (Width, Height)) = (position, size);
        public AnyRectangleI(AnyVector4I vector4) => (X, Y, Width, Height) = vector4;
        public AnyRectangleI(int x, int y, AnyVector2I size) => (X, Y, (Width, Height)) = (x, y, size);
        public AnyRectangleI(AnyVector2I position, int width, int height) => ((X, Y), Width, Height) = (position, width, height);
        public static AnyRectangleI CreateFromFloat(float x, float y, float width, float height) => new((int)x, (int)y, (int)width, (int)height);
        #endregion
        #region 方法
        #region 包含
        public readonly bool Contains(int x, int y) => x >= X && y >= Y && x <= X + Width && y <= Y + Height;
        public readonly bool Contains(AnyVector2I point) => Contains(point.X, point.Y);
        public readonly bool ContainsI(int x, int y) => x > X && y > Y && x < X + Width && y < Y + Height;
        public readonly bool ContainsI(AnyVector2I point) => ContainsI(point.X, point.Y);
        #endregion
        #region Grow
        public void Grow(int by) {
            X -= by;
            Y -= by;
            Width += by * 2;
            Height += by * 2;
        }
        public void Grow(int x, int y) {
            X -= x;
            Y -= y;
            Width += x * 2;
            Height += y * 2;
        }
        public void Grow(AnyVector2I vector2) => Grow(vector2.X, vector2.Y);
        public void Grow(int left, int top, int right, int bottom) {
            X -= left;
            Y -= top;
            Width += left + right;
            Height += top + bottom;
        }
        public readonly AnyRectangleI Grown(int by) {
            var result = this;
            result.Grow(by);
            return result;
        }
        public readonly AnyRectangleI Grown(int x, int y) {
            var result = this;
            result.Grow(x, y);
            return result;
        }
        public readonly AnyRectangleI Grown(AnyVector2I vector2) {
            var result = this;
            result.Grow(vector2);
            return result;
        }
        public readonly AnyRectangleI Grown(int left, int top, int right, int bottom) {
            var result = this;
            result.Grow(left, top, right, bottom);
            return result;
        }
        #endregion
        #endregion
        #region 相等
        public static bool operator ==(AnyRectangleI left, AnyRectangleI right) => left.Equals(right);
        public static bool operator !=(AnyRectangleI left, AnyRectangleI right) => !left.Equals(right);
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is AnyRectangleI other && Equals(other);
        public readonly bool Equals(AnyRectangleI other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
        #endregion
        #region 类型转换
        #region ToString
        public override readonly string ToString() => $"({X}, {Y}, {Width}, {Height})";
        public readonly string ToString(string? format) => $"({X.ToString(format)}, {Y.ToString(format)}, {Width.ToString(format)}, {Height.ToString(format)})";
        public readonly string ToString(string? xFormat, string? yFormat, string? widthFormat, string? heightFormat) => $"({X.ToString(xFormat)}, {Y.ToString(yFormat)}, {Width.ToString(widthFormat)}, {Height.ToString(heightFormat)})";
        #endregion
        public static implicit operator AnyVector4I(AnyRectangleI rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        public static implicit operator AnyRectangleI(AnyVector4I vector4) => new(vector4.X, vector4.Y, vector4.Z, vector4.W);
        public static implicit operator AnyRectangle(AnyRectangleI rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        public static explicit operator AnyRectangleI(AnyRectangle rectangle) => CreateFromFloat(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
#if XNA
        public static implicit operator XNARectangleI(AnyRectangleI rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        public static implicit operator AnyRectangleI(XNARectangleI rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
#endif
#if GODOT
        public static implicit operator GDRectangle(AnyRectangleI rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        public static explicit operator AnyRectangleI(GDRectangle rectangle) => CreateFromFloat(rectangle.Position.X, rectangle.Position.Y, rectangle.Size.X, rectangle.Size.Y);
        public static implicit operator GDRectangleI(AnyRectangleI rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        public static implicit operator AnyRectangleI(GDRectangleI rectangle) => new(rectangle.Position.X, rectangle.Position.Y, rectangle.Size.X, rectangle.Size.Y);
#endif
        #endregion
    }
}
