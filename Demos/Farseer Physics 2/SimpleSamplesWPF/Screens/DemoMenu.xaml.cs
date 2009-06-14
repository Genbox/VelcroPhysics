using System;
using System.Windows.Controls;
using SimpleSamplesWPF.Demos;

namespace SimpleSamplesWPF.Screens
{
    /// <summary>
    /// Interaction logic for DemoMenu.xaml
    /// </summary>
    public partial class DemoMenu : UserControl
    {
        public event EventHandler ResumeToDemo;
        public event EventHandler Quit; 

        public DemoMenu(Demo demo)
        {
            InitializeComponent();

            title.Text = demo.Title;
            details.Text = demo.Details;

            MenuItem resumeItem = new MenuItem("Resume Demo");
            resumeItem.Click += ResumeItemOnClick;
            menuStack.Children.Add(resumeItem);

            MenuItem exitItem = new MenuItem("Quit Demo");
            exitItem.Click += ExitItemOnClick;
            menuStack.Children.Add(exitItem);
        }

        private void ExitItemOnClick(object sender, EventArgs args)
        {
            if (Quit != null)
                Quit(this, new EventArgs());
        }

        void ResumeItemOnClick(object sender, EventArgs e)
        {
            if (ResumeToDemo != null)
                ResumeToDemo(this, new EventArgs());
        }
    }
}
