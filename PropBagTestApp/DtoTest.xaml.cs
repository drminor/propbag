using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;

using PropBagTestApp.Models;

using AutoMapper;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for DtoTest.xaml
    /// </summary>
    public partial class DtoTest : Window
    {

        [PropBagInstanceAttribute("OurData", "There is only one ViewModel in this View.")]
        public DtoTestViewModel OurData
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

            Binding c = new Binding("[System.Double, Size]")
            {
                Converter = new PropValueConverter(),
                ConverterParameter = new TwoTypes(typeof(double), typeof(string)),
                TargetNullValue = string.Empty
            };
            TextBox tb = (TextBox)this.FindName("Sz");
            tb.SetBinding(TextBox.TextProperty, c);
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            MyModel mm = new MyModel
            {
                ProductId = Guid.NewGuid(),
                Amount = 145,
                Size = 17.8
            };

            //MapUsingDict();

            ReadWithMap(mm, OurData);

            //ourData["ProductId"] = Guid.NewGuid();
            //ourData["Amount"] = 123;
            //ourData["Size"] = 2.09111d;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            //MyModel m1 = new MyModel
            //{
            //    ProductId = (Guid)OurData["ProductId"],
            //    Amount = (int)OurData["Amount"],
            //    Size = (double)OurData["Size"]
            //};
        }

        private void ReadWithMap(MyModel mm, DtoTestViewModel vm)
        {
            GetMapper().Map<MyModel, DtoTestViewModel>(mm, vm);
        }

        private IMapper _mapper = null;
        private IMapper GetMapper()
        {
            if (_mapper == null)
            {
                _mapper = new MapperConfiguration(cfg => { }).CreateMapper();
            }
            return _mapper;
        }
    }
}
