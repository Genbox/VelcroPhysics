using System;
//using DemoBaseXNA.DrawingSystem;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{

    public class LineRenderHelper : BasePrimitiveRenderHelper
    {
        // Methods
        public LineRenderHelper(int vertexcapacity)
            : base(vertexcapacity)
        {
        }

        public void Submit(Vector3 start, Vector3 end, Color color)
        {
            this.Submit(start, end, color, color);
        }

        public void Submit(Vector3 start, Vector3 end, Color startcolor, Color endcolor)
        {
            base.SubmitVertex(start, startcolor);
            base.SubmitVertex(end, endcolor);
        }

        // Properties
        public override int PrimitiveCount
        {
            get
            {
                return (base.VertexCount / 2);
            }
        }

        protected override PrimitiveType PrimitiveType
        {
            get
            {
                return PrimitiveType.LineList;
            }
        }
    }
}

