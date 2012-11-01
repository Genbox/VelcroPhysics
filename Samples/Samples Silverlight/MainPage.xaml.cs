using System.Windows.Controls;

namespace FarseerPhysics
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