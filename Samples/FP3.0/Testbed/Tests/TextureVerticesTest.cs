using FarseerPhysics.Common.Siedel;
using FarseerPhysics.Common.Siedel.Shapes;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Point = FarseerPhysics.Common.Siedel.Shapes.Point;

namespace FarseerPhysics.TestBed.Tests
{
    public class TextureVerticesTest : Test
    {
        private Body _polygonBody;
        private Texture2D _polygonTexture;
        private Vector2[] _vertices;
        private TrapezoidalMap map;

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
            Vector2 scale = new Vector2(0.1f, 0.1f);
            PolygonTools.Scale(ref verts, ref scale);

            List<Common.Siedel.Shapes.Point> points = new List<Common.Siedel.Shapes.Point>();

            foreach (Vector2 vector2 in verts)
            {
                points.Add(new Common.Siedel.Shapes.Point(vector2.X, vector2.Y));
            }

            Triangulator tr = new Triangulator(points);
            map = tr.trapezoidalMap;

            //_vertices = verts.ToArray();

            //PolygonShape shape = new PolygonShape(verts, 100);

            //Use the body factory to create the physics body
            //_polygonBody = World.CreateBody();
            //_polygonBody.Position = new Vector2(0, 0);
            //_polygonBody.CreateFixture(shape);

            base.Initialize();
        }

        public override void Update(Framework.Settings settings)
        {
            //for (int i = 0; i < _vertices.Length; i++)
            //{
            //    DebugView.DrawCircle(_vertices[i], 0.1f, Color.White);
            //}

            foreach (Trapezoid t in map.map)
            {
                List<Point> point = t.vertices();

                //Vector2[] vec = new Vector2[point.Count];
                for (int i = 0; i < point.Count; i++)
                {
                    //vec[i] = new Vector2(point[i].x, point[i].y);

                    //DebugView.DrawPoint(new Vector2(point[i].x, point[i].y), 0.1f, Color.White);
                    if (i== point.Count -1)
                        break;
                    DebugView.DrawSegment(new Vector2(point[i].x, point[i].y), new Vector2(point[i + 1].x, point[i + 1].x), Color.White);

                }

                break;


                //DebugView.DrawPolygon(ref vec, vec.Length, Color.Red);
            }

            base.Update(settings);
        }

        public static Test Create()
        {
            return new TextureVerticesTest();
        }
    }
}