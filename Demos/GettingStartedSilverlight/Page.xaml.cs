using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FarseerSilverlightDemos.Demos.Demo1;
using FarseerSilverlightDemos.Demos.Demo2;
using FarseerSilverlightDemos.Demos.Demo3;
using FarseerSilverlightDemos.Demos.Demo4;
using FarseerSilverlightDemos.Demos.Demo5;
using FarseerSilverlightDemos.Demos.Demo6;
using FarseerSilverlightDemos.Demos.Demo7;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Drawing;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Mathematics;

namespace FarseerSilverlightDemos
{
    public partial class Page : UserControl
    {
        public static GameLoop gameLoop;
        public static KeyHandler KeyHandler;
        public SimulatorView currentDemo = null;
        MainMenu mainMenu;
        Splash splash;
        DateTime splashTime;
        bool firstTime = true;

        public Page()
        {
            // Required to initialize variables
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(Page_SizeChanged);
        }

        public void Page_Loaded(object o, EventArgs e)
        {
            // Required to initialize variables
            splash = new Splash();
            canvas.Children.Add(splash);
            splash.SetValue(Canvas.ZIndexProperty, 20000);
            KeyHandler = new KeyHandler();
            KeyHandler.Attach(this);
            this.Focus();
            gameLoop = new GameLoop();
            gameLoop.Attach(parentCanvas);
            gameLoop.Update += new GameLoop.UpdateDelegate(gameLoop_Update);
            Fps fps = new Fps();
            canvas.Children.Add(fps);
            fps.SetValue(Canvas.ZIndexProperty, 1000);
            mainMenu = new MainMenu();
            canvas.Children.Add(mainMenu);
            mainMenu.MenuItemSelected += new MenuItemSelectedEvent(mainMenu_MenuItemSelected);
            Page_SizeChanged(null, null);
        }

        void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            float scaleY = (float)(this.ActualHeight / 768);
            float scaleX = (float)(this.ActualWidth / 1024);
            float scl = Math.Min(scaleX, scaleY);
            if (scl <= 0) return;
            translate.X = ((this.ActualWidth) - 1024d * scl) / 2d;
            translate.Y = ((this.ActualHeight) - 768d * scl) / 2d;
            scale.ScaleX = scl;
            scale.ScaleY = scl;
        }


        void mainMenu_MenuItemSelected(int index)
        {
            switch (index)
            {
                case 0:
                    currentDemo = new Demo1();
                    break;
                case 1:
                    currentDemo = new Demo2();
                    break;
                case 2:
                    currentDemo = new Demo3();
                    break;
                case 3:
                    currentDemo = new Demo4();
                    break;
                case 4:
                    currentDemo = new Demo5();
                    break;
                case 5:
                    currentDemo = new Demo6();
                    break;
                case 6:
                    currentDemo = new Demo7();
                    break;
            }
            currentDemo.Quit += new SimulatorView.QuitEvent(currentDemo_Quit);
            mainMenu.Visible = false;
            canvas.Children.Add(currentDemo);
        }

        void currentDemo_Quit()
        {
            currentDemo.Dispose();
            canvas.Children.Remove(currentDemo);
            mainMenu.Visible = true;
        }

        void HandleKeyboard()
        {
        }

        void gameLoop_Update(TimeSpan ElapsedTime)
        {
            if (firstTime)
            {
                PhysicsSimulator sim = new PhysicsSimulator();
                firstTime = false;
                splashTime = DateTime.Now;
            }
            if (splash.Visibility != Visibility.Collapsed)
            {
                if ((DateTime.Now - splashTime).TotalSeconds > 2)
                {
                    splash.Visibility = Visibility.Collapsed;
                }
            }

        }

    }
}
