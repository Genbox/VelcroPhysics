namespace VelcroPhysics.Shared
{
    public class GraphNode<T, T1>
    {
        public GraphNode(T item = default(T))
        {
            Item = item;
        }

        /// <summary>
        /// The item.
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        /// The next item in the list.
        /// </summary>
        public GraphNode<T, T1> Next { get; set; }

        /// <summary>
        /// The previous item in the list.
        /// </summary>
        public GraphNode<T, T1> Prev { get; set; }

        public T1 Other { get; set; }

        internal void Invalidate()
        {
            Next = null;
            Prev = null;
        }

        public void Clear()
        {
            Item = default(T);
            Invalidate();
        }
    }
}