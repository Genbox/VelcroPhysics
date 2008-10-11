using FarseerGames.AdvancedSamples.DrawingSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.AdvancedSamples.Demos.Demo1
{
    internal class Floor
    {
        private Body _floorBody;
        private Geom _floorGeom;
        private Vector2 _floorOrigin;
        private Texture2D _floorTexture;
        private int _height;
        private Vector2 _position;
        private int _width;

        public Floor(int width, int height, Vector2 position)
        {
            _width = width;
            _height = height;
            _position = position;
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _floorTexture = DrawingHelper.CreateRectangleTexture(graphicsDevice, _width, _height, 0, 1, 1, Color.White,
                                                                 Color.Black);
            _floorOrigin = new Vector2(_floorTexture.Width/2f, _floorTexture.Height/2f);

            //use the body factory to create the physics body
            _floorBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _width, _height, 1);
            _floorBody.IsStatic = true;
            _floorGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _floorBody, _width, _height);
            _floorGeom.RestitutionCoefficient = .4f;
            _floorGeom.FrictionCoefficient = .4f;
            _floorBody.Position = _position;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_floorTexture, _floorBody.Position, null, Color.White, _floorBody.Rotation, _floorOrigin, 1,
                             SpriteEffects.None, 0f);
        }
    }
}