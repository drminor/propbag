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

        ConfiguredMappers _conMappers;

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

            Func<string, Action<IProfileExpression>, Profile> profileGen = (x, y) => new PropBagProfile(x, y);
            _conMappers = new ConfiguredMappers(new MapperConfigurationProvider().BaseConfigBuilder);

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
                Amount = 145,
                Size = 17.8
            };

            ReadWithMap(mm, OurData);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var mapper = (PropBagMapper<MyModel, DtoTestViewModelEmit>) _conMappers.GetMapperToUse(_mapperKey);
            MyModel m1 = (MyModel)mapper.MapTo(OurData);
        }

        private void ReadWithMap(MyModel mm, DtoTestViewModelEmit vm)
        {
            var mapper = (PropBagMapper<MyModel, DtoTestViewModelEmit>)_conMappers.GetMapperToUse(_mapperKey);

            DtoTestViewModelEmit tt = (DtoTestViewModelEmit)mapper.MapFrom(mm, vm);

            // Now try creating a new one from mm.
            DtoTestViewModelEmit test = (DtoTestViewModelEmit)mapper.MapFrom(mm);

            DefineMappers2("OurData");

            MyModel2 mm2 = new MyModel2
            {
                ProductId = Guid.NewGuid(),
                Amount = 1451,
                Size = 17.8
            };

            //var mapper2 = (PropBagMapper<MyModel2, DtoTestViewModelEmit>)_conMappers.GetMapperToUse(_mapperKey2);
            //mapper2.MapFrom(mm2, vm);
        }

        private void DefineMappers(string instanceKey)
        {
            BoundPropBag boundPB = _boundPropBags[instanceKey];

            _conMappers.Register(_mapperKey);

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


            _conMappers.Register(_mapperKey2);

            //_pbMapper = (PropBagMapper<MyModel, DtoTestViewModelEmit>)_conMappers.GetMapperToUse(mapperKey);
            //_pbMapper2 = (PropBagMapper<MyModel2, DtoTestViewModelEmit>)_conMappers.GetMapperToUse(_mapperKey2);

        }

        private void DefineMapingKeys(string instanceKey)
        {
            BoundPropBag boundPB = _boundPropBags[instanceKey];

            _mapperKey
                = new PropBagMapperKey<MyModel, DtoTestViewModelEmit>(boundPB.PropModel, boundPB.RtViewModelType);

            _mapperKey2
                = new PropBagMapperKey<MyModel2, DtoTestViewModelEmit>(boundPB.PropModel, boundPB.RtViewModelType);
        }



    }
}
