using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.SimpleSamplesSilverlight.Demos.DemoShare
{
    public class SpringRectangleRope
    {
        private int _collisionGroup;
        private float _dampningConstant = 1;
        private LinearSpring[] _linearSpring;
        private Vector2 _position = Vector2.Zero;
        private Body[] _rectangleBody;
        private int _rectangleCount = 20;
        private Geom[] _rectangleGeom;
        private int _rectangleHeight = 20;
        private float _rectangleMass = 1;
        private int _rectangleWidth = 20;
        private float _springConstant = 1;
        private float _springLength = 25;

        public Body FirstBody
        {
            get { return _rectangleBody[0]; }
            set { _rectangleBody[0] = value; }
        }

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public int RectangleCount
        {
            get { return _rectangleCount; }
            set { _rectangleCount = value; }
        }

        public int RectangleWidth
        {
            get { return _rectangleWidth; }
            set { _rectangleWidth = value; }
        }

        public int RectangleHeight
        {
            get { return _rectangleHeight; }
            set { _rectangleHeight = value; }
        }

        public float RectangleMass
        {
            get { return _rectangleMass; }
            set { _rectangleMass = value; }
        }

        public float SpringLength
        {
            get { return _springLength; }
            set { _springLength = value; }
        }

        public float SpringConstant
        {
            get { return _springConstant; }
            set { _springConstant = value; }
        }

        public float DampningConstant
        {
            get { return _dampningConstant; }
            set { _dampningConstant = value; }
        }

        public int CollisionGroup
        {
            get { return _collisionGroup; }
            set { _collisionGroup = value; }
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            _linearSpring = new LinearSpring[_rectangleCount - 1];
            _rectangleBody = new Body[_rectangleCount];
            _rectangleBody[0] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _rectangleWidth,
                                                                         _rectangleHeight, _rectangleMass);
            _rectangleBody[0].Position = _position;
            view.AddRectangleToCanvas(_rectangleBody[0], Colors.White, new Vector2(_rectangleWidth, _rectangleHeight));
            for (int i = 1; i < _rectangleBody.Length; i++)
            {
                _rectangleBody[i] = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, _rectangleWidth,
                                                                             _rectangleHeight, _rectangleMass);
                _rectangleBody[i].Position = _rectangleBody[i - 1].Position + new Vector2(0, _springLength);
                view.AddRectangleToCanvas(_rectangleBody[i], Colors.White,
                                          new Vector2(_rectangleWidth, _rectangleHeight));
            }

            _rectangleGeom = new Geom[_rectangleCount];
            _rectangleGeom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _rectangleBody[0],
                                                                         _rectangleWidth, _rectangleHeight);
            _rectangleGeom[0].CollisionGroup = _collisionGroup;
            for (int j = 1; j < _rectangleGeom.Length; j++)
            {
                _rectangleGeom[j] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _rectangleBody[j],
                                                                             _rectangleWidth, _rectangleHeight);
                _rectangleGeom[j].CollisionGroup = _collisionGroup;
            }

            for (int k = 0; k < _linearSpring.Length; k++)
            {
                _linearSpring[k] = SpringFactory.Instance.CreateLinearSpring(physicsSimulator, _rectangleBody[k],
                                                                             Vector2.Zero, _rectangleBody[k + 1],
                                                                             Vector2.Zero, _springConstant,
                                                                             _dampningConstant);
            }
        }
    }
}