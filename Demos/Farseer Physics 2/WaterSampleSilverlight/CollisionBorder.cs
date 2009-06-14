using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.WaterSampleSilverlight
{
    public class CollisionBorder
    {
        public Body Body;
        public float BorderWidth;
        public Geom[] Geom;
        public float Height;
        public float Width;

        private bool _isStatic;
        private float _mass = 1;
        private Vector2 _position = Vector2.Zero;
        private float _rotation;

        public CollisionBorder(float width, float height, float borderWidth, Vector2 position)
        {
            Width = width;
            Height = height;
            BorderWidth = borderWidth;
            _position = position;
        }

        public Vector2 Position
        {
            get
            {
                if (Body != null)
                {
                    return Body.Position;
                }
                return _position;
            }
            set
            {
                if (Body != null)
                {
                    Body.Position = value;
                }
                _position = value;
            }
        }

        public float Rotation
        {
            get
            {
                if (Body != null)
                {
                    return Body.Rotation;
                }
                return _rotation;
            }
            set
            {
                if (Body != null)
                {
                    Body.Rotation = value;
                }
                _rotation = value;
            }
        }

        public float Mass
        {
            get
            {
                if (Body != null)
                {
                    return Body.Mass;
                }
                return _mass;
            }
            set
            {
                if (Body != null)
                {
                    Body.Mass = value;
                }
                _mass = value;
            }
        }

        public bool IsStatic
        {
            get { return _isStatic; }
            set
            {
                if (Body != null)
                {
                    Body.IsStatic = _isStatic;
                }
                _isStatic = value;
            }
        }

        public void Initialize(PhysicsSimulator physicsSimulator)
        {
            //use the body factory to create the physics body
            Body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, Width, Height, 1);
            Body.IsStatic = true;
            Body.Position = _position;

            LoadPhysics(physicsSimulator);
        }

        private void LoadPhysics(PhysicsSimulator physicsSimulator)
        {
            Geom = new Geom[4];
            //left border
            Vector2 geomOffset = new Vector2(-Width * .5f - BorderWidth * .5f, 0);
            Geom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, Body, BorderWidth, Height + BorderWidth,
                                                               geomOffset, 0);
            Geom[0].RestitutionCoefficient = .2f;
            Geom[0].FrictionCoefficient = 0f;
            Geom[0].CollisionGroup = 100;

            //right border (clone left border since geom is same size)
            geomOffset = new Vector2(Width * .5f + BorderWidth * .5f, 0);
            Geom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, Body, Geom[0], geomOffset, 0);

            //top border
            geomOffset = new Vector2(0, -Height * .5f - BorderWidth * .5f);
            Geom[2] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, Body, Width + BorderWidth, BorderWidth,
                                                               geomOffset, 0, 20);
            Geom[2].RestitutionCoefficient = 0f;
            Geom[2].FrictionCoefficient = 1f;
            Geom[2].CollisionGroup = 100;

            //bottom border (clone top border since geom is same size)
            geomOffset = new Vector2(0, Height * .5f + BorderWidth * .5f);
            Geom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, Body, Geom[2], geomOffset, 0);
        }
    }
}