using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerGames.FarseerXNAPhysics.Collisions;
using FarseerGames.FarseerXNAPhysics.Dynamics;

using FarseerGames.FarseerXNATestharness.EntityViews;

namespace FarseerGames.FarseerXNATestharness.Entities {
    public class ShipEntity : PolygonRigidBody {
        private float _width;
        private float _height;
        protected IEntityView _entityView;

        private float _thrust = 50;
        private float _turningTorque = 5;

        public float Thrust {
            get { return _thrust; }
            set { _thrust = value; }
        }

        public float TurningTorque {
            get { return _turningTorque; }
            set { _turningTorque = value; }
        }	

        public void ApplyThrust() {
            ApplyForceAtLocalPoint(new Vector2(BodyMatrix.Up.X * _thrust, BodyMatrix.Up.Y * _thrust), new Vector2(0, 0));
        }

        public void TurnRight() {
            ApplyTorque(_turningTorque);
        }

        public void TurnLeft() {
            ApplyTorque(-_turningTorque);
        }

        public ShipEntity(Game game, float width, float height, Vector2 position, float orientation)  {
            _width = width;
            _height = height;
            Position = position;
            Orientation = orientation;
            Mass = 5;
            MomentOfInertia = Mass * (_width * _width + _height * _height) / 12f;
            RotationalDragCoefficient = 2.9f;
            LinearDragCoefficient = 1f;
            IsStatic = false;

            Vertices vertices = new Vertices();
            vertices.Add(new Vector2(0,_height*.5f));
            vertices.Add(new Vector2(_width*.5f, -_height*.5f));
            vertices.Add(new Vector2(0, -_height*.25f));
            vertices.Add(new Vector2(-_width * .5f, -_height * .5f));
            SetVertices(vertices);
            
            InitializeEntityView(game);
        }

        public void InitializeEntityView(Game game) {
            ShipShape shipShape = new ShipShape(game, _width, _height);
            _entityView = shipShape;
        }

        public void Draw() {
            _entityView.Update(Position, Orientation);
            _entityView.Draw();
        }
    }
}
