using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace VelcroPhysics.Shared
{
    // This graph is a doubly linked circular list.
    public sealed class Graph<T> : IEnumerable<T>
    {
        public int Count { get; private set; }

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

        public GraphNode<T> Add(T value)
        {
            GraphNode<T> result = new GraphNode<T>(value);
            Add(result);
            return result;
        }

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

        public bool Contains(T value)
        {
            return Find(value) != null;
        }

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
    }
}