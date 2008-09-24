using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
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

namespace FarseerSilverlightDemos.Demos.DemoShare
{
    public class Agent
    {
        Body agentBody;
        Geom[] agentGeom;

        CollisionCategory collisionCategory = CollisionCategory.All;
        CollisionCategory collidesWith = CollisionCategory.All;

        Vector2 position;

        public Agent(Vector2 position)
        {
            this.position = position;
        }

        public Body Body
        {
            get { return agentBody; }
        }

        public void ApplyForce(Vector2 force)
        {
            agentBody.ApplyForce(force);
        }

        public void ApplyTorque(float torque)
        {
            agentBody.ApplyTorque(torque);
        }

        public CollisionCategory CollisionCategory
        {
            get { return collisionCategory; }
            set { collisionCategory = value; }
        }

        public CollisionCategory CollidesWith
        {
            get { return collidesWith; }
            set { collidesWith = value; }
        }


        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            agentBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 80, 80, 5);
            agentBody.Position = position;
            view.AddAgentToCanvas(agentBody);
            agentGeom = new Geom[7];
            agentGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, agentBody, 16, 10, new Vector2(-40, -40), 0);
            agentGeom[0].RestitutionCoefficient = .4f;
            agentGeom[0].FrictionCoefficient = .2f;
            agentGeom[0].CollisionGroup = 1;
            agentGeom[0].CollisionCategories = collisionCategory;
            agentGeom[0].CollidesWith = collidesWith;
            agentGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0], new Vector2(-40, 40), 0);
            agentGeom[2] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0], new Vector2(40, -40), 0);
            agentGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0], new Vector2(40, 40), 0);
            agentGeom[4] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0], new Vector2(0, 0), 0);

            agentGeom[5] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 120, Vector2.Zero, MathHelper.PiOver4);
            agentGeom[5].CollisionGroup = 1;
            agentGeom[5].CollisionCategories = collisionCategory;
            agentGeom[5].CollidesWith = collidesWith;

            agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 120, Vector2.Zero, -MathHelper.PiOver4);
            agentGeom[6].CollisionGroup = 1;
            agentGeom[6].CollisionCategories = collisionCategory;
            agentGeom[6].CollidesWith = collidesWith;
        }
    }
}
