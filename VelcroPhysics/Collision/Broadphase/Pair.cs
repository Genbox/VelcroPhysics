using System;

namespace VelcroPhysics.Collision.Broadphase
{
    internal struct Pair : IComparable<Pair>
    {
        public int ProxyIdA;
        public int ProxyIdB;

        #region IComparable<Pair> Members

        public int CompareTo(Pair other)
        {
            if (ProxyIdA < other.ProxyIdA)
            {
                return -1;
            }
            if (ProxyIdA == other.ProxyIdA)
            {
                if (ProxyIdB < other.ProxyIdB)
                {
                    return -1;
                }
                if (ProxyIdB == other.ProxyIdB)
                {
                    return 0;
                }
            }

            return 1;
        }

        #endregion
    }
}