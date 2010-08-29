using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    public class TriangleRenderHelper : BasePrimitiveRenderHelper
    {
        // Methods
        public TriangleRenderHelper(int vertexcapacity, GraphicsDevice gd)
            : base(vertexcapacity, gd)
        {
        }

        public void Submit(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            this.Submit(a, b, c, color, color, color);
        }

        public void Submit(Vector3 a, Vector3 b, Vector3 c, Color acolor, Color bcolor, Color ccolor)
        {
            base.SubmitVertex(a, acolor);
            base.SubmitVertex(b, bcolor);
            base.SubmitVertex(c, ccolor);
        }

        // Properties
        public override int PrimitiveCount
        {
            get
            {
                return (base.VertexCount / 3);
            }
        }

        protected override PrimitiveType PrimitiveType
        {
            get
            {
                return PrimitiveType.TriangleList;
            }
        }
    }
}