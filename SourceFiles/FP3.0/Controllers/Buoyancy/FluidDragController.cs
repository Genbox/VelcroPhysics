using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Interfaces;
using FarseerPhysics;
using FarseerPhysics.Controllers;
using Microsoft.Xna.Framework;

namespace FarseerGames.FarseerPhysics.Controllers
{
    /// <summary>
    /// FluidDragController applies fluid physics to the bodies within it.  Things like fluid drag and fluid density
    /// can be adjusted to give semi-realistic motion for bodies in fluid.
    /// 
    /// The FluidDragController does nothing to define or control the MOTION of the fluid itself. It simply knows
    /// how to apply fluid forces to the bodies it contains.
    /// 
    /// In order for the FluidDragController to know when to apply forces and when not to apply forces, it needs to know
    /// when a body enters it.  This is done by supplying the FluidDragController with an <see cref="IFluidContainer"/> object.
    /// 
    /// <see cref="IFluidContainer"/> has two simple methods that need to be implemented. Intersect(AABB aabb), returns true if a given
    /// AABB object intersects it, false otherwise.  Contains(ref Vector2 vector) returns true if a given point is inside the 
    /// fluid container, false otherwise.
    /// 
    /// For a very simple example of a very simple fluid container. See the <see cref="AABBFluidContainer"/>.  This represents a fluid container
    /// in the shape of an AABB.
    /// 
    /// More complex fluid containers are where things get interesting.  The <see cref="WaveController"/> object is an example of a complex
    /// fluid container.  The <see cref="WaveController"/> simulates wave motion. It's driven by an algorithm (not physics) which dynamically 
    /// alters a polygonal shape to mimic waves.  Where it gets interesting is the <see cref="WaveController"/> also implements <see cref="IFluidContainer"/>. This allows 
    /// it to be used in conjunction with the FluidDragController.  Anything that falls into the dynamically changing fluid container
    /// defined by the <see cref="WaveController"/> will have fluid physics applied to it.
    /// 
    /// </summary>
    public sealed class FluidDragController : Controller
    {
        public delegate void EntryEventHandler(Fixture fixture, Vertices verts);

        private IFluidContainer _fluidContainer;
        private Dictionary<Fixture, bool> _geomInFluidList;
        private List<Fixture> _geomList;
        private Vector2 _gravity = Vector2.Zero;

        public EntryEventHandler Entry;

        public FluidDragController()
        {
            _geomList = new List<Fixture>();
            _geomInFluidList = new Dictionary<Fixture, bool>();
        }

        /// <summary>
        /// Density of the fluid.  Higher values will make things more buoyant, lower values will cause things to sink.
        /// </summary>
        public float Density { get; set; }

        /// <summary>
        /// Controls the linear drag that the fluid exerts on the bodies within it.  Use higher values will simulate thick fluid, like honey, lower values to
        /// simulate water-like fluids.
        /// </summary>
        public float LinearDragCoefficient { get; set; }

        /// <summary>
        /// Controls the rotational drag that the fluid exerts on the bodies within it. Use higher values will simulate thick fluid, like honey, lower values to
        /// simulate water-like fluids. 
        /// </summary>
        public float AngularDragCoefficient { get; set; }

        public Vector2 Normal = new Vector2(-1, 0);

        public Vector2 Velocity = Vector2.Zero;

        /// <summary>
        /// Initializes the fluid drag controller
        /// </summary>
        /// <param name="fluidContainer">An object that implements <see cref="IFluidContainer"/></param>
        /// <param name="density">Density of the fluid</param>
        /// <param name="linearDragCoefficient">Linear drag coefficient of the fluid</param>
        /// <param name="rotationalDragCoefficient">Rotational drag coefficient of the fluid</param>
        /// <param name="gravity">The direction gravity acts. Buoyancy force will act in opposite direction of gravity.</param>
        public void Initialize(IFluidContainer fluidContainer, float density, float linearDragCoefficient,
                               float rotationalDragCoefficient, Vector2 gravity)
        {
            _fluidContainer = fluidContainer;
            Density = density;
            LinearDragCoefficient = linearDragCoefficient;
            AngularDragCoefficient = rotationalDragCoefficient;
            _gravity = gravity;
        }

        /// <summary>
        /// Add a geom to be controlled by the fluid drag controller.  The geom does not need to already be in
        /// the fluid to add it to the controller. By calling this method you are telling the fluid drag controller
        /// to watch this geom and it if enters my fluid container, apply the fluid physics.
        /// </summary>
        /// <param name="geom">The geom to be added.</param>
        public void AddGeom(Fixture geom)
        {
            _geomList.Add(geom);
            _geomInFluidList.Add(geom, false);
        }

        /// <summary>
        /// Removes a geometry from the fluid drag controller.
        /// </summary>
        /// <param name="geom">The geom.</param>
        public void RemoveGeom(Fixture geom)
        {
            _geomList.Remove(geom);
            _geomInFluidList.Remove(geom);
        }

        /// <summary>
        /// Resets the fluid drag controller
        /// </summary>
        public void Reset()
        {
            _geomInFluidList.Clear();
            for (int i = 0; i < _geomList.Count; i++)
            {
                _geomInFluidList.Add(_geomList[i], false);
            }
        }

        public override void Update(float dt)
        {
            for (int i = 0; i < _geomList.Count; i++)
            {
                Body body = _geomList[i].Body;

                if (body.Awake == false || body.IsStatic || !body.Enabled)
                {
                    //Buoyancy force is just a function of position,
                    //so unlike most forces, it is safe to ignore sleeping bodes
                    continue;
                }

                Vector2 areac = Vector2.Zero;
                Vector2 massc = Vector2.Zero;
                float area = 0;
                float mass = 0;

                for (Fixture fixture = body.FixtureList; fixture != null; fixture = fixture.NextFixture)
                {
                    Vector2 sc;
                    Transform transform;
                    body.GetTransform(out transform);

                    float sarea = fixture.Shape.ComputeSubmergedArea(ref Normal, -2, ref transform, out sc);
                    area += sarea;
                    areac.X += sarea * sc.X;
                    areac.Y += sarea * sc.Y;

                    mass += sarea * fixture.Shape.Density;
                    massc.X += sarea * sc.X * fixture.Shape.Density;
                    massc.Y += sarea * sc.Y * fixture.Shape.Density;
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
                //TODO: Something that makes more physical sense?
                body.ApplyTorque(-body.Inertia / body.Mass * area * body.AngularVelocity * AngularDragCoefficient);

                //if (_geomInFluidList[_geomList[i]] == false)
                //{
                //    //The geometry is now in the water. Fire the Entry event
                //    _geomInFluidList[_geomList[i]] = true;
                //    if (Entry != null)
                //    {
                //        Entry(_geomList[i], _vertices);
                //    }
                //}
            }
        }

        /// <summary>
        /// Finds what vertices of the geometry that is inside the fluidcontainer
        /// </summary>
        /// <param name="geom">The geometry to check against</param>
        //private void FindVerticesInFluid(Fixture geom)
        //{
        //    _vertices.Clear();
        //    AABB aabb;
        //    geom.GetAABB(out aabb);

        //    for (int i = 0; i < geom.worldVertices.Count; i++)
        //    {
        //        _vert = geom.worldVertices[i];
        //        if (_fluidContainer.Contains(ref _vert))
        //        {
        //            _vertices.Add(_vert);
        //        }
        //    }
        //}

        /// <summary>
        /// Calculates the linear and rotational drag of the geometry
        /// </summary>
        //private void CalculateDrag(Fixture geom)
        //{
        //    //localCentroid = geom.body.GetLocalPosition(_centroid);
        //    _centroidVelocity= geom.Body.GetLinearVelocityFromWorldPoint(_centroid);

        //    _axis.X = -_centroidVelocity.Y;
        //    _axis.Y = _centroidVelocity.X;

        //    //can't normalize a zero length vector
        //    if (_axis.X != 0 || _axis.Y != 0)
        //        _axis.Normalize();

        //    _vertices.ProjectToAxis(ref _axis, out _min, out _max);

        //    _dragArea = Math.Abs(_max - _min);

        //    _partialMass = geom.Body.Mass * (_area / _totalArea);

        //    _linearDragForce = -.5f * Density * _dragArea * LinearDragCoefficient * _partialMass * _centroidVelocity;

        //    _rotationalDragTorque = -geom.Body.AngularVelocity * AngularDragCoefficient * _partialMass;
        //}
    }
}