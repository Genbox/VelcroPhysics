#region Using System
using System;
using System.IO;
using System.Collections.Generic;
#endregion
#region Using XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion
#region Using Farseer
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
#endregion

namespace FarseerPhysics.Samples.MediaSystem
{
  public class AssetCreator : GameComponent
  {
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

    private const int CircleSegments = 32;

    private static AssetCreator _assetCreator = null;

    private static BasicEffect _effect;
    private static Dictionary<string, Texture2D> _materials = new Dictionary<string, Texture2D>();

    private AssetCreator(Game game)
      : base(game)
    {
      // Add all materials
      DirectoryInfo assetFolder = new DirectoryInfo(game.Content.RootDirectory + "/Materials");
      FileInfo[] fileList = assetFolder.GetFiles("*.xnb");

      for (int i = 0; i < fileList.Length; i++)
      {
        string materialName = System.IO.Path.GetFileNameWithoutExtension(fileList[i].Name);
        _materials[materialName] = game.Content.Load<Texture2D>("Materials/" + materialName);
        _materials[materialName].Name = materialName;
      }

      _effect = new BasicEffect(game.GraphicsDevice);
    }

    public static void Initialize(Game game)
    {
      if (_assetCreator == null && game != null)
      {
        _assetCreator = new AssetCreator(game);
        game.Components.Add(_assetCreator);
      }
    }

    public static Vector2 CalculateOrigin(Body body)
    {
      Vector2 lowerBound = new Vector2(float.MaxValue);
      AABB bounds;
      Transform transform;
      body.GetTransform(out transform);

      for (int i = 0; i < body.FixtureList.Count; i++)
      {
        for (int j = 0; j < body.FixtureList[i].Shape.ChildCount; j++)
        {
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
      return TextureFromShape(shape, "blank", color, color, outlineColor, 1f);
    }

    public static Texture2D TextureFromShape(Shape shape, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
    {
      if (_assetCreator != null)
      {
        switch (shape.ShapeType)
        {
          case ShapeType.Circle:
            return CircleTexture(((CircleShape)shape).Radius, pattern, mainColor, patternColor, outlineColor, materialScale);
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
      return CircleTexture(radius, "blank", color, color, outlineColor, 1f);
    }

    public static Texture2D CircleTexture(float radius, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
    {
      if (_assetCreator != null)
      {
        if (!_materials.ContainsKey(pattern))
        {
          pattern = "blank";
        }

        VertexPositionColorTexture[] verticesFill = new VertexPositionColorTexture[3 * (CircleSegments - 2)];
        VertexPositionColor[] verticesOutline = new VertexPositionColor[2 * CircleSegments];

        const float segmentSize = MathHelper.TwoPi / CircleSegments;
        float theta = segmentSize;

        radius = ConvertUnits.ToDisplayUnits(radius);
        materialScale /= _materials[pattern].Width;

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

        if (pattern == "blank")
        {
          return _assetCreator.RenderTexture((int)(radius * 2f), (int)(radius * 2f), null, Color.Transparent, verticesFill, verticesOutline);
        }
        else
        {
          return  _assetCreator.RenderTexture((int)(radius * 2f), (int)(radius * 2f), _materials[pattern], patternColor, verticesFill, verticesOutline);
        }
      }
      return null;
    }

    public static Texture2D PolygonTexture(Vector2[] vertices, Color color, Color outlineColor)
    {
      return PolygonTexture(vertices, "blank", color, color, outlineColor, 1f);
    }

    public static Texture2D PolygonTexture(Vector2[] vertices, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
    {
      Vertices temp = new Vertices(vertices);
      return PolygonTexture(temp, pattern, mainColor, patternColor, outlineColor, materialScale);
    }

    public static Texture2D PolygonTexture(Vertices vertices, Color color, Color outlineColor)
    {
      return PolygonTexture(vertices, "blank", color, color, outlineColor, 1f);
    }

    public static Texture2D PolygonTexture(Vertices vertices, string pattern, Color mainColor, Color patternColor, Color outlineColor, float materialScale)
    {
      if (_assetCreator != null)
      {
        if (!_materials.ContainsKey(pattern))
        {
          pattern = "blank";
        }

        // copy vertices
        Vertices scaledVertices = new Vertices(vertices);

        // scale to display units (i.e. pixels) for rendering to texture
        Vector2 scale = ConvertUnits.ToDisplayUnits(Vector2.One);
        scaledVertices.Scale(ref scale);

        // translate the boundingbox center to the texture center
        // because we use an orthographic projection for rendering later
        AABB verticesBounds = scaledVertices.GetCollisionBox();
        scaledVertices.Translate(-verticesBounds.Center);

        List<Vertices> decomposedVertices;
        if (!scaledVertices.IsConvex())
        {
          decomposedVertices = EarclipDecomposer.ConvexPartition(scaledVertices);
        }
        else
        {
          decomposedVertices = new List<Vertices>();
          decomposedVertices.Add(scaledVertices);
        }

        List<VertexPositionColorTexture[]> verticesFill = new List<VertexPositionColorTexture[]>(decomposedVertices.Count);

        materialScale /= _materials[pattern].Width;

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

        if (pattern == "blank")
        {
          return _assetCreator.RenderTexture((int)vertsSize.X, (int)vertsSize.Y, null, Color.Transparent, verticesFill, verticesOutline);
        }
        else
        {
          return _assetCreator.RenderTexture((int)vertsSize.X, (int)vertsSize.Y, _materials[pattern], patternColor, verticesFill, verticesOutline);
        }
      }
      return null;
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
      _effect.Texture = _materials["blank"];
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
      _effect.TextureEnabled = false;
      _effect.Techniques[0].Passes[0].Apply();
      Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verticesOutline, 0, verticesOutline.Length / 2);
      Game.GraphicsDevice.SetRenderTarget(null);
      return texture;
    }
  }
}