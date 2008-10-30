using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FarseerPhysicsWaterDemo.Views
{
    public partial class SplashScreenView : UserControl
    {
        DispatcherTimer timer = new DispatcherTimer();
        public event EventHandler Complete;

        public SplashScreenView()
        {
            InitializeComponent();
            Loaded += SplashView_Loaded;
        }

        void SplashView_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Interval = new TimeSpan(0, 0, 0, 2);
            timer.Tick += timer_Tick;
        }

        public void Start()
        {
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            if (Complete != null) Complete(this, new EventArgs());
        }
    }
}
