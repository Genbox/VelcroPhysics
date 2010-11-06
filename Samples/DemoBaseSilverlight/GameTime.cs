using System;

namespace FarseerPhysics.DemoBaseSilverlight
{
    public class GameTime
    {
        public DateTime GameStartTime { get; set; }
        public DateTime FrameStartTime { get; set; }
        public TimeSpan ElapsedGameTime { get; set; }
        public TimeSpan TotalGameTime { get; set; }
    }
}
