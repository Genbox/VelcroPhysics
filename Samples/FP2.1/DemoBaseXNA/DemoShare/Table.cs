using System;
using DemoBaseXNA.DrawingSystem;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DemoShare
{
    public class Table
    {
        private Body _topBody;
        private Geom _topGeom;
        private Body _rightLegBody;
        private Geom _rightLegGeom;
        private Body _leftLegBody;
        private Geom _leftLegGeom;
        private WeldJoint _leftWeldJoint;
        private WeldJoint _rightWeldJoint;
        private float _height;
        private PolygonBrush _topBrush;
        private PolygonBrush _leftBrush;
        private PolygonBrush _rightBrush;

        public Table(Vector2 position, float width, float height)
        {
            Random rand = new Random();

            _topBody = BodyFactory.Instance.CreateRectangleBody(width, 10, 6);
            _rightLegBody = BodyFactory.Instance.CreateRectangleBody(10, height, 3);
            _leftLegBody = BodyFactory.Instance.CreateRectangleBody(10, height, 3);

            _topGeom = GeomFactory.Instance.CreateRectangleGeom(_topBody, width, 10);
            _rightLegGeom = GeomFactory.Instance.CreateRectangleGeom(_rightLegBody, 10, height);
            _leftLegGeom = GeomFactory.Instance.CreateRectangleGeom(_leftLegBody, 10, height);

            _topBody.Position = position;
            _leftLegBody.Position = new Vector2(position.X - (width / 2) + 10, position.Y + (height / 2) + 5);
            _rightLegBody.Position = new Vector2(position.X + (width / 2) - 10, position.Y + (height / 2) + 5);

            int group = rand.Next(1, 100);

            _rightLegGeom.CollisionGroup = group;
            _leftLegGeom.CollisionGroup = group;
            _topGeom.CollisionGroup = group;

            _rightLegGeom.RestitutionCoefficient = 0;
            _leftLegGeom.RestitutionCoefficient = 0;
            _topGeom.RestitutionCoefficient = 0;

            _height = height;
        }

        public void Load(PhysicsSimulator physicsSimulator, GraphicsDevice device)
        {
            Random rand = new Random();
            
            physicsSimulator.Add(_topBody);
            physicsSimulator.Add(_rightLegBody);
            physicsSimulator.Add(_leftLegBody);
            physicsSimulator.Add(_topGeom);
            physicsSimulator.Add(_rightLegGeom);
            physicsSimulator.Add(_leftLegGeom);

            _leftWeldJoint = new WeldJoint(_leftLegBody, _topBody, _leftLegBody.Position - new Vector2(-5, _height / 2));
            _leftWeldJoint.Breakpoint = (float)rand.NextDouble() * 3f + 1f;
            _leftWeldJoint.Broke += _leftWeldJoint_Broke;
            physicsSimulator.Add(_leftWeldJoint);

            _rightWeldJoint = new WeldJoint(_rightLegBody, _topBody, _rightLegBody.Position - new Vector2(5, _height / 2));
            _rightWeldJoint.Breakpoint = (float)rand.NextDouble() * 3f + 1f;
            _rightWeldJoint.Broke += _rightWeldJoint_Broke;
            physicsSimulator.Add(_rightWeldJoint);

            _topBrush = new PolygonBrush(_topGeom.LocalVertices, Color.BurlyWood, Color.Black, 1.0f, 0.5f);
            _leftBrush = new PolygonBrush(_leftLegGeom.LocalVertices, Color.BurlyWood, Color.Black, 1.0f, 0.5f);
            _rightBrush = new PolygonBrush(_rightLegGeom.LocalVertices, Color.BurlyWood, Color.Black, 1.0f, 0.5f);

            _topBrush.Load(device);
            _leftBrush.Load(device);
            _rightBrush.Load(device);
        }

        private void _rightWeldJoint_Broke(object sender, EventArgs e)
        {
            _rightLegGeom.CollisionGroup = 0;
            _topGeom.CollisionGroup = 0;
        }

        private void _leftWeldJoint_Broke(object sender, EventArgs e)
        {
            _leftLegGeom.CollisionGroup = 0;
            _topGeom.CollisionGroup = 0;
        }

        public void Draw()
        {
            _topBrush.Draw(_topBody.Position, _topBody.Rotation);
            _leftBrush.Draw(_leftLegBody.Position, _leftLegBody.Rotation);
            _rightBrush.Draw(_rightLegBody.Position, _rightLegBody.Rotation);
        }
    }
}