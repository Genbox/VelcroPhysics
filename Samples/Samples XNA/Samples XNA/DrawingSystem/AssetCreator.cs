using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarseerPhysics.SamplesFramework
{
    public enum MaterialType
    {
        Blank, Circles, Dots, Face, Squares, Waves, Pavement
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
            _materials[MaterialType.Circles] = contentManager.Load<Texture2D>("Materials/circles");
            _materials[MaterialType.Dots] = contentManager.Load<Texture2D>("Materials/dots");
            _materials[MaterialType.Squares] = contentManager.Load<Texture2D>("Materials/squares");
            _materials[MaterialType.Waves] = contentManager.Load<Texture2D>("Materials/waves");
            _materials[MaterialType.Pavement] = contentManager.Load<Texture2D>("Materials/pavement");
            _materials[MaterialType.Face] = contentManager.Load<Texture2D>("Materials/face");
        }

        public Texture2D CreateCircleSprite(MaterialType type, Color color, float radius)
        {
            return CreateElipseSprite(type, color, radius, radius);
        }

        public Texture2D CreateElipseSprite(MaterialType type, Color color, float radiusX, float radiusY)
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

            return RenderTexture((int)Math.Ceiling(radiusX * 2.0), (int)Math.Ceiling(radiusY * 2.0), type, verticesFill, verticesOutline);
        }

        private Texture2D RenderTexture(int width, int height, MaterialType type,
                                        VertexPositionColorTexture[] verticesFill,
                                        VertexPositionColor[] verticesOutline)
        {
            RenderTarget2D texture = new RenderTarget2D(_device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, _device.PresentationParameters.MultiSampleCount, RenderTargetUsage.DiscardContents);
            _device.RasterizerState = RasterizerState.CullNone;
            if (type == MaterialType.Face)
            {
                _device.SamplerStates[0] = SamplerState.LinearClamp;
            }
            else
            {
                _device.SamplerStates[0] = SamplerState.LinearWrap;
            }
            _device.SetRenderTarget(texture);
            _device.Clear(Color.Transparent);
            _effect.Projection = Matrix.CreateOrthographic(width, height, 0f, 1f);
            // render shape;
            _effect.TextureEnabled = true;
            _effect.Texture = _materials[type];
            _effect.VertexColorEnabled = true;
            _effect.Techniques[0].Passes[0].Apply();
            _device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verticesFill, 0, verticesFill.Length - 2);
            // render outline;
            _effect.TextureEnabled = false;
            _effect.Techniques[0].Passes[0].Apply();
            _device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verticesOutline, 0, verticesOutline.Length - 2);
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