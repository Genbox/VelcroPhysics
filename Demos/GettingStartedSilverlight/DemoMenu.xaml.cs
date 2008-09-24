using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FarseerSilverlightDemos
{
    public partial class DemoMenu : UserControl
    {
        MenuItem resume;
        MenuItem quit;
        public Key lastKey = Key.ENTER;
        public bool QuitSelected = false;

        void WireUpMouseEvents(MenuItem itm)
        {
            itm.MouseLeftButtonDown += new MouseButtonEventHandler(itm_MouseLeftButtonDown);
            itm.MouseEnter += new MouseEventHandler(itm_MouseEnter);
            itm.MouseLeave += new MouseEventHandler(itm_MouseLeave);
        }

        public DemoMenu()
        {
            InitializeComponent();
            Canvas c = ir as Canvas;
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
            this.Loaded += new RoutedEventHandler(DemoMenu_Loaded);
        }

        void itm_MouseLeftButtonDown(object sender, MouseEventArgs e)
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

        void itm_MouseEnter(object sender, MouseEventArgs e)
        {
            MenuItem itm = sender as MenuItem;
            itm.Selected = true;
        }

        void itm_MouseLeave(object sender, EventArgs e)
        {
            MenuItem itm = sender as MenuItem;
            itm.Selected = false;
        }


        void DemoMenu_Loaded(object sender, EventArgs e)
        {
        }

        public void HandleKeyboard()
        {
            Key k = (Key)0;
            if (Page.KeyHandler.IsKeyPressed(Key.ESCAPE)) k = Key.ESCAPE;
            if (k == lastKey) return;
            if (k == Key.ESCAPE)
            {
                QuitSelected = true;
            }
            lastKey = k;
        }

        public bool Visible
        {
            get
            {
                return ir.Visibility == Visibility.Visible;
            }
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

        public void Dispose()
        {
            resume.Selected = false;
            quit.Selected = false;
        }

        public string Title
        {
            set
            {
                title.Text = value;
            }
        }

        public string Details
        {
            set
            {
                details.Text = value;
            }
        }
    }
}
