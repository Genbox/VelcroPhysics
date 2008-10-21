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

using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerPhysicsWaterDemo.Models
{
    public class BoxModelDef
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public float Mass { get; set; }
        public Vector2 Position { get; set; }
    }
}
