using System;

#if XNA
using XNARectangleI = Microsoft.Xna.Framework.Rectangle;
#endif

namespace TigerUtilsLib;

partial class TigerClasses {
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
    public struct DirectedLine(AnyVector2 start, AnyVector2 end) {
        public DirectedLine(AnyVector2 end) : this(AnyVector2.Zero, end) { }
        public DirectedLine(float startX, float startY, float endX, float endY) : this(new AnyVector2(startX, startY), new AnyVector2(endX, endY)) { }
        public AnyVector2 Start { readonly get => start; set => start = value; }
        public AnyVector2 End { readonly get => end; set => end = value; }
        public AnyVector2 Delta { readonly get => end - start; set => end = start + value; }

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
        public readonly AnyVector2? GetCollidePosition(Rect rect) {
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
            return new(GetXOnLineByYF(collideY), collideY);
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
                AnyVector2 middle = new(rangeRight, middleY);
                return reverse ? new DirectedLine(middle, end) : new DirectedLine(start, middle);
            }
            if (rightOver) {
                float middleY = GetYOnLineByXF(rangeLeft);
                AnyVector2 middle = new(rangeLeft, middleY);
                return reverse ? new DirectedLine(start, middle) : new DirectedLine(middle, end);
            }
            float middleY1 = GetYOnLineByXF(rangeLeft);
            float middleY2 = GetYOnLineByXF(rangeRight);
            AnyVector2 middle1 = new(rangeLeft, middleY1);
            AnyVector2 middle2 = new(rangeRight, middleY2);
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
                AnyVector2 middle = new(middleX, rangeBottom);
                return reverse ? new DirectedLine(middle, end) : new DirectedLine(start, middle);
            }
            if (topOver) {
                float middleX = GetXOnLineByYF(rangeTop);
                AnyVector2 middle = new(middleX, rangeTop);
                return reverse ? new DirectedLine(start, middle) : new DirectedLine(middle, end);
            }
            float middleX1 = GetXOnLineByYF(rangeTop);
            float middleX2 = GetXOnLineByYF(rangeBottom);
            AnyVector2 middle1 = new(middleX1, rangeTop);
            AnyVector2 middle2 = new(middleX2, rangeBottom);
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

        public static implicit operator DirectedLine((AnyVector2, AnyVector2) tuple) {
            return new(tuple.Item1, tuple.Item2);
        }
    }
    /// <summary>
    /// 代表一个矩形, 允许长和宽为负数
    /// </summary>
    public struct Rect(float x, float y, float width, float height) {
        public Rect(AnyVector2 position, AnyVector2 size) : this(position.X, position.Y, size.X, size.Y) { }
        public AnyVector2 Position { readonly get => new(x, y); set => (x, y) = value; }
        public AnyVector2 Size { readonly get => new(width, height); set => (width, height) = value; }
        public float X { readonly get => x; set => x = value; }
        public float Y { readonly get => y; set => y = value; }
        public float Width { readonly get => width; set => width = value; }
        public float Height { readonly get => height; set => height = value; }
        public float Left { readonly get => x; set => x = value; }
        public float Right { readonly get => x + width; set => x = value - width; }
        public float Top { readonly get => y; set => y = value; }
        public float Bottom { readonly get => y + height; set => y = value - height; }
        public AnyVector2 RealSize { readonly get => new(RealWidth, RealHeight); set => (RealWidth, RealHeight) = value; }
        public float RealWidth { readonly get => MathF.Abs(width); set => width = ToInt(width >= 0) * value; }
        public float RealHeight { readonly get => MathF.Abs(height); set => height = ToInt(height >= 0) * value; }
        public float RealLeft { readonly get => width >= 0 ? x : (x + width); set => x = width >= 0 ? value : (value - width); }
        public float RealRight { readonly get => width <= 0 ? x : (x + width); set => x = width <= 0 ? value : (value - width); }
        public float RealTop { readonly get => height >= 0 ? y : (y + height); set => y = height >= 0 ? value : (value - height); }
        public float RealBottom { readonly get => height <= 0 ? y : (y + height); set => y = height <= 0 ? value : (value - height); }

        public AnyVector2 Center { readonly get => new(x + width / 2, y + height / 2); set => (x, y) = (value.X - width / 2, value.Y - height / 2); }

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
        public readonly bool Collide(AnyVector2 point) {
            GetRange(out float left, out float right, out float top, out float bottom);
            return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
        }
        public readonly bool CollideI(AnyVector2 point) {
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
        public readonly float Distance(AnyVector2 point) {
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
        public readonly float DistanceSquared(AnyVector2 point) {
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

#if XNA
        public static explicit operator XNARectangleI(Rect rect) => NewXNARectangleI(rect.X, rect.Y, rect.Width, rect.Height);
        public static implicit operator Rect(XNARectangleI rectangle) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
#endif
    }
    public struct Circle {
        public float CenterX { readonly get; set; }
        public float CenterY { readonly get; set; }
        public AnyVector2 Center { readonly get => new(CenterX, CenterY); set => (CenterX, CenterY) = value; }
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
        public Circle(AnyVector2 center, float radius) : this(center.X, center.Y, radius) { }
        /// <summary>
        /// 外切矩形
        /// </summary>
        public readonly Rect EnclosingRect => new(CenterX - Radius, CenterY - Radius, 2 * Radius, 2 * Radius);
        #region Collide
        public readonly bool Collide(AnyVector2 point) {
            return Center.DistanceSquaredTo(point) <= Radius * Radius;
        }
        public readonly bool CollideI(AnyVector2 point) {
            return Center.DistanceSquaredTo(point) < Radius * Radius;
        }
        public readonly bool CollideO(AnyVector2 point) {
            return Center.DistanceSquaredTo(point) == Radius * Radius;
        }
        public readonly bool Collide(DirectedLine line) => line.Collide(this);
        public readonly bool CollideI(DirectedLine line) => line.CollideI(this);
        public readonly bool CollideO(DirectedLine line) => line.CollideO(this);
        public readonly bool CollideOI(DirectedLine line) => line.CollideOI(this);
        public readonly bool Collide(Circle circle) {
            var radiusSum = Radius + circle.Radius;
            return Center.DistanceSquaredTo(circle.Center) <= radiusSum * radiusSum;
        }
        public readonly bool CollideI(Circle circle) {
            var radiusSum = Radius + circle.Radius;
            return Center.DistanceSquaredTo(circle.Center) < radiusSum * radiusSum;
        }
        public readonly bool Collide(Rect rect) => rect.Collide(this);
        public readonly bool CollideI(Rect rect) => rect.CollideI(this);
        public readonly bool Contains(AnyVector2 point) {
            return Center.DistanceSquaredTo(point) <= Radius * Radius;
        }
        public readonly bool ContainsI(AnyVector2 point) {
            return Center.DistanceSquaredTo(point) < Radius * Radius;
        }
        #endregion
        #region Distance
        public readonly float Distance(AnyVector2 point) {
            return (Center.DistanceTo(point) - Radius).WithMin(0);
        }
        public readonly float Distance(Circle circle) {
            return (Center.DistanceTo(circle.Center) - Radius - circle.Radius).WithMin(0);
        }
        public readonly float Distance(Rect rect) {
            return rect.Distance(this);
        }
        #endregion
    }
}
