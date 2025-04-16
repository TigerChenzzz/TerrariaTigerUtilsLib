#if XNA
using XNARectangleI = Microsoft.Xna.Framework.Rectangle;
#endif
#if GODOT
using GDRectangle = Godot.Rect2;
using GDRectangleI = Godot.Rect2I;
#endif

namespace TigerUtilsLib;

partial class TigerUtils {
    #region NewRectangleI
#if XNA
    public static XNARectangleI NewXNARectangleI(AnyVector2 position, AnyVector2 size, AnyVector2 anchor = default)
        => NewXNARectangleI(position.X, position.Y, size.X, size.Y, anchor.X, anchor.Y);
    public static XNARectangleI NewXNARectangleI(int x, int y, int width, int height, float anchorX, float anchorY)
        => new((int)(x - anchorX * width), (int)(y - anchorY * height), width, height);
    public static XNARectangleI NewXNARectangleI(float x, float y, float width, float height, float anchorX, float anchorY)
        => new((int)(x - anchorX * width), (int)(y - anchorY * height), (int)width, (int)height);
    public static XNARectangleI NewXNARectangleI(float x, float y, float width, float height) => new((int)x, (int)y, (int)width, (int)height);
#endif
#if GODOT
    public static GDRectangleI NewGDRectangleI(AnyVector2 position, AnyVector2 size, AnyVector2 anchor = default)
        => NewGDRectangleI(position.X, position.Y, size.X, size.Y, anchor.X, anchor.Y);
    public static GDRectangleI NewGDRectangleI(int x, int y, int width, int height, float anchorX, float anchorY)
        => new((int)(x - anchorX * width), (int)(y - anchorY * height), width, height);
    public static GDRectangleI NewGDRectangleI(float x, float y, float width, float height, float anchorX, float anchorY)
        => new((int)(x - anchorX * width), (int)(y - anchorY * height), (int)width, (int)height);
    public static GDRectangleI NewGDRectangleI(float x, float y, float width, float height) => new((int)x, (int)y, (int)width, (int)height);
#endif
    #endregion
}

partial class TigerExtensions {
    #region Deconstruct
#if XNA
    public static void Deconstruct(this XNARectangleI rectangle, out int x, out int y, out int width, out int height)
        => (x, y, width, height) = (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
#endif
#if GODOT
    public static void Deconstruct(this GDRectangle rectangle, out float x, out float y, out float width, out float height)
        => (x, y, width, height) = (rectangle.Position.X, rectangle.Position.Y, rectangle.Size.X, rectangle.Size.Y);
    public static void Deconstruct(this GDRectangle rectangle, out AnyVector2 position, out AnyVector2 size)
        => (position, size) = (rectangle.Position, rectangle.Size);
    public static void Deconstruct(this GDRectangleI rectangle, out int x, out int y, out int width, out int height)
        => (x, y, width, height) = (rectangle.Position.X, rectangle.Position.Y, rectangle.Size.X, rectangle.Size.Y);
    public static void Deconstruct(this GDRectangleI rectangle, out AnyVector2I position, out AnyVector2I size)
        => (position, size) = (rectangle.Position, rectangle.Size);
#endif
    #endregion
}
