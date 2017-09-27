using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;


using PropBagTestApp.Models;

using AutoMapper;
using System.Collections.Generic;
using System.Reflection;
using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for DtoTest.xaml
    /// </summary>
    public partial class DtoTestExtra : Window
    {
        Dictionary<string, BoundPropBag> _boundPropBags;
        PropBagMapperKey<MyModel, DtoTestViewModelExtra> _mapperKey;
        PropBagMapperKey<MyModel2, DtoTestViewModelExtra> _mapperKey2;

        ConfiguredMappers _conMappers;

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
            _conMappers = new ConfiguredMappers(new MapperConfigurationProvider().BaseConfigBuilder);

            InitializeComponent();

            Grid topGrid = (Grid)this.FindName("TopGrid");

            _boundPropBags = ViewModelGenerator.StandUpViewModels(topGrid, this);

            DefineMapingKeys("OurData");
            DefineMappers("OurData");

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
            var mapper = (PropBagMapperCustom<MyModel, DtoTestViewModelExtra>)_conMappers.GetMapperToUse(_mapperKey);
            MyModel m1 = (MyModel)mapper.MapTo(OurData);
        }

        private void ReadWithMap(MyModel mm, DtoTestViewModelExtra vm)
        {
            var mapper = (PropBagMapperCustom<MyModel, DtoTestViewModelExtra>)_conMappers.GetMapperToUse(_mapperKey);

            DtoTestViewModelExtra tt = (DtoTestViewModelExtra)mapper.MapFrom(mm, vm);

            // Now try creating a new one from mm.
            DtoTestViewModelExtra test = (DtoTestViewModelExtra)mapper.MapFrom(mm);

            DefineMappers2("OurData");

            MyModel2 mm2 = new MyModel2
            {
                ProductId = Guid.NewGuid(),
                Amount = 1451,
                Size = 17.8
            };

            //var mapper2 = (PropBagMapperCustom<MyModel2, DtoTestViewModelExtra>)_conMappers.GetMapperToUse(_mapperKey2);
            //mapper2.MapFrom(mm2, vm);
        }

        private void DefineMappers(string instanceKey)
        {
            BoundPropBag boundPB = _boundPropBags[instanceKey];

            _conMappers.Register(_mapperKey);

            //_conMappers.Register(mapperKey2);

            //_pbMapper = (PropBagMapper<MyModel, DtoTestViewModelExtra>) _conMappers.GetMapperToUse(_mapperKey);
            //_pbMapper2 = (PropBagMapper<MyModel2, DtoTestViewModelExtra>) _conMappers.GetMapperToUse(mapperKey2);
        }

        private void DefineMappers2(string instanceKey)
        {
            BoundPropBag boundPB = _boundPropBags[instanceKey];

            //PropBagMapperKey<MyModel, DtoTestViewModelExtra> mapperKey
            //    = new PropBagMapperKey<MyModel, DtoTestViewModelExtra>(boundPB.PropModel, boundPB.RtViewModelType);

            //_conMappers.Register(mapperKey);


            _conMappers.Register(_mapperKey2);

            //_pbMapper = (PropBagMapper<MyModel, DtoTestViewModelExtra>)_conMappers.GetMapperToUse(mapperKey);
            //_pbMapper2 = (PropBagMapper<MyModel2, DtoTestViewModelExtra>)_conMappers.GetMapperToUse(_mapperKey2);

        }

        private void DefineMapingKeys(string instanceKey)
        {
            BoundPropBag boundPB = _boundPropBags[instanceKey];

            _mapperKey
                = new PropBagMapperKey<MyModel, DtoTestViewModelExtra>(boundPB.PropModel, boundPB.RtViewModelType, useCustom: true);

            _mapperKey2
                = new PropBagMapperKey<MyModel2, DtoTestViewModelExtra>(boundPB.PropModel, boundPB.RtViewModelType, useCustom: true);
        }

    }
}
