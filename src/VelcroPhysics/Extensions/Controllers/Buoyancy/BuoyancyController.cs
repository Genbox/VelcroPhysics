using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Extensions.Controllers.ControllerBase;
using VelcroPhysics.Shared;
using VelcroPhysics.Utilities;

namespace VelcroPhysics.Extensions.Controllers.Buoyancy
{
    public sealed class BuoyancyController : Controller
    {
        private AABB _container;

        private Vector2 _gravity;
        private Vector2 _normal;
        private float _offset;
        private Dictionary<int, Body> _uniqueBodies = new Dictionary<int, Body>();

        /// <summary>
        /// Controls the rotational drag that the fluid exerts on the bodies within it. Use higher values will simulate thick
        /// fluid, like honey, lower values to
        /// simulate water-like fluids.
        /// </summary>
        public float AngularDragCoefficient;

        /// <summary>
        /// Density of the fluid. Higher values will make things more buoyant, lower values will cause things to sink.
        /// </summary>
        public float Density;

        /// <summary>
        /// Controls the linear drag that the fluid exerts on the bodies within it.  Use higher values will simulate thick fluid,
        /// like honey, lower values to
        /// simulate water-like fluids.
        /// </summary>
        public float LinearDragCoefficient;

        /// <summary>
        /// Acts like waterflow. Defaults to 0,0.
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuoyancyController" /> class.
        /// </summary>
        /// <param name="container">Only bodies inside this AABB will be influenced by the controller</param>
        /// <param name="density">Density of the fluid</param>
        /// <param name="linearDragCoefficient">Linear drag coefficient of the fluid</param>
        /// <param name="rotationalDragCoefficient">Rotational drag coefficient of the fluid</param>
        /// <param name="gravity">The direction gravity acts. Buoyancy force will act in opposite direction of gravity.</param>
        public BuoyancyController(AABB container, float density, float linearDragCoefficient, float rotationalDragCoefficient, Vector2 gravity)
            : base(ControllerType.BuoyancyController)
        {
            Container = container;
            _normal = new Vector2(0, 1);
            Density = density;
            LinearDragCoefficient = linearDragCoefficient;
            AngularDragCoefficient = rotationalDragCoefficient;
            _gravity = gravity;
        }

        public AABB Container
        {
            get { return _container; }
            set
            {
                _container = value;
                _offset = _container.UpperBound.Y;
            }
        }

        public override void Update(float dt)
        {
            _uniqueBodies.Clear();
            World.QueryAABB(fixture =>
            {
                if (fixture.Body.IsStatic || !fixture.Body.Awake)
                    return true;

                if (!_uniqueBodies.ContainsKey(fixture.Body.BodyId))
                    _uniqueBodies.Add(fixture.Body.BodyId, fixture.Body);

                return true;
            }, ref _container);

            foreach (KeyValuePair<int, Body> kv in _uniqueBodies)
            {
                Body body = kv.Value;

                Vector2 areac = Vector2.Zero;
                Vector2 massc = Vector2.Zero;
                float area = 0;
                float mass = 0;

                for (int j = 0; j < body.FixtureList.Count; j++)
                {
                    Fixture fixture = body.FixtureList[j];

                    if (fixture.Shape.ShapeType != ShapeType.Polygon && fixture.Shape.ShapeType != ShapeType.Circle)
                        continue;

                    Shape shape = fixture.Shape;

                    Vector2 sc;
                    float sarea = ComputeSubmergedArea(shape, ref _normal, _offset, ref body._xf, out sc);
                    area += sarea;
                    areac.X += sarea * sc.X;
                    areac.Y += sarea * sc.Y;

                    mass += sarea * shape.Density;
                    massc.X += sarea * sc.X * shape.Density;
                    massc.Y += sarea * sc.Y * shape.Density;
                }

                areac.X /= area;
                areac.Y /= area;
                massc.X /= mass;
                massc.Y /= mass;

                if (area < Settings.Epsilon)
                    continue;

                //Buoyancy
                Vector2 buoyancyForce = -Density * area * _gravity;
                body.ApplyForce(buoyancyForce, massc);

                //Linear drag
                Vector2 dragForce = body.GetLinearVelocityFromWorldPoint(areac) - Velocity;
                dragForce *= -LinearDragCoefficient * area;
                body.ApplyForce(dragForce, areac);

                //Angular drag
                body.ApplyTorque(-body.Inertia / body.Mass * area * body.AngularVelocity * AngularDragCoefficient);
            }
        }

        private float ComputeSubmergedArea(Shape shape, ref Vector2 normal, float offset, ref Transform xf, out Vector2 sc)
        {
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circleShape = (CircleShape)shape;

                        sc = Vector2.Zero;

                        Vector2 p = MathUtils.Mul(ref xf, circleShape.Position);
                        float l = -(Vector2.Dot(normal, p) - offset);
                        if (l < -circleShape.Radius + Settings.Epsilon)
                        {
                            //Completely dry
                            return 0;
                        }
                        if (l > circleShape.Radius)
                        {
                            //Completely wet
                            sc = p;
                            return Settings.Pi * circleShape._2radius;
                        }

                        //Magic
                        float l2 = l * l;
                        float area = circleShape._2radius * (float)((Math.Asin(l / circleShape.Radius) + Settings.Pi / 2) + l * Math.Sqrt(circleShape._2radius - l2));
                        float com = -2.0f / 3.0f * (float)Math.Pow(circleShape._2radius - l2, 1.5f) / area;

                        sc.X = p.X + normal.X * com;
                        sc.Y = p.Y + normal.Y * com;

                        return area;
                    }
                case ShapeType.Edge:
                    sc = Vector2.Zero;
                    return 0;
                case ShapeType.Polygon:
                    {
                        sc = Vector2.Zero;

                        PolygonShape polygonShape = (PolygonShape)shape;

                        //Transform plane into shape co-ordinates
                        Vector2 normalL = MathUtils.MulT(xf.q, normal);
                        float offsetL = offset - Vector2.Dot(normal, xf.p);

                        float[] depths = new float[Settings.MaxPolygonVertices];
                        int diveCount = 0;
                        int intoIndex = -1;
                        int outoIndex = -1;

                        bool lastSubmerged = false;
                        int i;
                        for (i = 0; i < polygonShape.Vertices.Count; i++)
                        {
                            depths[i] = Vector2.Dot(normalL, polygonShape.Vertices[i]) - offsetL;
                            bool isSubmerged = depths[i] < -Settings.Epsilon;
                            if (i > 0)
                            {
                                if (isSubmerged)
                                {
                                    if (!lastSubmerged)
                                    {
                                        intoIndex = i - 1;
                                        diveCount++;
                                    }
                                }
                                else
                                {
                                    if (lastSubmerged)
                                    {
                                        outoIndex = i - 1;
                                        diveCount++;
                                    }
                                }
                            }
                            lastSubmerged = isSubmerged;
                        }
                        switch (diveCount)
                        {
                            case 0:
                                if (lastSubmerged)
                                {
                                    //Completely submerged
                                    sc = MathUtils.Mul(ref xf, polygonShape.MassData.Centroid);
                                    return polygonShape.MassData.Mass / Density;
                                }

                                //Completely dry
                                return 0;
                            case 1:
                                if (intoIndex == -1)
                                {
                                    intoIndex = polygonShape.Vertices.Count - 1;
                                }
                                else
                                {
                                    outoIndex = polygonShape.Vertices.Count - 1;
                                }
                                break;
                        }

                        int intoIndex2 = (intoIndex + 1) % polygonShape.Vertices.Count;
                        int outoIndex2 = (outoIndex + 1) % polygonShape.Vertices.Count;

                        float intoLambda = (0 - depths[intoIndex]) / (depths[intoIndex2] - depths[intoIndex]);
                        float outoLambda = (0 - depths[outoIndex]) / (depths[outoIndex2] - depths[outoIndex]);

                        Vector2 intoVec = new Vector2(polygonShape.Vertices[intoIndex].X * (1 - intoLambda) + polygonShape.Vertices[intoIndex2].X * intoLambda, polygonShape.Vertices[intoIndex].Y * (1 - intoLambda) + polygonShape.Vertices[intoIndex2].Y * intoLambda);
                        Vector2 outoVec = new Vector2(polygonShape.Vertices[outoIndex].X * (1 - outoLambda) + polygonShape.Vertices[outoIndex2].X * outoLambda, polygonShape.Vertices[outoIndex].Y * (1 - outoLambda) + polygonShape.Vertices[outoIndex2].Y * outoLambda);

                        //Initialize accumulator
                        float area = 0;
                        Vector2 center = new Vector2(0, 0);
                        Vector2 p2 = polygonShape.Vertices[intoIndex2];

                        const float k_inv3 = 1.0f / 3.0f;

                        //An awkward loop from intoIndex2+1 to outIndex2
                        i = intoIndex2;
                        while (i != outoIndex2)
                        {
                            i = (i + 1) % polygonShape.Vertices.Count;
                            Vector2 p3;
                            if (i == outoIndex2)
                                p3 = outoVec;
                            else
                                p3 = polygonShape.Vertices[i];

                            //Add the triangle formed by intoVec,p2,p3
                            {
                                Vector2 e1 = p2 - intoVec;
                                Vector2 e2 = p3 - intoVec;

                                float D = MathUtils.Cross(e1, e2);

                                float triangleArea = 0.5f * D;

                                area += triangleArea;

                                // Area weighted centroid
                                center += triangleArea * k_inv3 * (intoVec + p2 + p3);
                            }

                            p2 = p3;
                        }

                        //Normalize and transform centroid
                        center *= 1.0f / area;

                        sc = MathUtils.Mul(ref xf, center);

                        return area;
                    }
                case ShapeType.Chain:
                    sc = Vector2.Zero;
                    return 0;
                case ShapeType.Unknown:
                case ShapeType.TypeCount:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}