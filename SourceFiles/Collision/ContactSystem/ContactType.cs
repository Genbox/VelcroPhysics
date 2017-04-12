namespace VelcroPhysics.Collision.ContactSystem
{
    public enum ContactType : byte
    {
        NotSupported,
        Polygon,
        PolygonAndCircle,
        Circle,
        EdgeAndPolygon,
        EdgeAndCircle,
        ChainAndPolygon,
        ChainAndCircle
    }
}