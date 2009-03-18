using System.Collections.Generic;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics.Collisions
{
    /// <summary>
    /// Encapsulates the collision details between a line and a Geom.
    /// </summary>
    public class LineIntersectInfo
    {
        private Geom _geom;
        private List<Vector2> _points;

        public LineIntersectInfo(Geom geom, List<Vector2> points)
        {
            _geom = geom;
            _points = points;
        }

        public List<Vector2> Points
        {
            get { return _points; }
        }

        public Geom Geom
        {
            get { return _geom; }
        }
    }
}