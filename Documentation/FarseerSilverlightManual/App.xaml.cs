using System.Windows;
using FarseerSilverlightManual.Screens;

namespace FarseerSilverlightManual
{
    public partial class App
    {
        public App()
        {
            Startup += Application_Startup;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Load the main control
            string inputParm = "1";

            if (e.InitParams.ContainsKey("demo"))
            {
                inputParm = e.InitParams["demo"];
            }

            RootVisual = new Page(inputParm);
        }
    }
}