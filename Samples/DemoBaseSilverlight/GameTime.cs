using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

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
