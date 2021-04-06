using System;

namespace VelcroPhysics.Dynamics
{
    [Flags]
    public enum BodyFlags : byte
    {
        Unknown = 0,
        IslandFlag = 1,
        AwakeFlag = 2,
        AutoSleepFlag = 4,
        BulletFlag = 8,
        FixedRotationFlag = 16,
        Enabled = 32,
        IgnoreGravity = 64,
        IgnoreCCD = 128
    }
}