using System.Windows.Media;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Mathematics;

namespace SimpleSamplesWPF.Demos.Demo8
{
    public sealed class Ragdoll
    {
        private Body _headBody;
        private Geom _headGeom;

        private Body _bodyBody;
        private Geom _bodyGeom;

        private Body _leftUpperArmBody;
        private Geom _leftUpperArmGeom;

        private Body _leftArmBody;
        private Geom _leftArmGeom;

        private Body _rightUpperArmBody;
        private Geom _rightUpperArmGeom;

        private Body _rightArmBody;
        private Geom _rightArmGeom;

        private Body _leftThighBody;
        private Geom _leftThighGeom;

        private Body _rightThighBody;
        private Geom _rightThighGeom;

        private Body _rightCalfBody;
        private Geom _rightCalfGeom;

        private Body _leftCalfBody;
        private Geom _leftCalfGeom;

        private Vector2 _position;

        public Ragdoll(Vector2 position)
        {
            _position = position;
        }

        public void Load(Demo demo, PhysicsSimulator physicsSimulator)
        {
            const int offset = 2;

            //Body
            const int bodyYRadius = 34;
            const int bodyXRadius = 18;

            _bodyBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, bodyXRadius, bodyYRadius, 1);
            _bodyBody.Position = _position;
            _bodyGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _bodyBody, bodyXRadius, bodyYRadius, 14, 0);
            demo.AddEllipseToCanvas(_bodyBody, Colors.White, bodyXRadius, bodyYRadius);

            //Head
            const int headYRadius = 14;
            const int headXRadius = 10;

            _headBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, headXRadius, headYRadius, 1);
            _headBody.Position = _position - new Vector2(0, bodyYRadius + headYRadius + offset);
            _headGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _headBody, headXRadius, headYRadius, 10);
            demo.AddEllipseToCanvas(_headBody, Colors.White, headXRadius, headYRadius);

            //Left upper arm
            const int leftUpperArmYRadius = 10;
            const int leftUpperArmXRadius = 6;

            _leftUpperArmBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, leftUpperArmXRadius, leftUpperArmYRadius, 1);
            _leftUpperArmBody.Position = _bodyBody.Position - new Vector2(bodyXRadius + leftUpperArmXRadius + offset, bodyYRadius / 2f);
            _leftUpperArmGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _leftUpperArmBody, leftUpperArmXRadius, leftUpperArmYRadius, 8, 0);
            demo.AddEllipseToCanvas(_leftUpperArmBody, Colors.White, leftUpperArmXRadius, leftUpperArmYRadius);

            //Right upper arm
            const int rightUpperArmYRadius = 10;
            const int rightUpperArmXRadius = 6;

            _rightUpperArmBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, rightUpperArmXRadius, rightUpperArmYRadius, 1);
            _rightUpperArmBody.Position = _bodyBody.Position + new Vector2(bodyXRadius + rightUpperArmXRadius + offset, -bodyYRadius / 2f);
            _rightUpperArmGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _rightUpperArmBody, rightUpperArmXRadius, rightUpperArmYRadius, 8, 0);
            demo.AddEllipseToCanvas(_rightUpperArmBody, Colors.White, rightUpperArmXRadius, rightUpperArmYRadius);

            //Left arm
            const int leftArmYRadius = 12;
            const int leftArmXRadius = 6;

            _leftArmBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, leftArmXRadius, leftArmYRadius, 1);
            _leftArmBody.Position = _leftUpperArmBody.Position + new Vector2(0, leftArmYRadius + leftArmYRadius);
            _leftArmGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _leftArmBody, leftArmXRadius, leftArmYRadius, 8, 0);
            demo.AddEllipseToCanvas(_leftArmBody, Colors.White, leftArmXRadius, leftArmYRadius);

            //Right arm
            const int rightArmYRadius = 12;
            const int rightArmXRadius = 6;

            _rightArmBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, rightArmXRadius, rightArmYRadius, 1);
            _rightArmBody.Position = _rightUpperArmBody.Position + new Vector2(0, rightArmYRadius + rightArmYRadius);
            _rightArmGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _rightArmBody, rightArmXRadius, rightArmYRadius, 8, 0);
            demo.AddEllipseToCanvas(_rightArmBody, Colors.White, rightArmXRadius, rightArmYRadius);

            //Left thigh
            const int leftThighYRadius = 12;
            const int leftThighXRadius = 6;

            _leftThighBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, leftThighXRadius, leftThighYRadius, 1);
            _leftThighBody.Position = _bodyBody.Position + new Vector2(bodyXRadius, bodyYRadius + leftThighYRadius);
            _leftThighGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _leftThighBody, leftThighXRadius, leftThighYRadius, 8, 0);
            demo.AddEllipseToCanvas(_leftThighBody, Colors.White, leftThighXRadius, leftThighYRadius);

            //right thigh
            const int rightThighYRadius = 12;
            const int rightThighXRadius = 6;

            _rightThighBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, rightThighXRadius, rightThighYRadius, 1);
            _rightThighBody.Position = _bodyBody.Position + new Vector2(-bodyXRadius, bodyYRadius + rightThighYRadius);
            _rightThighGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _rightThighBody, rightThighXRadius, rightThighYRadius, 8, 0);
            demo.AddEllipseToCanvas(_rightThighBody, Colors.White, rightThighXRadius, rightThighYRadius);

            //Left calf
            const int leftCalfYRadius = 12;
            const int leftCalfXRadius = 6;

            _leftCalfBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, leftCalfXRadius, leftCalfYRadius, 1);
            _leftCalfBody.Position = _leftThighBody.Position + new Vector2(0, leftThighYRadius + leftCalfYRadius);
            _leftCalfGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _leftCalfBody, leftCalfXRadius, leftCalfYRadius, 8, 0);
            demo.AddEllipseToCanvas(_leftCalfBody, Colors.White, leftCalfXRadius, leftCalfYRadius);

            //Right calf
            const int rightCalfYRadius = 12;
            const int rightCalfXRadius = 6;

            _rightCalfBody = BodyFactory.Instance.CreateEllipseBody(physicsSimulator, rightCalfXRadius, rightCalfYRadius, 1);
            _rightCalfBody.Position = _rightThighBody.Position + new Vector2(0, rightThighYRadius + rightCalfYRadius);
            _rightCalfGeom = GeomFactory.Instance.CreateEllipseGeom(physicsSimulator, _rightCalfBody, rightCalfXRadius, rightCalfYRadius, 8, 0);
            demo.AddEllipseToCanvas(_rightCalfBody, Colors.White, rightCalfXRadius, rightCalfYRadius);

            //Create joints
            //Head -> body
            JointFactory.Instance.CreatePinJoint(physicsSimulator, _headBody, new Vector2(0, headYRadius), _bodyBody, new Vector2(0, -bodyYRadius));

            //Left upper arm -> body
            PinJoint p = JointFactory.Instance.CreatePinJoint(physicsSimulator, _leftUpperArmBody, new Vector2(0, -leftUpperArmYRadius), _bodyBody, new Vector2(-bodyXRadius / 2f, -bodyYRadius + bodyXRadius / 2));
            p.TargetDistance = 3;

            //Right upper arm -> body
            p = JointFactory.Instance.CreatePinJoint(physicsSimulator, _rightUpperArmBody, new Vector2(0, -rightUpperArmYRadius), _bodyBody, new Vector2(bodyXRadius / 2f, -bodyYRadius + bodyXRadius / 2));
            p.TargetDistance = 3;

            //Left arm -> left upper arm
            JointFactory.Instance.CreatePinJoint(physicsSimulator, _leftArmBody, new Vector2(0, -leftArmYRadius), _leftUpperArmBody, new Vector2(0, leftUpperArmYRadius));

            //Right arm -> right upper arm
            JointFactory.Instance.CreatePinJoint(physicsSimulator, _rightArmBody, new Vector2(0, -rightArmYRadius), _rightUpperArmBody, new Vector2(0, rightUpperArmYRadius));

            //Left thigh -> body
            p = JointFactory.Instance.CreatePinJoint(physicsSimulator, _leftThighBody, new Vector2(0, -leftThighYRadius), _bodyBody, new Vector2(bodyXRadius / 2f, bodyYRadius - bodyXRadius / 2));
            p.TargetDistance = 3;

            //Right thigh -> body
            p = JointFactory.Instance.CreatePinJoint(physicsSimulator, _rightThighBody, new Vector2(0, -rightThighYRadius), _bodyBody, new Vector2(-bodyXRadius / 2f, bodyYRadius - bodyXRadius / 2));
            p.TargetDistance = 3;

            //Left calf -> left thigh
            JointFactory.Instance.CreatePinJoint(physicsSimulator, _leftCalfBody, new Vector2(0, -leftCalfYRadius), _leftThighBody, new Vector2(0, leftThighYRadius));

            //Right calf -> right thigh
            JointFactory.Instance.CreatePinJoint(physicsSimulator, _rightCalfBody, new Vector2(0, -rightCalfYRadius), _rightThighBody, new Vector2(0, rightThighYRadius));
        }
    }
}