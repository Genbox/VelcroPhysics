using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace FarseerSilverlightDemos
{
    public partial class MainMenu : UserControl
    {
        List<MenuItem> menuItems = new List<MenuItem>();
        int selectedItem = 0;
        Key lastKey;
        public event MenuItemSelectedEvent MenuItemSelected;

        public MainMenu()
        {
            InitializeComponent();
            MenuEntriesAdd("Demo1: A Single Body");
            MenuEntriesAdd("Demo2: Two Bodies With Geom");
            MenuEntriesAdd("Demo3: Static Bodies And Offset Geometries");
            MenuEntriesAdd("Demo4: Stacked Bodies");
            MenuEntriesAdd("Demo5: Collision Categories");
            MenuEntriesAdd("Demo6: Linear and Angular Spring Controllers");
            MenuEntriesAdd("Demo7: Dynamic Angle Joints");
        }

        void MenuEntriesAdd(string text)
        {
            MenuItem itm = new MenuItem();
            itm.Text = text;
            menuStack.Children.Add(itm);
            itm.Index = menuItems.Count;
            menuItems.Add(itm);
            itm.MouseLeftButtonDown += new MouseButtonEventHandler(itm_MouseLeftButtonDown);
            itm.MouseEnter += new MouseEventHandler(itm_MouseEnter);
            itm.MouseLeave += new MouseEventHandler(itm_MouseLeave);
            getFarseer.MouseLeftButtonUp += new MouseButtonEventHandler(getFarseer_MouseLeftButtonUp);
            getFarseer.MouseEnter += new MouseEventHandler(getFarseer_MouseEnter);
            getFarseer.MouseLeave += new MouseEventHandler(getFarseer_MouseLeave);
        }

        void getFarseer_MouseLeave(object sender, EventArgs e)
        {
            getFarseer.TextDecorations = null;
        }

        void getFarseer_MouseEnter(object sender, MouseEventArgs e)
        {
            getFarseer.TextDecorations = TextDecorations.Underline;
        }

        void getFarseer_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("http://www.codeplex.com/FarseerPhysics", UriKind.Absolute), "blank");
        }

        void itm_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            MenuItem itm = sender as MenuItem;
            if (MenuItemSelected != null)
            {
                MenuItemSelected(itm.Index);
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
    }
}
