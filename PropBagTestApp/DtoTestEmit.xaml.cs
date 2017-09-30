﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using System.Collections.Generic;

using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;


using PropBagTestApp.Models;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ViewModelBuilder;
using AutoMapper.Configuration;

using System.ComponentModel.Design;
using System.ComponentModel;

using DRM.PropBag;
using AutoMapper;

namespace PropBagTestApp
{
    /// <summary>
    /// Interaction logic for DtoTest.xaml
    /// </summary>
    public partial class DtoTestEmit : Window
    {
        Dictionary<string, BoundPropBag> _boundPropBags;
        //PropBagMapper<MyModel, DtoTestViewModelEmit> _pbMapper = null;
        //PropBagMapper<MyModel2, DtoTestViewModelEmit> _pbMapper2 = null;

        PropBagMapperKey<MyModel, DtoTestViewModelEmit> _mapperKey;
        PropBagMapperKey<MyModel2, DtoTestViewModelEmit> _mapperKey2;

        ConfiguredMappers _autoMappers;

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
            if (_isInDesignMode.HasValue && _isInDesignMode.Value)
            {
                System.Diagnostics.Debug.WriteLine("In Design");
                OurData = new DtoTestViewModelEmit(PropBagTypeSafetyMode.Tight);
                OurData.AddProp<int>("RunTimeInt", null, false, null, null, 17);
                OurData.SetIt<int>(21, "RunTimeInt");
            }

            InitializeComponent();

            // Create Proxy Classes dynamically for this View Model to enable AutoMapper mappings.
            _autoMappers = GetAutoMappers(PropBagMappingStrategyEnum.EmitProxy);

            Grid topGrid = (Grid)this.FindName("TopGrid");

            _boundPropBags = ViewModelGenerator.StandUpViewModels(topGrid, this);

            _autoMappers = PrepareAutoMappings(PropBagMappingStrategyEnum.EmitProxy);

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
                Amount = 145,
                Size = 17.8
            };

            ReadWithMap(mm, OurData);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var mapper = (PropBagMapper<MyModel, DtoTestViewModelEmit>) _autoMappers.GetMapperToUse(_mapperKey);
            MyModel m1 = (MyModel)mapper.MapToSource(OurData);
        }

        private void ReadWithMap(MyModel mm, DtoTestViewModelEmit vm)
        {
            var mapper = (PropBagMapper<MyModel, DtoTestViewModelEmit>)_autoMappers.GetMapperToUse(_mapperKey);

            DtoTestViewModelEmit tt = (DtoTestViewModelEmit)mapper.MapToDestination(mm, vm);

            // Now try creating a new one from mm.
            DtoTestViewModelEmit test = (DtoTestViewModelEmit)mapper.MapToDestination(mm);

            DefineMappers2("OurData");

            MyModel2 mm2 = new MyModel2
            {
                ProductId = Guid.NewGuid(),
                Amount = 1451,
                Size = 17.8
            };

            var mapper2 = (PropBagMapper<MyModel2, DtoTestViewModelEmit>)_autoMappers.GetMapperToUse(_mapperKey2);
            mapper2.MapToDestination(mm2, vm);
        }

        private void DefineMappers(string instanceKey)
        {
            BoundPropBag boundPB = _boundPropBags[instanceKey];

            _autoMappers.Register(_mapperKey);

            //_conMappers.Register(mapperKey2);

            //_pbMapper = (PropBagMapper<MyModel, DtoTestViewModelEmit>) _conMappers.GetMapperToUse(_mapperKey);
            //_pbMapper2 = (PropBagMapper<MyModel2, DtoTestViewModelEmit>) _conMappers.GetMapperToUse(mapperKey2);
        }

        private void DefineMappers2(string instanceKey)
        {
            BoundPropBag boundPB = _boundPropBags[instanceKey];

            //PropBagMapperKey<MyModel, DtoTestViewModelEmit> mapperKey
            //    = new PropBagMapperKey<MyModel, DtoTestViewModelEmit>(boundPB.PropModel, boundPB.RtViewModelType);

            //_conMappers.Register(mapperKey);


            _autoMappers.Register(_mapperKey2);

            //_pbMapper = (PropBagMapper<MyModel, DtoTestViewModelEmit>)_conMappers.GetMapperToUse(mapperKey);
            //_pbMapper2 = (PropBagMapper<MyModel2, DtoTestViewModelEmit>)_conMappers.GetMapperToUse(_mapperKey2);

        }

        private void DefineMapingKeys(string instanceKey)
        {
            BoundPropBag boundPB = _boundPropBags[instanceKey];

            _mapperKey
                = new PropBagMapperKey<MyModel, DtoTestViewModelEmit>
                (boundPB.PropModel, boundPB.RtViewModelType, mappingStrategy: PropBagMappingStrategyEnum.EmitProxy);

            _mapperKey2
                = new PropBagMapperKey<MyModel2, DtoTestViewModelEmit>
                (boundPB.PropModel, boundPB.RtViewModelType, mappingStrategy: PropBagMappingStrategyEnum.EmitProxy);
        }

        private ConfiguredMappers PrepareAutoMappings(PropBagMappingStrategyEnum mapperStrategy)
        {
            Func<Action<IMapperConfigurationExpression>, IConfigurationProvider> configBuilder =
                new MapperConfigurationProvider().BaseConfigBuilder;

            MapperConfigInitializerProvider configExressionProvider =
                new MapperConfigInitializerProvider(mapperStrategy);

            ConfiguredMappers result = new ConfiguredMappers(configBuilder, configExressionProvider);

            return result;
        }

        public static bool? _isInDesignMode;

        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)DependencyPropertyDescriptor
                                        .FromProperty(prop, typeof(FrameworkElement))
                                        .Metadata.DefaultValue;
                }

                return _isInDesignMode.Value;
            }
        }

        private ConfiguredMappers GetAutoMappers(PropBagMappingStrategyEnum mappingStrategy)
        {
            Func<Action<IMapperConfigurationExpression>, IConfigurationProvider> configBuilder
                = new MapperConfigurationProvider().BaseConfigBuilder;

            MapperConfigInitializerProvider mapperConfigExpression
                = new MapperConfigInitializerProvider(PropBagMappingStrategyEnum.ExtraMembers);

            ConfiguredMappers result = new ConfiguredMappers(configBuilder, mapperConfigExpression);

            return result;
        }




    }
}
