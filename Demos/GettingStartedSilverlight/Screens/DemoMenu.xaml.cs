using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FarseerSilverlightDemos
{
    public partial class DemoMenu : UserControl
    {
        public Key lastKey = Key.ENTER;
        private MenuItem quit;
        public bool QuitSelected;
        private MenuItem resume;

        public DemoMenu()
        {
            InitializeComponent();
            Canvas c = ir;
            resume = new MenuItem();
            resume.SetValue(Canvas.LeftProperty, Convert.ToDouble(175));
            resume.SetValue(Canvas.TopProperty, Convert.ToDouble(320));
            resume.Text = "Resume Demo";
            resume.Index = 0;
            WireUpMouseEvents(resume);
            c.Children.Add(resume);
            quit = new MenuItem();
            quit.SetValue(Canvas.LeftProperty, Convert.ToDouble(175));
            quit.SetValue(Canvas.TopProperty, Convert.ToDouble(360));
            quit.Text = "Quit Demo";
            quit.Index = 1;
            WireUpMouseEvents(quit);
            c.Children.Add(quit);
            Loaded += DemoMenu_Loaded;
        }

        public bool Visible
        {
            get { return ir.Visibility == Visibility.Visible; }
            set
            {
                if (value)
                {
                    ir.Visibility = Visibility.Visible;
                }
                else
                {
                    ir.Visibility = Visibility.Collapsed;
                }
            }
        }

        public string Title
        {
            set { title.Text = value; }
        }

        public string Details
        {
            set { details.Text = value; }
        }

        private void WireUpMouseEvents(MenuItem itm)
        {
            itm.MouseLeftButtonDown += itm_MouseLeftButtonDown;
            itm.MouseEnter += itm_MouseEnter;
            itm.MouseLeave += itm_MouseLeave;
        }

        private void itm_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            MenuItem itm = sender as MenuItem;
            if (itm.Index == 0)
            {
                Visible = false;
            }
            else
            {
                QuitSelected = true;
            }
        }

        private void itm_MouseEnter(object sender, MouseEventArgs e)
        {
            MenuItem itm = sender as MenuItem;
            itm.Selected = true;
        }

        private void itm_MouseLeave(object sender, EventArgs e)
        {
            MenuItem itm = sender as MenuItem;
            itm.Selected = false;
        }


        private void DemoMenu_Loaded(object sender, EventArgs e)
        {
        }

        public void HandleKeyboard()
        {
            Key k = 0;
            if (Page.KeyHandler.IsKeyPressed(Key.ESCAPE)) k = Key.ESCAPE;
            if (k == lastKey) return;
            if (k == Key.ESCAPE)
            {
                QuitSelected = true;
            }
            lastKey = k;
        }

        public void Dispose()
        {
            resume.Selected = false;
            quit.Selected = false;
        }
    }
}