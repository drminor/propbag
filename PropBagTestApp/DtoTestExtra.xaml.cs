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
using AutoMapper.Configuration;

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
            var configBuilder = new MapperConfigurationProvider().BaseConfigBuilder;
            var initialMapperConfigExpProvider = new MapperStrategyConfigExpProvider(PropBagMappingStrategyEnum.ExtraMembers);

            _conMappers = new ConfiguredMappers(configBuilder, initialMapperConfigExpProvider);

            InitializeComponent();

            Grid topGrid = (Grid)this.FindName("TopGrid");

            _boundPropBags = ViewModelGenerator.StandUpViewModels(topGrid, this);

            DefineMapingKeys("OurData");
            DefineMappers();

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

        private void BtnRead2_Click(object sender, RoutedEventArgs e)
        {
            MyModel2 mm2 = new MyModel2
            {
                ProductId = Guid.NewGuid(),
                Amount = 1451,
                Size = 17.8
            };

            ReadWithMap2(mm2, OurData);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var mapper = (PropBagMapperCustom<MyModel, DtoTestViewModelExtra>)_conMappers.GetMapperToUse(_mapperKey);
            MyModel m1 = (MyModel)mapper.MapToSource(OurData);
        }

        private void ReadWithMap(MyModel mm, DtoTestViewModelExtra vm)
        {
            var mapper = (PropBagMapperCustom<MyModel, DtoTestViewModelExtra>)_conMappers.GetMapperToUse(_mapperKey);

            DtoTestViewModelExtra tt = (DtoTestViewModelExtra)mapper.MapToDestination(mm, vm);

            // Now try creating a new one from mm.
            DtoTestViewModelExtra test = (DtoTestViewModelExtra)mapper.MapToDestination(mm);
        }

        private void ReadWithMap2(MyModel2 mm2, DtoTestViewModelExtra vm)
        {
            DefineMappers2();

            var mapper2 = (PropBagMapperCustom<MyModel2, DtoTestViewModelExtra>)_conMappers.GetMapperToUse(_mapperKey2);
            DtoTestViewModelExtra tt = (DtoTestViewModelExtra)mapper2.MapToDestination(mm2, vm);
        }

        private void DefineMappers()
        {
            _conMappers.Register(_mapperKey);
        }

        private void DefineMappers2()
        {
            _conMappers.Register(_mapperKey2);
        }

        private void DefineMapingKeys(string instanceKey)
        {
            BoundPropBag boundPB = _boundPropBags[instanceKey];

            _mapperKey
                = new PropBagMapperKey<MyModel, DtoTestViewModelExtra>
                (boundPB.PropModel, boundPB.RtViewModelType, mappingStrategy: PropBagMappingStrategyEnum.ExtraMembers);

            _mapperKey2
                = new PropBagMapperKey<MyModel2, DtoTestViewModelExtra>
                (boundPB.PropModel, boundPB.RtViewModelType, mappingStrategy: PropBagMappingStrategyEnum.ExtraMembers);
        }


    }
}
