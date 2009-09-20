using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed.Tests
{
    public class OneSidedPlatform : Test
    {
        private float _radius, _top, _bottom;
        private State _state;
        private Fixture _platform;
        private Fixture _character;

        public enum State
        {
            e_unknown,
            e_above,
            e_below,
        };

        OneSidedPlatform()
        {
            // Ground
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vec2(-20.0f, 0.0f), new Vec2(20.0f, 0.0f));
                ground.CreateFixture(shape, 0);
            }

            // Platform
            {
                BodyDef bd = new BodyDef();
                bd.Position.Set(0.0f, 10.0f);
                Body body = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsBox(3.0f, 0.5f);
                _platform = body.CreateFixture(shape, 0);

                _bottom = 10.0f - 0.5f;
                _top = 10.0f + 0.5f;
            }

            // Actor
            {
                BodyDef bd = new BodyDef();
                bd.Position.Set(0.0f, 12.0f);
                Body body = _world.CreateBody(bd);

                _radius = 0.5f;
                CircleShape shape = new CircleShape();
                shape._radius = _radius;
                _character = body.CreateFixture(shape, 1.0f);

                body.SetLinearVelocity(new Vec2(0.0f, -50.0f));

                _state = State.e_unknown;
            }
        }

        public override void PreSolve(Contact contact, Manifold oldManifold)
        {
            base.PreSolve(contact, oldManifold);

            Fixture fixtureA = contact.GetFixtureA();
            Fixture fixtureB = contact.GetFixtureB();

            if (fixtureA != _platform && fixtureA != _character)
            {
                return;
            }

            if (fixtureB != _character && fixtureB != _character)
            {
                return;
            }

            Vec2 position = _character.GetBody().GetPosition();

            if (position.Y < _top)
            {
                contact.Disable();
            }
        }

        public override void Step(Settings settings)
        {
            base.Step(settings);
            OpenGLDebugDraw.DrawString(5, _textLine, "Press: (c) create a shape, (d) destroy a shape.");
            _textLine += 15;
        }

        public static Test Create()
        {
            return new OneSidedPlatform();
        }
    }
}
