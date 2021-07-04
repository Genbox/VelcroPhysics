using Genbox.VelcroPhysics.Collision.Filtering;
using Genbox.VelcroPhysics.Collision.Shapes;

namespace Genbox.VelcroPhysics.Definitions
{
    public class FixtureDef : IDef
    {
        public FixtureDef()
        {
            SetDefaults();
        }

        //Velcro: removed density from fixtures. It is only present on shapes

        /// <summary>Contact filtering data.</summary>
        public Filter Filter { get; set; }

        /// <summary>The friction coefficient, usually in the range [0,1].</summary>
        public float Friction { get; set; }

        /// <summary>A sensor shape collects contact information but never generates a collision response.</summary>
        public bool IsSensor { get; set; }

        /// <summary>The restitution (elasticity) usually in the range [0,1].</summary>
        public float Restitution { get; set; }

        /// <summary>
        /// Restitution velocity threshold, usually in m/s. Collisions above this speed have restitution applied (will bounce).
        /// </summary>
        public float RestitutionThreshold { get; set; }

        /// <summary>The shape, this must be set. The shape will be cloned, so you can create the shape on the stack.</summary>
        public Shape Shape { get; set; }

        /// <summary>Use this to store application specific fixture data.</summary>
        public object? UserData { get; set; }

        public void SetDefaults()
        {
            Shape = null;
            Friction = 0.2f;
            Restitution = 0.0f;
            RestitutionThreshold = 1.0f;
            IsSensor = false;
            Filter = new Filter();
        }
    }
}