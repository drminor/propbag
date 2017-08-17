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

using System.Collections;

using System.Reflection; // This is temporary -- just for testing.

using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [PropBagInstanceAttribute("MainViewModel")]
        public MainViewModel ourData { get; set; }

        [PropBagInstanceAttribute("MainViewModel2")]
        private MainViewModel ourData2 { get; set; }

        //public bool PropFirstDidChange;
        //public bool PropMyStringDidChange;
        
        public MainWindow()
        {
            InitializeComponent();

            Grid topGrid =  (Grid) this.FindName("TopGrid");

            StandUpViewModels(topGrid);

            topGrid.DataContext = ourData;

            Grid insideGrid = (Grid)this.FindName("InsideGrid");
            insideGrid.DataContext = ourData2;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    } 
}
