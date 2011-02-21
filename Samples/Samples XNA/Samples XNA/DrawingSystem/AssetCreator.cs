using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;

namespace FarseerPhysics.SamplesFramework
{
    public enum MaterialType
    {
        Blank, Dots, Squares, Waves, Pavement
    }

    public class AssetCreator
    {
        public const int CircleSegments = 32;

        private Dictionary<MaterialType, Texture2D> _materials = new Dictionary<MaterialType, Texture2D>();
        private BasicEffect _effect;
        private GraphicsDevice _device;

        public AssetCreator(GraphicsDevice device)
        {
            _device = device;
            _effect = new BasicEffect(_device);
        }

        public void LoadContent(ContentManager contentManager)
        {
            _materials[MaterialType.Blank] = contentManager.Load<Texture2D>("Materials/blank");
            _materials[MaterialType.Dots] = contentManager.Load<Texture2D>("Materials/dots");
            _materials[MaterialType.Squares] = contentManager.Load<Texture2D>("Materials/squares");
            _materials[MaterialType.Waves] = contentManager.Load<Texture2D>("Materials/waves");
            _materials[MaterialType.Pavement] = contentManager.Load<Texture2D>("Materials/pavement");
        }

        public Texture2D CreateTextureFromShape(Shape shape, MaterialType type, Color color, float scale)
        {
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                    return CreateCircleSprite((shape as CircleShape).Radius, type, color, scale);
                case ShapeType.Polygon:
                    return CreateTextureFromVertices((shape as PolygonShape).Vertices, type, color, scale);
                default:
                    throw new NotSupportedException("The specified shape type is not supported.");

            }
        }

        public Texture2D CreateTextureFromVertices(Vertices verts, MaterialType type, Color color, float scale)
        {
            //return RenderTexture( // TODO);
            return null;
        }

        public Texture2D CreateCircleSprite(float radius, MaterialType type, Color color, float scale)
        {
            return CreateElipseSprite(radius, radius, type, color, scale);
        }

        public Texture2D CreateElipseSprite(float radiusX, float radiusY, MaterialType type, Color color, float scale)
        {
            VertexPositionColorTexture[] verticesFill = new VertexPositionColorTexture[CircleSegments];
            VertexPositionColor[] verticesOutline = new VertexPositionColor[2 * CircleSegments + 2];
            const double TwoPi = Math.PI * 2.0;
            const double segmentSize = TwoPi / CircleSegments;
            double theta = 0.0;

            verticesOutline[0].Position = new Vector3(radiusX, 0f, 0f);
            verticesOutline[0].Color = Color.Black;
            for (int i = 0; i < CircleSegments; ++i)
            {
                Vector2 position;
                if (i % 2 == 0)
                {
                    position = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                    theta += segmentSize;
                }
                else
                {
                    position = new Vector2((float)Math.Cos(TwoPi - theta), (float)Math.Sin(TwoPi - theta));
                }
                // fill vertices
                verticesFill[i].Position = new Vector3(position.X * radiusX, position.Y * radiusY, 0f);
                verticesFill[i].Color = color;
                verticesFill[i].TextureCoordinate.X = 0.5f + position.X * 0.5f;
                verticesFill[i].TextureCoordinate.Y = 0.5f - position.Y * radiusY / radiusX * 0.5f;
                // outline vertices
                verticesOutline[2 * i + 1].Position = new Vector3((float)Math.Cos(segmentSize * i) * (radiusX - 2f),
                                                                  (float)Math.Sin(segmentSize * i) * (radiusY - 2f), 0f);
                verticesOutline[2 * i + 1].Color = Color.Black;
                verticesOutline[2 * i + 2].Position = new Vector3((float)Math.Cos(segmentSize * (i + 1)) * radiusX,
                                                                  (float)Math.Sin(segmentSize * (i + 1)) * radiusY, 0f);
                verticesOutline[2 * i + 2].Color = Color.Black;
            }
            verticesOutline[2 * CircleSegments + 1].Position = new Vector3(radiusX - 2f, 0f, 0f);
            verticesOutline[2 * CircleSegments + 1].Color = Color.Black;

            return RenderTexture((int)Math.Ceiling(radiusX * 2.0), (int)Math.Ceiling(radiusY * 2.0),
                                 _materials[type], verticesFill, verticesOutline);
        }

        private Texture2D RenderTexture(int width, int height, Texture2D material,
                                        VertexPositionColorTexture[] verticesFill,
                                        VertexPositionColor[] verticesOutline)
        {
            List<VertexPositionColorTexture[]> fill = new List<VertexPositionColorTexture[]>(1);
            fill.Add(verticesFill);
            return RenderTexture(width, height, material, fill, verticesOutline);
        }

        private Texture2D RenderTexture(int width, int height, Texture2D material,
                                        List<VertexPositionColorTexture[]> verticesFill,
                                        VertexPositionColor[] verticesOutline)
        {
            PresentationParameters pp = _device.PresentationParameters;
            RenderTarget2D texture = new RenderTarget2D(_device, width, height, false, pp.BackBufferFormat,
                                                        DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            _device.RasterizerState = RasterizerState.CullNone;
            _device.SamplerStates[0] = SamplerState.LinearWrap;

            _device.SetRenderTarget(texture);
            _device.Clear(Color.Transparent);
            _effect.Projection = Matrix.CreateOrthographic(width, height, 0f, 1f);
            // render shape;
            _effect.TextureEnabled = true;
            _effect.Texture = material;
            _effect.VertexColorEnabled = true;
            _effect.Techniques[0].Passes[0].Apply();
            for (int i = 0; i < verticesFill.Count; ++i)
            {
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, verticesFill[i], 0, verticesFill[i].Length / 3);
            }
            // render outline;
            _effect.TextureEnabled = false;
            _effect.Techniques[0].Passes[0].Apply();
            _device.DrawUserPrimitives(PrimitiveType.LineList, verticesOutline, 0, verticesOutline.Length / 2);
            _device.SetRenderTarget(null);
            return (Texture2D)texture;
        }
    }
}
/*            

        private void DrawTexturedPolygon(Fixture fixture)
        {
            Transform xf;
            fixture.Body.GetTransform(out xf);
            PolygonShape poly = (PolygonShape)fixture.Shape;
            DebugMaterial material = (DebugMaterial)fixture.UserData;
            int count = poly.Vertices.Count;

            if (count == 2)
            {
                return;
            }

            Color colorFill = material.Color;
            float depth = material.Depth;
            Vector2 texCoordCenter = new Vector2(.5f, .5f);
            if (material.CenterOnBody)
            {
                texCoordCenter += poly.Vertices[0] / material.Scale;
            }

            Vector2 v0 = MathUtils.Multiply(ref xf, poly.Vertices[0]);
            for (int i = 1; i < count - 1; i++)
            {
                Vector2 v1 = MathUtils.Multiply(ref xf, poly.Vertices[i]);
                Vector2 v2 = MathUtils.Multiply(ref xf, poly.Vertices[i + 1]);

                _vertsFill[_fillCount * 3].Position = new Vector3(v0, depth);
                _vertsFill[_fillCount * 3].Color = colorFill;
                _vertsFill[_fillCount * 3].TextureCoordinate = texCoordCenter;
                _vertsFill[_fillCount * 3].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3].TextureCoordinate.Y;

                _vertsFill[_fillCount * 3 + 1].Position = new Vector3(v1, depth);
                _vertsFill[_fillCount * 3 + 1].Color = colorFill;
                _vertsFill[_fillCount * 3 + 1].TextureCoordinate = texCoordCenter + (poly.Vertices[i] - poly.Vertices[0]) / material.Scale;
                _vertsFill[_fillCount * 3 + 1].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3 + 1].TextureCoordinate.Y;

                _vertsFill[_fillCount * 3 + 2].Position = new Vector3(v2, depth);
                _vertsFill[_fillCount * 3 + 2].Color = colorFill;
                _vertsFill[_fillCount * 3 + 2].TextureCoordinate = texCoordCenter + (poly.Vertices[i + 1] - poly.Vertices[0]) / material.Scale;
                _vertsFill[_fillCount * 3 + 2].TextureCoordinate.Y = 1 - _vertsFill[_fillCount * 3 + 2].TextureCoordinate.Y;

                _fillCount++;

                // outline
                DrawTexturedLine(v1, v2);
            }
            DrawTexturedLine(v0, MathUtils.Multiply(ref xf, poly.Vertices[1]));
            DrawTexturedLine(MathUtils.Multiply(ref xf, poly.Vertices[count - 1]), v0);
        }

     
*/