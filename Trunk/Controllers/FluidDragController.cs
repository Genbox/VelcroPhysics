using System;
using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerGames.FarseerPhysics.Controllers
{
    public class FluidDragController : Controller
    {
        #region Delegates

        public delegate void OnEntryHandler(Geom geom);

        #endregion

        private readonly Dictionary<Geom, bool> _geomInFluidList;

        private readonly List<Geom> _geomList;
        private float _area;
        private Vector2 _axis = Vector2.Zero;
        private Vector2 _buoyancyForce = Vector2.Zero;
        private Vector2 _centroid = Vector2.Zero;

        private Vector2 _centroidVelocity;
        private float _density;
        private float _dragArea;
        private IFluidContainer _fluidContainer;
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
        public OnEntryHandler OnEntry;

        public FluidDragController()
        {
            _geomList = new List<Geom>();
            _geomInFluidList = new Dictionary<Geom, bool>();
        }

        public void Initialize(IFluidContainer fluidContainer, float density, float linearDragCoeficient,
                               float rotationalDragCoeficient, Vector2 gravity)
        {
            _fluidContainer = fluidContainer;
            _density = density;
            _linearDragCoefficient = linearDragCoeficient;
            _rotationalDragCoeficient = rotationalDragCoeficient;
            _gravity = gravity;
            _vertices = new Vertices();
        }

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
            foreach (Geom geom in _geomList)
            {
                _geomInFluidList.Add(geom, false);
            }
        }

        public override void Update(float dt)
        {
            for (int i = 0; i < _geomList.Count; i++)
            {
                _totalArea = _geomList[i].LocalVertices.GetArea(false);
                if (!_fluidContainer.Intersect(_geomList[i].AABB)) continue;
                FindVerticesInFluid(_geomList[i]);
                if (_vertices.Count < 3) continue;

                _area = _vertices.GetArea(false);
                if (_area < .000001) continue;

                _centroid = _vertices.GetCentroid(_area);

                CalculateBuoyancy();

                CalculateDrag(_geomList[i]);

                Vector2.Add(ref _buoyancyForce, ref _linearDragForce, out _totalForce);
                _geomList[i].Body.ApplyForceAtWorldPoint(ref _totalForce, ref _centroid);

                _geomList[i].Body.ApplyTorque(_rotationalDragTorque);

                if (_geomInFluidList[_geomList[i]] == false)
                {
                    _geomInFluidList[_geomList[i]] = true;
                    if (OnEntry != null)
                    {
                        OnEntry(_geomList[i]);
                    }
                }
            }
        }

        private void FindVerticesInFluid(Geom geom)
        {
            _vertices.Clear();
            for (int i = 0; i < geom.WorldVertices.Count; i++)
            {
                _vert = geom.WorldVertices[i];
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
            //localCentroid = geom.body.GetLocalPosition(centroid);
            geom.Body.GetVelocityAtWorldPoint(ref _centroid, out _centroidVelocity);

            _axis.X = -_centroidVelocity.Y;
            _axis.Y = _centroidVelocity.X;
            //can't normalize a zero length vector
            if (_axis.X != 0 || _axis.Y != 0)
            {
                _axis.Normalize();
            }

            _vertices.ProjectToAxis(ref _axis, out _min, out _max);

            _dragArea = Math.Abs(_max - _min);

            _partialMass = geom.Body._mass*(_area/_totalArea);

            _linearDragForce = -.5f*_density*_dragArea*_linearDragCoefficient*_partialMass*_centroidVelocity;

            _rotationalDragTorque = -geom.Body.angularVelocity*_rotationalDragCoeficient*_partialMass;
        }
    }
}