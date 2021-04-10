using BenchmarkDotNet.Attributes;

namespace Benchmarks.Core.CLR
{
    public class Class8
    {
        public int Value1;
        public int Value2;
    }

    public class Class32
    {
        public Class8 Value1;
        public Class8 Value2;
        public Class8 Value3;
        public Class8 Value4;
    }

    public class Class64
    {
        public Class32 Value1;
        public Class32 Value2;
    }

    public class ClassBenchmarks
    {
        private Class8[] _class8;
        private Class32[] _class32;
        private Class64[] _class64;

        [Setup]
        public void Setup()
        {
            _class8 = new Class8[1000];
            _class32 = new Class32[1000];
            _class64 = new Class64[1000];
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
        public Class64 Copy64()
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
    }
}
