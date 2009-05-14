using System.Collections.Generic;
using FarseerGames.FarseerPhysics.Factories;
using FarseerGames.FarseerPhysics.Dynamics;
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
    public class WeldJoint
    {
        private List<Geom> _geomList;
        private List<Body> _bodyList;
        private PhysicsSimulator _physicsSimulator;

        private Body _combinedBodies;
        private List<Vector2> _offsetList;

        public WeldJoint()
        {
            _bodyList = new List<Body>();
            _geomList = new List<Geom>();
            _offsetList = new List<Vector2>();
        }

        /// <summary>
        /// Weld is used to temporarily hold many geoms together.
        /// </summary>
        /// <param name="physicsSimulator"></param>
        /// <param name="geoms"></param>
        public WeldJoint(PhysicsSimulator physicsSimulator, List<Geom> geoms)
        {
            _physicsSimulator = physicsSimulator;
            _bodyList = new List<Body>();
            _geomList = new List<Geom>();
            _offsetList = new List<Vector2>();

            float accumulatedMass = 0;
            float averageMOI = 0;
            Vector2 averagePosition = Vector2.Zero;

            foreach (Geom geom in geoms)
            {
                _bodyList.Add(geom.body);
                _geomList.Add(geom);
                
                accumulatedMass += geom.body.mass;
                averageMOI += geom.body.MomentOfInertia;
                averagePosition += geom.body.position;
            }

            averageMOI *= geoms.Count;
            averagePosition.X /= geoms.Count;
            averagePosition.Y /= geoms.Count;

            _combinedBodies = BodyFactory.Instance.CreateBody(accumulatedMass, averageMOI);
            _combinedBodies.position = averagePosition;

            _physicsSimulator.Add(_combinedBodies);

            for (int i = 0; i < _geomList.Count; i++)
            {
                _offsetList.Add(_bodyList[i].position - _combinedBodies.position);         // store the offset
                _physicsSimulator.Remove(geoms[i]);                                        // remove the old geoms
                _physicsSimulator.Remove(geoms[i].Body);                                   // remove the old bodys
                _geomList[i] = new Geom(_combinedBodies, _geomList[i], _offsetList[i], 0); // create new geoms for the new combined body
                _physicsSimulator.Add(_geomList[i]);                                       // add them
            }
        }

        /// <summary>
        /// Recalculate the combined body.
        /// </summary>
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

            _physicsSimulator.Add(_combinedBodies);

            for (int i = 0; i < _geomList.Count; i++)
            {
                _offsetList.Add(_bodyList[i].position - _combinedBodies.position);          // store the offset
                _physicsSimulator.Remove(_geomList[i]);                                     // remove the old geoms
                _physicsSimulator.Remove(_geomList[i].Body);                                // remove the old bodys
                _geomList[i] = new Geom(_combinedBodies, _geomList[i], _offsetList[i], 0);  // create new geoms for the new combined body
                _physicsSimulator.Add(_geomList[i]);                                        // add them
            }
        }

        public void Fracture()
        {
            _physicsSimulator.Remove(_combinedBodies);

            for (int i = 0; i < _bodyList.Count; i++)
            {
                _bodyList[i].position = Vector2.Transform((_offsetList[i]), Matrix.CreateRotationZ(_combinedBodies.rotation));
                _bodyList[i].position += _combinedBodies.position;
                _bodyList[i].rotation = _combinedBodies.rotation;
                _combinedBodies.GetVelocityAtWorldPoint(ref _bodyList[i].position, out _bodyList[i].LinearVelocity);
                _physicsSimulator.Add(_bodyList[i]);
            }
            for (int i = 0; i < _geomList.Count; i++)
            {
                _physicsSimulator.Remove(_geomList[i]);
                _physicsSimulator.Add(new Geom(_bodyList[i], _geomList[i], new Vector2(), 0));
            }
        }
    }
}