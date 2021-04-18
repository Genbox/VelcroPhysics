using BenchmarkDotNet.Attributes;
using Genbox.VelcroPhysics.Benchmarks.Code;
using Genbox.VelcroPhysics.Benchmarks.Code.TestClasses;

namespace Genbox.VelcroPhysics.Benchmarks.Tests.CLR
{
    public class ClassBenchmarks : UnmeasuredBenchmark
    {
        private const int _size = 1000;
        private Class32[] _class32;
        private Class64[] _class64;
        private Class8[] _class8;

        private Struct32[] _struct32;
        private Struct64[] _struct64;
        private Struct8[] _struct8;

        [GlobalSetup]
        public void Setup()
        {
            _class8 = new Class8[_size];
            _class32 = new Class32[_size];
            _class64 = new Class64[_size];
            _struct8 = new Struct8[_size];
            _struct32 = new Struct32[_size];
            _struct64 = new Struct64[_size];
        }

        [Benchmark]
        public void ClassSize8()
        {
            for (int i = 0; i < _class8.Length; i++)
            {
                Class8 s = new Class8();
                s.Value1 = i;
                s.Value2 = i;
                _class8[i] = s;
            }
        }

        [Benchmark]
        public void ClassSize32()
        {
            for (int i = 0; i < _class32.Length; i++)
            {
                Class32 s = new Class32();
                s.Value1 = new Class8();
                s.Value2 = new Class8();
                s.Value3 = new Class8();
                s.Value4 = new Class8();
                s.Value1.Value1 = i;
                s.Value1.Value2 = i;
                s.Value2.Value1 = i;
                s.Value2.Value2 = i;
                s.Value3.Value1 = i;
                s.Value3.Value2 = i;
                s.Value4.Value1 = i;
                s.Value4.Value2 = i;
                _class32[i] = s;
            }
        }

        [Benchmark]
        public void ClassSize64()
        {
            for (int i = 0; i < _class64.Length; i++)
            {
                Class64 s = new Class64();
                s.Value1 = new Class32();
                s.Value2 = new Class32();
                s.Value1.Value1 = new Class8();
                s.Value1.Value2 = new Class8();
                s.Value1.Value3 = new Class8();
                s.Value1.Value4 = new Class8();

                s.Value2.Value1 = new Class8();
                s.Value2.Value2 = new Class8();
                s.Value2.Value3 = new Class8();
                s.Value2.Value4 = new Class8();

                s.Value1.Value1.Value1 = i;
                s.Value1.Value1.Value2 = i;
                s.Value1.Value2.Value1 = i;
                s.Value1.Value2.Value2 = i;
                s.Value1.Value3.Value1 = i;
                s.Value1.Value3.Value2 = i;
                s.Value1.Value4.Value1 = i;
                s.Value1.Value4.Value2 = i;

                s.Value2.Value1.Value1 = i;
                s.Value2.Value1.Value2 = i;
                s.Value2.Value2.Value1 = i;
                s.Value2.Value2.Value2 = i;
                s.Value2.Value3.Value1 = i;
                s.Value2.Value3.Value2 = i;
                s.Value2.Value4.Value1 = i;
                s.Value2.Value4.Value2 = i;

                _class64[i] = s;
            }
        }

        [Benchmark]
        public Class64 CopyClass64()
        {
            Class64 s = new Class64();
            s.Value1 = new Class32();
            s.Value2 = new Class32();
            s.Value1.Value1 = new Class8();
            s.Value1.Value2 = new Class8();
            s.Value1.Value3 = new Class8();
            s.Value1.Value4 = new Class8();

            s.Value2.Value1 = new Class8();
            s.Value2.Value2 = new Class8();
            s.Value2.Value3 = new Class8();
            s.Value2.Value4 = new Class8();

            s.Value1.Value1.Value1 = 1;
            s.Value1.Value1.Value2 = 2;
            s.Value1.Value2.Value1 = 3;
            s.Value1.Value2.Value2 = 4;
            s.Value1.Value3.Value1 = 5;
            s.Value1.Value3.Value2 = 6;
            s.Value1.Value4.Value1 = 7;
            s.Value1.Value4.Value2 = 8;

            s.Value2.Value1.Value1 = 9;
            s.Value2.Value1.Value2 = 10;
            s.Value2.Value2.Value1 = 11;
            s.Value2.Value2.Value2 = 12;
            s.Value2.Value3.Value1 = 13;
            s.Value2.Value3.Value2 = 14;
            s.Value2.Value4.Value1 = 15;
            s.Value2.Value4.Value2 = 16;

            for (int i = 0; i < 100.000; i++)
            {
                s = CopyBack(s);
            }

            return s;
        }

        private Class64 CopyBack(Class64 c)
        {
            return c;
        }

        [Benchmark]
        public void StructSize8()
        {
            for (int i = 0; i < _struct8.Length; i++)
            {
                Struct8 s = _struct8[i];
                s.Value1 = i;
                s.Value2 = i;
                _struct8[i] = s;
            }
        }

        [Benchmark]
        public void StructSize32()
        {
            for (int i = 0; i < _struct32.Length; i++)
            {
                Struct32 s = _struct32[i];
                s.Value1 = new Struct8();
                s.Value2 = new Struct8();
                s.Value3 = new Struct8();
                s.Value4 = new Struct8();
                s.Value1.Value1 = i;
                s.Value1.Value2 = i;
                s.Value2.Value1 = i;
                s.Value2.Value2 = i;
                s.Value3.Value1 = i;
                s.Value3.Value2 = i;
                s.Value4.Value1 = i;
                s.Value4.Value2 = i;
                _struct32[i] = s;
            }
        }

        [Benchmark]
        public void StructSize64()
        {
            for (int i = 0; i < _struct64.Length; i++)
            {
                Struct64 s = _struct64[i];
                s.Value1 = new Struct32();
                s.Value2 = new Struct32();
                s.Value1.Value1 = new Struct8();
                s.Value1.Value2 = new Struct8();
                s.Value1.Value3 = new Struct8();
                s.Value1.Value4 = new Struct8();

                s.Value2.Value1 = new Struct8();
                s.Value2.Value2 = new Struct8();
                s.Value2.Value3 = new Struct8();
                s.Value2.Value4 = new Struct8();

                s.Value1.Value1.Value1 = i;
                s.Value1.Value1.Value2 = i;
                s.Value1.Value2.Value1 = i;
                s.Value1.Value2.Value2 = i;
                s.Value1.Value3.Value1 = i;
                s.Value1.Value3.Value2 = i;
                s.Value1.Value4.Value1 = i;
                s.Value1.Value4.Value2 = i;

                s.Value2.Value1.Value1 = i;
                s.Value2.Value1.Value2 = i;
                s.Value2.Value2.Value1 = i;
                s.Value2.Value2.Value2 = i;
                s.Value2.Value3.Value1 = i;
                s.Value2.Value3.Value2 = i;
                s.Value2.Value4.Value1 = i;
                s.Value2.Value4.Value2 = i;

                _struct64[i] = s;
            }
        }

        [Benchmark]
        public Struct64 CopyStruct64()
        {
            Struct64 s = new Struct64();
            s.Value1 = new Struct32();
            s.Value2 = new Struct32();
            s.Value1.Value1 = new Struct8();
            s.Value1.Value2 = new Struct8();
            s.Value1.Value3 = new Struct8();
            s.Value1.Value4 = new Struct8();

            s.Value2.Value1 = new Struct8();
            s.Value2.Value2 = new Struct8();
            s.Value2.Value3 = new Struct8();
            s.Value2.Value4 = new Struct8();

            s.Value1.Value1.Value1 = 1;
            s.Value1.Value1.Value2 = 2;
            s.Value1.Value2.Value1 = 3;
            s.Value1.Value2.Value2 = 4;
            s.Value1.Value3.Value1 = 5;
            s.Value1.Value3.Value2 = 6;
            s.Value1.Value4.Value1 = 7;
            s.Value1.Value4.Value2 = 8;

            s.Value2.Value1.Value1 = 9;
            s.Value2.Value1.Value2 = 10;
            s.Value2.Value2.Value1 = 11;
            s.Value2.Value2.Value2 = 12;
            s.Value2.Value3.Value1 = 13;
            s.Value2.Value3.Value2 = 14;
            s.Value2.Value4.Value1 = 15;
            s.Value2.Value4.Value2 = 16;

            for (int i = 0; i < 100.000; i++)
            {
                s = CopyBack(s);
            }

            return s;
        }

        private Struct64 CopyBack(Struct64 c)
        {
            return c;
        }

        [Benchmark]
        public Struct64 CopyStruct64Ref()
        {
            Struct64 s = new Struct64();
            s.Value1 = new Struct32();
            s.Value2 = new Struct32();
            s.Value1.Value1 = new Struct8();
            s.Value1.Value2 = new Struct8();
            s.Value1.Value3 = new Struct8();
            s.Value1.Value4 = new Struct8();

            s.Value2.Value1 = new Struct8();
            s.Value2.Value2 = new Struct8();
            s.Value2.Value3 = new Struct8();
            s.Value2.Value4 = new Struct8();

            s.Value1.Value1.Value1 = 1;
            s.Value1.Value1.Value2 = 2;
            s.Value1.Value2.Value1 = 3;
            s.Value1.Value2.Value2 = 4;
            s.Value1.Value3.Value1 = 5;
            s.Value1.Value3.Value2 = 6;
            s.Value1.Value4.Value1 = 7;
            s.Value1.Value4.Value2 = 8;

            s.Value2.Value1.Value1 = 9;
            s.Value2.Value1.Value2 = 10;
            s.Value2.Value2.Value1 = 11;
            s.Value2.Value2.Value2 = 12;
            s.Value2.Value3.Value1 = 13;
            s.Value2.Value3.Value2 = 14;
            s.Value2.Value4.Value1 = 15;
            s.Value2.Value4.Value2 = 16;

            for (int i = 0; i < 100.000; i++)
            {
                s = CopyStructBackRef(ref s);
            }

            return s;
        }

        private Struct64 CopyStructBackRef(ref Struct64 c)
        {
            return c;
        }

        [Benchmark]
        public Struct64 CopyStruct64RefOut()
        {
            Struct64 s = new Struct64();
            s.Value1 = new Struct32();
            s.Value2 = new Struct32();
            s.Value1.Value1 = new Struct8();
            s.Value1.Value2 = new Struct8();
            s.Value1.Value3 = new Struct8();
            s.Value1.Value4 = new Struct8();

            s.Value2.Value1 = new Struct8();
            s.Value2.Value2 = new Struct8();
            s.Value2.Value3 = new Struct8();
            s.Value2.Value4 = new Struct8();

            s.Value1.Value1.Value1 = 1;
            s.Value1.Value1.Value2 = 2;
            s.Value1.Value2.Value1 = 3;
            s.Value1.Value2.Value2 = 4;
            s.Value1.Value3.Value1 = 5;
            s.Value1.Value3.Value2 = 6;
            s.Value1.Value4.Value1 = 7;
            s.Value1.Value4.Value2 = 8;

            s.Value2.Value1.Value1 = 9;
            s.Value2.Value1.Value2 = 10;
            s.Value2.Value2.Value1 = 11;
            s.Value2.Value2.Value2 = 12;
            s.Value2.Value3.Value1 = 13;
            s.Value2.Value3.Value2 = 14;
            s.Value2.Value4.Value1 = 15;
            s.Value2.Value4.Value2 = 16;

            for (int i = 0; i < 100.000; i++)
            {
                CopyBackRefOut(ref s, out s);
            }

            return s;
        }

        private void CopyBackRefOut(ref Struct64 c, out Struct64 o)
        {
            o = c;
        }
    }
}