/*
Test case for collision/jerking issue.
*/

using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework;
using Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Framework.Input;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Testbed.Tests
{
    internal class SkierTest : Test
    {
        private readonly Body _skier;
        private readonly float _platformWidth;
        private bool _fixedCamera;

        private SkierTest()
        {
            Body ground;
            {
                BodyDef bd = new BodyDef();
                ground = BodyFactory.CreateFromDef(World, bd);

                float PlatformWidth = 8.0f;

                /*
			        First angle is from the horizontal and should be negative for a downward slope.
			        Second angle is relative to the preceding slope, and should be positive, creating a kind of
			        loose 'Z'-shape from the 3 edges.
			        If A1 = -10, then A2 <= ~1.5 will result in the collision glitch.
			        If A1 = -30, then A2 <= ~10.0 will result in the glitch.
			    */
                float Angle1Degrees = -30.0f;
                float Angle2Degrees = 10.0f;

                /*
			        The larger the value of SlopeLength, the less likely the glitch will show up.
			    */
                float SlopeLength = 2.0f;

                float SurfaceFriction = 0.2f;

                // Convert to radians
                float Slope1Incline = -Angle1Degrees * MathConstants.Pi / 180.0f;
                float Slope2Incline = Slope1Incline - Angle2Degrees * MathConstants.Pi / 180.0f;

                //

                _platformWidth = PlatformWidth;

                // Horizontal platform
                Vector2 v1 = new Vector2(-PlatformWidth, 0.0f);
                Vector2 v2 = new Vector2(0.0f, 0.0f);
                Vector2 v3 = new Vector2(SlopeLength * MathUtils.Cosf(Slope1Incline), -SlopeLength * MathUtils.Sinf(Slope1Incline));
                Vector2 v4 = new Vector2(v3.X + SlopeLength * MathUtils.Cosf(Slope2Incline), v3.Y - SlopeLength * MathUtils.Sinf(Slope2Incline));
                Vector2 v5 = new Vector2(v4.X, v4.Y - 1.0f);

                Vertices vertices = new Vertices { v5, v4, v3, v2, v1 };

                ChainShape shape = new ChainShape(vertices, true);
                FixtureDef fd = new FixtureDef();
                fd.Shape = shape;
                fd.Friction = SurfaceFriction;

                ground.AddFixture(fd);
            }

            {
                float BodyWidth = 1.0f;
                float BodyHeight = 2.5f;
                float SkiLength = 3.0f;

                /*
			Larger values for this seem to alleviate the issue to some extent.
			*/
                float SkiThickness = 0.3f;

                float SkiFriction = 0.0f;
                float SkiRestitution = 0.15f;

                BodyDef bd = new BodyDef();
                bd.Type = BodyType.Dynamic;

                float initial_y = BodyHeight / 2 + SkiThickness;
                bd.Position = new Vector2(-_platformWidth / 2, initial_y);

                Body skier = BodyFactory.CreateFromDef(World, bd);

                PolygonShape ski = new PolygonShape(1.0f);
                Vertices verts = new Vertices(4);
                verts.Add(new Vector2(-SkiLength / 2 - SkiThickness, -BodyHeight / 2));
                verts.Add(new Vector2(-SkiLength / 2, -BodyHeight / 2 - SkiThickness));
                verts.Add(new Vector2(SkiLength / 2, -BodyHeight / 2 - SkiThickness));
                verts.Add(new Vector2(SkiLength / 2 + SkiThickness, -BodyHeight / 2));
                ski.Vertices = verts;

                FixtureDef fd = new FixtureDef();

                fd.Friction = SkiFriction;
                fd.Restitution = SkiRestitution;

                fd.Shape = ski;
                skier.AddFixture(fd);

                skier.LinearVelocity = new Vector2(0.5f, 0.0f);

                _skier = skier;
            }

            _fixedCamera = true;
        }

        public override void Initialize()
        {
            GameInstance.ViewCenter = new Vector2(_platformWidth / 2.0f, 0.0f);
            GameInstance.ViewZoom = 0.4f;

            base.Initialize();
        }

        public override void Keyboard(KeyboardManager keyboard)
        {
            if (keyboard.IsNewKeyPress(Keys.C))
            {
                _fixedCamera = !_fixedCamera;

                if (_fixedCamera)
                    GameInstance.ViewCenter = new Vector2(_platformWidth / 2.0f, 0.0f);
            }

            base.Keyboard(keyboard);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Keys: c = Camera fixed/tracking");

            if (!_fixedCamera)
                GameInstance.ViewCenter = _skier.Position;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new SkierTest();
        }
    }
}