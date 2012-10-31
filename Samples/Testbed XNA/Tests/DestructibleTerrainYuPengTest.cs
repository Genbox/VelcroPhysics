using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.TestBed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.TestBed.Tests
{
    internal class DestructibleTerrainYuPengTest : Test
    {
        private Vertices _clipCircle;
        private Vector2 _mousePos;
        private float _radius;

        private DestructibleTerrainYuPengTest()
        {
            Settings.MaxPolygonVertices = 16;

            //Ground
            BodyFactory.CreateEdge(World, new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            //Create 7 blocks
            const float size = 2.0f;
            Vertices v = PolygonTools.CreateRectangle(size, size);

            for (int i = 0; i < 7; ++i)
            {
                DestructableBody db = new DestructableBody(World, v);
                db.Body.Position = new Vector2(-15.0f + size * 3 * i, 20.0f);
            }

            Radius = 3;
        }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                _clipCircle = PolygonTools.CreateCircle(Radius, 16);
            }
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            _mousePos = GameInstance.ConvertScreenToWorld(state.X, state.Y);

            if (state.LeftButton == ButtonState.Pressed)
            {
                Explode();
            }
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.A))
            {
                Radius = MathHelper.Clamp(Radius - 0.1f, 0, 20);
            }
            if (keyboardManager.IsKeyDown(Keys.S))
            {
                Radius = MathHelper.Clamp(Radius + 0.1f, 0, 20);
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DebugView.DrawString(50, TextLine, "Press: left mouse button to remove at mouse position.");
            TextLine += 15;
            DebugView.DrawString(50, TextLine, "Press: (A) to decrease the removal radius, (S) to increase it.");
            TextLine += 15;
            // Fighting against float decimals
            float radiusnumber = (float)((int)(Radius * 10)) / 10;
            DebugView.DrawString(50, TextLine, "Radius: " + radiusnumber);

            Color color = new Color(0.4f, 0.7f, 0.8f);

            //Transform shape to mouseposition and then draw
            Vertices tempshape = new Vertices(_clipCircle);
            tempshape.Translate(ref _mousePos);
            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            DebugView.DrawPolygon(tempshape.ToArray(), _clipCircle.Count, color);
            DebugView.EndCustomDraw();
        }

        private void Explode()
        {
            Vector2 min = _mousePos - new Vector2(Radius);
            Vector2 max = _mousePos + new Vector2(Radius);

            AABB affected = new AABB(ref min, ref max);

            List<Fixture> affectedFixtures = new List<Fixture>();

            World.QueryAABB(fixture =>
                                {
                                    affectedFixtures.Add(fixture);
                                    return true;
                                }, ref affected);

            HashSet<Body> uniqueBodies = new HashSet<Body>();

            foreach (Fixture f in affectedFixtures)
            {
                uniqueBodies.Add(f.Body);
            }

            foreach (Body body in uniqueBodies)
            {
                //Check if body is a Destructablebody)
                if (body.UserData is DestructableBody)
                {
                    //Clip the destructablebody against the clip shape
                    DestructableBody db = (DestructableBody)body.UserData;
                    db.Clip(_clipCircle, _mousePos);
                }
            }
        }

        internal static Test Create()
        {
            return new DestructibleTerrainYuPengTest();
        }
    }

    public class DestructableBody
    {
        private const float _density = 1.0f;
        public Body Body;
        public Vertices Vertices;
        private World _world;

        public DestructableBody(World w, Vertices s)
        {
            _world = w;
            CreateBody();
            SetShape(s);
        }

        private void CreateBody()
        {
            //Create Body
            Body = new Body(_world);
            Body.BodyType = BodyType.Dynamic;
            Body.UserData = this;
        }

        private void CreateFixtures()
        {
            //Partition shape into convex pieces
            List<Vertices> verts;
            if (!Vertices.IsConvex())
                verts = BayazitDecomposer.ConvexPartition(Vertices);
            else
            {
                verts = new List<Vertices>();
                verts.Add(Vertices);
            }

            //Create fixtures for each piece
            foreach (Vertices v in verts)
            {
                PolygonShape shape = new PolygonShape(v, _density);
                Body.CreateFixture(shape);
            }

            //wake body up
            Body.Awake = true;
        }

        private void DestroyFixtures()
        {
            for (int i = Body.FixtureList.Count - 1; i >= 0; i--)
            {
                Body.DestroyFixture(Body.FixtureList[i]);
            }
        }

        public void SetShape(Vertices s)
        {
            Vertices = new Vertices(s);
            DestroyFixtures();
            CreateFixtures();
        }

        public bool Clip(Vertices clipVertices, Vector2 position)
        {
            Rot rot = new Rot(0);
            Transform t = new Transform(ref position, ref rot);

            //Transform shape
            Transform thistransform;
            Body.GetTransform(out thistransform);

            //Transform the shape
            Vertices transformedshape = new Vertices(clipVertices.Count);
            foreach (Vector2 v in clipVertices)
            {
                Vector2 newv = v;
                newv = MathUtils.Mul(ref t, ref newv);
                newv = MathUtils.MulT(ref thistransform, ref newv);
                transformedshape.Add(newv);
            }

            PolyClipError error;
            List<Vertices> result = YuPengClipper.Difference(Vertices, transformedshape, out error);

            //Need to check if the entire shape was cut, 
            //so we can destroy/erase it
            if (result.Count == 0)
                return false;

            //The shape was split up, 
            //so create a new DestructableBody for each piece
            if (result.Count > 1)
            {
                //Create a new destructable body for each extra shape
                for (int i = 1; i < result.Count; i++)
                {
                    DestructableBody db = new DestructableBody(_world, result[i]);
                    db.Body.Position = Body.Position;
                }
            }

            //Set Shape
            Vertices newshape = result[0];
            SetShape(newshape);

            return true;
        }
    }
}