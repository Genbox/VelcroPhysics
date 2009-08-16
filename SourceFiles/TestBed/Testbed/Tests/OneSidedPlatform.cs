using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace TestBed.Tests
{
    public class OneSidedPlatform : Test
    {
        float m_radius, m_top, m_bottom;
        State m_state;
        Fixture m_platform;
        Fixture m_character;

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
                m_platform = body.CreateFixture(shape, 0);

                m_bottom = 10.0f - 0.5f;
                m_top = 10.0f + 0.5f;
            }

            // Actor
            {
                BodyDef bd = new BodyDef();
                bd.Position.Set(0.0f, 12.0f);
                Body body = _world.CreateBody(bd);

                m_radius = 0.5f;
                CircleShape shape = new CircleShape();
                shape.Radius = m_radius;
                m_character = body.CreateFixture(shape, 1.0f);
                body.SetMassFromShapes();

                body.SetLinearVelocity(new Vec2(0.0f, -50.0f));

                m_state = State.e_unknown;
            }
        }

        public override void PreSolve(Contact contact, Manifold oldManifold)
        {
            PreSolve(contact, oldManifold);

            Fixture fixtureA = contact.GetFixtureA();
            Fixture fixtureB = contact.GetFixtureB();

            if (fixtureA != m_platform && fixtureA != m_character)
            {
                return;
            }

            if (fixtureB != m_character && fixtureB != m_character)
            {
                return;
            }

            Vec2 position = m_character.GetBody().GetPosition();

            if (position.Y < m_top)
            {
                contact.Disable();
            }
        }

        public override void Step(Settings settings)
        {
            Step(settings);
            OpenGLDebugDraw.DrawString(5, _textLine, "Press: (c) create a shape, (d) destroy a shape.");
            _textLine += 15;
        }
    }
}
