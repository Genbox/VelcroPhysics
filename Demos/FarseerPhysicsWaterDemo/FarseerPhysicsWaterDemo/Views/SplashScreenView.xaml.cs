using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
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
            this.Loaded += new RoutedEventHandler(SplashView_Loaded);
        }

        void SplashView_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Interval = new TimeSpan(0, 0, 0, 4);
            timer.Tick += new EventHandler(timer_Tick);
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
