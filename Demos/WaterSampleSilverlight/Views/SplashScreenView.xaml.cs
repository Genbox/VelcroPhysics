using System;
using System.Windows;
using System.Windows.Threading;

namespace FarseerGames.WaterSampleSilverlight.Views
{
    public partial class SplashScreenView
    {
        private DispatcherTimer _timer = new DispatcherTimer();

        public SplashScreenView()
        {
            InitializeComponent();
            Loaded += SplashView_Loaded;
        }

        public event EventHandler Complete;

        private void SplashView_Loaded(object sender, RoutedEventArgs e)
        {
            _timer.Interval = new TimeSpan(0, 0, 0, 2);
            _timer.Tick += timer_Tick;
        }

        public void Start()
        {
            _timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            if (Complete != null) Complete(this, new EventArgs());
        }
    }
}