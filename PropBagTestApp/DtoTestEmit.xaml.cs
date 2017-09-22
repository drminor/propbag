using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using System.Collections.Generic;

using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.LiveClassGenerator;

using DRM.TypeSafePropertyBag;

using PropBagTestApp.Models;
using DRM.AutoMapperSupport;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for DtoTest.xaml
    /// </summary>
    public partial class DtoTestEmit : Window
    {
        Dictionary<string, BoundPropBag> _boundPropBags;
        PropBagMapper<MyModel, DtoTestViewModelEmit> _pbMapper = null;
        PropBagMapper<MyModel2, DtoTestViewModelEmit> _pbMapper2 = null;
        ConfiguredMappers _conMappers = new ConfiguredMappers();

        [PropBagInstanceAttribute("OurData", "There is only one ViewModel in this View.")]
        public DtoTestViewModelEmit OurData
        {
            get
            {
                return (DtoTestViewModelEmit)this.DataContext;
            }
            set
            {
                this.DataContext = value;
            }
        }

        public DtoTestEmit()
        {
            InitializeComponent();

            Grid topGrid = (Grid)this.FindName("TopGrid");

            _boundPropBags =  ViewModelGenerator.StandUpViewModels(topGrid, this);

            // This is an example of how to create the binding whose source is a property in the Property Bag
            // to a UI Element in this class' view.
            // This same binding could have been created in XAML.

            Binding c = new Binding("[System.Double, Size]")
            {
                Converter = new PropValueConverter(),
                ConverterParameter = new TwoTypes(typeof(string), typeof(double)),
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
            PropBagMapper<MyModel, DtoTestViewModelEmit> mapper = GetPropBagMapper("OurData");
            MyModel m1 = mapper.MapFrom(OurData);
        }

        private void ReadWithMap(MyModel mm, DtoTestViewModelEmit vm)
        {
            PropBagMapper<MyModel, DtoTestViewModelEmit> mapper = GetPropBagMapper("OurData");
            mapper.MapTo(mm, vm);

            //// Now try creating a new one from mm.
            //DtoTestViewModelEmit test = (DtoTestViewModelEmit) mapper.MapTo(mm);

            //MyModel2 mm2 = new MyModel2
            //{
            //    ProductId = Guid.NewGuid(),
            //    Amount = 1451,
            //    Size = 17.8
            //};

            //_pbMapper2.MapTo(mm2,vm);

        }


        private PropBagMapper<MyModel, DtoTestViewModelEmit> GetPropBagMapper(string instanceKey)
        {
            //BoundPropBag boundPB = _boundPropBags[instanceKey];
            //_pbMapper = new PropBagMapper<MyModel, DtoTestViewModelEmit>(boundPB);
            //return _pbMapper;

            if(_pbMapper == null)
            {
                BoundPropBag boundPB = _boundPropBags[instanceKey];

                _pbMapper = new PropBagMapper<MyModel, DtoTestViewModelEmit>(boundPB);
                _conMappers.AddMapReq(_pbMapper);

                _pbMapper2 = new PropBagMapper<MyModel2, DtoTestViewModelEmit>(boundPB);
                _conMappers.AddMapReq(_pbMapper2);

                // Add other type combinations here.

                _conMappers.SealThis();
            }

            return _pbMapper;
        }

    }
}
