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
    public delegate void MenuItemSelectedEvent(int index);
    public partial class MenuItem : UserControl
    {
        bool selected;
        public int Index;

        public MenuItem()
        {
            InitializeComponent();
        }

        public string Text
        {
            set
            {
                text.Text = value;
            }
            get
            {
                return text.Text;
            }
        }

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                if (value != selected)
                {
                    if (value)
                    {
                        text.Foreground = new SolidColorBrush(Colors.White);
                        selectedStoryboard.Begin();
                    }
                    else
                    {
                        selectedStoryboard.Stop();
                        text.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
                selected = value;
            }
        }
    }
}
