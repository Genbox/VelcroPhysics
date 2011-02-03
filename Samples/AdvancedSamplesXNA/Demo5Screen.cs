using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.DebugViews;
using FarseerPhysics.DemoBaseXNA;
using FarseerPhysics.DemoBaseXNA.ScreenSystem;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarseerPhysics.AdvancedSamplesXNA
{
    internal class Demo5Screen : PhysicsGameScreen, IDemoScreen
    {
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Demo5: Breakable bodies and explosions";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }

        #endregion

        private Vector2 _scale = new Vector2(0.05f, -0.05f);

        public override void LoadContent()
        {
            World = new World(new Vector2(0, -0f));
            base.LoadContent();

            DebugMaterial defaultMaterial = new DebugMaterial(MaterialType.Blank)
            {
                Color = Color.WhiteSmoke
            };

            Texture2D alphabet = ScreenManager.ContentManager.Load<Texture2D>("Alphabet");

            uint[] data = new uint[alphabet.Width * alphabet.Height];
            alphabet.GetData(data);

            List<Vertices> list = PolygonTools.CreatePolygon(data, alphabet.Width, 3.5f, 20, true, true);

            float yOffset = 0;
            float xOffset = -30;
            for (int i = 0; i < list.Count; i++)
            {
                xOffset += 4;

                if (i == 14)
                {
                    yOffset = -5;
                    xOffset = -26;
                }

                Vertices polygon = list[i];
                Vector2 centroid = -polygon.GetCentroid();
                polygon.Translate(ref centroid);
                polygon = SimplifyTools.CollinearSimplify(polygon);
                polygon = SimplifyTools.ReduceByDistance(polygon, 4);
                List<Vertices> triangulated = BayazitDecomposer.ConvexPartition(polygon);

                foreach (Vertices vertices in triangulated)
                {
                    vertices.Scale(ref _scale);

                    //When we flip the y-axis, the orientation can change.
                    //We need to remember that FPE works with CCW polygons only.
                    vertices.ForceCounterClockWise();
                }

                BreakableBody breakableBody = new BreakableBody(triangulated, World, 1, defaultMaterial);
                breakableBody.MainBody.Position = new Vector2(xOffset, yOffset);
                breakableBody.Strength = 100;
                World.AddBreakableBody(breakableBody);
            }
        }

#if (!XBOX)
        public override void HandleInput(InputHelper input)
        {
            if (input.CurrentMouseState.RightButton == ButtonState.Pressed && input.LastMouseState.RightButton == ButtonState.Released)
            {
                Vector2 mousePos = Camera2D.ConvertScreenToWorld(new Vector2(input.MousePosition.X, input.MousePosition.Y));

                Vector2 min = mousePos - new Vector2(10, 10);
                Vector2 max = mousePos + new Vector2(10, 10);

                AABB aabb = new AABB(ref min, ref max);

                World.QueryAABB(fixture =>
                                {
                                    Vector2 fv = fixture.Body.Position - mousePos;
                                    fv.Normalize();
                                    fv *= 40;
                                    fixture.Body.ApplyLinearImpulse(ref fv);
                                    return true;
                                }, ref aabb);
            }

            base.HandleInput(input);
        }
#endif
    }
}