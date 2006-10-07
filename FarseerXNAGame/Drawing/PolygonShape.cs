using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FarseerGames.FarseerXNAGame.Drawing {
    /// <summary>
    /// A base class for drawing simple vector shapes like the retro mode graphics
    /// </summary>
    public abstract class PolygonShape : Shape {
        private static Effect effect;
        private static EffectParameter worldViewProjectionParam;

        public PolygonShape(Game game)
            : base(game) {
        }

        /// <summary>
        /// Creates the vertex buffers and calls Fill buffer to get the inhertied classes to complete the task
        /// </summary>
        public override void Create() {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.GameServices.GetService(typeof(IGraphicsDeviceService));
            buffer = new VertexBuffer(graphicsService.GraphicsDevice, typeof(VertexPositionColor), 2 * NumberOfVectors, ResourceUsage.WriteOnly, ResourcePool.Managed);

            VertexPositionColor[] data = new VertexPositionColor[2 * NumberOfVectors];
            FillBuffer(data);

            buffer.SetData<VertexPositionColor>(data);

            vertexDecl = new VertexDeclaration(graphicsService.GraphicsDevice, VertexPositionColor.VertexElements);

            //Load the correct shader and set up the parameters
            if (effect == null || effect.IsDisposed) {
                Reset();
            }
        }

        /// <summary>
        /// Override this method to fill in the vertex buffer with your shape data
        /// </summary>
        /// <param name="data">A blob of vertex PositionColored data</param>
        abstract protected void FillBuffer(VertexPositionColor[] data);

        /// <summary>
        /// Override this to indicate how many vectors in your vector shape
        /// </summary>
        abstract protected int NumberOfVectors {
            get;
        }

        public void Update(Vector2 position, float orientation) {
            Vector3 position3D = new Vector3(position, 0);
            Matrix rotationMatrix = Matrix.CreateRotationZ(orientation);
            Matrix translationMatrix = Matrix.CreateTranslation(position3D);
            World = rotationMatrix * translationMatrix;
        }

        /// <summary>
        /// Draws the vector shape
        /// </summary>
        public override void Draw() {
            base.Draw();

            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.GameServices.GetService(typeof(IGraphicsDeviceService));
            GraphicsDevice device = graphicsService.GraphicsDevice;

            device.VertexDeclaration = vertexDecl;
            device.Vertices[0].VertexBuffer = buffer;
            device.Vertices[0].VertexStride = VertexPositionColor.SizeInBytes;

            effect.Begin(EffectStateOptions.Default);
            effect.Techniques[0].Passes[0].Begin();

            Matrix projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4, 1024f / 768f, 10f, 700f);
            //Matrix projection = Matrix.CreateOrthographic(1024,768, 10f, 700f);
            Matrix view = Matrix.CreateLookAt(new Vector3(0,0,-50), Vector3.Zero, -Vector3.Up);
            worldViewProjectionParam.SetValue(World * view * projection);
            effect.CommitChanges();

            device.DrawPrimitives(PrimitiveType.LineList, 0, NumberOfVectors);

            effect.Techniques[0].Passes[0].End();
            effect.End();
        }

        public override void Reset() {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.GameServices.GetService(typeof(IGraphicsDeviceService));

            CompiledEffect compiledEffect = Effect.CompileEffectFromFile(@"Resources\Effects\simple.fx", null, null, CompilerOptions.None, TargetPlatform.Windows);
            effect = new Effect(graphicsService.GraphicsDevice, compiledEffect.GetShaderCode(), CompilerOptions.None, null);

            worldViewProjectionParam = effect.Parameters["worldViewProjection"];

            buffer = new VertexBuffer(graphicsService.GraphicsDevice, typeof(VertexPositionColor), 2 * NumberOfVectors, ResourceUsage.WriteOnly, ResourcePool.Managed);
            VertexPositionColor[] data = new VertexPositionColor[2 * NumberOfVectors];
            FillBuffer(data);
            buffer.SetData<VertexPositionColor>(data);

            vertexDecl = new VertexDeclaration(graphicsService.GraphicsDevice, VertexPositionColor.VertexElements);
        }
    }
}
