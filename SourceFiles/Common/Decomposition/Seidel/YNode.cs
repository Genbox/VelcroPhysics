namespace FarseerPhysics.Common.Decomposition.Seidel
{
    internal class YNode : Node
    {
        private Edge _edge;

        public YNode(Edge edge, Node lChild, Node rChild)
            : base(lChild, rChild)
        {
            _edge = edge;
        }

        public override Sink Locate(Edge edge)
        {
            if (_edge.IsAbove(edge.P))
                // Move down the graph
                return RightChild.Locate(edge);

            if (_edge.IsBelow(edge.P))
                // Move up the graph
                return LeftChild.Locate(edge);

            // s and segment share the same endpoint, p
            if (edge.Slope < _edge.Slope)
                // Move down the graph
                return RightChild.Locate(edge);

            // Move up the graph
            return LeftChild.Locate(edge);
        }
    }
}