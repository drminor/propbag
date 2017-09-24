using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.LiveClassGenerator;

using PropBagTestApp.Models;

using AutoMapper;
using System.Collections.Generic;
using System.Reflection;
using DRM.PropBag;
using DRM.AutoMapperSupport;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for DtoTest.xaml
    /// </summary>
    public partial class DtoTestExtra : Window
    {
        Dictionary<string, BoundPropBag> _boundPropBags;
        PropBagMapperCustom<MyModel, DtoTestViewModelExtra> _pbMapper = null;
        PropBagMapperCustom<MyModel2, DtoTestViewModelExtra> _pbMapper2 = null;
        ConfiguredMappers _conMappers = new ConfiguredMappers();

        [PropBagInstanceAttribute("OurData", "There is only one ViewModel in this View.")]
        public DtoTestViewModelExtra OurData
        {
            get
            {
                return (DtoTestViewModelExtra)this.DataContext;
            }
            set
            {
                this.DataContext = value;
            }
        }

        public DtoTestExtra()
        {
            InitializeComponent();

            Grid topGrid = (Grid)this.FindName("TopGrid");

            _boundPropBags = ViewModelGenerator.StandUpViewModels(topGrid, this);

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
                Amount = 38,
                Size = 20.02
            };

            ReadWithMap(mm, OurData);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            PropBagMapperCustom<MyModel, DtoTestViewModelExtra> mapper = GetPropBagMapper("OurData");
            MyModel m1 = mapper.MapFrom(OurData);
        }

        private void ReadWithMap(MyModel mm, DtoTestViewModelExtra vm)
        {
            PropBagMapperCustom<MyModel, DtoTestViewModelExtra> mapper = GetPropBagMapper("OurData");
            mapper.MapTo(mm, vm);

            //// Now try creating a new one from mm.
            DtoTestViewModelExtra test = (DtoTestViewModelExtra)mapper.MapTo(mm);

            MyModel2 mm2 = new MyModel2
            {
                ProductId = Guid.NewGuid(),
                Amount = 1451,
                Size = 17.8
            };

            _pbMapper2.MapTo(mm2, vm);
        }


        private PropBagMapperCustom<MyModel, DtoTestViewModelExtra> GetPropBagMapper(string instanceKey)
        {
            if (_pbMapper == null)
            {
                BoundPropBag boundPB = _boundPropBags[instanceKey];

                _pbMapper = new PropBagMapperCustom<MyModel, DtoTestViewModelExtra>(boundPB);
                _conMappers.AddMapReq(_pbMapper);

                _pbMapper2 = new PropBagMapperCustom<MyModel2, DtoTestViewModelExtra>(boundPB);
                _conMappers.AddMapReq(_pbMapper2);

                // Add other type combinations here.

                _conMappers.SealThis();
            }

            return _pbMapper;
        }

    }
}
