using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Controllers;

namespace FarseerPhysics.Controllers
{
    public class SimpleWindForce:AbstractForceController
    {
        //public PerlinNoise Noise;
        public Noise Noise;

        Random Randomizer;

        public SimpleWindForce(): base()
        {
            Randomizer = new Random(12345);
        }

        public override void ApplyForce(float dt, float Strength)
        {
            foreach (Body body in this.World.BodyList)
            {
                Vector2 forceVector = body.Position - this.Position;
                float distance = forceVector.Length();
                forceVector.Normalize();

                body.ApplyForce(forceVector * Strength * (10 / distance) * (float)Randomizer.NextDouble());
            }
        }

    }
}
