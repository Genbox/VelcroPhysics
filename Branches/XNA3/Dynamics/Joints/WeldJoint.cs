using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerGames.FarseerPhysics.Mathematics;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;

#if (XNA)
using Microsoft.Xna.Framework;
#else
using FarseerGames.FarseerPhysics.Mathematics;
#endif

namespace FarseerGames.FarseerPhysics
{
    /// <summary>
    /// 
    /// </summary>
    public class Weld
    {
        private List<Geom> _geomList;
        private List<Body> _bodyList;
        private PhysicsSimulator _ps;

        private Body _combinedBodies;
        private List<Vector2> _offsetList;

        public Weld()
        {
            _bodyList = new List<Body>();
            _geomList = new List<Geom>();
            _offsetList = new List<Vector2>();
        }

        /// <summary>
        /// Weld is used to temporarily hold many geoms together.
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="geoms"></param>
        public Weld(PhysicsSimulator ps, List<Geom> geoms)
        {
            _ps = ps;
            _bodyList = new List<Body>();
            _geomList = new List<Geom>();
            _offsetList = new List<Vector2>();

            float accumulatedMass = 0;
            float averageMOI = 0;
            Vector2 averagePosition = new Vector2();

            foreach (Geom g in geoms)
            {
                _bodyList.Add(g.body);
                _geomList.Add(g);
                
                accumulatedMass += g.body.mass;
                averageMOI += g.body.MomentOfInertia;
                averagePosition += g.body.position;
            }

            averageMOI *= geoms.Count;
            averagePosition.X /= geoms.Count;
            averagePosition.Y /= geoms.Count;

            _combinedBodies = BodyFactory.Instance.CreateBody(accumulatedMass, averageMOI);
            _combinedBodies.position = averagePosition;

            _ps.Add(_combinedBodies);

            for (int i = 0; i < _geomList.Count; i++)
            {
                _offsetList.Add(_bodyList[i].position - _combinedBodies.position);      // store the offset
                _ps.Remove(geoms[i]);                                                   // remove the old geoms
                _ps.Remove(geoms[i].Body);                                              // remove the old bodys
                _geomList[i] = new Geom(_combinedBodies, _geomList[i], _offsetList[i], 0);  // create new geoms for the new combined body
                _ps.Add(_geomList[i]);                                                  // add them
            }
        }

        /// <summary>
        /// Weld is used to temporarily hold many geoms together.
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="geoms"></param>
        public void Reweld()
        {
            float accumulatedMass = 0;
            float averageMOI = 0;
            Vector2 averagePosition = new Vector2();

            for (int i = 0; i < _geomList.Count; i++)
            {
                _bodyList.Add(_geomList[i].body);
                _geomList.Add(_geomList[i]);

                accumulatedMass += _geomList[i].body.mass;
                averageMOI += _geomList[i].body.MomentOfInertia;
                averagePosition += _geomList[i].body.position;
            }

            averageMOI /= _geomList.Count;
            averagePosition.X /= _geomList.Count;
            averagePosition.Y /= _geomList.Count;

            _combinedBodies = BodyFactory.Instance.CreateBody(accumulatedMass, averageMOI);
            _combinedBodies.position = averagePosition;

            _ps.Add(_combinedBodies);

            for (int i = 0; i < _geomList.Count; i++)
            {
                _offsetList.Add(_bodyList[i].position - _combinedBodies.position);      // store the offset
                _ps.Remove(_geomList[i]);                                                   // remove the old geoms
                _ps.Remove(_geomList[i].Body);                                              // remove the old bodys
                _geomList[i] = new Geom(_combinedBodies, _geomList[i], _offsetList[i], 0);  // create new geoms for the new combined body
                _ps.Add(_geomList[i]);                                                  // add them
            }
        }

        public void Fracture()
        {
            _ps.Remove(_combinedBodies);

            for (int i = 0; i < _bodyList.Count; i++)
            {
                _bodyList[i].position = Vector2.Transform((_offsetList[i]), Matrix.CreateRotationZ(_combinedBodies.rotation));
                _bodyList[i].position += _combinedBodies.position;
                _bodyList[i].rotation = _combinedBodies.rotation;
                _combinedBodies.GetVelocityAtWorldPoint(ref _bodyList[i].position, out _bodyList[i].LinearVelocity);
                _ps.Add(_bodyList[i]);
            }
            for (int i = 0; i < _geomList.Count; i++)
            {
                _ps.Remove(_geomList[i]);
                _ps.Add(new Geom(_bodyList[i], _geomList[i], new Vector2(), 0));
            }
        }
    }
}