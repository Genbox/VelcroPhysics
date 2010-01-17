namespace FarseerPhysics.Controllers
{
    public abstract class Controller
    {
        public bool Enabled
        {
            get;
            set;
        }

        public abstract void Update();

        public World World
        {
            get;
            set;
        }
    }
}
