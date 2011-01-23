using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FarseerPhysics.TestBed.Tests
{
    public class DestructibleTerrainTest : Test
    {
        Texture2D destroyTexture;
        Texture2D terrainTexture;
        float[,] terrainMap;
        float[,] destroyMap;
        List<Fixture>[,] terrainFixtures;
        int xnum;
        int ynum;
        float gwid;

        private DestructibleTerrainTest()
        {
            World = new World(new Vector2(0, -9.82f));
        }

        public override void Initialize()
        {
            this.GameInstance.IsFixedTimeStep = false;

            // texture used to hold initial terrain shape
            terrainTexture = this.GameInstance.Content.Load<Texture2D>("Terrain");

            // all values
            Color[] fFlat = new Color[terrainTexture.Width * terrainTexture.Height];

            terrainTexture.GetData(fFlat);

            terrainMap = new float[terrainTexture.Width, terrainTexture.Height];

            for (int y = 0; y < terrainTexture.Height; y++)
            {
                for (int x = 0; x < terrainTexture.Width; x++)
                {
                    Color c = fFlat[(y * terrainTexture.Width) + x];
                    if (c == Color.White)
                        terrainMap[x, y] = 1.0f;
                    else
                        terrainMap[x, y] = -1.0f;
                }
            }

            // texture used to hold initial terrain shape
            destroyTexture = this.GameInstance.Content.Load<Texture2D>("Circle");

            // all values
            fFlat = new Color[destroyTexture.Width * destroyTexture.Height];

            destroyTexture.GetData(fFlat);

            destroyMap = new float[destroyTexture.Width, destroyTexture.Height];

            for (int y = 0; y < destroyTexture.Height; y++)
            {
                for (int x = 0; x < destroyTexture.Width; x++)
                {
                    Color c = fFlat[(y * destroyTexture.Width) + x];
                    if (c == Color.White)
                        destroyMap[x, y] = 1.0f;
                    else
                        destroyMap[x, y] = -1.0f;
                }
            }

            gwid = 20;
            xnum = (int)(terrainTexture.Width / gwid);
            ynum = (int)(terrainTexture.Height / gwid);

            terrainFixtures = new List<Fixture>[xnum, ynum];

            // generate terrain
            for (int gy = 0; gy < ynum; gy++)
            {
                for (int gx = 0; gx < xnum; gx++)
                {
                    GenerateTerrain(gx, gy);
                }
            }

            base.Initialize();
        }

        private void GenerateTerrain(int gx, int gy)
        {
            float ax = gx * gwid;
            float ay = gy * gwid;

            List<Vertices> polys = MarchingSquares.DetectSquares(new AABB(new Vector2(ax, ay), new Vector2(ax + gwid, ay + gwid)), 4, 4, EvalGround, 2, false);
            if (polys.Count == 0) return;

            terrainFixtures[gx, gy] = new List<Fixture>();

            // create physics object for this grid cell
            foreach (var item in polys)
            {
                Vector2 _scale = new Vector2(0.07f, -0.07f);
                Vector2 _translate = new Vector2(-512, -512);

                item.Translate(ref _translate);

                item.Scale(ref _scale);
                item.ForceCounterClockWise();

                terrainFixtures[gx, gy].Add(FixtureFactory.CreatePolygon(World, item, 1));
            }
        }

        private float EvalGround(float x, float y)
        {
            return terrainMap[(int)x, (int)y];
        }

        private void Destroy()
        {
            float x = Microsoft.Xna.Framework.Input.Mouse.GetState().X;
            float y = Microsoft.Xna.Framework.Input.Mouse.GetState().Y;

            //if (EvalGround(x, y) < 0)
            if (x > 30 && x < terrainTexture.Width - 30 && y > 30 && y < terrainTexture.Height - 30)
            {
                for (int by = 0; by < 60; by++)
                {
                    for (int bx = 0; bx < 60; bx++)
                    {
                        var ax = x - 30 + bx;
                        var ay = y - 30 + by;
                        if (destroyMap[bx, by] < 0)
                            terrainMap[(int)ax, (int)ay] = -destroyMap[bx, by];
                    }
                }
                     
                //iterate effected cells
                var gx0 = (int)((x - 30) / gwid); var gx1 = (int)((x + 30) / gwid) + 1; if (gx0 < 0) gx0 = 0; if(gx1>xnum) gx1 = xnum;
                var gy0 = (int)((y - 30) / gwid); var gy1 = (int)((y + 30) / gwid) + 1; if (gy0 < 0) gy0 = 0; if (gy1 > ynum) gy1 = ynum;
                    
                for (int gx = gx0; gx < gx1; gx++)
                {
                    for (int gy = gy0; gy < gy1; gy++)
                    {
                        //remove old terrain object at grid cell
                        if (terrainFixtures[gx, gy] != null)
                        {
                            for (int i = 0; i < terrainFixtures[gx, gy].Count; i++)
                            {
                                World.RemoveBody(terrainFixtures[gx, gy][i].Body);
                            }
                        }

                        terrainFixtures[gx, gy] = null;
                                         
                        //generate new one
                        GenerateTerrain(gx, gy);
                    }
                }
                         
            }
            //else 
            {
                        /*
                    //create random object
                    if(Math.random()<0.75) {
                            var obj = Tools.createRegular(x, y, Math.random() * 12 + 12, Math.random() * 12 + 12, Std.int(Math.random() * 3 + 3), 0, 0, 0, false, false, Material.Wood);
                            space.addObject(obj);
                            addChild(obj.graphic);
                    }else {
                            var obj = Tools.createCircle(x, y, Math.random() * 12 + 12, 0, 0, 0, false, true, Material.Wood);
                            space.addObject(obj);
                            addChild(obj.graphic);
                    }
                        * */
            }
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            if (state.LeftButton == ButtonState.Pressed)
                Destroy();

            if (state.RightButton == ButtonState.Pressed && oldState.RightButton == ButtonState.Released)
            {
                Vector2 position = GameInstance.ConvertScreenToWorld(state.X, state.Y);

                Fixture f = FixtureFactory.CreateCircle(World, 1, 1, position);

                f.Body.IsStatic = false;
            }

            base.Mouse(state, oldState);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DebugView.DrawString(50, TextLine, "Left click and drag the mouse to destroy terrain!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Right click to create objects!");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Total Polygons: " + World.BodyList.Count);
            TextLine += 15;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new DestructibleTerrainTest();
        }
    }
}
