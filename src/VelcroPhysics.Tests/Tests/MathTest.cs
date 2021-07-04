using System;
using Genbox.VelcroPhysics.Collision.TOI;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;
using Xunit;

namespace Genbox.VelcroPhysics.Tests.Tests
{
    public class MathTest
    {
        [Fact]
        public void SweepGetTransform()
        {
            // From issue https://github.com/erincatto/box2d/issues/447
            Sweep sweep = new Sweep();
            sweep.LocalCenter = Vector2.Zero;
            sweep.C0 = new Vector2(-2.0f, 4.0f);
            sweep.C = new Vector2(3.0f, 8.0f);
            sweep.A0 = 0.5f;
            sweep.A = 5.0f;
            sweep.Alpha0 = 0.0f;

            sweep.GetTransform(out Transform transform, 0.0f);
            Assert.Equal(transform.p.X, sweep.C0.X);
            Assert.Equal(transform.p.Y, sweep.C0.Y);
            Assert.Equal(transform.q.c, (float)Math.Cos(sweep.A0));
            Assert.Equal(transform.q.s, (float)Math.Sin(sweep.A0));

            sweep.GetTransform(out transform, 1.0f);
            Assert.Equal(transform.p.X, sweep.C.X);
            Assert.Equal(transform.p.Y, sweep.C.Y);
            Assert.Equal(transform.q.c, (float)Math.Cos(sweep.A));
            Assert.Equal(transform.q.s, (float)Math.Sin(sweep.A));
        }
    }
}