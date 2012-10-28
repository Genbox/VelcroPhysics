using System;

namespace FarseerPhysics.TestBed.Framework
{
    public struct TestEntry
    {
        public Func<Test> CreateFcn;
        public string Name;
    }
}