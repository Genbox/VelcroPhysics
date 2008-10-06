using System;
using System.Windows;
using FarseerSilverlightManual.Demos;

namespace FarseerSilverlightManual.Screens
{
    public partial class Page
    {
        public static GameLoop gameLoop;
        public int _demo;
        public SimulatorView currentDemo;

        public Page(string demo)
        {
            // Required to initialize variables
            InitializeComponent();

            Loaded += Page_Loaded;
            SizeChanged += Page_SizeChanged;

            _demo = int.Parse(demo);
        }

        public void Page_Loaded(object o, EventArgs e)
        {
            // Required to initialize variables
            Focus();
            gameLoop = new GameLoop();
            gameLoop.Attach(parentCanvas);
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

        private void LoadDemo()
        {
            switch (_demo)
            {
                case 1:
                    currentDemo = new RevoluteJointDemo();
                    break;
                case 2:
                    currentDemo = new AngleJointDemo();
                    break;
                case 3:
                    currentDemo = new AngleLimitJointDemo();
                    break;
                case 4:
                    currentDemo = new PinSliderJointDemo();
                    break;
                case 5:
                    currentDemo = new LinearSpringDemo();
                    break;
                case 6:
                    currentDemo = new AngleSpringDemo();
                    break;
            }

            canvas.Children.Add(currentDemo);
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.Visibility = Visibility.Collapsed;
            LoadDemo();
        }
    }
}