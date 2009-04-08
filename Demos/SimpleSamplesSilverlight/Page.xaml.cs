using System;
using System.Windows;
using System.Windows.Controls;
using FarseerGames.SimpleSamplesSilverlight.Components;
using FarseerGames.SimpleSamplesSilverlight.Demos.Demo1;
using FarseerGames.SimpleSamplesSilverlight.Demos.Demo2;
using FarseerGames.SimpleSamplesSilverlight.Demos.Demo3;
using FarseerGames.SimpleSamplesSilverlight.Demos.Demo4;
using FarseerGames.SimpleSamplesSilverlight.Demos.Demo5;
using FarseerGames.SimpleSamplesSilverlight.Demos.Demo6;
using FarseerGames.SimpleSamplesSilverlight.Demos.Demo7;
using FarseerGames.SimpleSamplesSilverlight.Screens;

namespace FarseerGames.SimpleSamplesSilverlight
{
    public partial class Page
    {
        public static GameLoop gameLoop;
        public static KeyHandler KeyHandler;
        private bool _firstTime = true;
        private MainMenu _mainMenu;
        private Splash _splash;
        private DateTime _splashTime;
        public SimulatorView currentDemo;

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
            _splash = new Splash();
            canvas.Children.Add(_splash);
            _splash.SetValue(Canvas.ZIndexProperty, 20000);
            KeyHandler = new KeyHandler();
            KeyHandler.Attach(this);
            Focus();
            gameLoop = new GameLoop();
            gameLoop.Attach(parentCanvas);
            gameLoop.Update += gameLoop_Update;
            Fps fps = new Fps();
            canvas.Children.Add(fps);
            fps.SetValue(Canvas.ZIndexProperty, 1000);
            _mainMenu = new MainMenu();
            canvas.Children.Add(_mainMenu);
            _mainMenu.MenuItemSelected += mainMenu_MenuItemSelected;
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
            _mainMenu.Visible = false;
            canvas.Children.Add(currentDemo);
        }

        private void currentDemo_Quit()
        {
            currentDemo.Dispose();
            canvas.Children.Remove(currentDemo);
            _mainMenu.Visible = true;
        }

        private void gameLoop_Update(TimeSpan elapsedTime)
        {
            if (_firstTime)
            {
                _firstTime = false;
                _splashTime = DateTime.Now;
            }
            if (_splash.Visibility != Visibility.Collapsed)
            {
                if ((DateTime.Now - _splashTime).TotalSeconds > 2)
                {
                    _splash.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}