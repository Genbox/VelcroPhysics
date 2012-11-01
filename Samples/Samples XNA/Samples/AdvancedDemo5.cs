using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Samples.Samples.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.Samples.Samples
{
    internal class AdvancedDemo5 : PhysicsGameScreen, IDemoScreen
    {
        private Border _border;
        private List<List<Vertices>> _breakableObject;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Breakable bodies and explosions";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TODO: Add sample description!");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Explode (at cursor): B button");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Explode (at cursor): Right click");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            _border = new Border(World, this, ScreenManager.GraphicsDevice.Viewport);
            _breakableObject = new List<List<Vertices>>();

            Texture2D alphabet = ScreenManager.Content.Load<Texture2D>("Samples/alphabet");

            uint[] data = new uint[alphabet.Width * alphabet.Height];
            alphabet.GetData(data);

            List<Vertices> list = PolygonTools.CreatePolygon(data, alphabet.Width, 3.5f, 20, true, true);

            float yOffset = -5f;
            float xOffset = -14f;
            for (int i = 0; i < list.Count; i++)
            {
                if (i == 9)
                {
                    yOffset = 0f;
                    xOffset = -14f;
                }
                if (i == 18)
                {
                    yOffset = 5f;
                    xOffset = -12.25f;
                }
                Vertices polygon = list[i];
                Vector2 centroid = -polygon.GetCentroid();
                polygon.Translate(ref centroid);
                //polygon = SimplifyTools.CollinearSimplify(polygon); // this breaks the split hole function
                polygon = SimplifyTools.ReduceByDistance(polygon, 4); 
                List<Vertices> triangulated = BayazitDecomposer.ConvexPartition(polygon);

#if WINDOWS_PHONE
                const float scale = 0.6f;
#else
                const float scale = 1f;
#endif
                Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * scale;
                foreach (Vertices vertices in triangulated)
                {
                    vertices.Scale(ref vertScale);
                }

                BreakableBody breakableBody = new BreakableBody(triangulated, World, 1);
                breakableBody.MainBody.Position = new Vector2(xOffset, yOffset);
                breakableBody.Strength = 100;
                breakableBody.MainBody.UserData = i;
                World.AddBreakableBody(breakableBody);

                polygon.Scale(ref vertScale);
                _breakableObject.Add(polygon.SplitAtHoles());

                xOffset += 3.5f;
            }
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
                                        fv *= 40;
                                        fixture.Body.ApplyLinearImpulse(ref fv);
                                        return true;
                                    }, ref aabb);
            }

            base.HandleInput(input, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _border.Draw();
            ScreenManager.LineBatch.Begin(Camera.SimProjection, Camera.SimView);
            for (int i = 0; i < World.BodyList.Count; ++i)
            {
                Body b = World.BodyList[i];
                if (b.FixtureList.Count == 1 &&
                    b.FixtureList[0].ShapeType == ShapeType.Polygon)
                {
                    PolygonShape s = (PolygonShape)b.FixtureList[0].Shape;
                    Vertices temp = new Vertices();
                    Transform t;
                    b.GetTransform(out t);
                    for (int j = 0; j < s.Vertices.Count; ++j)
                    {
                        temp.Add(MathUtils.Mul(ref t, s.Vertices[j]));
                    }
                    ScreenManager.LineBatch.DrawVertices(temp);
                }
            }
            for (int i = 0; i < World.BreakableBodyList.Count; ++i)
            {
                Transform t;
                World.BreakableBodyList[i].MainBody.GetTransform(out t);
                int index = (int)World.BreakableBodyList[i].MainBody.UserData;
                for (int j = 0; j < _breakableObject[index].Count; ++j)
                {
                    Vertices temp = new Vertices();
                    for (int k = 0; k < _breakableObject[index][j].Count; ++k)
                    {
                        temp.Add(MathUtils.Mul(ref t, _breakableObject[index][j][k]));
                    }
                    ScreenManager.LineBatch.DrawVertices(temp);
                }
            }
            ScreenManager.LineBatch.End();
            base.Draw(gameTime);
        }
    }
}