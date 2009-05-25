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

        private Body _leftUpperArmBody;
        private Geom _leftUpperArmGeom;
        private EllipseBrush _leftUpperArmBrush;

        private Body _leftArmBody;
        private Geom _leftArmGeom;
        private EllipseBrush _leftArmBrush;

        private Body _rightUpperArmBody;
        private Geom _rightUpperArmGeom;
        private EllipseBrush _rightUpperArmBrush;

        private Body _rightArmBody;
        private Geom _rightArmGeom;
        private EllipseBrush _rightArmBrush;

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
            int headHeight = 14;
            int headWidth = 10;

            //Head
            _headBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, headWidth, headHeight, 1);
            _headBody.Position = _position;
            _headGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _headBody, headWidth, headHeight, 10);
            _headBrush = new EllipseBrush(headWidth, headHeight, Color.White, Color.Black);
            _headBrush.Load(graphicsDevice);

            int bodyHeight = 24;
            int bodyWidth = 18;

            //Body
            _bodyBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, bodyWidth, bodyHeight, 1);
            _bodyBody.Position = _headBody.Position + new Vector2(0, bodyHeight + headHeight / 2);
            _bodyGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _bodyBody, bodyWidth, bodyHeight, 14, 0);
            _bodyBrush = new EllipseBrush(bodyWidth, bodyHeight, Color.White, Color.Black);
            _bodyBrush.Load(graphicsDevice);

            int leftUpperArmHeight = 10;
            int leftUpperArmWidth = 6;

            //Left upper arm
            _leftUpperArmBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, leftUpperArmWidth, leftUpperArmHeight, 1);
            _leftUpperArmBody.Position = _bodyBody.Position + new Vector2(-bodyWidth, -bodyHeight / 2f);
            _leftUpperArmGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _leftUpperArmBody, leftUpperArmWidth, leftUpperArmHeight, 8, 0);
            _leftUpperArmBrush = new EllipseBrush(leftUpperArmWidth, leftUpperArmHeight, Color.White, Color.Black);
            _leftUpperArmBrush.Load(graphicsDevice);

            int rightUpperArmHeight = 10;
            int rightUpperArmWidth = 6;

            //Right upper arm
            _rightUpperArmBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, rightUpperArmWidth, rightUpperArmHeight, 1);
            _rightUpperArmBody.Position = _bodyBody.Position + new Vector2(bodyWidth, -bodyHeight / 2f);
            _rightUpperArmGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _rightUpperArmBody, rightUpperArmWidth, rightUpperArmHeight, 8, 0);
            _rightUpperArmBrush = new EllipseBrush(rightUpperArmWidth, rightUpperArmHeight, Color.White, Color.Black);
            _rightUpperArmBrush.Load(graphicsDevice);

            int leftArmHight = 10;
            int leftArmWidth = 6;

            //Left arm
            _leftArmBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, leftArmWidth, leftArmHight, 1);
            _leftArmBody.Position = _leftUpperArmBody.Position + new Vector2(0, leftArmHight + (leftArmHight / 2));
            _leftArmGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _leftArmBody, leftArmWidth, leftArmHight, 8, 0);
            _leftArmBrush = new EllipseBrush(leftArmWidth, leftArmHight, Color.White, Color.Black);
            _leftArmBrush.Load(graphicsDevice);

            int rightArmHight = 10;
            int rightArmWidth = 6;

            //Left arm
            _rightArmBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, rightArmWidth, rightArmHight, 1);
            _rightArmBody.Position = _rightUpperArmBody.Position + new Vector2(0, rightArmHight + (rightArmHight / 2));
            _rightArmGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _rightArmBody, rightArmWidth, rightArmHight, 8, 0);
            _rightArmBrush = new EllipseBrush(rightArmWidth, rightArmHight, Color.White, Color.Black);
            _rightArmBrush.Load(graphicsDevice);

            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _headBrush.Draw(spriteBatch, _headBody.Position, _headBody.Rotation);
            _bodyBrush.Draw(spriteBatch, _bodyBody.Position, _bodyBody.Rotation);
            _leftUpperArmBrush.Draw(spriteBatch, _leftUpperArmBody.Position, _leftUpperArmBody.Rotation);
            _rightUpperArmBrush.Draw(spriteBatch, _rightUpperArmBody.Position, _rightUpperArmBody.Rotation);
            _leftArmBrush.Draw(spriteBatch, _leftArmBody.Position, _leftArmBody.Rotation);
            _rightArmBrush.Draw(spriteBatch, _rightArmBody.Position, _rightArmBody.Rotation);

            //_rightThighBrush.Draw(spriteBatch, _rightThighBody.Position);
            //_leftThighBrush.Draw(spriteBatch, _leftThighBody.Position);
            //_rightCalfBrush.Draw(spriteBatch, _rightCalfBody.Position);
            //_leftCalfBrush.Draw(spriteBatch, _leftCalfBody.Position);
            //_rightFootBrush.Draw(spriteBatch, _rightFootBody.Position);
            //_leftFootBrush.Draw(spriteBatch, _leftFootBody.Position);
        }
    }
}