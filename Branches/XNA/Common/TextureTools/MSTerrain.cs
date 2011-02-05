using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Factories;

namespace FarseerPhysics.Common
{
    public enum Decomposer
    {
        Bayazit,
        CDT,
        Earclip,
        Flipcode,
        Seidel,
    }

    public delegate bool InsideTerrain(Color color);

    /// <summary>
    /// Simple class to maintain a terrain.
    /// </summary>
    public class MSTerrain
    {
        public World World;
        public Vector2 Position = new Vector2(-300, -100);
        public int TerrainWidth;
        public int TerrainHeight;
        public float CellSize = 25;
        public float MetersPerUnit = 0.1f;
        public Decomposer Decomposer;
        public sbyte[,] TerrainMap;
        public List<Fixture>[,] FixtureMap;

        private int xnum;
        private int ynum;
        public AABB dirtyArea;
        private object userData;

        public MSTerrain(World world, Texture2D terrainTexture, InsideTerrain insideTerrain, object userData)
        {
            World = world;

            this.userData = userData;

            Decomposer = Decomposer.Earclip;

            dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue), new Vector2(float.MinValue, float.MinValue));

            TerrainWidth = terrainTexture.Width;
            TerrainHeight = terrainTexture.Height;

            Color[] colorData = new Color[terrainTexture.Width * terrainTexture.Height];

            terrainTexture.GetData(colorData);

            TerrainMap = new sbyte[terrainTexture.Width, terrainTexture.Height];

            for (int y = 0; y < terrainTexture.Height; y++)
            {
                for (int x = 0; x < terrainTexture.Width; x++)
                {
                    bool inside = insideTerrain(colorData[(y * terrainTexture.Width) + x]);

                    if (!inside)
                        TerrainMap[x, y] = 1;
                    else
                        TerrainMap[x, y] = -1;
                }
            }

            xnum = (int)(terrainTexture.Width / CellSize);
            ynum = (int)(terrainTexture.Height / CellSize);

            FixtureMap = new List<Fixture>[xnum, ynum];

            // generate terrain
            for (int gy = 0; gy < ynum; gy++)
            {
                for (int gx = 0; gx < xnum; gx++)
                {
                    GenerateTerrain(gx, gy);
                }
            }
        }

        public void ModifyTerrain(Vector2 location, sbyte value)
        {
            if (location.X > 0 && location.X < TerrainWidth && location.Y > 0 && location.Y < TerrainHeight)
            {
                TerrainMap[(int)location.X, (int)location.Y] = value;

                // expand dirty area
                if (location.X < dirtyArea.LowerBound.X) dirtyArea.LowerBound.X = location.X;
                if (location.X > dirtyArea.UpperBound.X) dirtyArea.UpperBound.X = location.X;

                if (location.Y < dirtyArea.LowerBound.Y) dirtyArea.LowerBound.Y = location.Y;
                if (location.Y > dirtyArea.UpperBound.Y) dirtyArea.UpperBound.Y = location.Y;
            }
        }

        public void RegenerateTerrain()
        {
            //iterate effected cells
            var gx0 = (int)(dirtyArea.LowerBound.X / CellSize);
            var gx1 = (int)(dirtyArea.UpperBound.X / CellSize) + 1;
            if (gx0 < 0) gx0 = 0;
            if (gx1 > xnum) gx1 = xnum;
            var gy0 = (int)(dirtyArea.LowerBound.Y / CellSize);
            var gy1 = (int)(dirtyArea.UpperBound.Y / CellSize) + 1;
            if (gy0 < 0) gy0 = 0;
            if (gy1 > ynum) gy1 = ynum;

            for (int gx = gx0; gx < gx1; gx++)
            {
                for (int gy = gy0; gy < gy1; gy++)
                {
                    //remove old terrain object at grid cell
                    if (FixtureMap[gx, gy] != null)
                    {
                        for (int i = 0; i < FixtureMap[gx, gy].Count; i++)
                        {
                            World.RemoveBody(FixtureMap[gx, gy][i].Body);
                        }
                    }

                    FixtureMap[gx, gy] = null;

                    //generate new one
                    GenerateTerrain(gx, gy);
                }
            }

            dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue), new Vector2(float.MinValue, float.MinValue));
        }

        public Vector2 WorldToMap(Vector2 point)
        {
            point *= new Vector2(1f / MetersPerUnit, -1f / MetersPerUnit);
            point -= Position;
            return point;
        }

        private void GenerateTerrain(int gx, int gy)
        {
            float ax = gx * CellSize;
            float ay = gy * CellSize;

            List<Vertices> polys = MarchingSquares.DetectSquares(new AABB(new Vector2(ax, ay), new Vector2(ax + CellSize, ay + CellSize)), 3, 3, TerrainMap, 2, true);
            if (polys.Count == 0) return;

            FixtureMap[gx, gy] = new List<Fixture>();

            // create the scale vector
            Vector2 scale = new Vector2(MetersPerUnit, -MetersPerUnit);

            // create physics object for this grid cell
            foreach (var item in polys)
            {
                // does this need to be negative?
                item.Translate(ref Position);
                item.Scale(ref scale);
                item.ForceCounterClockWise();
                Vertices p = FarseerPhysics.Common.PolygonManipulation.SimplifyTools.CollinearSimplify(item);
                List<Vertices> decompPolys = new List<Vertices>();

                switch (Decomposer)
                {
                    case Decomposer.Bayazit:
                        decompPolys = FarseerPhysics.Common.Decomposition.BayazitDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.CDT:
                        decompPolys = FarseerPhysics.Common.Decomposition.CDTDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.Earclip:
                        decompPolys = FarseerPhysics.Common.Decomposition.EarclipDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.Flipcode:
                        decompPolys = FarseerPhysics.Common.Decomposition.FlipcodeDecomposer.ConvexPartition(p);
                        break;
                    case Decomposer.Seidel:
                        decompPolys = FarseerPhysics.Common.Decomposition.SeidelDecomposer.ConvexPartition(p, 0.001f);
                        break;
                    default:
                        break;
                }

                foreach (var poly in decompPolys)
                {
                        FixtureMap[gx, gy].Add(FixtureFactory.CreatePolygon(World, poly, 1, userData));
                }
            }
        }
    }
}
