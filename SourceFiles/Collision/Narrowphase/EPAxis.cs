namespace VelcroPhysics.Collision.Narrowphase
{
    /// <summary>
    /// This structure is used to keep track of the best separating axis.
    /// </summary>
    public struct EPAxis
    {
        public int Index;
        public float Separation;
        public EPAxisType Type;
    }
}