using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerGames.AdvancedSamples.Demos.Demo1
{
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