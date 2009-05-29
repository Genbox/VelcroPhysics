#if (!XNA)
using System.Runtime.InteropServices;

namespace FarseerGames.FarseerPhysics.Mathematics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;
    }
}

#endif