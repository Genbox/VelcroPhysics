namespace FarseerPhysics.Controllers
{
    public abstract class Controller
    {
        public bool Enabled
        {
            get;
            set;
        }

        public abstract void Update(float dt);

        public World World
        {
            get;
            set;
        }
    }
}
