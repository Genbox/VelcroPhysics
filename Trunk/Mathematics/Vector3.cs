#if (!XNA)
using System;
using System.Runtime.InteropServices;

namespace FarseerGames.FarseerPhysics.Mathematics
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;
    }
}

#endif