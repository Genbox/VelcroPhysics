namespace FarseerGames.FarseerPhysics.Collisions
{
    public struct ColliderData
    {
        public float GridCellSize;
        public static ColliderData DefaultSettings
        {
            get
            {
                ColliderData data = new ColliderData();
                data.GridCellSize = 0;
                return data;
            }
        }
    }
}
