using System.Windows.Forms;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Box2DX.Stuff;

namespace TestBed.Tests
{
    public class DynamicTreeTest : Test, IRayCastEnabled, IQueryEnabled
    {
        private float _worldExtent;
        private float _proxyExtent;

        private DynamicTree _tree = new DynamicTree();
        private AABB _queryAABB;
        private RayCastInput _rayCastInput;
        private RayCastOutput _rayCastOutput;
        private Actor _rayActor;
        private Actor[] _actors = new Actor[_actorCount];
        private int _stepCount;
        private bool _automated;
        private const int _actorCount = 128;

        public class Actor
        {
            public AABB aabb;
            public float fraction;
            public bool overlap;
            public int proxyId;
        }

        public DynamicTreeTest()
        {
            _worldExtent = 15.0f;
            _proxyExtent = 0.5f;

            //srand(888);

            for (int i = 0; i < _actorCount; ++i)
            {
                _actors[i] = new Actor();
                Actor actor = _actors[i];
                GetRandomAABB(actor.aabb);
                actor.proxyId = _tree.CreateProxy(actor.aabb, actor);
            }

            _stepCount = 0;

            float h = _worldExtent;
            _queryAABB.LowerBound.Set(-3.0f, -4.0f + h);
            _queryAABB.UpperBound.Set(5.0f, 6.0f + h);

            _rayCastInput.P1.Set(-5.0f, 5.0f + h);
            _rayCastInput.P2.Set(7.0f, -4.0f + h);
            //_rayCastInput.p1.Set(0.0f, 2.0f + h);
            //_rayCastInput.p2.Set(0.0f, -2.0f + h);
            _rayCastInput.MaxFraction = 1.0f;

            _automated = false;
        }

        public static Test Create()
        {
            return new DynamicTreeTest();
        }

        public override void Step(Settings settings)
        {
            //B2_NOT_USED(settings);

            _rayActor = null;
            for (int i = 0; i < _actorCount; ++i)
            {
                _actors[i].fraction = 1.0f;
                _actors[i].overlap = false;
            }

            if (_automated == true)
            {
                int actionCount = Math.Max(1, _actorCount >> 2);

                for (int i = 0; i < actionCount; ++i)
                {
                    Action();
                }
            }

            Query();
            RayCast();

            for (int i = 0; i < _actorCount; ++i)
            {
                Actor actor = _actors[i];
                if (actor.proxyId == DynamicTree.NullNode)
                    continue;

                Color c = new Color(0.9f, 0.9f, 0.9f);
                if (actor == _rayActor && actor.overlap)
                {
                    c.Set(0.9f, 0.6f, 0.6f);
                }
                else if (actor == _rayActor)
                {
                    c.Set(0.6f, 0.9f, 0.6f);
                }
                else if (actor.overlap)
                {
                    c.Set(0.6f, 0.6f, 0.9f);
                }

                OpenGLDebugDraw.DrawAABB(actor.aabb, c);
            }

            Color cn = new Color(0.7f, 0.7f, 0.7f);
            OpenGLDebugDraw.DrawAABB(_queryAABB, cn);

            _debugDraw.DrawSegment(_rayCastInput.P1, _rayCastInput.P2, cn);

            Color c1 = new Color(0.2f, 0.9f, 0.2f);
            Color c2 = new Color(0.9f, 0.2f, 0.2f);
            OpenGLDebugDraw.DrawPoint(_rayCastInput.P1, 6.0f, c1);
            OpenGLDebugDraw.DrawPoint(_rayCastInput.P2, 6.0f, c2);

            if (_rayActor != null)
            {
                Color cr = new Color(0.2f, 0.2f, 0.9f);
                Vec2 p = _rayCastInput.P1 + _rayActor.fraction * (_rayCastInput.P2 - _rayCastInput.P1);
                OpenGLDebugDraw.DrawPoint(p, 6.0f, cr);
            }

            ++_stepCount;
        }

        public override void Keyboard(Keys key)
        {
            switch (key)
            {
                case Keys.A:
                    _automated = !_automated;
                    break;

                case Keys.C:
                    CreateProxy();
                    break;

                case Keys.D:
                    DestroyProxy();
                    break;

                case Keys.M:
                    MoveProxy();
                    break;
            }
        }

        public bool QueryCallback(int proxyId)
        {
            Actor actor = (Actor)_tree.GetUserData(proxyId);
            actor.overlap = Collision.TestOverlap(_queryAABB, actor.aabb);
            return true;
        }

        public float RayCastCallback(RayCastInput input, int proxyId)
        {
            Actor actor = (Actor)_tree.GetUserData(proxyId);

            RayCastOutput output;
            actor.aabb.RayCast(out output, input);

            if (output.Hit)
            {
                _rayCastOutput = output;
                _rayActor = actor;
                _rayActor.fraction = output.Fraction;
                return output.Fraction;
            }

            return input.MaxFraction;
        }

        private void GetRandomAABB(AABB aabb)
        {
            Vec2 w = new Vec2(); w.Set(2.0f * _proxyExtent, 2.0f * _proxyExtent);
            //aabb.lowerBound.x = -_proxyExtent;
            //aabb.lowerBound.y = -_proxyExtent + _worldExtent;
            aabb.LowerBound.X = Math.Random(-_worldExtent, _worldExtent);
            aabb.LowerBound.Y = Math.Random(0.0f, 2.0f * _worldExtent);
            aabb.UpperBound = aabb.LowerBound + w;
        }

        private void MoveAABB(AABB aabb)
        {
            Vec2 d = new Vec2();
            d.X = Math.Random(-0.5f, 0.5f);
            d.Y = Math.Random(-0.5f, 0.5f);
            //d.x = 2.0f;
            //d.y = 0.0f;
            aabb.LowerBound += d;
            aabb.UpperBound += d;

            Vec2 c0 = 0.5f * (aabb.LowerBound + aabb.UpperBound);
            Vec2 min = new Vec2(); min.Set(-_worldExtent, 0.0f);
            Vec2 max = new Vec2(); max.Set(_worldExtent, 2.0f * _worldExtent);
            Vec2 c = Math.Clamp(c0, min, max);

            aabb.LowerBound += c - c0;
            aabb.UpperBound += c - c0;
        }

        private void CreateProxy()
        {
            for (int i = 0; i < _actorCount; ++i)
            {
                int j = (int)Math.Random() % _actorCount;
                Actor actor = _actors[j];
                if (actor.proxyId == DynamicTree.NullNode)
                {
                    GetRandomAABB(actor.aabb);
                    actor.proxyId = _tree.CreateProxy(actor.aabb, actor);
                    return;
                }
            }
        }

        private void DestroyProxy()
        {
            for (int i = 0; i < _actorCount; ++i)
            {
                int j = (int)Math.Random() % _actorCount;
                Actor actor = _actors[j];
                if (actor.proxyId != DynamicTree.NullNode)
                {
                    _tree.DestroyProxy(actor.proxyId);
                    actor.proxyId = DynamicTree.NullNode;
                    return;
                }
            }
        }

        private void MoveProxy()
        {
            for (int i = 0; i < _actorCount; ++i)
            {
                int j = (int)Math.Random() % _actorCount;
                Actor actor = _actors[j];
                if (actor.proxyId == DynamicTree.NullNode)
                {
                    continue;
                }

                AABB aabb0 = actor.aabb;
                MoveAABB(actor.aabb);
                Vec2 displacement = actor.aabb.GetCenter() - aabb0.GetCenter();
                _tree.MoveProxy(actor.proxyId, actor.aabb, displacement);
                return;
            }
        }

        private void Action()
        {
            int choice = (int)Math.Random() % 20;

            switch (choice)
            {
                case 0:
                    CreateProxy();
                    break;

                case 1:
                    DestroyProxy();
                    break;

                default:
                    MoveProxy();
                    break;
            }
        }

        private void Query()
        {
            _tree.Query(this, _queryAABB);

            for (int i = 0; i < _actorCount; ++i)
            {
                if (_actors[i].proxyId == DynamicTree.NullNode)
                {
                    continue;
                }

                bool overlap = Collision.TestOverlap(_queryAABB, _actors[i].aabb);
                //B2_NOT_USED(overlap);
                Box2DX.Box2DXDebug.Assert(overlap == _actors[i].overlap);
            }
        }

        private void RayCast()
        {
            _rayActor = null;

            RayCastInput input = _rayCastInput;

            // Ray cast against the dynamic tree.
            _tree.RayCast(this, input);

            // Brute force ray cast.
            Actor bruteActor = null;
            RayCastOutput bruteOutput = new RayCastOutput();
            for (int i = 0; i < _actorCount; ++i)
            {
                if (_actors[i].proxyId == DynamicTree.NullNode)
                {
                    continue;
                }

                RayCastOutput output;
                _actors[i].aabb.RayCast(out output, input);
                if (output.Hit)
                {
                    bruteActor = _actors[i];
                    bruteOutput = output;
                    input.MaxFraction = output.Fraction;
                }
            }

            if (bruteActor != null)
            {
                Box2DX.Box2DXDebug.Assert(bruteOutput.Fraction == _rayCastOutput.Fraction);
            }
        }
    }
}
