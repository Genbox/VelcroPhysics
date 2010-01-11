using System;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FarseerPhysics.TestBed.Tests
{
    public class TextureVerticesTest : Test
    {
        private Body _polygonBody;
        private Texture2D _polygonTexture;
        private Vector2[] _vertices;

        private TextureVerticesTest()
        {
            {
                Body ground = World.CreateBody();

                Vertices edge = PolygonTools.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                PolygonShape shape = new PolygonShape(edge, 0);
                ground.CreateFixture(shape);
            }
        }

        public override void Initialize()
        {
            //load texture that will represent the physics body
            _polygonTexture = GameInstance.Content.Load<Texture2D>("Texture");

            //Create an array to hold the data from the texture
            uint[] data = new uint[_polygonTexture.Width * _polygonTexture.Height];

            //Transfer the texture data to the array
            _polygonTexture.GetData(data);

            Vertices verts = PolygonTools.CreatePolygon(data, _polygonTexture.Width, _polygonTexture.Height);
            Vector2 scale = new Vector2(0.07f, 0.07f);
            verts.Scale(ref scale);

            _vertices = verts.ToArray();


            Vertices vertices = new Vertices();
            vertices.Add(new Vector2(-172f, -203f));
            vertices.Add(new Vector2(-81f, -36f));
            vertices.Add(new Vector2(-249f, -177f));
            vertices.Add(new Vector2(-198f, 172f));
            vertices.Add(new Vector2(-51f, 58f));
            vertices.Add(new Vector2(-129f, 202f));
            vertices.Add(new Vector2(167f, 181f));
            vertices.Add(new Vector2(82f, 41f));
            vertices.Add(new Vector2(211f, 153f));
            vertices.Add(new Vector2(174f, -171f));
            vertices.Add(new Vector2(62f, -54f));
            vertices.Add(new Vector2(155f, -217f));
            
            vertices.MakeCCW();
            vertices.Scale(ref scale);
            Vector2 trans = new Vector2(0, 10);
            vertices.Translate(ref trans);

            Polygon polygon = new Polygon(vertices);
             list = polygon.convexPartition();

            colors = new Color[list.Count];
            Random random = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < list.Count; i++)
            {
                colors[i] = new Color((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
            }


            //PolygonShape shape = new PolygonShape(verts, 100);

            //Use the body factory to create the physics body
            //_polygonBody = World.CreateBody();
            //_polygonBody.Position = new Vector2(0, 0);
            //_polygonBody.CreateFixture(shape);

            base.Initialize();
        }

        private List<Polygon> list;
        private Color[] colors;

        public override void Update(Framework.Settings settings)
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                DebugView.DrawCircle(_vertices[i], 0.1f, Color.White);
            }

            for (int i = 0; i < list.Count; i++)
            {
                Polygon v = list[i];
                Vector2[] vector2s = v.ToArray();

                DebugView.DrawSolidPolygon(ref vector2s, v.Count, colors[i]);

                //DebugView.DrawCircle(_vertices[i], 0.1f, Color.White);
            }


            base.Update(settings);
        }

        public static Test Create()
        {
            return new TextureVerticesTest();
        }
    }
}