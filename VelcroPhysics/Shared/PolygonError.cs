namespace VelcroPhysics.Shared
{
    public enum PolygonError
    {
        /// <summary>
        /// There were no errors in the polygon
        /// </summary>
        NoError,

        /// <summary>
        /// Polygon must have between 3 and Settings.MaxPolygonVertices vertices.
        /// </summary>
        InvalidAmountOfVertices,

        /// <summary>
        /// Polygon must be simple. This means no overlapping edges.
        /// </summary>
        NotSimple,

        /// <summary>
        /// Polygon must have a counter clockwise winding.
        /// </summary>
        NotCounterClockWise,

        /// <summary>
        /// The polygon is concave, it needs to be convex.
        /// </summary>
        NotConvex,

        /// <summary>
        /// Polygon area is too small.
        /// </summary>
        AreaTooSmall,

        /// <summary>
        /// The polygon has a side that is too short.
        /// </summary>
        SideTooSmall
    }
}