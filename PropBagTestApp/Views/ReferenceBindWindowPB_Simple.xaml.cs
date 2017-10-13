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

            //PropBagTemplate pbt = PropBagTemplateResources.GetPropBagTemplate("ReferenceBindViewModelPB");
            //PropModel pm = pbt.GetPropModel();
            //IPropFactory pf = SettingsExtensions.ThePropFactory;

            //ReferenceBindViewModelPB rbvm = new ReferenceBindViewModelPB(pm, pf);

            ReferenceBindViewModelPB rbvm = (ReferenceBindViewModelPB)this.DataContext;

            MyModel4 mod4 = new MyModel4() { MyString = "Start" };
            rbvm.SetIt<MyModel4>(mod4, "Deep");

            //this.DataContext = rbvm;
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            //CollectionViewSource aa = new CollectionViewSource();
            ReferenceBindViewModelPB m = (ReferenceBindViewModelPB)this.DataContext;

            m.SetIt<int>(21, "Amount");
            //m.Amount = 21;

            m.SetIt<double>(30.3, "Size");
            //m.Size = 30.3;

            m.SetIt<Guid>(Guid.NewGuid(), "ProductId");
            //m.ProductId = Guid.NewGuid();

            m.SetIt<double>(0.01, "TestDouble");
            //m.TestDouble = 0.01;

            MyModel4 r = m.GetIt<MyModel4>("Deep");
            if(r != null)
            {
                r.MyString = "Jacob2";
            }
            else
            {
                MyModel4 mod4 = new MyModel4() { MyString = "Jacob" };
                m.SetIt<MyModel4>(mod4, "Deep");
            }

        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ReferenceBindViewModelPB m = (ReferenceBindViewModelPB)this.DataContext;

            int amount = m.GetIt<int>("Amount");
            double size = m.GetIt<double>("Size");
            Guid productId = m.GetIt<Guid>("ProductId");
            double testDouble = m.GetIt<double>("TestDouble");
            string myString = m.GetIt<MyModel4>("Deep")?.MyString;

            System.Diagnostics.Debug.WriteLine($"Amount = {amount}.");
            System.Diagnostics.Debug.WriteLine($"Size = {size}.");
            System.Diagnostics.Debug.WriteLine($"ProductId = {productId}.");
            System.Diagnostics.Debug.WriteLine($"TestDouble = {testDouble}.");
            System.Diagnostics.Debug.WriteLine($"Deep.MyString = {myString}.");
        }
    }




}
