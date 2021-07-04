using System;
using System.Collections.Generic;
using System.IO;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Tools.Triangulation.TriangulationBase;
using Genbox.VelcroPhysics.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Genbox.VelcroPhysics.MonoGame.Samples.Demo.MediaSystem
{
    public class TextureManager : IDisposable
    {
        private const int _circleSegments = 32;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _effect;
        private readonly Dictionary<string, Texture2D> _textureList = new Dictionary<string, Texture2D>();

        public TextureManager(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            // First create a blank texture
            _textureList["Blank"] = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _textureList["Blank"].SetData(new[] { Color.White });
            _textureList["Blank"].Name = "Blank";

            // Load all graphics
            string[] gfxFolders = { "Common", "DemoGFX", "Materials" };
            foreach (string folder in gfxFolders)
            {
                DirectoryInfo currentAssetFolder = new DirectoryInfo(content.RootDirectory + "/" + folder);
                FileInfo[] currentFileList = currentAssetFolder.GetFiles("*.xnb");

                for (int i = 0; i < currentFileList.Length; i++)
                {
                    string textureName = Path.GetFileNameWithoutExtension(currentFileList[i].Name);
                    _textureList[textureName] = content.Load<Texture2D>(folder + "/" + textureName);
                    _textureList[textureName].Name = textureName;
                }
            }

            // Add basic effect for texture generation
            _effect = new BasicEffect(_graphicsDevice);
        }

        public Texture2D GetTexture(string textureName)
        {
            if (_textureList.TryGetValue(textureName, out Texture2D texture))
                return texture;

            throw new FileNotFoundException($"Texture \"{textureName}\" not found!");
        }

        public Vector2 CalculateOrigin(Body body)
        {
            Vector2 lowerBound = new Vector2(float.MaxValue);
            body.GetTransform(out Transform transform);

            for (int i = 0; i < body.FixtureList.Count; i++)
            {
                Shape shape = body.FixtureList[i].Shape;

                for (int j = 0; j < shape.ChildCount; j++)
                {
                    shape.ComputeAABB(ref transform, j, out AABB bounds);
                    Vector2.Min(ref lowerBound, ref bounds.LowerBound, out lowerBound);
                }
            }

            // calculate body offset from its center and add a 1 pixel border
            // because we generate the textures a little bigger than the actual body's fixtures
            return ConvertUnits.ToDisplayUnits(body.Position - lowerBound) + new Vector2(1f);
        }

        public Texture2D TextureFromShape(Shape shape, Color color, Color outlineColor)
        {
            return TextureFromShape(shape, "Blank", color, color, outlineColor, 1f);
        }

        public Texture2D TextureFromShape(Shape shape, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
        {
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                    return CircleTexture(shape.Radius, pattern, mainColor, patternColor, outlineColor, materialScale);
                case ShapeType.Polygon:
                    return PolygonTexture(((PolygonShape)shape).Vertices, pattern, mainColor, patternColor, outlineColor, materialScale);
                default:
                    throw new NotSupportedException("The specified shape type is not supported.");
            }
        }

        public Texture2D CircleTexture(float radius, Color color, Color outlineColor)
        {
            return CircleTexture(radius, "Blank", color, color, outlineColor, 1f);
        }

        public Texture2D CircleTexture(float radius, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
        {
            VertexPositionColorTexture[] verticesFill = new VertexPositionColorTexture[3 * (_circleSegments - 2)];
            VertexPositionColor[] verticesOutline = new VertexPositionColor[2 * _circleSegments];

            const float segmentSize = MathHelper.TwoPi / _circleSegments;
            float theta = segmentSize;

            radius = ConvertUnits.ToDisplayUnits(radius);
            if (_textureList.ContainsKey(pattern))
                materialScale /= _textureList[pattern].Width;
            else
                materialScale = 1f;

            Vector2 start = new Vector2(radius, 0f);

            for (int i = 0; i < _circleSegments - 2; i++)
            {
                Vector2 p1 = new Vector2(radius * (float)Math.Cos(theta), radius * (float)Math.Sin(theta));
                Vector2 p2 = new Vector2(radius * (float)Math.Cos(theta + segmentSize), radius * (float)Math.Sin(theta + segmentSize));

                // fill vertices
                verticesFill[3 * i].Position = new Vector3(start, 0f);
                verticesFill[3 * i + 1].Position = new Vector3(p1, 0f);
                verticesFill[3 * i + 2].Position = new Vector3(p2, 0f);
                verticesFill[3 * i].TextureCoordinate = start * materialScale;
                verticesFill[3 * i + 1].TextureCoordinate = p1 * materialScale;
                verticesFill[3 * i + 2].TextureCoordinate = p2 * materialScale;
                verticesFill[3 * i].Color = verticesFill[3 * i + 1].Color = verticesFill[3 * i + 2].Color = mainColor;

                // outline vertices
                if (i == 0)
                {
                    verticesOutline[0].Position = new Vector3(start, 0f);
                    verticesOutline[1].Position = new Vector3(p1, 0f);
                    verticesOutline[0].Color = verticesOutline[1].Color = outlineColor;
                }

                if (i == _circleSegments - 3)
                {
                    verticesOutline[2 * _circleSegments - 2].Position = new Vector3(p2, 0f);
                    verticesOutline[2 * _circleSegments - 1].Position = new Vector3(start, 0f);
                    verticesOutline[2 * _circleSegments - 2].Color = verticesOutline[2 * _circleSegments - 1].Color = outlineColor;
                }

                verticesOutline[2 * i + 2].Position = new Vector3(p1, 0f);
                verticesOutline[2 * i + 3].Position = new Vector3(p2, 0f);
                verticesOutline[2 * i + 2].Color = verticesOutline[2 * i + 3].Color = outlineColor;

                theta += segmentSize;
            }

            return RenderTexture((int)(radius * 2f), (int)(radius * 2f), _textureList.ContainsKey(pattern) ? _textureList[pattern] : null, patternColor, verticesFill, verticesOutline);
        }

        public Texture2D PolygonTexture(Vector2[] vertices, Color color, Color outlineColor)
        {
            return PolygonTexture(vertices, "Blank", color, color, outlineColor, 1f);
        }

        public Texture2D PolygonTexture(Vector2[] vertices, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
        {
            Vertices temp = new Vertices(vertices);
            return PolygonTexture(temp, pattern, mainColor, patternColor, outlineColor, materialScale);
        }

        public Texture2D PolygonTexture(Vertices vertices, Color color, Color outlineColor)
        {
            return PolygonTexture(vertices, "Blank", color, color, outlineColor, 1f);
        }

        public Texture2D PolygonTexture(Vertices vertices, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
        {
            // copy vertices
            Vertices scaledVertices = new Vertices(vertices);

            // scale to display units (i.e. pixels) for rendering to texture
            Vector2 scale = ConvertUnits.ToDisplayUnits(Vector2.One);
            scaledVertices.Scale(ref scale);

            // translate the boundingbox center to the texture center
            // because we use an orthographic projection for rendering later
            AABB verticesBounds = scaledVertices.GetAABB();
            scaledVertices.Translate(-verticesBounds.Center);

            List<Vertices> decomposedVertices;
            if (!scaledVertices.IsConvex())
                decomposedVertices = Triangulate.ConvexPartition(scaledVertices, TriangulationAlgorithm.Earclip);
            else
            {
                decomposedVertices = new List<Vertices>();
                decomposedVertices.Add(scaledVertices);
            }

            List<VertexPositionColorTexture[]> verticesFill = new List<VertexPositionColorTexture[]>(decomposedVertices.Count);

            if (_textureList.ContainsKey(pattern))
                materialScale /= _textureList[pattern].Width;
            else
                materialScale = 1f;

            for (int i = 0; i < decomposedVertices.Count; i++)
            {
                verticesFill.Add(new VertexPositionColorTexture[3 * (decomposedVertices[i].Count - 2)]);
                for (int j = 0; j < decomposedVertices[i].Count - 2; j++)
                {
                    // fill vertices
                    verticesFill[i][3 * j].Position = new Vector3(decomposedVertices[i][0], 0f);
                    verticesFill[i][3 * j + 1].Position = new Vector3(decomposedVertices[i].NextVertex(j), 0f);
                    verticesFill[i][3 * j + 2].Position = new Vector3(decomposedVertices[i].NextVertex(j + 1), 0f);
                    verticesFill[i][3 * j].TextureCoordinate = decomposedVertices[i][0] * materialScale;
                    verticesFill[i][3 * j + 1].TextureCoordinate = decomposedVertices[i].NextVertex(j) * materialScale;
                    verticesFill[i][3 * j + 2].TextureCoordinate = decomposedVertices[i].NextVertex(j + 1) * materialScale;
                    verticesFill[i][3 * j].Color = verticesFill[i][3 * j + 1].Color = verticesFill[i][3 * j + 2].Color = mainColor;
                }
            }

            // calculate outline
            VertexPositionColor[] verticesOutline = new VertexPositionColor[2 * scaledVertices.Count];
            for (int i = 0; i < scaledVertices.Count; i++)
            {
                verticesOutline[2 * i].Position = new Vector3(scaledVertices[i], 0f);
                verticesOutline[2 * i + 1].Position = new Vector3(scaledVertices.NextVertex(i), 0f);
                verticesOutline[2 * i].Color = verticesOutline[2 * i + 1].Color = outlineColor;
            }

            Vector2 vertsSize = new Vector2(verticesBounds.UpperBound.X - verticesBounds.LowerBound.X, verticesBounds.UpperBound.Y - verticesBounds.LowerBound.Y);

            return RenderTexture((int)vertsSize.X, (int)vertsSize.Y, _textureList.ContainsKey(pattern) ? _textureList[pattern] : null, patternColor, verticesFill, verticesOutline);
        }

        public IList<Texture2D> BreakableTextureFragments(BreakableBody body, string textureName)
        {
            List<Texture2D> result = new List<Texture2D>();

            Vector2 scale = ConvertUnits.ToDisplayUnits(Vector2.One);
            foreach (Fixture f in body.Parts)
            {
                Vertices v = null;
                if (f.Shape is PolygonShape polygonShape)
                {
                    v = new Vertices(polygonShape.Vertices);
                    v.Scale(ref scale);
                }

                if (v != null)
                {
                    AABB polygonBounds = v.GetAABB();
                    List<Vertices> decomposedVertices;
                    if (!v.IsConvex())
                        decomposedVertices = Triangulate.ConvexPartition(v, TriangulationAlgorithm.Bayazit);
                    else
                    {
                        decomposedVertices = new List<Vertices>();
                        decomposedVertices.Add(v);
                    }

                    List<VertexPositionColorTexture[]> verticesFill = new List<VertexPositionColorTexture[]>(decomposedVertices.Count);
                    for (int i = 0; i < decomposedVertices.Count; i++)
                    {
                        verticesFill.Add(new VertexPositionColorTexture[3 * (decomposedVertices[i].Count - 2)]);
                        for (int j = 0; j < decomposedVertices[i].Count - 2; j++)
                        {
                            // fill vertices
                            verticesFill[i][3 * j].Position = new Vector3(decomposedVertices[i][0] - polygonBounds.Center, 0f);
                            verticesFill[i][3 * j + 1].Position = new Vector3(decomposedVertices[i].NextVertex(j) - polygonBounds.Center, 0f);
                            verticesFill[i][3 * j + 2].Position = new Vector3(decomposedVertices[i].NextVertex(j + 1) - polygonBounds.Center, 0f);

                            verticesFill[i][3 * j].TextureCoordinate = new Vector2(decomposedVertices[i][0].X / _textureList[textureName].Width, decomposedVertices[i][0].Y / _textureList[textureName].Height - 1f);
                            verticesFill[i][3 * j + 1].TextureCoordinate = new Vector2(decomposedVertices[i].NextVertex(j).X / _textureList[textureName].Width, decomposedVertices[i].NextVertex(j).Y / _textureList[textureName].Height - 1f);
                            verticesFill[i][3 * j + 2].TextureCoordinate = new Vector2(decomposedVertices[i].NextVertex(j + 1).X / _textureList[textureName].Width, decomposedVertices[i].NextVertex(j + 1).Y / _textureList[textureName].Height - 1f);
                            verticesFill[i][3 * j].Color = verticesFill[i][3 * j + 1].Color = verticesFill[i][3 * j + 2].Color = Color.Transparent;
                        }
                    }

                    Vector2 vertsSize = new Vector2(polygonBounds.UpperBound.X - polygonBounds.LowerBound.X, polygonBounds.UpperBound.Y - polygonBounds.LowerBound.Y);
                    result.Add(RenderTexture((int)vertsSize.X, (int)vertsSize.Y, _textureList.ContainsKey(textureName) ? _textureList[textureName] : null, Color.White, verticesFill, Array.Empty<VertexPositionColor>()));
                }
                else
                    result.Add(_textureList["Blank"]);
            }

            return result;
        }

        private Texture2D RenderTexture(int width, int height, Texture2D pattern, Color patternColor, VertexPositionColorTexture[] verticesFill, VertexPositionColor[] verticesOutline)
        {
            List<VertexPositionColorTexture[]> fill = new List<VertexPositionColorTexture[]>(1);
            fill.Add(verticesFill);
            return RenderTexture(width, height, pattern, patternColor, fill, verticesOutline);
        }

        private Texture2D RenderTexture(int width, int height, Texture2D pattern, Color patternColor, List<VertexPositionColorTexture[]> verticesFill, VertexPositionColor[] verticesOutline)
        {
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0f);
            PresentationParameters pp = _graphicsDevice.PresentationParameters;
            RenderTarget2D texture = new RenderTarget2D(_graphicsDevice, width + 2, height + 2, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            _graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

            _graphicsDevice.SetRenderTarget(texture);
            _graphicsDevice.Clear(Color.Transparent);
            _effect.Projection = Matrix.CreateOrthographic(width + 2f, -height - 2f, 0f, 1f);
            _effect.View = halfPixelOffset;

            // render shape;
            _effect.TextureEnabled = true;
            _effect.Texture = _textureList["Blank"];
            _effect.VertexColorEnabled = true;

            _effect.CurrentTechnique.Passes[0].Apply();

            for (int i = 0; i < verticesFill.Count; i++)
            {
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verticesFill[i], 0, verticesFill[i].Length / 3);
            }

            if (pattern != null)
            {
                _effect.Texture = pattern;
                _effect.CurrentTechnique.Passes[0].Apply();
                for (int i = 0; i < verticesFill.Count; i++)
                {
                    for (int j = 0; j < verticesFill[i].Length; j++)
                    {
                        verticesFill[i][j].Color = patternColor;
                    }

                    _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verticesFill[i], 0, verticesFill[i].Length / 3);
                }
            }

            // render outline;
            if (verticesOutline.Length > 1)
            {
                _effect.TextureEnabled = false;
                _effect.CurrentTechnique.Passes[0].Apply();
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verticesOutline, 0, verticesOutline.Length / 2);
            }

            _graphicsDevice.SetRenderTarget(null);
            return texture;
        }

        public void Dispose()
        {
            _effect?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}