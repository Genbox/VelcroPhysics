using System;
using System.Windows;
using System.Windows.Controls;
using FarseerGames.FarseerPhysics;
using FarseerSilverlightDemos.Demos.Demo1;
using FarseerSilverlightDemos.Demos.Demo2;
using FarseerSilverlightDemos.Demos.Demo3;
using FarseerSilverlightDemos.Demos.Demo4;
using FarseerSilverlightDemos.Demos.Demo5;
using FarseerSilverlightDemos.Demos.Demo6;
using FarseerSilverlightDemos.Demos.Demo7;

namespace FarseerSilverlightDemos
{
    public partial class Page : UserControl
    {
        public static GameLoop gameLoop;
        public static KeyHandler KeyHandler;
        public SimulatorView currentDemo;
        private bool firstTime = true;
        private MainMenu mainMenu;
        private Splash splash;
        private DateTime splashTime;

        public Page()
        {
            // Required to initialize variables
            InitializeComponent();
            Loaded += Page_Loaded;
            SizeChanged += Page_SizeChanged;
        }

        public void Page_Loaded(object o, EventArgs e)
        {
            // Required to initialize variables
            splash = new Splash();
            canvas.Children.Add(splash);
            splash.SetValue(Canvas.ZIndexProperty, 20000);
            KeyHandler = new KeyHandler();
            KeyHandler.Attach(this);
            Focus();
            gameLoop = new GameLoop();
            gameLoop.Attach(parentCanvas);
            gameLoop.Update += gameLoop_Update;
            Fps fps = new Fps();
            canvas.Children.Add(fps);
            fps.SetValue(Canvas.ZIndexProperty, 1000);
            mainMenu = new MainMenu();
            canvas.Children.Add(mainMenu);
            mainMenu.MenuItemSelected += mainMenu_MenuItemSelected;
            Page_SizeChanged(null, null);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            float scaleY = (float) (ActualHeight/768);
            float scaleX = (float) (ActualWidth/1024);
            float scl = Math.Min(scaleX, scaleY);
            if (scl <= 0) return;
            translate.X = ((ActualWidth) - 1024d*scl)/2d;
            translate.Y = ((ActualHeight) - 768d*scl)/2d;
            scale.ScaleX = scl;
            scale.ScaleY = scl;
        }


        private void mainMenu_MenuItemSelected(int index)
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
            currentDemo.Quit += currentDemo_Quit;
            mainMenu.Visible = false;
            canvas.Children.Add(currentDemo);
        }

        private void currentDemo_Quit()
        {
            currentDemo.Dispose();
            canvas.Children.Remove(currentDemo);
            mainMenu.Visible = true;
        }

        private void HandleKeyboard()
        {
        }

        private void gameLoop_Update(TimeSpan ElapsedTime)
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