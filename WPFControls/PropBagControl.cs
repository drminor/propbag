using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace DRM.PropBag.ControlsWPF
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DRM.WPFControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DRM.WPFControls;assembly=WPFControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:PropBagControl/>
    ///
    /// </summary>
    /// 
    [TemplatePart(Name = "PART_MainBorder", Type = typeof(Border))]
    [TemplatePart(Name = "PART_body", Type = typeof(ContentControl))]
    public class PropBagControl : Control
    {
        static PropBagControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropBagControl), new FrameworkPropertyMetadata(typeof(PropBagControl)));
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color",
            typeof(Color),
            typeof(PropBagControl),
            new PropertyMetadata(Colors.Green));

        public Color Color
        {
            get
            {
                return (Color)this.GetValue(ColorProperty);
            }
            set
            {
                this.SetValue(ColorProperty, value);
            }
        }


        Border MainBorder;
        ContentControl Body;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.Template != null)
            {
                Border mainBorder = this.Template.FindName("PART_MainBorder", this) as Border;
                if (mainBorder != MainBorder)
                {
                    //First unhook existing handler
                    if (MainBorder != null)
                    {
                        MainBorder.MouseEnter -= new MouseEventHandler(MainBorder_MouseEnter);
                        MainBorder.MouseLeave -= new MouseEventHandler(MainBorder_MouseLeave);
                    }
                    MainBorder = mainBorder;
                    if (MainBorder != null)
                    {
                        MainBorder.MouseEnter += new MouseEventHandler(MainBorder_MouseEnter);
                        MainBorder.MouseLeave += new MouseEventHandler(MainBorder_MouseLeave);
                    }
                }

                Body = this.Template.FindName("PART_body", this) as ContentControl;

            }
        }

        void MainBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            Border thisBorder = sender as Border;
            if (thisBorder != null)
            {
                thisBorder.Background = new SolidColorBrush(Colors.Red);
                if (Body != null)
                {
                    Run r = new Run("Mouse Left!");
                    r.Foreground = new SolidColorBrush(Colors.White);
                    Body.Content = r;
                }
            }
        }

        void MainBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            Border thisBorder = sender as Border;
            if (thisBorder != null)
            {
                thisBorder.Background = new SolidColorBrush(Colors.Blue);
                if (Body != null)
                {
                    Run r = new Run("Mouse Entered!");
                    r.Foreground = new SolidColorBrush(Colors.White);
                    Body.Content = r;
                }
            }
        }

    }
}
