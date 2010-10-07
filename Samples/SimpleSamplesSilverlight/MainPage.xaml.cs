using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FarseerPhysics.SimpleSamplesSilverlight
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();

            FarseerPhysicsGame game = new FarseerPhysicsGame(this, Game, DebugCanvas, txtFPS, txtDebug);
            game.Run();
        }
    }
}
