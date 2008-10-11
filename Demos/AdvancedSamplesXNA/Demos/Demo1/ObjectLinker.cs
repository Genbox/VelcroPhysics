using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerGames.AdvancedSamples.Demos.Demo4
{
    // POINT OF INTEREST
    // This is a class to link the physics object to the simulator
    // After each physics update, the physics processor is going to
    // copy the position and rotation into this class. While drawing
    // the drawer should use this valuse instead of the physics
    // body's, as the body could be in use by the physics thread.
    public class ObjectLinker
    {
        private Body _body;
        private Geom _geom;

        // POINT OF INTEREST
        // Shadowed values
        private Vector2 _position;
        private float _rotation;

        public ObjectLinker(Body body)
        {
            _body = body;
            _geom = null;
            // POINT OF INTEREST
            // Set the initial values
            Syncronize();
        }

        public ObjectLinker(Geom geom)
        {
            _body = null;
            _geom = geom;
            // POINT OF INTEREST
            // Set the initial values
            Syncronize();
        }

        public Vector2 Position
        {
            get { return _position; }
        }

        public float Rotation
        {
            get { return _rotation; }
        }

        public void Syncronize()
        {
            // POINT OF INTEREST
            // Copy the values into the shadowed variables
            if (_body != null)
            {
                _position = _body.Position;
                _rotation = _body.Rotation;
            }
            else
            {
                _position = _geom.Position;
                _rotation = _geom.Rotation;
            }
        }
    }
}