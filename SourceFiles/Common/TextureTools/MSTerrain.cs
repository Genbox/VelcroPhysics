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
        /// <summary>
        /// World to manage terrain in.
        /// </summary>
        public World World;

        /// <summary>
        /// Center of terrain in world units.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// Width of terrain in world units.
        /// </summary>
        public float Width;

        /// <summary>
        /// Height of terrain in world units.
        /// </summary>
        public float Height;

        /// <summary>
        /// Points per each world unit used to define the terrain in the point cloud.
        /// </summary>
        public int PointsPerUnit;

        /// <summary>
        /// Points per cell.
        /// </summary>
        public int CellSize;

        /// <summary>
        /// Points per sub cell.
        /// </summary>
        public int SubCellSize;

        /// <summary>
        /// Number of iterations to perform in the Marching Squares algorithm.
        /// Note: More then 3 has almost no effect on quality.
        /// </summary>
        public int Iterations = 2;

        /// <summary>
        /// Decomposer to use when regenerating terrain. Can be changed on the fly without consequence.
        /// Note: Some decomposerers are unstable.
        /// </summary>
        public Decomposer Decomposer;

        // TODO - should everything below be private?

        /// <summary>
        /// Point cloud defining the terrain.
        /// </summary>
        public sbyte[,] TerrainMap;

        /// <summary>
        /// Generated bodies.
        /// </summary>
        public List<Body>[,] BodyMap;

        private float localWidth;
        private float localHeight;
        private int xnum;
        private int ynum;
        private AABB dirtyArea;
        private Vector2 topLeft;

        public MSTerrain(World world, AABB area)
        {
            World = world;
            Width = area.Extents.X * 2;
            Height = area.Extents.Y * 2;
            Center = area.Center;
        }

        public MSTerrain(World world, Vector2 position, Texture2D terrainTexture, InsideTerrain insideTerrain)
        {
            World = world;

            Center = position;

            dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue), new Vector2(float.MinValue, float.MinValue));

            Width = terrainTexture.Width;
            Height = terrainTexture.Height;

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

            BodyMap = new List<Body>[xnum, ynum];

            // generate terrain
            for (int gy = 0; gy < ynum; gy++)
            {
                for (int gx = 0; gx < xnum; gx++)
                {
                    GenerateTerrain(gx, gy);
                }
            }
        }

        public void Initialize()
        {
            // find top left of terrain in world space
            topLeft = new Vector2(Center.X - (Width * 0.5f), Center.Y - (-Height * 0.5f));

            // convert the terrains size to a point cloud size
            localWidth = Width * PointsPerUnit;
            localHeight = Height * PointsPerUnit;

            TerrainMap = new sbyte[(int)localWidth + 1, (int)localHeight + 1];

            // TODO - we shouldn't need this
            for (int y = 0; y < localHeight; y++)
            {
                for (int x = 0; x < localWidth; x++)
                {
                    TerrainMap[x, y] = -1;
                }
            }

            xnum = (int)(localWidth / CellSize);
            ynum = (int)(localHeight / CellSize);
            BodyMap = new List<Body>[xnum, ynum];

            for (int gx = 0; gx < xnum; gx++)
            {
                for (int gy = 0; gy < ynum; gy++)
                {
                    //remove old terrain object at grid cell
                    if (BodyMap[gx, gy] != null)
                    {
                        for (int i = 0; i < BodyMap[gx, gy].Count; i++)
                        {
                            World.RemoveBody(BodyMap[gx, gy][i]);
                        }
                    }

                    BodyMap[gx, gy] = null;

                    //generate new one
                    GenerateTerrain(gx, gy);
                }
            }

            // make sure to mark the dirty area to an infinitely small box
            dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue), new Vector2(float.MinValue, float.MinValue));
        }

        /// <summary>
        /// Modify a single point in the terrain.
        /// </summary>
        /// <param name="location">World location to modify. Automatically clipped.</param>
        /// <param name="value">-1 = inside terrain, 1 = outside terrain</param>
        public void ModifyTerrain(Vector2 location, sbyte value)
        {
            // find local position
            // make position local to map space
            Vector2 p = location - topLeft;
            
            // find map position for each axis
            p.X = p.X * localWidth / Width;
            p.Y = p.Y * -localHeight / Height;

            if (p.X >= 0 && p.X < localWidth && p.Y >= 0 && p.Y < localHeight)
            {
                TerrainMap[(int)p.X, (int)p.Y] = value;

                // expand dirty area
                if (p.X < dirtyArea.LowerBound.X) dirtyArea.LowerBound.X = p.X;
                if (p.X > dirtyArea.UpperBound.X) dirtyArea.UpperBound.X = p.X;

                if (p.Y < dirtyArea.LowerBound.Y) dirtyArea.LowerBound.Y = p.Y;
                if (p.Y > dirtyArea.UpperBound.Y) dirtyArea.UpperBound.Y = p.Y;
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
                    if (BodyMap[gx, gy] != null)
                    {
                        for (int i = 0; i < BodyMap[gx, gy].Count; i++)
                        {
                            World.RemoveBody(BodyMap[gx, gy][i]);
                        }
                    }

                    BodyMap[gx, gy] = null;

                    //generate new one
                    GenerateTerrain(gx, gy);
                }
            }

            dirtyArea = new AABB(new Vector2(float.MaxValue, float.MaxValue), new Vector2(float.MinValue, float.MinValue));
        }

        private void GenerateTerrain(int gx, int gy)
        {
            float ax = gx * CellSize;
            float ay = gy * CellSize;

            List<Vertices> polys = MarchingSquares.DetectSquares(new AABB(new Vector2(ax, ay), new Vector2(ax + CellSize, ay + CellSize)), SubCellSize, SubCellSize, TerrainMap, Iterations, true);
            if (polys.Count == 0) return;

            BodyMap[gx, gy] = new List<Body>();

            // create the scale vector
            Vector2 scale = new Vector2(1f / PointsPerUnit, 1f / -PointsPerUnit);

            // create physics object for this grid cell
            foreach (var item in polys)
            {
                // does this need to be negative?
                item.Scale(ref scale);
                item.Translate(ref topLeft);
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
                    if (poly.Count > 2)
                        BodyMap[gx, gy].Add(BodyFactory.CreatePolygon(World, poly, 1));
                }
            }
        }
    }
}
