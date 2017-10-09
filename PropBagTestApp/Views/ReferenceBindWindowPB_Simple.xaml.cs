using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.ControlsWPF.WPFHelpers;

using PropBagTestApp.ViewModels;
using PropBagTestApp.Models;

using System;
using System.Windows;
using System.Windows.Data;

namespace PropBagTestApp.View
{
    /// <summary>
    /// Interaction logic for ReferenceBindWindowPB_Simple.xaml
    /// </summary>
    public partial class ReferenceBindWindowPB_Simple : Window
    {
        public ReferenceBindWindowPB_Simple()
        {
            InitializeComponent();

            PropBagTemplate pbt = PropBagTemplateResources.GetPropBagTemplate("ReferenceBindViewModelPB");
            PropModel pm = pbt.GetPropModel();
            IPropFactory pf = SettingsExtensions.ThePropFactory;

            ReferenceBindViewModelPB rbvm = new ReferenceBindViewModelPB(pm, pf);

            MyModel4 mod4 = new MyModel4() { MyString = "Start" };
            rbvm.SetIt<MyModel4>(mod4, "Deep");

            this.DataContext = rbvm;
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            //CollectionViewSource aa = new CollectionViewSource();
            ReferenceBindViewModelPB m = (ReferenceBindViewModelPB)this.DataContext;

            m.SetIt<int>(42, "Amount");
            //m.Amount = 21;
            //m.Size = 30.3;
            //m.ProductId = Guid.NewGuid();
            //m.TestDouble = 0.01;


            MyModel4 mod4 = new MyModel4() { MyString = "hello" };
            m.SetIt<MyModel4>(mod4, "Deep");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ReferenceBindViewModelPB m = (ReferenceBindViewModelPB)this.DataContext;

            double size = m.GetIt<double>("Size");

            string myString = m.GetIt<MyModel4>("Deep")?.MyString;

            System.Diagnostics.Debug.WriteLine($"Size = {size}.");
            System.Diagnostics.Debug.WriteLine($"Deep.MyString = {myString}.");
        }
    }




}
