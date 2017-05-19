using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Samples.Demo.Demos.Prefabs;
using VelcroPhysics.Samples.Demo.MediaSystem;
using VelcroPhysics.Samples.Demo.ScreenSystem;
using VelcroPhysics.Shared;
using VelcroPhysics.Templates;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Samples.Demo.Demos
{
    internal class D16_BreakableBody : PhysicsDemoScreen
    {
        private readonly BreakableBody[] _breakableCookie = new BreakableBody[3];
        private Border _border;
        private List<Sprite> _breakableSprite;
        private Sprite _completeSprite;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            _border = new Border(World, Lines, Framework.GraphicsDevice);
            for (int i = 0; i < 3; i++)
            {
                _breakableCookie[i] = Framework.Content.Load<BodyContainer>("Pipeline/BreakableBody")["Cookie"].CreateBreakable(World);
                _breakableCookie[i].Strength = 120f;
                _breakableCookie[i].MainBody.Position = new Vector2(-20.33f + 15f * i, -5.33f);
            }

            _breakableSprite = new List<Sprite>();
            List<Texture2D> textures = ContentWrapper.BreakableTextureFragments(_breakableCookie[0], "Cookie");
            for (int i = 0; i < _breakableCookie[0].Parts.Count; i++)
            {
                AABB bounds;
                Transform transform;
                _breakableCookie[0].Parts[i].Body.GetTransform(out transform);
                _breakableCookie[0].Parts[i].Shape.ComputeAABB(out bounds, ref transform, 0);
                Vector2 origin = ConvertUnits.ToDisplayUnits(_breakableCookie[0].Parts[i].Body.Position - bounds.LowerBound);
                _breakableSprite.Add(new Sprite(textures[i], origin));
            }
            _completeSprite = new Sprite(ContentWrapper.GetTexture("Cookie"), Vector2.Zero);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsNewMouseButtonPress(MouseButtons.RightButton) ||
                input.IsNewButtonPress(Buttons.B))
            {
                Vector2 cursorPos = Camera.ConvertScreenToWorld(input.Cursor);

                Vector2 min = cursorPos - new Vector2(10, 10);
                Vector2 max = cursorPos + new Vector2(10, 10);

                AABB aabb = new AABB(ref min, ref max);

                World.QueryAABB(fixture =>
                {
                    Vector2 fv = fixture.Body.Position - cursorPos;
                    fv.Normalize();
                    fv *= 80;
                    fixture.Body.ApplyLinearImpulse(ref fv);
                    return true;
                }, ref aabb);
            }

            base.HandleInput(input, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);
            for (int i = 0; i < 3; i++)
            {
                if (_breakableCookie[i].Broken)
                {
                    for (int j = 0; j < _breakableCookie[i].Parts.Count; j++)
                    {
                        Body b = _breakableCookie[i].Parts[j].Body;
                        Sprites.Draw(_breakableSprite[j].Image, ConvertUnits.ToDisplayUnits(b.Position), null, Color.White, b.Rotation, _breakableSprite[j].Origin, 1f, SpriteEffects.None, 0f);
                    }
                }
                else
                {
                    Sprites.Draw(_completeSprite.Image, ConvertUnits.ToDisplayUnits(_breakableCookie[i].MainBody.Position), null, Color.White, _breakableCookie[i].MainBody.Rotation, _completeSprite.Origin, 1f, SpriteEffects.None, 0f);
                }
            }
            Sprites.End();

            _border.Draw(Camera.SimProjection, Camera.SimView);

            base.Draw(gameTime);
        }

        #region Demo description

        public override string GetTitle()
        {
            return "Breakable body and explosions";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a breakable cookie, imported from a SVG.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Explosion (at cursor): B button");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse");
            sb.AppendLine("  - Explosion (at cursor): Right click");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.Append("  - Drag grabbed object: Move mouse");
#endif
            return sb.ToString();
        }

        #endregion
    }
}