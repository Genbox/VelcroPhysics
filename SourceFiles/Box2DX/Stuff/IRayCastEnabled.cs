using Box2DX.Collision;

namespace Box2DX.Stuff
{
    public interface IRayCastEnabled
    {
        float RayCastCallback(RayCastInput input, int proxyId);
    }
}
