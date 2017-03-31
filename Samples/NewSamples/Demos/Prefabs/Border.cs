using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.MediaSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.Samples.Demos.Prefabs
{
    public class Border
    {
        private Body _anchor;

        private BasicEffect _basicEffect;
        private VertexPositionColorTexture[] _borderVertices;
        private short[] _indexBuffer;

        private GraphicsDevice _graphics;
        private LineBatch _lines;

        public Border(World world, LineBatch lines, GraphicsDevice graphics)
        {
            _graphics = graphics;
            _lines = lines;

            // Physics
            float halfWidth = ConvertUnits.ToSimUnits(graphics.Viewport.Width) / 2f - 0.75f;
            float halfHeight = ConvertUnits.ToSimUnits(graphics.Viewport.Height) / 2f - 0.75f;

            Vertices borders = new Vertices(4);
            borders.Add(new Vector2(-halfWidth, halfHeight));  // Lower left
            borders.Add(new Vector2(halfWidth, halfHeight));   // Lower right
            borders.Add(new Vector2(halfWidth, -halfHeight));  // Upper right
            borders.Add(new Vector2(-halfWidth, -halfHeight)); // Upper left

            _anchor = BodyFactory.CreateLoopShape(world, borders);
            _anchor.CollisionCategories = Category.All;
            _anchor.CollidesWith = Category.All;

            // GFX
            _basicEffect = new BasicEffect(graphics);
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.TextureEnabled = true;

            _borderVertices = new[] {
                new VertexPositionColorTexture(new Vector3(-halfWidth, -halfHeight, 0f),Color.White, new Vector2(-halfWidth, -halfHeight) / 5.25f),
                new VertexPositionColorTexture(new Vector3(halfWidth, -halfHeight, 0f),Color.White, new Vector2(halfWidth, -halfHeight) / 5.25f),
                new VertexPositionColorTexture(new Vector3(halfWidth, halfHeight, 0f), Color.White,new Vector2(halfWidth, halfHeight) / 5.25f),
                new VertexPositionColorTexture(new Vector3(-halfWidth, halfHeight, 0f),Color.White, new Vector2(-halfWidth, halfHeight) / 5.25f),
                new VertexPositionColorTexture(new Vector3(-halfWidth - 2f, -halfHeight - 2f, 0f),Color.White, new Vector2(-halfWidth - 2f, -halfHeight - 2f) / 5.25f),
                new VertexPositionColorTexture(new Vector3(halfWidth + 2f, -halfHeight - 2f, 0f),Color.White, new Vector2(halfWidth + 2f, -halfHeight - 2f) / 5.25f),
                new VertexPositionColorTexture(new Vector3(halfWidth + 2f, halfHeight + 2f, 0f), Color.White,new Vector2(halfWidth + 2f, halfHeight + 2f) / 5.25f),
                new VertexPositionColorTexture(new Vector3(-halfWidth - 2f, halfHeight + 2f, 0f), Color.White,new Vector2(-halfWidth - 2f, halfHeight + 2f) / 5.25f)
           };

            _indexBuffer = new short[] { 0, 5, 4, 0, 1, 5, 1, 6, 5, 1, 2, 6, 2, 7, 6, 2, 3, 7, 3, 4, 7, 3, 0, 4 };
        }

        public void Draw(Matrix projection, Matrix view)
        {
            _graphics.SamplerStates[0] = SamplerState.AnisotropicWrap;
            _graphics.RasterizerState = RasterizerState.CullNone;

            _basicEffect.Projection = projection;
            _basicEffect.View = view;
            _basicEffect.Texture = ContentWrapper.GetTexture("Blank");
            _basicEffect.DiffuseColor = ContentWrapper.Black.ToVector3();
            _basicEffect.CurrentTechnique.Passes[0].Apply();
            _graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _borderVertices, 0, 8, _indexBuffer, 0, 8);

            _basicEffect.Texture = ContentWrapper.GetTexture("Stripe");
            _basicEffect.DiffuseColor = ContentWrapper.Grey.ToVector3();
            _basicEffect.CurrentTechnique.Passes[0].Apply();
            _graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _borderVertices, 0, 8, _indexBuffer, 0, 8);

            _lines.Begin(projection, view);
            _lines.DrawLineShape(_anchor.FixtureList[0].Shape);
            _lines.End();
        }
    }
}