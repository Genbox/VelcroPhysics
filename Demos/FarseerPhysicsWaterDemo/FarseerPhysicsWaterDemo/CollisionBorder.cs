using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerPhysicsWaterDemo
{
    public class CollisionBorder
    {
        public Geom[] geom;

        float width;
        float height;
        float borderWidth;

        protected Body body;
        protected Vector2 position = Vector2.Zero;
        protected float rotation = 0;
        protected float mass = 1;
        protected bool isStatic = false;

        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        public float BorderWidth
        {
            get { return borderWidth; }
            set { borderWidth = value; }
        }

        public Geom[] Geom
        {
            get { return geom; }
            set { geom = value; }
        }

        public Body Body
        {
            get { return body; }
            set { body = value; }
        }

        public Vector2 Position
        {
            get
            {
                if (body != null) { return body.Position; }
                return position;
            }
            set
            {
                if (body != null) { body.Position = value; }
                position = value;
            }
        }

        public float Rotation
        {
            get
            {
                if (body != null) { return body.Rotation; }
                return rotation;
            }
            set
            {
                if (body != null) { body.Rotation = value; }
                rotation = value;
            }
        }

        public float Mass
        {
            get
            {
                if (body != null) { return body.Mass; }
                return mass;
            }
            set
            {
                if (body != null) { body.Mass = value; }
                mass = value;
            }
        }

        public bool IsStatic
        {
            get { return isStatic; }
            set
            {
                if (body != null) { body.IsStatic = isStatic; }
                isStatic = value;
            }
        }

        public CollisionBorder(float width, float height, float borderWidth, Vector2 position)
        {
            this.width = width;
            this.height = height;
            this.borderWidth = borderWidth;
            this.position = position;
        }

        public void Initialize(PhysicsSimulator physicsSimulator)
        {
            //use the body factory to create the physics body
            body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);
            body.IsStatic = true;
            body.Position = position;

            LoadPhysics(physicsSimulator);
        }

        private void LoadPhysics(PhysicsSimulator physicsSimulator)
        {
            Vector2 geomOffset = Vector2.Zero;

            geom = new Geom[4];
            //left border
            geomOffset = new Vector2(-width * .5f - borderWidth * .5f, 0);
            geom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, body, borderWidth, height + borderWidth, geomOffset, 0);
            geom[0].RestitutionCoefficient = .2f;
            geom[0].FrictionCoefficient = 0f;
            geom[0].CollisionGroup = 100;

            //right border (clone left border since geom is same size)
            geomOffset = new Vector2(width * .5f + borderWidth * .5f, 0);
            geom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, body, geom[0], geomOffset, 0);

            //top border
            geomOffset = new Vector2(0, -height * .5f - borderWidth * .5f);
            geom[2] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, body, width + borderWidth, borderWidth, geomOffset, 0);
            geom[2].RestitutionCoefficient = 0f;
            geom[2].FrictionCoefficient = 1f;
            geom[2].CollisionGroup = 100;
            //borderGeom[2].CollisonGridCellSize = 20;
            geom[2].ComputeCollisionGrid();

            //bottom border (clone top border since geom is same size)
            geomOffset = new Vector2(0, height * .5f + borderWidth * .5f);
            geom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, body, geom[2], geomOffset, 0);
        }

    }
}
