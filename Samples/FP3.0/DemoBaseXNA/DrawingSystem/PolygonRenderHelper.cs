using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DemoBaseXNA.DrawingSystem
{
    public class PolygonRenderHelper : BasePrimitiveRenderHelper
    {
        // Methods
        public PolygonRenderHelper(int vertexcapacity)
            : base(vertexcapacity)
        {
        }

        public void Submit(Vector3[] vertexArray, int num, Color color)
        {
            for (int i = 0; i < num; i++)
            {
                base.SubmitVertex(vertexArray[i], color);
            }
        }

        public void Submit(Vertices vertexArray, Color color)
        {
            for (int i = 0; i < vertexArray.Count; i++)
            {
                base.SubmitVertex(new Vector3(vertexArray[i], 0), color);
            }
        }

        // Properties
        public override int PrimitiveCount
        {
            get
            {
                return (base.VertexCount - 2);
            }
        }

        protected override PrimitiveType PrimitiveType
        {
            get
            {
                return PrimitiveType.TriangleFan;
            }
        }
    }
}