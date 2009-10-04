﻿using System;
using FarseerGames.FarseerPhysics.Dynamics;

namespace FarseerGames.FarseerPhysics.Collisions
{
    public class GeomPairBitmap
    {
        private int _geomCount = -1;
        private bool[] _bitmap;

        public GeomPairBitmap(int geomCount, ArbiterList arbiterList)
        {
            Clear(geomCount, arbiterList);
        }

        public void Clear(int newGeomCount, ArbiterList arbiterList)
        {
            if (_geomCount >= newGeomCount)
            {
                Array.Clear(_bitmap, 0, _bitmap.Length);
            }
            else
            {
                _geomCount = newGeomCount;
                _bitmap = new bool[CalculateSize(newGeomCount)];
            }

            if (arbiterList != null)
            {
                foreach (Arbiter arbiter in arbiterList)
                {
                    TestAndSet(arbiter.GeometryA, arbiter.GeometryB);
                }
            }
        }

        public bool TestAndSet(Geom geom1, Geom geom2)
        {
            int index = CalculateIndex(geom1, geom2);

            bool result = _bitmap[index];
            _bitmap[index] = true;
            return result;
        }

        private int CalculateSize(int geomCount)
        {
            if ((geomCount % 2) == 0)
            {
                return (geomCount * (geomCount - 1)) - ((geomCount * ((geomCount - 1) / 2)) + (geomCount / 2));
            }

            return (geomCount * (geomCount - 1)) - (geomCount * (geomCount / 2));
        }

        private int CalculateIndex(Geom geom1, Geom geom2)
        {
            int x;
            int y;
            if (geom1.CollisionId < geom2.CollisionId)
            {
                x = geom1.CollisionId;
                y = geom2.CollisionId;
            }
            else
            {
                x = geom2.CollisionId;
                y = geom1.CollisionId;
            }

            int result = x * _geomCount;
            if (x % 2 == 0)
            {
                result -= ((x + 1) * (x / 2));
            }
            else
            {
                result -= (x + 1) * (x / 2) + (x + 1) / 2;
            }

            result += y - x - 1;

            return result;
        }
    }
}