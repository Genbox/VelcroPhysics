using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Factories;
using Media = System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Drawing;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;
using System.Collections.Generic;
using FarseerSilverlightDemos.Drawing;
using FarseerSilverlightDemos.Demos.DemoShare;


namespace FarseerSilverlightDemos.Demos.DemoShare
{
    public class AngularSpringLever
    {
        int attachPoint = 0; //0=left, 1=top, 2=right,3=bottom
        int rectangleWidth = 100;
        int rectangleHeight = 20;
        Vector2 position;

        float springConstant = 1;
        float dampningConstant = 1;

        Body angleSpringleverBody;
        Geom circleGeom;
        Geom rectangleGeom;

        Body staticBody;
        FixedRevoluteJoint revoluteJoint;
        FixedAngleSpring fixedAngleSpring;

        int collisionGroup = 0;


        public AngularSpringLever()
        {
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public int AttachPoint
        {
            get { return attachPoint; }
            set { attachPoint = value; }
        }
        public int RectangleWidth
        {
            get { return rectangleWidth; }
            set { rectangleWidth = value; }
        }

        public int RectangleHeight
        {
            get { return rectangleHeight; }
            set { rectangleHeight = value; }
        }

        public float SpringConstant
        {
            get { return springConstant; }
            set { springConstant = value; }
        }

        public float DampningConstant
        {
            get { return dampningConstant; }
            set { dampningConstant = value; }
        }


        public int CollisionGroup
        {
            get { return collisionGroup; }
            set { collisionGroup = value; }
        }

        public Body Body
        {
            get { return angleSpringleverBody; }
        }

        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            int radius;
            if (attachPoint == 0 | attachPoint == 2)
            {
                radius = rectangleHeight;
            }
            else
            {
                radius = rectangleWidth;
            }

            //body is created as rectangle so that it has the moment of inertia closer to the final shape of the object.
            angleSpringleverBody = BodyFactory.Instance.CreateBody(physicsSimulator, 1, BodyFactory.MOIForRectangle(rectangleWidth, rectangleHeight, 1f));
            view.AddRectangleToCanvas(angleSpringleverBody, Media.Colors.White, new Vector2(rectangleWidth, rectangleHeight));

            rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, angleSpringleverBody, rectangleWidth, rectangleHeight);
            rectangleGeom.FrictionCoefficient = .5f;
            rectangleGeom.CollisionGroup = collisionGroup;

            Vector2 offset = Vector2.Zero;
            switch (attachPoint)
            {
                case 0:
                    {
                        offset = new Vector2(-rectangleWidth / 2, 0); //offset to rectangle to left
                        break;
                    }
                case 1:
                    {
                        offset = new Vector2(0, -rectangleHeight / 2); //offset to rectangle to top
                        break;
                    }
                case 2:
                    {
                        offset = new Vector2(rectangleWidth / 2, 0); //offset to rectangle to right
                        break;
                    }
                case 3:
                    {
                        offset = new Vector2(0, rectangleHeight / 2); //offset to rectangle to bottom
                        break;
                    }
            }

            angleSpringleverBody.Position = position - offset;
            CircleBrush circle = view.AddCircleToCanvas(null, Media.Colors.White, 20);
            circle.Extender.Position = position;
            circleGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, angleSpringleverBody, radius, 20, offset, 0);
            circleGeom.FrictionCoefficient = .5f;
            circleGeom.CollisionGroup = collisionGroup;

            revoluteJoint = JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator, angleSpringleverBody, position);
            physicsSimulator.Add(revoluteJoint);
            fixedAngleSpring = ControllerFactory.Instance.CreateFixedAngleSpring(physicsSimulator, angleSpringleverBody, springConstant, dampningConstant);
        }
    }
}
