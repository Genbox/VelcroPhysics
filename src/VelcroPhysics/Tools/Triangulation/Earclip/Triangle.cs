using Microsoft.Xna.Framework;
using VelcroPhysics.Shared;

namespace VelcroPhysics.Tools.Triangulation.Earclip {
    public class Triangle : Vertices
    {
        //Constructor automatically fixes orientation to ccw
        public Triangle(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            float cross = (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
            if (cross > 0)
            {
                Add(new Vector2(x1, y1));
                Add(new Vector2(x2, y2));
                Add(new Vector2(x3, y3));
            }
            else
            {
                Add(new Vector2(x1, y1));
                Add(new Vector2(x3, y3));
                Add(new Vector2(x2, y2));
            }
        }

        public bool IsInside(float x, float y)
        {
            Vector2 a = this[0];
            Vector2 b = this[1];
            Vector2 c = this[2];

            if (x < a.X && x < b.X && x < c.X) return false;
            if (x > a.X && x > b.X && x > c.X) return false;
            if (y < a.Y && y < b.Y && y < c.Y) return false;
            if (y > a.Y && y > b.Y && y > c.Y) return false;

            float vx2 = x - a.X;
            float vy2 = y - a.Y;
            float vx1 = b.X - a.X;
            float vy1 = b.Y - a.Y;
            float vx0 = c.X - a.X;
            float vy0 = c.Y - a.Y;

            float dot00 = vx0 * vx0 + vy0 * vy0;
            float dot01 = vx0 * vx1 + vy0 * vy1;
            float dot02 = vx0 * vx2 + vy0 * vy2;
            float dot11 = vx1 * vx1 + vy1 * vy1;
            float dot12 = vx1 * vx2 + vy1 * vy2;
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            return ((u > 0) && (v > 0) && (u + v < 1));
        }
    }
}