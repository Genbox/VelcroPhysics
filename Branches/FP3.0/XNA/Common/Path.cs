using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{

    /// <summary>
    ///    Path: 
    ///    
    /// Very similar to Vertices, but this 
    /// class contains vectors describing 
    /// control points on a Catmull-Rom 
    /// curve.
    /// </summary>
    [XmlRoot("Path")]
    public class Path
    {
        /// <summary>
        /// True if the curve is closed.
        /// </summary>
        [XmlElement("Closed")]
        public bool Closed { get { return this.closed; } set { this.closed = value; } }
        
        [XmlElement("ControlPoints")]
        public List<Vector2> ControlPoints;

        private bool closed;
        private float delta_t;
        
        public Path()
        {
            ControlPoints = new List<Vector2>();
        }


        public Path(ref Vector2[] vector2)
        {
            ControlPoints = new List<Vector2>();
            for (int i = 0; i < vector2.Length; i++)
            {
                Add(vector2[i]);
            }
        }

        public Path(IList<Vector2> vertices)
        {
            ControlPoints = new List<Vector2>();
            for (int i = 0; i < vertices.Count; i++)
            {
                Add(vertices[i]);
            }
        }

        public int NextIndex(int index)
        {
            if (index == ControlPoints.Count - 1)
            {
                return 0;
            }
            return index + 1;
        }

        /// <summary>
        /// Gets the previous index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int PreviousIndex(int index)
        {
            if (index == 0)
            {
                return ControlPoints.Count - 1;
            }
            return index - 1;
        }

        /// <summary>
        /// Translates the control points by the specified vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public void Translate(ref Vector2 vector)
        {
            for (int i = 0; i < ControlPoints.Count; i++)
                ControlPoints[i] = Vector2.Add(ControlPoints[i], vector);
        }

        /// <summary>
        /// Scales the control points by the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public void Scale(ref Vector2 value)
        {
            for (int i = 0; i < ControlPoints.Count; i++)
                ControlPoints[i] = Vector2.Multiply(ControlPoints[i], value);
        }

        /// <summary>
        /// Rotate the control points by the defined value in radians.
        /// </summary>
        /// <param name="value">The amount to rotate by in radians.</param>
        public void Rotate(float value)
        {
            Matrix rotationMatrix;
            Matrix.CreateRotationZ(value, out rotationMatrix);

            for (int i = 0; i < ControlPoints.Count; i++)
                ControlPoints[i] = Vector2.Transform(ControlPoints[i], rotationMatrix);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                builder.Append(ControlPoints[i].ToString());
                if (i < ControlPoints.Count - 1)
                {
                    builder.Append(" ");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns a set of points defining the
        /// curve with the specifed number of divisions
        /// between each control point.
        /// </summary>
        /// <param name="divisions">Number of divisions between each control point.</param>
        /// <returns></returns>
        public List<Vector2> GetVertices(int divisions)
        {
            List<Vector2> verts = new List<Vector2>();

            float timeStep = 1f / (float)divisions;

            for (float i = 0; i < 1f; i += timeStep)
            {
                verts.Add(GetPosition(i));
            }

            return verts;
        }

        public Vector2 GetPosition(float time)
        {
            Vector2 temp;

            if (ControlPoints.Count < 2)
                throw new Exception("You need at least 2 control points to calculate a position.");

            if (Closed)
            {
                this.Add(ControlPoints[0]);

                delta_t = 1f / (float)(ControlPoints.Count - 1);

                int p = (int)(time / delta_t);

                // use a circular indexing system
                int p0 = p - 1;
                if (p0 < 0) p0 = p0 + (ControlPoints.Count - 1); else if (p0 >= (int)ControlPoints.Count - 1) p0 = p0 - (ControlPoints.Count - 1);
                int p1 = p;
                if (p1 < 0) p1 = p1 + (ControlPoints.Count - 1); else if (p1 >= (int)ControlPoints.Count - 1) p1 = p1 - (ControlPoints.Count - 1);
                int p2 = p + 1;
                if (p2 < 0) p2 = p2 + (ControlPoints.Count - 1); else if (p2 >= (int)ControlPoints.Count - 1) p2 = p2 - (ControlPoints.Count - 1);
                int p3 = p + 2;
                if (p3 < 0) p3 = p3 + (ControlPoints.Count - 1); else if (p3 >= (int)ControlPoints.Count - 1) p3 = p3 - (ControlPoints.Count - 1);

                // relative time
                float lt = (time - delta_t * (float)p) / delta_t;

                temp = Vector2.CatmullRom(ControlPoints[p0], ControlPoints[p1], ControlPoints[p2], ControlPoints[p3], lt);

                this.RemoveAt(ControlPoints.Count - 1);
            }
            else
            {
                int p = (int)(time / delta_t);

                // 
                int p0 = p - 1;
                if (p0 < 0) p0 = 0; else if (p0 >= (int)ControlPoints.Count - 1) p0 = ControlPoints.Count - 1;
                int p1 = p;
                if (p1 < 0) p1 = 0; else if (p1 >= (int)ControlPoints.Count - 1) p1 = ControlPoints.Count - 1;
                int p2 = p + 1;
                if (p2 < 0) p2 = 0; else if (p2 >= (int)ControlPoints.Count - 1) p2 = ControlPoints.Count - 1;
                int p3 = p + 2;
                if (p3 < 0) p3 = 0; else if (p3 >= (int)ControlPoints.Count - 1) p3 = ControlPoints.Count - 1;

                // relative time
                float lt = (time - delta_t * (float)p) / delta_t;

                temp = Vector2.CatmullRom(ControlPoints[p0], ControlPoints[p1], ControlPoints[p2], ControlPoints[p3], lt);
            }

            return temp;
        }

        /// <summary>
        /// Gets the normal for the given time.
        /// </summary>
        /// <param name="index">The time.</param>
        /// <returns>The normal.</returns>
        public Vector2 GetPositionNormal(float time)
        {
            float offsetTime = time + 0.0001f;

            Vector2 a = this.GetPosition(time);
            Vector2 b = this.GetPosition(offsetTime);

            Vector2 output, temp;

            Vector2.Subtract(ref a, ref b, out temp);

            output.X = -temp.Y;
            output.Y = temp.X;

            Vector2.Normalize(ref output, out output);

            return output;
        }

        public void Add(Vector2 point)
        {
            ControlPoints.Add(point);
            delta_t = 1f / (float)(ControlPoints.Count - 1);
        }

        public void Remove(Vector2 point)
        {
            ControlPoints.Remove(point);
            delta_t = 1f / (float)(ControlPoints.Count - 1);
        }

        public void RemoveAt(int index)
        {
            ControlPoints.RemoveAt(index);
            delta_t = 1f / (float)(ControlPoints.Count - 1);
        }

        public float GetLength()
        {
            List<Vector2> verts = this.GetVertices(ControlPoints.Count * 25);
            float length = 0;

            for (int i = 1; i < verts.Count; i++)
            {
                length += Vector2.Distance(verts[i - 1], verts[i]);
            }

            if (Closed)
                length += Vector2.Distance(verts[ControlPoints.Count - 1], verts[0]);
            
            return length;
        }

        public List<Vector3> SubdivideEvenly(int divisions)
        {
            List<Vector3> verts = new List<Vector3>();

            float length = this.GetLength();

            float deltaLength = length / (float)divisions + 0.001f;
            Vector2 start, end;
            float t = 0.000f;

            // we always start at the first control point
            start = this.ControlPoints[0];
            end = this.GetPosition(t);

            // increment t until we are at half the distance
            while (deltaLength * 0.5f >= Vector2.Distance(start, end))
            {
                end = this.GetPosition(t);
                t += 0.0001f;

                if (t >= 1f)
                    break;
            }

            start = end;

            // for each box
            for (int i = 1; i < divisions; i++)
            {
                Vector2 normal = this.GetPositionNormal(t);
                float angle = (float)Math.Atan2(normal.Y, normal.X);

                verts.Add(new Vector3(end, angle));

                // until we reach the correct distance down the curve
                while (deltaLength >= Vector2.Distance(start, end))
                {
                    end = this.GetPosition(t);
                    t += 0.00001f;

                    if (t >= 1f)
                        break;
                }
                if (t >= 1f)
                    break;

                start = end;
            }
            return verts;
        }
    }
}
