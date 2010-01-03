using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

[DebuggerDisplay("Count = {Count}")]
public class Vertices : List<Vector2>
{
    public Vertices()
    {

    }

    public Vertices(int capacity)
    {
        Capacity = capacity;
    }

    public Vertices(ref Vector2[] vector2)
    {
        for (int i = 0; i < vector2.Length; i++)
        {
            Add(vector2[i]);
        }
    }

    public Vertices(IList<Vector2> vertices)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            Add(vertices[i]);
        }
    }

    public Vector2[] GetVerticesArray()
    {
        return ToArray();
    }
}