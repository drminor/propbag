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

            StandUpViewModels(topGrid);

            // This is an example of how to create the binding whose source is a property in the Property Bag
            // to a UI Element in this class' view.
            // This same binding could have been created in XAML.

            Binding c = new Binding("[Size]");
            c.Converter = new PropValueConverter();
            c.ConverterParameter = new TwoTypes(typeof(string), typeof(double));
            TextBox tb = (TextBox)this.FindName("Sz");
            tb.SetBinding(TextBox.TextProperty, c);
        }

        /// <summary>
        /// Each View that uses a PropertyBag template needs to include a copy of this method.
        /// It looks for property in this class (the class in which this method is declared) marked with the 
        /// PropBagInstanceAttribute Attribute.
        /// It should be called in the view's constructor.
        /// If the DataContext is will be assigned via code, it should be done after this method is called.
        /// </summary>
        /// <param name="root">The UI Element at which to begin looking for PropBagTemplate elements.</param>
        private void StandUpViewModels(Panel root)
        {
            Type propModelType = typeof(DRM.PropBag.ControlModel.PropModel);

            IEnumerable<PropBagTemplate> propBagTemplates = root.Children.OfType<PropBagTemplate>();

            foreach (PropBagTemplate pbt in propBagTemplates)
            {
                // Build a control model from the XAML contents of the template.
                DRM.PropBag.ControlModel.PropModel pm = pbt.GetPropBagModel();
                if (pm != null)
                {
                    // Get a reference to the property that access the class that needs to be created.
                    Type thisType = this.GetType();
                    PropertyInfo classAccessor = ReflectionHelpers.GetPropBagClassProperty(thisType, pm.ClassName, pm.InstanceKey);

                    // Instantiate the target ViewModel
                    ReflectionHelpers.CreateTargetAndAssign(this, classAccessor, propModelType, pm);
                }
            }
        }

        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            MyModel mm = new MyModel();

            mm.ProductId = Guid.NewGuid();
            mm.Amount = 145;
            mm.Size = 17.8;

            ReadWithMap(mm, ourData);

            //ourData["ProductId"] = Guid.NewGuid();
            ourData["Amount"] = 123;
            //ourData["Size"] = 2.09111d;
        }

        private void ReadWithMap(MyModel mm, DtoTestViewModel vm)             
        {
            var config = new AutoMapper.MapperConfiguration(cfg => cfg.CreateMap<MyModel, DtoTestViewModel>());

            var mapper = config.CreateMapper();

            mapper.Map<MyModel, DtoTestViewModel>(mm, vm);
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
