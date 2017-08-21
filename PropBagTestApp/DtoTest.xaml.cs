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

        [PropBagInstanceAttribute("DtoTestViewModel")]
        public DtoTestViewModel ourData { get; set; }

        public DtoTest()
        {
            InitializeComponent();
            Grid topGrid = (Grid)this.FindName("TopGrid");

            StandUpViewModels(topGrid);

            topGrid.DataContext = ourData;

            Binding c = new Binding("[Size]");
            c.Converter = new PropValueConverter();
            c.ConverterParameter = new TwoTypes(typeof(string), typeof(double));
            //c.Mode = BindingMode.TwoWay;

            TextBox tb = (TextBox)this.FindName("Sz");
            var x = tb.SetBinding(TextBox.TextProperty, c);

            //Binding d = new Binding("Amount");
            //d.Mode = BindingMode.TwoWay;

            //TextBox tb1 = (TextBox)this.FindName("Amt");
            //var x1 = tb1.SetBinding(TextBox.TextProperty, d);


        }

        private void StandUpViewModels(Panel root)
        {
            Type thisType = this.GetType();
            Type propModelType = typeof(DRM.PropBag.ControlModel.PropModel);

            IEnumerable<PropBagTemplate> propBagTemplates = root.Children.OfType<PropBagTemplate>();

            foreach (PropBagTemplate pbt in propBagTemplates)
            {
                // Build a control model from the XAML contents of the template.
                DRM.PropBag.ControlModel.PropModel pm = pbt.GetPropBagModel();
                if (pm != null)
                {
                    // Get a reference to the property that access the class that needs to be created.
                    PropertyInfo classAccessor = ReflectionHelpers.GetPropBagClassProperty(thisType, pm.ClassName);

                    // Instantiate the target ViewModel
                    ReflectionHelpers.CreateTargetAndAssign(this, classAccessor, propModelType, pm);
                }
            }
        }

        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            ourData["ProductId"] = Guid.NewGuid();
            ourData["Amount"] = 12;
            ourData["Size"] = 2.09111d;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            MyModel m1 = new MyModel();
            m1.ProductId = (Guid)ourData["ProductId"];
            m1.Amount = (int)ourData["Amount"];
            m1.Size = (double)ourData["Size"];
        }

        private MyModel GetTestInstance()
        {
            MyModel m1 = new MyModel();
            m1.ProductId = Guid.NewGuid();
            m1.Amount = 10;
            m1.Size = 32.44;

            return m1;
        }
    }
}
