namespace Genbox.VelcroPhysics.Shared
{
    public class GraphNode<T>
    {
        public GraphNode(T item = default)
        {
            Item = item;
        }

        /// <summary>The item.</summary>
        public T Item { get; set; }

        /// <summary>The next item in the list.</summary>
        public GraphNode<T> Next { get; set; }

        /// <summary>The previous item in the list.</summary>
        public GraphNode<T> Prev { get; set; }

        internal void Invalidate()
        {
            Next = null;
            Prev = null;
        }

        public void Clear()
        {
            Item = default;
            Invalidate();
        }
    }
}