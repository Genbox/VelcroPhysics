using System;
using System.Windows;
using FarseerSilverlightManual.Demos.Demo1;
using FarseerSilverlightManual.Demos.Demo2;
using FarseerSilverlightManual.Demos.Demo3;
using FarseerSilverlightManual.Demos.Demo4;
using FarseerSilverlightManual.Demos.Demo5;
using FarseerSilverlightManual.Demos.Demo6;
using FarseerSilverlightManual.Demos.Demo7;

namespace FarseerSilverlightManual.Screens
{
    public partial class Page
    {
        public static GameLoop gameLoop;
        public static KeyHandler KeyHandler;
        public int _demo;
        public SimulatorView currentDemo;

        public Page(string demo)
        {
            // Required to initialize variables
            InitializeComponent();
            Loaded += Page_Loaded;
            SizeChanged += Page_SizeChanged;

            text.Text = "Demo: " + demo;
            _demo = int.Parse(demo);
        }

        public void Page_Loaded(object o, EventArgs e)
        {
            // Required to initialize variables
            KeyHandler = new KeyHandler();
            KeyHandler.Attach(this);
            Focus();
            gameLoop = new GameLoop();
            gameLoop.Attach(parentCanvas);
            Page_SizeChanged(null, null);

            LoadDemo();
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
                    currentDemo = new Demo1();
                    break;
                case 2:
                    currentDemo = new Demo2();
                    break;
                case 3:
                    currentDemo = new Demo3();
                    break;
                case 4:
                    currentDemo = new Demo4();
                    break;
                case 5:
                    currentDemo = new Demo5();
                    break;
                case 6:
                    currentDemo = new Demo6();
                    break;
                case 7:
                    currentDemo = new Demo7();
                    break;
            }

            canvas.Children.Add(currentDemo);
        }
    }
}