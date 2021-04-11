using System.Collections;
using System.Collections.Generic;
using Genbox.VelcroPhysics.Shared.Contracts;

namespace Genbox.VelcroPhysics.Shared
{
    /// <summary>This graph is a doubly linked circular list. It is circular to avoid branches in Add/Remove methods.</summary>
    public class Graph<T> : IEnumerable<T>
    {
        private readonly EqualityComparer<T> _comparer;

        public Graph()
        {
            _comparer = EqualityComparer<T>.Default;
        }

        public Graph(EqualityComparer<T> comparer)
        {
            Contract.Requires(comparer != null, "You supplied a null comparer");

            _comparer = comparer;
        }

        /// <summary>The number of items in the graph</summary>
        public int Count { get; private set; }

        /// <summary>The first node in the graph</summary>
        public GraphNode<T> First { get; private set; }

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

        /// <summary>Add a value to the graph</summary>
        /// <remarks>Note that this method is O(n) in worst case.</remarks>
        /// <returns>The node that represents the value</returns>
        public GraphNode<T> Add(T value)
        {
            GraphNode<T> result = new GraphNode<T>(value);
            Add(result);
            return result;
        }

        /// <summary>Add a node to the graph</summary>
        /// <remarks>Note that this method is O(1) in worst case.</remarks>
        public void Add(GraphNode<T> node)
        {
            Contract.Requires(node != null, nameof(node) + " must not be null");

            if (First == null)
            {
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

        /// <summary>Check if the specified value is contained within the graph.</summary>
        /// <remarks>Note that this method is O(n) in worst case.</remarks>
        /// <returns>True if it found the value, otherwise false.</returns>
        public bool Contains(T value)
        {
            return Find(value) != null;
        }

        /// <summary>Finds the specified value</summary>
        /// <remarks>Note that this method is O(n) in worst case.</remarks>
        /// <returns>The graph node that was found if any. Otherwise it returns null.</returns>
        public GraphNode<T> Find(T value)
        {
            GraphNode<T> node = First;

            if (node == null)
                return null;

            if (value != null)
            {
                do
                {
                    if (_comparer.Equals(node.Item, value))
                        return node;

                    node = node.Next;
                } while (node != First);
            }
            else
            {
                do
                {
                    if (node.Item == null)
                        return node;

                    node = node.Next;
                } while (node != First);
            }
            return null;
        }

        public void Clear()
        {
            GraphNode<T> node = First;

            for (int i = 0; i < Count; i++)
            {
                GraphNode<T> node0 = node;
                node = node.Next;
                node0.Invalidate();
            }
        }

        /// <summary>Remove the specified value</summary>
        /// <remarks>Note that this method is O(n) in worst case.</remarks>
        /// <returns>True if the value was removed, otherwise false.</returns>
        public bool Remove(T value)
        {
            GraphNode<T> node = Find(value);

            if (node == null)
                return false;

            Remove(node);
            return true;
        }

        /// <summary>Remove the specified node from the graph.</summary>
        /// <remarks>Note that this method is O(1) in worst case.</remarks>
        public void Remove(GraphNode<T> node)
        {
            Contract.Requires(node != null, nameof(node) + " must not be null");
            Contract.Warn(First != null, "You are trying to remove an item from an empty list.");

            //Invalid node
            if (node.Next == null && node.Prev == null)
                return;

            if (node.Next == node)
                First = null;
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

        public IEnumerable<GraphNode<T>> GetNodes()
        {
            GraphNode<T> node = First;

            for (int i = 0; i < Count; i++)
            {
                GraphNode<T> node0 = node;
                node = node.Next;
                yield return node0;
            }
        }
    }
}