using System;
//using DemoBaseXNA.DrawingSystem;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    public abstract class BasePrimitiveRenderHelper
    {
        // Fields
        private int count;
        private VertexPositionColor[] vertices;
        private VertexDeclaration vertexDeclaration;
        private BasicEffect effect;

        // Methods
        public BasePrimitiveRenderHelper(int vertexcapacity, GraphicsDevice gd)
        {
            this.vertices = new VertexPositionColor[vertexcapacity];
            effect = new BasicEffect(gd, null);
            effect.VertexColorEnabled = true;
        }

        public virtual void Clear()
        {
            this.count = 0;
        }

        public void Render(GraphicsDevice device)
        {
            if (this.count >= 2)
            {
                //device.VertexDeclaration = ;
                device.DrawUserPrimitives<VertexPositionColor>(this.PrimitiveType, this.vertices, 0, this.PrimitiveCount);
            }
        }

        public void Render(GraphicsDevice device, Matrix projection, Matrix view)
        {
            effect.Projection = projection;
            effect.View = view;

            Render(device, effect);
        }

        public void Render(GraphicsDevice device, Effect effect)
        {
            if (this.count >= 2)
            {
                effect.Begin();
                for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
                {
                    EffectPass pass = effect.CurrentTechnique.Passes[0];
                    pass.Begin();
                    this.Render(device);
                    pass.End();
                }
                effect.End();
            }
        }

        protected void SubmitVertex(Vector3 position, Color color)
        {
            this.vertices[this.count].Position = position;
            this.vertices[this.count].Color = color;
            count++;
        }

        // Properties
        public abstract int PrimitiveCount { get; }

        protected abstract PrimitiveType PrimitiveType { get; }

        public int VertexCapacity
        {
            get
            {
                return this.vertices.Length;
            }
        }

        public int VertexCount
        {
            get
            {
                return this.count;
            }
        }
    }
}


