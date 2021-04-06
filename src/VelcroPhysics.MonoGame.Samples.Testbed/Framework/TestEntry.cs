using System;

namespace VelcroPhysics.MonoGame.Samples.Testbed.Framework
{
    public struct TestEntry
    {
        public Func<Test> CreateTest;
        public string Name;
    }
}