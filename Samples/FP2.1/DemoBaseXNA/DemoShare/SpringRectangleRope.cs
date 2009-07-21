using DemoBaseXNA.DrawingSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class SpringRectangleRope
    {
        private LinearSpring[] _linearSpring;
        private Body[] _rectangleBody;
        private Geom[] _rectangleGeom;
        private Texture2D _rectangleTexture;

        public SpringRectangleRope()
        {
            DampingConstant = 1;
            SpringConstant = 1;
            SpringLength = 25;
            RectangleMass = 1;
            RectangleHeight = 20;
            RectangleWidth = 20;
            RectangleCount = 20;
            Position = Vector2.Zero;
        }

        public Body FirstBody
        {
            get { return _rectangleBody[0]; }
            set { _rectangleBody[0] = value; }
        }

        public Vector2 Position { get; set; }

        public int RectangleCount { get; set; }

        public int RectangleWidth { get; set; }

        public int RectangleHeight { get; set; }

        public float RectangleMass { get; set; }

        public float SpringLength { get; set; }

        public float SpringConstant { get; set; }

        public float DampingConstant { get; set; }

        public int CollisionGroup { get; set; }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _rectangleTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, RectangleWidth, RectangleHeight,
                                                                     Color.White, Color.Black);

            _linearSpring = new LinearSpring[RectangleCount - 1];
            _rectangleBody = new Body[RectangleCount];
            _rectangleBody[0] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, RectangleWidth,
                                                                         RectangleHeight, RectangleMass);
            _rectangleBody[0].Position = Position;
            for (int i = 1; i < _rectangleBody.Length; i++)
            {
                _rectangleBody[i] = BodyFactory.Instance.CreateBody(physicsSimulator, _rectangleBody[0]);
                _rectangleBody[i].Position = _rectangleBody[i - 1].Position + new Vector2(0, SpringLength);
            }

            _rectangleGeom = new Geom[RectangleCount];
            _rectangleGeom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _rectangleBody[0],
                                                                         RectangleWidth, RectangleHeight);
            _rectangleGeom[0].CollisionGroup = CollisionGroup;
            for (int j = 1; j < _rectangleGeom.Length; j++)
            {
                _rectangleGeom[j] = GeomFactory.Instance.CreateGeom(physicsSimulator, _rectangleBody[j],
                                                                    _rectangleGeom[0]);
            }

            for (int k = 0; k < _linearSpring.Length; k++)
            {
                _linearSpring[k] = SpringFactory.Instance.CreateLinearSpring(physicsSimulator, _rectangleBody[k],
                                                                             Vector2.Zero, _rectangleBody[k + 1],
                                                                             Vector2.Zero, SpringConstant,
                                                                             DampingConstant);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _rectangleGeom.Length; i++)
            {
                spriteBatch.Draw(_rectangleTexture, _rectangleGeom[i].Position, null, Color.White,
                                 _rectangleGeom[i].Rotation,
                                 new Vector2(_rectangleTexture.Width/2f, _rectangleTexture.Height/2f), 1,
                                 SpriteEffects.None,
                                 0);
            }
        }
    }
}