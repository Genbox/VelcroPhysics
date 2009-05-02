using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.SimpleSamplesXNA.DrawingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerGames.SimpleSamplesXNA.Demos.DemoShare
{
    public class Ragdoll
    {
        private Body _headBody;
        private Geom _headGeom;
        private EllipseBrush _headBrush;

        private Body _bodyBody;
        private Geom _bodyGeom;
        private EllipseBrush _bodyBrush;

        private Body _leftArmBody;
        private Geom _leftArmGeom;
        private EllipseBrush _leftArmBrush;

        private Body _leftUpperArmBody;
        private Geom _leftUpperArmGeom;
        private EllipseBrush _leftUpperArmBrush;

        private Body _rightArmBody;
        private Geom _rightArmGeom;
        private EllipseBrush _rightArmBrush;

        private Body _rightUpperArmBody;
        private Geom _rightUpperArmGeom;
        private EllipseBrush _rightUpperArmBrush;

        private Body _rightThighBody;
        private Geom _rightThighGeom;
        private EllipseBrush _rightThighBrush;

        private Body _leftThighBody;
        private Geom _leftThighGeom;
        private EllipseBrush _leftThighBrush;

        private Body _rightCalfBody;
        private Geom _rightCalfGeom;
        private EllipseBrush _rightCalfBrush;

        private Body _leftCalfBody;
        private Geom _leftCalfGeom;
        private EllipseBrush _leftCalfBrush;

        private Body _rightFootBody;
        private Geom _rightFootGeom;
        private EllipseBrush _rightFootBrush;

        private Body _leftFootBody;
        private Geom _leftFootGeom;
        private EllipseBrush _leftFootBrush;

        private Vector2 _position;

        public Ragdoll(Vector2 position)
        {
            _position = position;
        }

        public void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            _headBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, 10, 14, 1);
            _headBody.Position = _position;
            _headGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _headBody, 10, 14, 10);
            _headBrush = new EllipseBrush(10, 14, Color.White, Color.Black);
            _headBrush.Load(graphicsDevice);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _headBrush.Draw(spriteBatch, _headBody.Position, _headBody.Rotation);
            //_bodyBrush.Draw(spriteBatch, _bodyBody.Position);
            //_rightArmBrush.Draw(spriteBatch, _rightArmBody.Position);
            //_leftArmBrush.Draw(spriteBatch, _leftArmBody.Position);
            //_rightUpperArmBrush.Draw(spriteBatch, _rightUpperArmBody.Position);
            //_leftUpperArmBrush.Draw(spriteBatch, _leftUpperArmBody.Position);
            //_rightThighBrush.Draw(spriteBatch, _rightThighBody.Position);
            //_leftThighBrush.Draw(spriteBatch, _leftThighBody.Position);
            //_rightCalfBrush.Draw(spriteBatch, _rightCalfBody.Position);
            //_leftCalfBrush.Draw(spriteBatch, _leftCalfBody.Position);
            //_rightFootBrush.Draw(spriteBatch, _rightFootBody.Position);
            //_leftFootBrush.Draw(spriteBatch, _leftFootBody.Position);
        }
    }
}