using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Interfaces;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

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
    public class FluidDragController : Controller
    {
        #region Delegates

        public delegate void EntryEventHandler(Geom geom);

        #endregion

        private float _area;
        private Vector2 _axis = Vector2.Zero;
        private Vector2 _buoyancyForce = Vector2.Zero;
        private Vector2 _centroid = Vector2.Zero;
        private Vector2 _centroidVelocity;
        private float _density;

        private float _dragArea;
        private IFluidContainer _fluidContainer;
        private Dictionary<Geom, bool> _geomInFluidList;
        private List<Geom> _geomList;
        private Vector2 _gravity = Vector2.Zero;
        private float _linearDragCoefficient;
        private Vector2 _linearDragForce = Vector2.Zero;
        private float _max;
        private float _min;
        private float _partialMass;
        private float _rotationalDragCoeficient;
        private float _rotationalDragTorque;
        private float _totalArea;
        private Vector2 _totalForce;
        private Vector2 _vert;
        private Vertices _vertices;

        public EntryEventHandler Entry;

        public FluidDragController()
        {
            _geomList = new List<Geom>();
            _geomInFluidList = new Dictionary<Geom, bool>();
        }

        /// <summary>
        /// Density of the fluid.  Higher values will make things more buoyant, lower values will cause things to sink.
        /// </summary>
        public float Density
        {
            get { return _density; }
            set { _density = value; }
        }

        /// <summary>
        /// Controls the linear drag that the fluid exerts on the bodies within it.  Use higher values will simulate thick fluid, like honey, lower values to
        /// simulate water-like fluids.
        /// </summary>
        public float LinearDragCoefficient
        {
            get { return _linearDragCoefficient; }
            set { _linearDragCoefficient = value; }
        }

        /// <summary>
        /// Controls the rotational drag that the fluid exerts on the bodies within it. Use higher values will simulate thick fluid, like honey, lower values to
        /// simulate water-like fluids. 
        /// </summary>
        public float RotationalDragCoefficient
        {
            get { return _rotationalDragCoeficient; }
            set { _rotationalDragCoeficient = value; }
        }

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
            _density = density;
            _linearDragCoefficient = linearDragCoefficient;
            _rotationalDragCoeficient = rotationalDragCoefficient;
            _gravity = gravity;
            _vertices = new Vertices();
        }

        /// <summary>
        /// Add a geom to be controlled by the fluid drag controller.  The geom does not need to already be in
        /// the fluid to add it to the controller. By calling this method you are telling the fluid drag controller
        /// to watch this geom and it if enters my fluid container, apply the fluid physics.
        /// </summary>
        /// <param name="geom">The geom to be added.</param>
        public void AddGeom(Geom geom)
        {
            _geomList.Add(geom);
            _geomInFluidList.Add(geom, false);
        }

        public override void Validate()
        {
            //do nothing
        }

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
                _totalArea = _geomList[i].localVertices.GetArea();
                if (!_fluidContainer.Intersect(_geomList[i].aabb)) continue;
                FindVerticesInFluid(_geomList[i]);
                if (_vertices.Count < 3) continue;

                _area = _vertices.GetArea();
                if (_area < .000001) continue;

                _centroid = _vertices.GetCentroid(_area);

                CalculateBuoyancy();

                CalculateDrag(_geomList[i]);

                Vector2.Add(ref _buoyancyForce, ref _linearDragForce, out _totalForce);
                _geomList[i].body.ApplyForceAtWorldPoint(ref _totalForce, ref _centroid);

                _geomList[i].body.ApplyTorque(_rotationalDragTorque);

                if (_geomInFluidList[_geomList[i]] == false)
                {
                    _geomInFluidList[_geomList[i]] = true;
                    if (Entry != null)
                    {
                        Entry(_geomList[i]);
                    }
                }
            }
        }

        private void FindVerticesInFluid(Geom geom)
        {
            _vertices.Clear();
            for (int i = 0; i < geom.worldVertices.Count; i++)
            {
                _vert = geom.worldVertices[i];
                if (_fluidContainer.Contains(ref _vert))
                {
                    _vertices.Add(_vert);
                }
            }
        }

        private void CalculateBuoyancy()
        {
            _buoyancyForce = -_gravity*_area*_density;
        }


        private void CalculateDrag(Geom geom)
        {
            //localCentroid = geom.body.GetLocalPosition(_centroid);
            geom.body.GetVelocityAtWorldPoint(ref _centroid, out _centroidVelocity);

            _axis.X = -_centroidVelocity.Y;
            _axis.Y = _centroidVelocity.X;
            //can't normalize a zero length vector
            if (_axis.X != 0 || _axis.Y != 0)
            {
                _axis.Normalize();
            }

            _vertices.ProjectToAxis(ref _axis, out _min, out _max);

            _dragArea = Math.Abs(_max - _min);

            _partialMass = geom.body.mass*(_area/_totalArea);

            _linearDragForce = -.5f*_density*_dragArea*_linearDragCoefficient*_partialMass*_centroidVelocity;

            _rotationalDragTorque = -geom.body.AngularVelocity*_rotationalDragCoeficient*_partialMass;
        }
    }
}