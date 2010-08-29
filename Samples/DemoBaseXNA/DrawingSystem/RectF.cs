using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    /// <summary>
    /// Rectangle with floating point values.
    /// Based on System.Drawing.RectangleF.
    /// Needed because there is no System.Drawing on Xbox360.
    /// Differences -
    ///   o No PointF, used Vector2 instead.
    ///   o No Culture support. Easily added in if you need it.
    ///   o No Serialization. Might be added in later.
    /// </summary>
    public struct RectF
    {
        public static readonly RectF Empty;
        private float _x;
        private float _y;
        private float _width;
        private float _height;

        public RectF(float x, float y, float width, float height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public static RectF FromLTRB(float left, float top, float right, float bottom)
        {
            return new RectF(left, top, right - left, bottom - top);
        }

        public Vector2 Location { get { return new Vector2(X, Y); }  set { X = value.X; Y = value.Y; } }

        public Vector2 Size {  get {  return new Vector2(Width, Height);} set {  Width = value.X;  Height = value.Y; } }
        
        public float X { get { return _x; } set {  _x = value; } }
        
        public float Y { get { return _y; } set {  _y = value;  } }
       
        public float Width { get{ return _width; } set { _width = value; } }
        
        public float Height { get { return _height; } set { _height = value; } }

        public float Left { get { return X; } }

        public float Top { get { return Y; }}

        public float Right { get { return (X + Width); } }

        public float Bottom { get { return (Y + Height); } }

        public bool IsEmpty { get { if (Width > 0f) { return (Height <= 0f); } return true; } }

        public override bool Equals(object obj)
        {
            if (!(obj is RectF))
            {
                return false;
            }
            RectF ef = (RectF)obj;
            return ((((ef.X == X) && (ef.Y == Y)) && (ef.Width == Width)) && (ef.Height == Height));
        }

        public static bool operator ==(RectF left, RectF right)
        {
            return ((((left.X == right.X) && (left.Y == right.Y)) && (left.Width == right.Width)) && (left.Height == right.Height));
        }

        public static bool operator !=(RectF left, RectF right)
        {
            return !(left == right);
        }

        public bool Contains(float x, float y)
        {
            return ((((X <= x) && (x < (X + Width))) && (Y <= y)) && (y < (Y + Height)));
        }

        public bool Contains(Vector2 pt)
        {
            return Contains(pt.X, pt.Y);
        }

        public bool Contains(RectF rect)
        {
            return ((((X <= rect.X) && ((rect.X + rect.Width) <= (X + Width))) && (Y <= rect.Y)) && ((rect.Y + rect.Height) <= (Y + Height)));
        }

        public override int GetHashCode()
        {
            return (int)(((((uint)X) ^ ((((uint)Y) << 13) | (((uint)Y) >> 0x13))) ^ ((((uint)Width) << 0x1a) | (((uint)Width) >> 6))) ^ ((((uint)Height) << 7) | (((uint)Height) >> 0x19)));
        }

        public void Inflate(float x, float y)
        {
            X -= x;
            Y -= y;
            Width += 2f * x;
            Height += 2f * y;
        }

        public void Inflate(Vector2 size)
        {
            Inflate(size.X, size.Y);
        }

        public static RectF Inflate(RectF rect, float x, float y)
        {
            RectF ef = rect;
            ef.Inflate(x, y);
            return ef;
        }

        public void Intersect(RectF rect)
        {
            RectF ef = Intersect(rect, this);
            X = ef.X;
            Y = ef.Y;
            Width = ef.Width;
            Height = ef.Height;
        }

        public static RectF Intersect(RectF a, RectF b)
        {
            float x = Math.Max(a.X, b.X);
            float num2 = Math.Min((float)(a.X + a.Width), (float)(b.X + b.Width));
            float y = Math.Max(a.Y, b.Y);
            float num4 = Math.Min((float)(a.Y + a.Height), (float)(b.Y + b.Height));
            if ((num2 >= x) && (num4 >= y))
            {
                return new RectF(x, y, num2 - x, num4 - y);
            }
            return Empty;
        }

        public bool IntersectsWith(RectF rect)
        {
            return ((((rect.X < (X + Width)) && (X < (rect.X + rect.Width))) && (rect.Y < (Y + Height))) && (Y < (rect.Y + rect.Height)));
        }

        public static RectF Union(RectF a, RectF b)
        {
            float x = Math.Min(a.X, b.X);
            float num2 = Math.Max((float)(a.X + a.Width), (float)(b.X + b.Width));
            float y = Math.Min(a.Y, b.Y);
            float num4 = Math.Max((float)(a.Y + a.Height), (float)(b.Y + b.Height));
            return new RectF(x, y, num2 - x, num4 - y);
        }

        public void Offset(Vector2 pos)
        {
            Offset(pos.X, pos.Y);
        }

        public void Offset(float x, float y)
        {
            X += x;
            Y += y;
        }

        public static implicit operator RectF(Rectangle r)
        {
            return new RectF((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
        }

        public override string ToString()
        {
            return ("{X=" + X.ToString() + ",Y=" + Y.ToString() + ",Width=" + Width.ToString() + ",Height=" + Height.ToString() + "}");
        }

        static RectF()
        {
            Empty = new RectF();
        }
    }
}