using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.ControlsWPF.WPFHelpers;

using PropBagTestApp.ViewModels;
using System;
using System.Windows;

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
            PropModel pm = pbt.GetPropBagModel();
            PropFactory pf = new PropFactory(false, GetTypeFromName);

            ReferenceBindViewModelPB rbvm = new ReferenceBindViewModelPB(pm, pf);

            this.DataContext = rbvm;
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            ReferenceBindViewModelPB m = (ReferenceBindViewModelPB)this.DataContext;

            m.SetIt<int>(42, "Amount");
            //m.Amount = 21;
            //m.Size = 30.3;
            //m.ProductId = Guid.NewGuid();
            //m.TestDouble = 0.01;
            //m.Deep = new MyModel4() { MyString = "hello" };
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ReferenceBindViewModelPB m = (ReferenceBindViewModelPB)this.DataContext;

            double size =  m.GetIt<double>("Size");


            System.Diagnostics.Debug.WriteLine($"Size = {size}.");
            //System.Diagnostics.Debug.WriteLine($"Deep.MyString = {m.Deep.MyString}.");
        }


        public static Type GetTypeFromName(string typeName)
        {
            Type result;
            try
            {
                result = Type.GetType(typeName);
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.", e);
            }

            if (result == null)
            {
                throw new InvalidOperationException($"Cannot create a Type instance from the string: {typeName}.");
            }

            return result;
        }
    }


}
