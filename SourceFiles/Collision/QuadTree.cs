using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

public class QTElement<T>
{
    public AABB Span;
    public T Value;
    public QuadTree<T> Parent;

    public QTElement(T value, AABB span)
    {
        Span = span;
        Value = value;
        Parent = null;
    }

}

public class QuadTree<T> : IBroadPhaseBackend
{
    public AABB Span;
    public List<QTElement<T>> QTNodes;
    public QuadTree<T>[] SubTrees;

    public int MaxBucket;
    public int MaxDepth;

    public bool IsPartitioned
    {
        get { return SubTrees != null; }
    }

    public QuadTree(AABB span, int maxbucket, int maxdepth)
    {
        Span = span;
        QTNodes = new List<QTElement<T>>();

        MaxBucket = maxbucket;
        MaxDepth = maxdepth;
    }

    /// <summary>
    /// returns the quadrant of span that entirely contains test. if none, return 0.
    /// </summary>
    /// <param name="span"></param>
    /// <param name="test"></param>
    /// <returns></returns>
    private int partition(AABB span, AABB test)
    {
        if (span.Q1.Contains(ref test)) return 1;
        if (span.Q2.Contains(ref test)) return 2;
        if (span.Q3.Contains(ref test)) return 3;
        if (span.Q4.Contains(ref test)) return 4;

        return 0;
    }

    public void AddNode(QTElement<T> node)
    {
        if (!IsPartitioned)
        {

            if (QTNodes.Count >= MaxBucket && MaxDepth > 0) //bin is full and can still subdivide
            {
                //
                //partition into quadrants and sort existing nodes amonst quads.
                //
                QTNodes.Add(node); //treat new node just like other nodes for partitioning

                SubTrees = new QuadTree<T>[4];
                SubTrees[0] = new QuadTree<T>(this.Span.Q1, MaxBucket, MaxDepth - 1);
                SubTrees[1] = new QuadTree<T>(this.Span.Q2, MaxBucket, MaxDepth - 1);
                SubTrees[2] = new QuadTree<T>(this.Span.Q3, MaxBucket, MaxDepth - 1);
                SubTrees[3] = new QuadTree<T>(this.Span.Q4, MaxBucket, MaxDepth - 1);

                var remNodes = new List<QTElement<T>>(); //nodes that are not fully contained by any quadrant

                foreach (var n in QTNodes)
                {
                    switch (partition(this.Span, n.Span))
                    {
                        case 1: //quadrant 1
                            SubTrees[0].AddNode(n);
                            break;
                        case 2:
                            SubTrees[1].AddNode(n);
                            break;
                        case 3:
                            SubTrees[2].AddNode(n);
                            break;
                        case 4:
                            SubTrees[3].AddNode(n);
                            break;
                        case 0:
                            n.Parent = this;
                            remNodes.Add(n);
                            break;
                    }
                }

                QTNodes = remNodes;
            }
            else
            {
                node.Parent = this;
                QTNodes.Add(node); //if bin is not yet full or max depth has been reached, just add the node without subdividing
            }

        }
        else //we already have children nodes
        {
            //
            //add node to specific sub-tree
            //
            switch (partition(this.Span, node.Span))
            {
                case 1: //quadrant 1
                    SubTrees[0].AddNode(node);
                    break;
                case 2:
                    SubTrees[1].AddNode(node);
                    break;
                case 3:
                    SubTrees[2].AddNode(node);
                    break;
                case 4:
                    SubTrees[3].AddNode(node);
                    break;
                case 0:
                    node.Parent = this;
                    QTNodes.Add(node);
                    break;

            }
        }
    }

    public void QueryR(AABB searchR, ref List<QTElement<T>> hits)
    {
        if (searchR.Contains(ref this.Span))
        {
            GetAllNodesR(ref hits);
        }
        else if (AABB.TestOverlap(ref searchR, ref this.Span))
        {
            foreach (var n in QTNodes)
                if (AABB.TestOverlap(searchR, n.Span)) hits.Add(n);

            if (IsPartitioned)
                foreach (var st in SubTrees) st.QueryR(searchR, ref hits);
        }
    }

    public List<QTElement<T>> Query(AABB searchR)
    {
        var hits = new List<QTElement<T>>();
        QueryR(searchR, ref hits);
        return hits;
    }

    public void QueryP(Predicate<AABB> selector, ref List<QTElement<T>> hits)
    {
        if (selector(this.Span))
        {
            foreach (var n in QTNodes)
                if (selector(n.Span)) hits.Add(n);

            if (IsPartitioned)
                foreach (var st in SubTrees) st.QueryP(selector, ref hits);
        }
    }

    /// <summary>
    /// tests if ray intersects AABB
    /// </summary>
    /// <param name="aabb"></param>
    /// <param name="rayDir"></param>
    /// <returns></returns>
    public static bool RayCastAABB(AABB aabb, Vector2 p1, Vector2 p2)
    {
        AABB segmentAABB = new AABB();
        {
            Vector2.Min(ref p1, ref p2, out segmentAABB.LowerBound);
            Vector2.Max(ref p1, ref p2, out segmentAABB.UpperBound);
        }
        if (!AABB.TestOverlap(aabb, segmentAABB)) return false;

        var rayDir = p2 - p1;
        var rayPos = p1;

        var norm = new Vector2(-rayDir.Y, rayDir.X); //normal to ray
        if (norm.Length() == 0.0) return true; //if ray is just a point, return true (iff point is within aabb, as tested earlier)
        norm.Normalize();

        var dPos = Vector2.Dot(rayPos, norm);

        var verts = aabb.GetVertices();
        var d0 = Vector2.Dot(verts[0], norm) - dPos;
        for (int i = 1; i < 4; i++)
        {
            var d = Vector2.Dot(verts[i], norm) - dPos;
            if (Math.Sign(d) != Math.Sign(d0)) //return true if the ray splits the vertices (ie: sign of dot products with normal are not all same)
                return true;
        }

        return false;
    }

    public void Query_Callback(Func<QTElement<T>, bool> callback, ref AABB searchR)
    {
        Stack<QuadTree<T>> stack = new Stack<QuadTree<T>>();
        stack.Push(this);

        while (stack.Count > 0)
        {
            var qt = stack.Pop();
            if (AABB.TestOverlap(ref searchR, ref qt.Span))
            {
                foreach (var n in QTNodes)
                    if (AABB.TestOverlap(ref searchR, ref n.Span))
                    {
                        if (!callback(n)) return;
                    }

                if (IsPartitioned)
                    foreach (var st in SubTrees) stack.Push(st);
            }
        }
    }

    public void RayCast(Func<RayCastInput, QTElement<T>, float> callback, ref RayCastInput input)
    {
        Stack<QuadTree<T>> stack = new Stack<QuadTree<T>>();
        stack.Push(this);

        var maxFraction = input.MaxFraction;
        var p1 = input.Point1;
        var p2 = p1 + (input.Point2 - input.Point1) * maxFraction;

        while (stack.Count > 0)
        {
            var qt = stack.Pop();

            if (RayCastAABB(qt.Span, p1, p2))
            {
                foreach (var n in QTNodes)
                {
                    if (RayCastAABB(n.Span, p1, p2))
                    {
                        RayCastInput subInput;
                        subInput.Point1 = input.Point1;
                        subInput.Point2 = input.Point2;
                        subInput.MaxFraction = maxFraction;

                        float value = callback(subInput, n);
                        if (value == 0.0f)
                            return; // the client has terminated the raycast.

                        if (value > 0.0f)
                        {
                            maxFraction = value;
                            p2 = p1 + (input.Point2 - input.Point1) * maxFraction; //update segment endpoint
                        }

                    }
                }
                if (IsPartitioned)
                    foreach (var st in SubTrees) stack.Push(st);
            }
        }
    }

    public void GetAllNodesR(ref List<QTElement<T>> nodes)
    {
        foreach (var n in QTNodes) nodes.Add(n);

        if (IsPartitioned)
            foreach (var st in SubTrees) st.GetAllNodesR(ref nodes);
    }

    public void RemoveNode(QTElement<T> node)
    {
        node.Parent.QTNodes.Remove(node);

        //TODO: probably faster to reconstruct tree than to recursivly fix
        //this.Reconstruct();
    }

    public void RemoveNodes(List<QTElement<T>> nodes)
    {
        nodes.ForEach(n => n.Parent.QTNodes.Remove(n));

        Reconstruct();
    }

    public void Reconstruct()
    {
        List<QTElement<T>> allNodes = new List<QTElement<T>>();
        this.GetAllNodesR(ref allNodes);

        QTNodes = new List<QTElement<T>>();
        SubTrees = null;

        allNodes.ForEach(n => this.AddNode(n));
    }

    public void Query(Func<int, bool> callback, ref AABB aabb)
    {
        throw new NotImplementedException();
    }

    private Func<RayCastInput, QTElement<FixtureProxy>, float> transformRayCallback(
    Func<RayCastInput, int, float> callback)
    {
        Func<RayCastInput, QTElement<FixtureProxy>, float> newCallback =
            (RayCastInput input, QTElement<FixtureProxy> qtnode) => callback(input, qtnode.Value.ProxyId);
        return newCallback;
    }

    public void RayCast(Func<RayCastInput, int, float> callback, ref RayCastInput input)
    {
        throw new NotImplementedException();
    }
}