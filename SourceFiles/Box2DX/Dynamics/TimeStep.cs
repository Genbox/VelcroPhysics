namespace Box2DX.Dynamics
{
    public struct TimeStep
    {
        public float Dt; // time step
        public float Inv_Dt; // inverse time step (0 if dt == 0).
        public float DtRatio;	// dt * inv_dt0
        public int VelocityIterations;
        public int PositionIterations;
        public bool WarmStarting;
    }
}
