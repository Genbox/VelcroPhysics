using BenchmarkDotNet.Attributes;
using VelcroPhysics.Benchmarks.Code;

namespace VelcroPhysics.Benchmarks.Tests.CLR
{
    [InProcess]
    public class StructBenchmarks
    {
        private Struct8[] _struct8;
        private Struct32[] _struct32;
        private Struct64[] _struct64;

        [GlobalSetup]
        public void Setup()
        {
            _struct8 = new Struct8[1000];
            _struct32 = new Struct32[1000];
            _struct64 = new Struct64[1000];
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
        public Struct64 Copy64()
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
        public Struct64 Copy64Ref()
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
                s = CopyBackRef(ref s);
            }

            return s;
        }

        private Struct64 CopyBackRef(ref Struct64 c)
        {
            return c;
        }

        [Benchmark]
        public Struct64 Copy64RefOut()
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
