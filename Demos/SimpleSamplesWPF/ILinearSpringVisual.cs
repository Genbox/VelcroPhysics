using FarseerGames.FarseerPhysics.Mathematics;

namespace SimpleSamplesWPF
{
    public interface ILinearSpringVisual
    {
        Vector2 Endpoint1 { get; set; }
        Vector2 Endpoint2 { get; set; }
    }
}