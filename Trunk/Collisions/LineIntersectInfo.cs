using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;

//TODO: This is only used by CollisionHelper, remove it or use it.
//Could possibly be a struct

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