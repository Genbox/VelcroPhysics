using System;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;
using SimpleSamplesWPF.Demos;
using SimpleSamplesWPF.Demos.Demo1;
using SimpleSamplesWPF.Demos.Demo2;
using SimpleSamplesWPF.Demos.Demo3;
using SimpleSamplesWPF.Demos.Demo4;
using SimpleSamplesWPF.Demos.Demo5;
using SimpleSamplesWPF.Demos.Demo6;
using SimpleSamplesWPF.Demos.Demo7;
using SimpleSamplesWPF.Demos.Demo8;

namespace SimpleSamplesWPF.Screens
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : UserControl
    {
        public delegate void MenuItemClickedDelegate(Type chosenDemoType);
        public event MenuItemClickedDelegate MenuItemClicked;

        public MainMenu()
        {
            InitializeComponent();

            AddMenuItem("Demo1: A Single Body", typeof(Demo1));
            AddMenuItem("Demo2: Two Bodies With Geom", typeof(Demo2));
            AddMenuItem("Demo3: Static Bodies And Offset Geometries", typeof(Demo3));
            AddMenuItem("Demo4: Stacked Bodies", typeof(Demo4));
            AddMenuItem("Demo5: Collision Categories", typeof(Demo5));
            AddMenuItem("Demo6: Linear and Angular Spring Controllers", typeof(Demo6));
            AddMenuItem("Demo7: Dynamic Angle Joints", typeof(Demo7));
            AddMenuItem("Demo8: Ragdoll", typeof(Demo8));
        }

        private void AddMenuItem(string text, Type demoType)
        {
            if (!typeof(Demo).IsAssignableFrom(demoType)) throw new ArgumentException("must provide a demo type");

            MenuItem item = new MenuItem(text);
            item.Click += delegate
            {
                if (MenuItemClicked != null)
                    MenuItemClicked(demoType);
            };

            menuStack.Children.Add(item);
        }

        private void getFarseer_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
