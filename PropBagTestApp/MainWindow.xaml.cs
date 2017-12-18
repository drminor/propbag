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

using System.Reflection;

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
        public MainViewModel OurData { get; set; }

        [PropBagInstanceAttribute("MainViewModel2")]
        private MainViewModel OurData2 { get; set; }

        
        public MainWindow()
        {
            InitializeComponent();

            Grid topGrid =  (Grid) this.FindName("TopGrid");
            //ViewModelGenerator.StandUpViewModels(topGrid, this);

            topGrid.DataContext = OurData;

            Grid insideGrid = (Grid)this.FindName("InsideGrid");
            insideGrid.DataContext = OurData2;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    } 
}
