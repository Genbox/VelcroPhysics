namespace Genbox.VelcroPhysics.Benchmarks.Code.TestClasses
{
    public class Dummy
    {
        public Struct32 ValueProperty { get; set; }
        public Struct32 ValueField;

        public Struct32 ValueMethod()
        {
            return ValueField;
        }
    }
}