using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Input;

namespace FarseerGames.SimpleSamplesSilverlight.Screens
{
    public partial class MainMenu
    {
        private List<MenuItem> _menuItems = new List<MenuItem>();

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

        public event MenuItemSelectedEvent MenuItemSelected;

        private void MenuEntriesAdd(string text)
        {
            MenuItem itm = new MenuItem();
            itm.Text = text;
            menuStack.Children.Add(itm);
            itm.Index = _menuItems.Count;
            _menuItems.Add(itm);
            itm.MouseLeftButtonDown += itm_MouseLeftButtonDown;
            itm.MouseEnter += itm_MouseEnter;
            itm.MouseLeave += itm_MouseLeave;
            getFarseer.MouseLeftButtonUp += getFarseer_MouseLeftButtonUp;
            getFarseer.MouseEnter += getFarseer_MouseEnter;
            getFarseer.MouseLeave += getFarseer_MouseLeave;
        }

        private void getFarseer_MouseLeave(object sender, EventArgs e)
        {
            getFarseer.TextDecorations = null;
        }

        private void getFarseer_MouseEnter(object sender, MouseEventArgs e)
        {
            getFarseer.TextDecorations = TextDecorations.Underline;
        }

        private void getFarseer_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            HtmlPage.Window.Navigate(new Uri("http://www.codeplex.com/FarseerPhysics", UriKind.Absolute), "blank");
        }

        private void itm_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            MenuItem itm = sender as MenuItem;
            if (MenuItemSelected != null)
            {
                MenuItemSelected(itm.Index);
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
    }
}