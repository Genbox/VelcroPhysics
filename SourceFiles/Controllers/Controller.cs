using FarseerPhysics.Dynamics;

namespace FarseerPhysics.Controllers
{
    public abstract class Controller
    {
        public bool Enabled { get; set; }

        public World World { get; set; }
        public abstract void Update(float dt);
    }
}