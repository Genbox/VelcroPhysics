using System;

namespace FarseerGames.FarseerPhysics
{
#if(!XNA)
    public enum CurveLoopType
    {
        Constant,
        Cycle,
        CycleOffset,
        Oscillate,
        Linear
    }

    public enum CurveContinuity
    {
        Smooth,
        Step
    }

    public enum CurveTangent
    {
        Flat,
        Linear,
        Smooth
    }
#endif

    public enum NarrowPhaseCollider
    {
        DistanceGrid,
        SAT
    }

    /// <summary>
    /// Determine the type of joint or spring used to link bodies, in the path generator.
    /// </summary>
    public enum LinkType
    {
        /// <summary>
        /// Use a revolute point
        /// </summary>
        RevoluteJoint,
        /// <summary>
        /// Use a pin joint
        /// </summary>
        PinJoint,
        /// <summary>
        /// Use a slider joint
        /// </summary>
        SliderJoint,
        /// <summary>
        /// Use a linear spring
        /// </summary>
        LinearSpring
    }

    [Flags]
    public enum CollisionCategory
    {
        None = 0,
        All = int.MaxValue,
        Cat1 = 1,
        Cat2 = 2,
        Cat3 = 4,
        Cat4 = 8,
        Cat5 = 16,
        Cat6 = 32,
        Cat7 = 64,
        Cat8 = 128,
        Cat9 = 256,
        Cat10 = 512,
        Cat11 = 1024,
        Cat12 = 2048,
        Cat13 = 4096,
        Cat14 = 8192,
        Cat15 = 16384,
        Cat16 = 32768,
        Cat17 = 65536,
        Cat18 = 131072,
        Cat19 = 262144,
        Cat20 = 524288,
        Cat21 = 1048576,
        Cat22 = 2097152,
        Cat23 = 4194304,
        Cat24 = 8388608,
        Cat25 = 16777216,
        Cat26 = 33554432,
        Cat27 = 67108864,
        Cat28 = 134217728,
        Cat29 = 268435456,
        Cat30 = 536870912,
        Cat31 = 1073741824
    }

    /// <summary>
    /// Used to determine the type of friction applied to geometries.
    /// </summary>
    public enum FrictionType
    {
        /// <summary>
        /// Takes the average of the friction from the two geometries colliding.
        /// </summary>
        Average = 0,
        /// <summary>
        /// Takes the minimum of the friction from the two geometries colliding.
        /// </summary>
        Minimum = 1
    }

    /// <summary>
    /// Defines the type of gravity.
    /// </summary>
    public enum GravityType
    {
        /// <summary>
        /// Gives the most realistic gravity.
        /// </summary>
        DistanceSquared,
        /// <summary>
        /// Not as realistic as DistanceSquared, but performs better when controlling a lot of bodies.
        /// </summary>
        Linear
    }
}