namespace FarseerPhysics.Testbed.Framework
{
    public class GameSettings
    {
        public float Hz;
        public bool Pause;
        public bool SingleStep;

        public GameSettings()
        {
#if WINDOWS_PHONE
			Hz = 30.0f;
#else
            Hz = 60.0f;
#endif
        }
    }
}