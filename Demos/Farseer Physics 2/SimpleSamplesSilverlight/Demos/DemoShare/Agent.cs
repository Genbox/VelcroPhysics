using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.SimpleSamplesSilverlight.Demos.DemoShare
{
    public class Agent
    {
        private Body _agentBody;
        private Geom[] _agentGeom;

        private CollisionCategory _collidesWith = CollisionCategory.All;
        private CollisionCategory _collisionCategory = CollisionCategory.All;

        private Vector2 _position;

        public Agent(Vector2 position)
        {
            _position = position;
        }

        public Body Body
        {
            get { return _agentBody; }
        }

        public CollisionCategory CollisionCategory
        {
            get { return _collisionCategory; }
            set { _collisionCategory = value; }
        }

        public CollisionCategory CollidesWith
        {
            get { return _collidesWith; }
            set { _collidesWith = value; }
        }

        public void ApplyForce(Vector2 force)
        {
            _agentBody.ApplyForce(force);
        }

        public void ApplyTorque(float torque)
        {
            _agentBody.ApplyTorque(torque);
        }


        public void Load(SimulatorView view, PhysicsSimulator physicsSimulator)
        {
            _agentBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, 80, 80, 5);
            _agentBody.Position = _position;
            view.AddAgentToCanvas(_agentBody);
            _agentGeom = new Geom[7];
            _agentGeom[0] = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, _agentBody, 16, 10,
                                                                  new Vector2(-40, -40), 0);
            _agentGeom[0].RestitutionCoefficient = .4f;
            _agentGeom[0].FrictionCoefficient = .2f;
            _agentGeom[0].CollisionGroup = 1;
            _agentGeom[0].CollisionCategories = _collisionCategory;
            _agentGeom[0].CollidesWith = _collidesWith;
            _agentGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(-40, 40), 0);
            _agentGeom[2] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(40, -40), 0);
            _agentGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(40, 40), 0);
            _agentGeom[4] = GeomFactory.Instance.CreateGeom(physicsSimulator, _agentBody, _agentGeom[0],
                                                            new Vector2(0, 0),
                                                            0);

            _agentGeom[5] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _agentBody, 16, 120, Vector2.Zero,
                                                                     MathHelper.PiOver4);
            _agentGeom[5].CollisionGroup = 1;
            _agentGeom[5].CollisionCategories = _collisionCategory;
            _agentGeom[5].CollidesWith = _collidesWith;

            _agentGeom[6] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, _agentBody, 16, 120, Vector2.Zero,
                                                                     -MathHelper.PiOver4);
            _agentGeom[6].CollisionGroup = 1;
            _agentGeom[6].CollisionCategories = _collisionCategory;
            _agentGeom[6].CollidesWith = _collidesWith;
        }
    }
}