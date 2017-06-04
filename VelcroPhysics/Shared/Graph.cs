using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace VelcroPhysics.Shared
{
    /// <summary>
    /// This graph is a doubly linked circular list. It is circular to avoid branches in Add/Remove methods.
    /// </summary>
    public class Graph<T> : IEnumerable<T>
    {
        /// <summary>
        /// The number of items in the graph
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// The first node in the graph
        /// </summary>
        public GraphNode<T> First { get; private set; }

        /// <summary>
        /// Add a value to the graph
        /// </summary>
        /// <remarks>Note that this method is O(n) in worst case.</remarks>
        /// <returns>The node that represents the value</returns>
        public GraphNode<T> Add(T value)
        {
            GraphNode<T> result = new GraphNode<T>(value);
            Add(result);
            return result;
        }

        /// <summary>
        /// Add a node to to the graph
        /// </summary>
        /// <remarks>Note that this method is O(1) in worst case.</remarks>
        public void Add(GraphNode<T> node)
        {
            //if (node == null)
            //    throw new ArgumentNullException("node");

            if (First == null)
            {
                Debug.Assert(First == null && Count == 0, "LinkedList must be empty when this method is called!");
                node.Next = node;
                node.Prev = node;
                First = node;
                Count++;
            }
            else
            {
                node.Next = First;
                node.Prev = First.Prev;
                First.Prev.Next = node;
                First.Prev = node;
                Count++;
            }
        }

        /// <summary>
        /// Check if the specified value is contained within the graph.
        /// </summary>
        /// <remarks>Note that this method is O(n) in worst case.</remarks>
        /// <returns>True if it found the value, otherwise false.</returns>
        public bool Contains(T value)
        {
            return Find(value) != null;
        }

        /// <summary>
        /// Finds the specified value
        /// </summary>
        /// <remarks>Note that this method is O(n) in worst case.</remarks>
        /// <returns>The graph node that was found if any. Otherwise it returns null.</returns>
        public GraphNode<T> Find(T value)
        {
            GraphNode<T> node = First;
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            if (node != null)
            {
                if (value != null)
                {
                    do
                    {
                        if (c.Equals(node.Item, value))
                        {
                            return node;
                        }
                        node = node.Next;
                    } while (node != First);
                }
                else
                {
                    do
                    {
                        if (node.Item == null)
                        {
                            return node;
                        }
                        node = node.Next;
                    } while (node != First);
                }
            }
            return null;
        }

        /// <summary>
        /// Remove the specified value
        /// </summary>
        /// <remarks>Note that this method is O(n) in worst case.</remarks>
        /// <returns>True if the value was removed, otherwise false.</returns>
        public bool Remove(T value)
        {
            GraphNode<T> node = Find(value);
            if (node != null)
            {
                Remove(node);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove the specified node from the graph.
        /// </summary>
        /// <remarks>Note that this methid is O(1) in worst case.</remarks>
        public void Remove(GraphNode<T> node)
        {
            //if (node == null)
            //    throw new ArgumentNullException("node");

            //Debug.Assert(node.Graph == this, "Deleting the node from another list!");
            Debug.Assert(First != null, "This method shouldn't be called on empty list!");
            if (node.Next == node)
            {
                Debug.Assert(Count == 1 && First == node, "this should only be true for a list with only one node");
                First = null;
            }
            else
            {
                node.Next.Prev = node.Prev;
                node.Prev.Next = node.Next;

                if (First == node)
                    First = node.Next;
            }

            node.Invalidate();
            Count--;
        }

        public IEnumerator<T> GetEnumerator()
        {
            GraphNode<T> node = First;

            for (int i = 0; i < Count; i++)
            {
                GraphNode<T> node0 = node;
                node = node.Next;
                yield return node0.Item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}