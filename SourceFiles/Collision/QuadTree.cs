using System;
using System.Collections.Generic;

namespace FarseerPhysics.Collision
{
    public class Element<T>
    {
        public QuadTree<T> Parent;
        public AABB Span;
        public T Value;

        public Element(T value, AABB span)
        {
            Span = span;
            Value = value;
            Parent = null;
        }
    }

    public enum Quadrant
    {
        Parent,
        TopRight,
        TopLeft,
        BottomLeft,
        BottomRight
    }

    public class QuadTree<T>
    {
        public int MaxBucket;
        public int MaxDepth;
        public List<Element<T>> Nodes;
        public AABB Span;
        public QuadTree<T>[] SubTrees;

        public QuadTree(AABB span, int maxbucket, int maxdepth)
        {
            Span = span;
            Nodes = new List<Element<T>>();

            MaxBucket = maxbucket;
            MaxDepth = maxdepth;
        }

        public bool IsPartitioned
        {
            get { return SubTrees != null; }
        }

        /// <summary>
        /// Gets the quadrant of span that entirely contains test. if none, return 0.
        /// </summary>
        /// <param name="span">The AABB to test against.</param>
        /// <param name="test">The AABB to test with.</param>
        /// <returns>The </returns>
        private Quadrant GetPartition(ref AABB span, ref AABB test)
        {
            if (span.Q1.Contains(ref test)) return Quadrant.TopRight;
            if (span.Q2.Contains(ref test)) return Quadrant.TopLeft;
            if (span.Q3.Contains(ref test)) return Quadrant.BottomLeft;
            if (span.Q4.Contains(ref test)) return Quadrant.BottomRight;

            return Quadrant.Parent;
        }

        /// <summary>
        /// Adds the specified node to the tree.
        /// </summary>
        /// <param name="node">The node to add</param>
        public void AddNode(Element<T> node)
        {
            if (!IsPartitioned)
            {
                if (Nodes.Count >= MaxBucket && MaxDepth > 0) //bin is full and can still subdivide
                {
                    //partition into quadrants and sort existing nodes amonst quads.
                    Nodes.Add(node); //treat new node just like other nodes for partitioning

                    SubTrees = new QuadTree<T>[4];
                    SubTrees[0] = new QuadTree<T>(Span.Q1, MaxBucket, MaxDepth - 1);
                    SubTrees[1] = new QuadTree<T>(Span.Q2, MaxBucket, MaxDepth - 1);
                    SubTrees[2] = new QuadTree<T>(Span.Q3, MaxBucket, MaxDepth - 1);
                    SubTrees[3] = new QuadTree<T>(Span.Q4, MaxBucket, MaxDepth - 1);

                    List<Element<T>> remNodes = new List<Element<T>>();
                    //nodes that are not fully contained by any quadrant

                    foreach (Element<T> n in Nodes)
                    {
                        Quadrant quadrant = GetPartition(ref Span, ref n.Span);

                        switch (quadrant)
                        {
                            case Quadrant.TopRight: //Quadrant 1
                                SubTrees[0].AddNode(n);
                                break;
                            case Quadrant.TopLeft:
                                SubTrees[1].AddNode(n);
                                break;
                            case Quadrant.BottomLeft:
                                SubTrees[2].AddNode(n);
                                break;
                            case Quadrant.BottomRight:
                                SubTrees[3].AddNode(n);
                                break;
                            default:
                                n.Parent = this;
                                remNodes.Add(n);
                                break;
                        }
                    }

                    Nodes = remNodes;
                }
                else
                {
                    //if bin is not yet full or max depth has been reached, just add the node without subdividing
                    node.Parent = this;
                    Nodes.Add(node);
                }
            }
            else //we already have children nodes
            {
                //add node to specific sub-tree
                switch (GetPartition(ref Span, ref node.Span))
                {
                    case Quadrant.TopRight: //quadrant 1
                        SubTrees[0].AddNode(node);
                        break;
                    case Quadrant.TopLeft:
                        SubTrees[1].AddNode(node);
                        break;
                    case Quadrant.BottomLeft:
                        SubTrees[2].AddNode(node);
                        break;
                    case Quadrant.BottomRight:
                        SubTrees[3].AddNode(node);
                        break;
                    default:
                        node.Parent = this;
                        Nodes.Add(node);
                        break;
                }
            }
        }

        /// <summary>
        /// Get all the elements from the QuadTree that are within the searchAABB
        /// </summary>
        public void QueryAABB(Func<Element<T>, bool> callback, ref AABB searchAABB)
        {
            Stack<QuadTree<T>> stack = new Stack<QuadTree<T>>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                QuadTree<T> qt = stack.Pop();
                if (!AABB.TestOverlap(ref searchAABB, ref qt.Span))
                    continue;

                foreach (Element<T> n in qt.Nodes)
                    if (AABB.TestOverlap(ref searchAABB, ref n.Span))
                        if (!callback(n))
                            return;

                if (qt.IsPartitioned)
                    foreach (QuadTree<T> st in qt.SubTrees)
                        stack.Push(st);
            }
        }

        /// <summary>
        /// Raycast the QuadTree.
        /// </summary>
        /// <param name="callback">The userdefined callback method.</param>
        /// <param name="input">The properties of the raycast.</param>
        public void RayCast(Func<RayCastInput, Element<T>, float> callback, ref RayCastInput input)
        {
            Stack<QuadTree<T>> stack = new Stack<QuadTree<T>>();
            stack.Push(this);

            float maxFraction = input.MaxFraction;

            while (stack.Count > 0)
            {
                QuadTree<T> qt = stack.Pop();

                RayCastOutput output;
                if (!qt.Span.RayCast(out output, ref input, false))
                    continue;

                foreach (Element<T> n in qt.Nodes)
                {
                    if (!n.Span.RayCast(out output, ref input))
                        continue;

                    RayCastInput subInput;
                    subInput.Point1 = input.Point1;
                    subInput.Point2 = input.Point2;
                    subInput.MaxFraction = maxFraction;

                    float newMaxFraction = callback(subInput, n);
                    if (newMaxFraction == 0.0f)
                        return; // the client has terminated the raycast.

                    if (newMaxFraction <= 0.0f)
                        continue;

                    maxFraction = newMaxFraction;
                    input.Point2 = input.Point1 + (input.Point2 - input.Point1) * maxFraction; //update segment endpoint
                }

                if (qt.IsPartitioned)
                    foreach (QuadTree<T> st in qt.SubTrees)
                        stack.Push(st);
            }
        }

        /// <summary>
        /// Gets a list of all the nodes in the tree.
        /// </summary>
        public List<Element<T>> GetAllNodes()
        {
            List<Element<T>> nodes = new List<Element<T>>(Nodes);

            if (IsPartitioned)
                foreach (QuadTree<T> st in SubTrees)
                    nodes.AddRange(st.GetAllNodes());

            return nodes;
        }

        /// <summary>
        /// Remove the specified node from the QuadTree.
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(Element<T> node)
        {
            node.Parent.Nodes.Remove(node);
        }

        /// <summary>
        /// Recreate the QuadTree.
        /// </summary>
        public void Reconstruct()
        {
            List<Element<T>> allNodes = GetAllNodes();
            Clear();
            allNodes.ForEach(AddNode);
        }

        /// <summary>
        /// Remove all the nodes from the QuadTree.
        /// </summary>
        public void Clear()
        {
            Nodes.Clear();
            SubTrees = null;
        }
    }
}