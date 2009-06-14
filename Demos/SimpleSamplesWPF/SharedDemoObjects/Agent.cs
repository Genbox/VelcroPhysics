using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;
using SimpleSamplesWPF.Demos;

namespace SimpleSamplesWPF.SharedDemoObjects
{
    public class Agent
    {
        private Body agentBody;
        private Geom[] agentGeom;

        private CollisionCategory collidesWith = CollisionCategory.All;
        private CollisionCategory collisionCategory = CollisionCategory.All;

        private Vector2 _position;

        public Agent(Vector2 position)
        {
            _position = position;
        }

        public Body Body
        {
            get { return agentBody; }
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

        public void ApplyForce(Vector2 force)
        {
            agentBody.ApplyForce(force);
        }

        public void ApplyTorque(float torque)
        {
            agentBody.ApplyTorque(torque);
        }


        public void Load(Demo demo, PhysicsSimulator physicsSimulator)
        {
            agentBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 80, 80, 5);
            agentBody.Position = _position;
            demo.AddAgentToCanvas(agentBody);
            agentGeom = new Geom[7];
            agentGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, agentBody, 16, 10,
                                                                 new Vector2(-40, -40), 0);
            agentGeom[0].RestitutionCoefficient = .4f;
            agentGeom[0].FrictionCoefficient = .2f;
            agentGeom[0].CollisionGroup = 1;
            agentGeom[0].CollisionCategories = collisionCategory;
            agentGeom[0].CollidesWith = collidesWith;
            agentGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                           new Vector2(-40, 40), 0);
            agentGeom[2] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                           new Vector2(40, -40), 0);
            agentGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                           new Vector2(40, 40), 0);
            agentGeom[4] = GeomFactory.Instance.CreateGeom(physicsSimulator, agentBody, agentGeom[0],
                                                           new Vector2(0, 0),
                                                           0);

            agentGeom[5] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 120, Vector2.Zero,
                                                                    MathHelper.PiOver4);
            agentGeom[5].CollisionGroup = 1;
            agentGeom[5].CollisionCategories = collisionCategory;
            agentGeom[5].CollidesWith = collidesWith;

            agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, agentBody, 16, 120, Vector2.Zero,
                                                                    -MathHelper.PiOver4);
            agentGeom[6].CollisionGroup = 1;
            agentGeom[6].CollisionCategories = collisionCategory;
            agentGeom[6].CollidesWith = collidesWith;
        }
    }
}