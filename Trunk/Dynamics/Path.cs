using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Controllers;
using FarseerGames.FarseerPhysics.Dynamics.Joints;
using FarseerGames.FarseerPhysics.Dynamics.Springs;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Interfaces;
using FarseerGames.FarseerPhysics.Mathematics;

namespace Path_Generator
{
    class Path
    {
        private bool _recalculate;          // will be set to true if path needs to be recalculated
        private GeomList _geoms;            // holds all geoms for this path
        private BodyList _bodies;           // holds all bodies for this path
        private Vertices _controlPoints;    // holds all control points for this path
        private bool _loop;                 // is this path a loop
        private float _controlPointSize = 6.0f;     // size of control point used in PointInControlPoint
        private float _precision = 0.01f;         // a coeffient used to decide how precise to place bodies
        private float _width, _height;      // width and height of bodies to create

        // This is used in my editing application.
        /// <summary>
        /// Gets index of control point if point is inside it.
        /// </summary>
        /// <param name="point">Point to test against.</param>
        /// <returns>Index of control point or -1 if no intersection.</returns>
        public int PointInControlPoint(Vector2 point)
        {
            AABB controlPointAABB;

            foreach (Vector2 v in _controlPoints)
            {
                controlPointAABB = new AABB(new Vector2(v.X - (_controlPointSize / 2), v.Y - (_controlPointSize / 2)),
                    new Vector2(v.X + (_controlPointSize / 2), v.Y + (_controlPointSize / 2)));

                if (controlPointAABB.Contains(ref point))
                    return _controlPoints.IndexOf(v);
            }
            return -1;
        }

        /// <summary>
        /// Adds a control point to the paths end.
        /// </summary>
        /// <param name="controlPoint">Vector2 to add.</param>
        public void Add(Vector2 controlPoint)
        {
            if (!_controlPoints.Contains(controlPoint))     // if the control point is not already in the list 
            {
                _controlPoints.Add(controlPoint);           // then add it
                _recalculate = true;                        // be sure to recalculate the curve
            }
        }

        /// <summary>
        /// Removes a control point from the path.
        /// </summary>
        /// <param name="controlPoint">Vector2 to remove.</param>
        public void Remove(Vector2 controlPoint)
        {
            _controlPoints.Remove(controlPoint);
            _recalculate = true;                        // be sure to recalculate the curve
        }

        /// <summary>
        /// Removes a control point from the path by index.
        /// </summary>
        /// <param name="controlPoint">Index of Vector2 to remove.</param>
        public void Remove(int index)
        {
            _controlPoints.RemoveAt(index);
            _recalculate = true;                        // be sure to recalculate the curve
        }

        /// <summary>
        /// Performs a complete update of the path.
        /// NOTE: should not be performed on a path
        /// in simulation.
        /// </summary>
        public void Update()
        {
            float distance = 0.0f;
            float k = 0.0f;
            Body tempBody;
            Vector2 tempVectorA = new Vector2();
            Vector2 tempVectorB = new Vector2();

            if (_recalculate)   // only do the update if something has changed
            {
                // first we get our curve ready
                Curve xCurve = new Curve();
                Curve yCurve = new Curve();
                float curveIncrement = 1.0f / _controlPoints.Count;

                for (int i = 0; i < _controlPoints.Count; i++)       // for all the control points 
                {
                    xCurve.Keys.Add(new CurveKey(curveIncrement * i, _controlPoints[i].X));     // set the keys for x and y
                    yCurve.Keys.Add(new CurveKey(curveIncrement * i, _controlPoints[i].Y));     // with a time from 0-1
                }

                xCurve.ComputeTangents(CurveTangent.Smooth);        // compute x tangents
                yCurve.ComputeTangents(CurveTangent.Smooth);        // compute y tangents

                // next we find the first point at 1/2 the width because we are finding where the body's center will be placed
                while (distance <= (_width / 2.0f))      // while the distance along the curve is <= to width / 2  
                {
                    k += _precision;        // we increment along the line at this precision coeffient
                    tempVectorA = new Vector2(xCurve.Evaluate(k), yCurve.Evaluate(k));
                    distance = Vector2.Distance(_controlPoints[0], tempVectorA);      // get the distance
                }
                tempBody = BodyFactory.Instance.CreateRectangleBody(_width, _height, 1.0f);     // create the first body
                tempBody.Rotation = FindEdgeAngle(_controlPoints[0], tempVectorA);               // set the angle

                _bodies.Add(tempBody);          // add the first body

                tempVectorB = tempVectorA;

                // now that our first body is done we can start on all our other body's
                // since the curve was created with a time of 0-1 we can just stop creating bodies when k is 1
                while (k <= 1.0f)
                {
                    // next we find the first point at the width because we are finding where the body's center will be placed
                    while (distance <= _width)      // while the distance along the curve is <= to width
                    {
                        k += _precision;        // we increment along the line at this precision coeffient
                        tempVectorA = new Vector2(xCurve.Evaluate(k), yCurve.Evaluate(k));
                        distance = Vector2.Distance(tempVectorB, tempVectorA);      // get the distance
                    }
                    tempBody = BodyFactory.Instance.CreateRectangleBody(_width, _height, 1.0f);     // create the first body
                    tempBody.Rotation = FindEdgeAngle(tempVectorB, tempVectorA);               // set the angle

                    _bodies.Add(tempBody);      // add all the rest of the bodies

                    tempVectorB = tempVectorA;
                }
            }
        }

        /// <summary>
        /// Finds the mid-point of two Vector2.
        /// </summary>
        /// <param name="a">First Vector2.</param>
        /// <param name="b">Other Vector2.</param>
        /// <returns>Mid-point Vector2.</returns>
        private Vector2 FindMidpoint(Vector2 a, Vector2 b)
        {
            float midDeltaX, midDeltaY;

            if (a.X < b.X)
                midDeltaX = (a.X - b.X) * 0.5f;         // find x axis midpoint
            else
                midDeltaX = (b.X - a.X) * 0.5f;         // find x axis midpoint
            if (a.Y < b.Y)
                midDeltaY = (a.Y - b.Y) * 0.5f;         // find y axis midpoint
            else
                midDeltaY = (b.Y - a.Y) * 0.5f;         // find y axis midpoint

            return (new Vector2(a.X + midDeltaX, a.Y + midDeltaY));   // return mid point
        }

        /// <summary>
        /// Finds the angle of an edge.
        /// </summary>
        /// <param name="a">First Vector2.</param>
        /// <param name="b">Other Vector2.</param>
        /// <returns>Angle in radians of the edge.</returns>
        private float FindEdgeAngle(Vector2 a, Vector2 b)
        {
            b.X = -a.Y;     // get 2D normal
            b.Y = a.X;      // works only on counter clockwise polygons

            if ((b.Y > 0.0f) && (b.X > 0.0f))
                return ((float)Math.Atan((float)b.X / -b.Y));
            else if ((b.Y < 0.0f) && (b.X > 0.0f))
                return ((float)Math.Atan((float)b.X / -b.Y));
            else if ((b.Y > 0.0f) && (b.X < 0.0f))
                return ((float)Math.Atan((float)-b.X / b.Y));
            else if ((b.Y < 0.0f) && (b.X < 0.0f))
                return ((float)Math.Atan((float)-b.X / b.Y));

            return 0.0f;
        }
    }
}
