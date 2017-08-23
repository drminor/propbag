﻿using System;
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
using System.Windows.Shapes;

using System.Reflection;

using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;

using PropBagTestApp.Models;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for DtoTest.xaml
    /// </summary>
    public partial class DtoTest : Window
    {

        [PropBagInstanceAttribute("ourDataxx", "There is only one ViewModel in this View.")]
        public DtoTestViewModel ourData
        {
            get
            {
                return (DtoTestViewModel)this.DataContext;
            }
            set
            {
                this.DataContext = value;
            }
        }

        public DtoTest()
        {
            InitializeComponent();

            Grid topGrid = (Grid)this.FindName("TopGrid");

            ViewModelGenerator.StandUpViewModels(topGrid, this);

            // This is an example of how to create the binding whose source is a property in the Property Bag
            // to a UI Element in this class' view.
            // This same binding could have been created in XAML.

            Binding c = new Binding("[System.Double, Size]");
            c.Converter = new PropValueConverter();
            c.ConverterParameter = new TwoTypes(typeof(string), typeof(double));
            c.TargetNullValue = string.Empty;
            TextBox tb = (TextBox)this.FindName("Sz");
            tb.SetBinding(TextBox.TextProperty, c);
        }

        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            MyModel mm = new MyModel();

            mm.ProductId = Guid.NewGuid();
            mm.Amount = 145;
            mm.Size = 17.8;

            MapUsingDict();

            //ReadWithMap(mm, ourData);

            //ourData["ProductId"] = Guid.NewGuid();
            //ourData["Amount"] = 123;
            //ourData["Size"] = 2.09111d;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            MyModel m1 = new MyModel();
            m1.ProductId = (Guid)ourData["ProductId"];
            m1.Amount = (int)ourData["Amount"];
            m1.Size = (double)ourData["Size"];
        }

    }
}
