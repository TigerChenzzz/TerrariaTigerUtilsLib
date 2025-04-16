#if XNA
using XNARectangleI = Microsoft.Xna.Framework.Rectangle;
#endif

namespace TigerUtilsLib;

partial class TigerUtils {
#if XNA
    public static XNARectangleI NewXNARectangleI(AnyVector2 position, AnyVector2 size, AnyVector2 anchor = default)
        => NewXNARectangleI(position.X, position.Y, size.X, size.Y, anchor.X, anchor.Y);
    public static XNARectangleI NewXNARectangleI(int x, int y, int width, int height, float anchorX, float anchorY)
        => new((int)(x - anchorX * width), (int)(y - anchorY * height), width, height);
    public static XNARectangleI NewXNARectangleI(float x, float y, float width, float height, float anchorX, float anchorY)
        => new((int)(x - anchorX * width), (int)(y - anchorY * height), (int)width, (int)height);
    public static XNARectangleI NewXNARectangleI(float x, float y, float width, float height) => new((int)x, (int)y, (int)width, (int)height);
#endif
}

partial class TigerExtensions {
#if XNA
    public static void Deconstruct(this XNARectangleI rectangle, out int x, out int y, out int width, out int height)
        => (x, y, width, height) = (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
#endif
}
