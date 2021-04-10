using System;

namespace VelcroPhysics.Samples.Testbed.Framework
{
    public struct TestEntry
    {
        public Func<Test> CreateTest;
        public string Name;
    }
}