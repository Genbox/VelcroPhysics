using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FarseerGames.SimpleSamplesSilverlight.Screens
{
    public partial class DemoMenu
    {
        private MenuItem _quit;
        private MenuItem _resume;
        public Key lastKey = Key.ENTER;
        public bool QuitSelected;

        public DemoMenu()
        {
            InitializeComponent();
            Canvas c = ir;
            _resume = new MenuItem();
            _resume.SetValue(Canvas.LeftProperty, Convert.ToDouble(175));
            _resume.SetValue(Canvas.TopProperty, Convert.ToDouble(320));
            _resume.Text = "Resume Demo";
            _resume.Index = 0;
            WireUpMouseEvents(_resume);
            c.Children.Add(_resume);
            _quit = new MenuItem();
            _quit.SetValue(Canvas.LeftProperty, Convert.ToDouble(175));
            _quit.SetValue(Canvas.TopProperty, Convert.ToDouble(360));
            _quit.Text = "Quit Demo";
            _quit.Index = 1;
            WireUpMouseEvents(_quit);
            c.Children.Add(_quit);
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
            if (itm != null)
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
            if (itm != null) itm.Selected = true;
        }

        private void itm_MouseLeave(object sender, EventArgs e)
        {
            MenuItem itm = sender as MenuItem;
            if (itm != null) itm.Selected = false;
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
            _resume.Selected = false;
            _quit.Selected = false;
        }
    }
}