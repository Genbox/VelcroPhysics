using System;
using System.Collections.Generic;
using System.IO;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Path = System.IO.Path;

namespace FarseerPhysics.Samples.MediaSystem
{
    public class ContentWrapper : GameComponent
    {
        private const int CircleSegments = 32;
        private static int _soundVolume;

        public static Color Gold = new Color(246, 187, 53);
        public static Color Red = new Color(215, 1, 51);
        public static Color Green = new Color(102, 158, 68);
        public static Color Orange = new Color(218, 114, 44);
        public static Color Brown = new Color(123, 40, 11);

        public static Color Beige = new Color(233, 229, 217);
        public static Color Cream = new Color(246, 87, 84);
        public static Color Lime = new Color(146, 201, 43);
        public static Color Teal = new Color(66, 126, 120);
        public static Color Grey = new Color(73, 69, 69);

        public static Color Black = new Color(28, 19, 11);
        public static Color Sunset = new Color(194, 73, 24);
        public static Color Sky = new Color(185, 216, 221);

        public static Color Cyan = new Color(50, 201, 251);
        public static Color Blue = new Color(44, 138, 153);
        public static Color Ocean = new Color(57, 143, 171);

        private static ContentWrapper _contentWrapper;
        private static BasicEffect _effect;

        private static Dictionary<string, Texture2D> _textureList = new Dictionary<string, Texture2D>();
        private static Dictionary<string, SpriteFont> _fontList = new Dictionary<string, SpriteFont>();

        private static Dictionary<string, SoundEffect> _soundList = new Dictionary<string, SoundEffect>();

        public static int SoundVolume
        {
            get { return _soundVolume; }
            set
            {
                _soundVolume = (int)MathHelper.Clamp(value, 0f, 100f);
                SoundEffect.MasterVolume = _soundVolume / 100f;
            }
        }

        private ContentWrapper(Game game)
            : base(game)
        {
            DirectoryInfo currentAssetFolder;
            FileInfo[] currentFileList;

            // First create a blank texture
            _textureList["Blank"] = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _textureList["Blank"].SetData(new[] { Color.White });
            _textureList["Blank"].Name = "Blank";

            // Load all graphics
            string[] gfxFolders = { "Common", "DemoGFX", "Materials" };
            foreach (string folder in gfxFolders)
            {
                currentAssetFolder = new DirectoryInfo(game.Content.RootDirectory + "/" + folder);
                currentFileList = currentAssetFolder.GetFiles("*.xnb");
                for (int i = 0; i < currentFileList.Length; i++)
                {
                    string textureName = Path.GetFileNameWithoutExtension(currentFileList[i].Name);
                    _textureList[textureName] = game.Content.Load<Texture2D>(folder + "/" + textureName);
                    _textureList[textureName].Name = textureName;
                }
            }

            // Add samples fonts
            currentAssetFolder = new DirectoryInfo(game.Content.RootDirectory + "/Fonts");
            currentFileList = currentAssetFolder.GetFiles("*.xnb");

            for (int i = 0; i < currentFileList.Length; i++)
            {
                string fontName = Path.GetFileNameWithoutExtension(currentFileList[i].Name);
                _fontList[fontName] = game.Content.Load<SpriteFont>("Fonts/" + fontName);
            }

            // Add basic effect for texture generation
            _effect = new BasicEffect(game.GraphicsDevice);

            // Initialize audio playback
            currentAssetFolder = new DirectoryInfo(game.Content.RootDirectory + "/DemoSFX");
            currentFileList = currentAssetFolder.GetFiles("*.xnb");

            for (int i = 0; i < currentFileList.Length; i++)
            {
                string soundName = Path.GetFileNameWithoutExtension(currentFileList[i].Name);
                _soundList[soundName] = game.Content.Load<SoundEffect>("DemoSFX/" + soundName);
                _soundList[soundName].Name = soundName;
            }

            try
            {
                SoundVolume = 100;
            }
            catch (NoAudioHardwareException)
            {
                // silently fall back to silence
            }
        }

        public static void Initialize(Game game)
        {
            if (_contentWrapper == null && game != null)
            {
                _contentWrapper = new ContentWrapper(game);
                game.Components.Add(_contentWrapper);
            }
        }

        public static Texture2D GetTexture(string textureName)
        {
            if (_contentWrapper != null && _textureList.ContainsKey(textureName))
            {
                return _textureList[textureName];
            }
            else
            {
#if WINDOWS
                Console.WriteLine("Texture \"" + textureName + "\" not found!");
#endif
                return null;
            }
        }

        public static SpriteFont GetFont(string fontName)
        {
            if (_contentWrapper != null && _fontList.ContainsKey(fontName))
                return _fontList[fontName];
            else
                throw new FileNotFoundException();
        }

        public static Vector2 CalculateOrigin(Body body)
        {
            Vector2 lowerBound = new Vector2(float.MaxValue);
            Transform transform;
            body.GetTransform(out transform);

            for (int i = 0; i < body.FixtureList.Count; i++)
            {
                for (int j = 0; j < body.FixtureList[i].Shape.ChildCount; j++)
                {
                    AABB bounds;
                    body.FixtureList[i].Shape.ComputeAABB(out bounds, ref transform, j);
                    Vector2.Min(ref lowerBound, ref bounds.LowerBound, out lowerBound);
                }
            }
            // calculate body offset from its center and add a 1 pixel border
            // because we generate the textures a little bigger than the actual body's fixtures
            return ConvertUnits.ToDisplayUnits(body.Position - lowerBound) + new Vector2(1f);
        }

        public static Texture2D TextureFromShape(Shape shape, Color color, Color outlineColor)
        {
            return TextureFromShape(shape, "Blank", color, color, outlineColor, 1f);
        }

        public static Texture2D TextureFromShape(Shape shape, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
        {
            if (_contentWrapper != null)
            {
                switch (shape.ShapeType)
                {
                    case ShapeType.Circle:
                        return CircleTexture((shape).Radius, pattern, mainColor, patternColor, outlineColor, materialScale);
                    case ShapeType.Polygon:
                        return PolygonTexture(((PolygonShape)shape).Vertices, pattern, mainColor, patternColor, outlineColor, materialScale);
                    default:
                        throw new NotSupportedException("The specified shape type is not supported.");
                }
            }
            return null;
        }

        public static Texture2D CircleTexture(float radius, Color color, Color outlineColor)
        {
            return CircleTexture(radius, "Blank", color, color, outlineColor, 1f);
        }

        public static Texture2D CircleTexture(float radius, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
        {
            if (_contentWrapper != null)
            {
                VertexPositionColorTexture[] verticesFill = new VertexPositionColorTexture[3 * (CircleSegments - 2)];
                VertexPositionColor[] verticesOutline = new VertexPositionColor[2 * CircleSegments];

                const float segmentSize = MathHelper.TwoPi / CircleSegments;
                float theta = segmentSize;

                radius = ConvertUnits.ToDisplayUnits(radius);
                if (_textureList.ContainsKey(pattern))
                    materialScale /= _textureList[pattern].Width;
                else
                    materialScale = 1f;

                Vector2 start = new Vector2(radius, 0f);

                for (int i = 0; i < CircleSegments - 2; i++)
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
                    if (i == CircleSegments - 3)
                    {
                        verticesOutline[2 * CircleSegments - 2].Position = new Vector3(p2, 0f);
                        verticesOutline[2 * CircleSegments - 1].Position = new Vector3(start, 0f);
                        verticesOutline[2 * CircleSegments - 2].Color = verticesOutline[2 * CircleSegments - 1].Color = outlineColor;
                    }
                    verticesOutline[2 * i + 2].Position = new Vector3(p1, 0f);
                    verticesOutline[2 * i + 3].Position = new Vector3(p2, 0f);
                    verticesOutline[2 * i + 2].Color = verticesOutline[2 * i + 3].Color = outlineColor;

                    theta += segmentSize;
                }

                return _contentWrapper.RenderTexture((int)(radius * 2f), (int)(radius * 2f), _textureList.ContainsKey(pattern) ? _textureList[pattern] : null, patternColor, verticesFill, verticesOutline);
            }
            return null;
        }

        public static Texture2D PolygonTexture(Vector2[] vertices, Color color, Color outlineColor)
        {
            return PolygonTexture(vertices, "Blank", color, color, outlineColor, 1f);
        }

        public static Texture2D PolygonTexture(Vector2[] vertices, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
        {
            Vertices temp = new Vertices(vertices);
            return PolygonTexture(temp, pattern, mainColor, patternColor, outlineColor, materialScale);
        }

        public static Texture2D PolygonTexture(Vertices vertices, Color color, Color outlineColor)
        {
            return PolygonTexture(vertices, "Blank", color, color, outlineColor, 1f);
        }

        public static Texture2D PolygonTexture(Vertices vertices, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
        {
            if (_contentWrapper != null)
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
                {
                    decomposedVertices = Triangulate.ConvexPartition(scaledVertices, TriangulationAlgorithm.Earclip);
                }
                else
                {
                    decomposedVertices = new List<Vertices>();
                    decomposedVertices.Add(scaledVertices);
                }

                List<VertexPositionColorTexture[]> verticesFill = new List<VertexPositionColorTexture[]>(decomposedVertices.Count);

                if (_textureList.ContainsKey(pattern))
                {
                    materialScale /= _textureList[pattern].Width;
                }
                else
                {
                    materialScale = 1f;
                }

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

                return _contentWrapper.RenderTexture((int)vertsSize.X, (int)vertsSize.Y, _textureList.ContainsKey(pattern) ? _textureList[pattern] : null, patternColor, verticesFill, verticesOutline);
            }
            return null;
        }

        public static List<Texture2D> BreakableTextureFragments(BreakableBody body, string textureName)
        {
            List<Texture2D> result = new List<Texture2D>();
            if (_contentWrapper != null)
            {
                Vector2 scale = ConvertUnits.ToDisplayUnits(Vector2.One);
                foreach (Fixture f in body.Parts)
                {
                    Vertices v = null;
                    if (f.Shape is PolygonShape)
                    {
                        v = new Vertices(((PolygonShape)f.Shape).Vertices);
                        v.Scale(ref scale);
                    }
                    if (v != null)
                    {
                        AABB polygonBounds = v.GetAABB();
                        List<Vertices> decomposedVertices;
                        if (!v.IsConvex())
                        {
                            decomposedVertices = Triangulate.ConvexPartition(v, TriangulationAlgorithm.Bayazit);
                        }
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
                        result.Add(_contentWrapper.RenderTexture((int)vertsSize.X, (int)vertsSize.Y, _textureList.ContainsKey(textureName) ? _textureList[textureName] : null, Color.White, verticesFill, new VertexPositionColor[0]));
                    }
                    else
                    {
                        result.Add(_textureList["Blank"]);
                    }
                }
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
            PresentationParameters pp = Game.GraphicsDevice.PresentationParameters;
            RenderTarget2D texture = new RenderTarget2D(Game.GraphicsDevice, width + 2, height + 2, false, SurfaceFormat.Color, DepthFormat.None, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Game.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

            Game.GraphicsDevice.SetRenderTarget(texture);
            Game.GraphicsDevice.Clear(Color.Transparent);
            _effect.Projection = Matrix.CreateOrthographic(width + 2f, -height - 2f, 0f, 1f);
            _effect.View = halfPixelOffset;
            // render shape;
            _effect.TextureEnabled = true;
            _effect.Texture = _textureList["Blank"];
            _effect.VertexColorEnabled = true;
            _effect.Techniques[0].Passes[0].Apply();

            for (int i = 0; i < verticesFill.Count; i++)
            {
                Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verticesFill[i], 0, verticesFill[i].Length / 3);
            }

            if (pattern != null)
            {
                _effect.Texture = pattern;
                _effect.Techniques[0].Passes[0].Apply();
                for (int i = 0; i < verticesFill.Count; i++)
                {
                    for (int j = 0; j < verticesFill[i].Length; j++)
                    {
                        verticesFill[i][j].Color = patternColor;
                    }
                    Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verticesFill[i], 0, verticesFill[i].Length / 3);
                }
            }
            // render outline;
            if (verticesOutline.Length > 1)
            {
                _effect.TextureEnabled = false;
                _effect.Techniques[0].Passes[0].Apply();
                Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verticesOutline, 0, verticesOutline.Length / 2);
            }
            Game.GraphicsDevice.SetRenderTarget(null);
            return texture;
        }

        /// <summary>
        /// Plays a fire-and-forget sound effect by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        public static void PlaySoundEffect(string soundName)
        {
            if (_contentWrapper != null && _soundList.ContainsKey(soundName))
                _soundList[soundName].Play();
            else
                throw new FileNotFoundException();
        }

        /// <summary>
        /// Plays a sound effect by name and returns an instance of that sound.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        /// <param name="looped">True if sound effect should loop.</param>
        public static SoundEffectInstance PlaySoundEffect(string soundName, bool looped)
        {
            SoundEffectInstance instance = null;
            if (_contentWrapper != null && _soundList != null && _soundList.ContainsKey(soundName))
            {
                try
                {
                    instance = _soundList[soundName].CreateInstance();
                    if (instance != null)
                    {
                        instance.IsLooped = looped;
                        instance.Play();
                    }
                }
                catch (InstancePlayLimitException)
                {
                    // silently fail (returns null instance) if instance limit reached
                }
            }
            else
            {
                throw new FileNotFoundException();
            }
            return instance;
        }
    }
}
