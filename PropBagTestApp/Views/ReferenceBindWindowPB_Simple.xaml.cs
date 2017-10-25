using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.ControlsWPF.WPFHelpers;

using PropBagTestApp.ViewModels;
using PropBagTestApp.Models;

using System;
using System.Windows;
using System.Windows.Data;
using DRM.TypeSafePropertyBag;
using DRM.PropBag.AutoMapperSupport;
using System.Collections.Generic;
using AutoMapper;
using System.Windows.Controls;
using DRM.PropBag.ViewModelBuilder;

namespace PropBagTestApp.View
{
    /// <summary>
    /// Interaction logic for ReferenceBindWindowPB_Simple.xaml
    /// </summary>
    public partial class ReferenceBindWindowPB_Simple : Window
    {

        IPropModelProvider _propModelProvider;
        AutoMapperProvider _autoMapperProvider;

        //Dictionary<string, BoundPropBag> _boundPropBags;
        //PropBagMapperKey<MyModel, ReferenceBindViewModelPB> _mapperKey;
        ////PropBagMapperKey<MyModel2, DtoTestViewModelExtra> _mapperKey2;


        public ReferenceBindWindowPB_Simple()
        {
            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources);
            _propModelProvider = new PropModelProvider(propBagTemplateProvider);

            _autoMapperProvider = InitializeAutoMappers(_propModelProvider);

            InitializeComponent();

            //Grid topGrid = (Grid)this.FindName("TopGrid");

            //_boundPropBags = ViewModelGenerator.StandUpViewModels(topGrid, this);


            ReferenceBindViewModelPB rbvm = (ReferenceBindViewModelPB)this.DataContext;

            MyModel4 mod4 = new MyModel4() { MyString = "Start" };
            rbvm.SetIt<MyModel4>(mod4, "Deep");

            //DefineMapingKeys("ReferenceBindViewModelPB");
            //DefineMappers();
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            ReferenceBindViewModelPB vm = (ReferenceBindViewModelPB)this.DataContext;

            MyModel mm = new MyModel
            {
                ProductId = Guid.NewGuid(),
                Amount = 38,
                Size = 20.02
            };

            ReadWithMap(mm, vm);

            if (vm == null)
            {
                vm = GetNewViewModel();
                this.DataContext = vm;
            }
        }

        private void BtnRead_Click_OLD(object sender, RoutedEventArgs e)
        {
            //CollectionViewSource aa = new CollectionViewSource();
            ReferenceBindViewModelPB vm = (ReferenceBindViewModelPB)this.DataContext;

            if (vm == null)
            {
                vm = GetNewViewModel();
                this.DataContext = vm;
            }

            vm.SetIt<int>(21, "Amount");
            int tttt = vm.GetIt<int>("Amount");
            //m.Amount = 21;

            vm.SetIt<double>(30.3, "Size");
            //m.Size = 30.3;

            vm.SetIt<Guid>(Guid.NewGuid(), "ProductId");
            //m.ProductId = Guid.NewGuid();

            vm.SetIt<double>(0.01, "TestDouble");
            //m.TestDouble = 0.01;

            MyModel4 r = vm.GetIt<MyModel4>("Deep");
            if (r != null)
            {
                r.MyString = "Jacob2";
            }
            else
            {
                MyModel4 mod4 = new MyModel4() { MyString = "Jacob" };
                vm.SetIt<MyModel4>(mod4, "Deep");
            }

        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ReferenceBindViewModelPB m = (ReferenceBindViewModelPB)this.DataContext;
            if(m == null)
            {
                System.Diagnostics.Debug.WriteLine("The DataContext is null.");
                return;
            }

            int amount = m.GetIt<int>("Amount");
            double size = m.GetIt<double>("Size");
            Guid productId = m.GetIt<Guid>("ProductId");
            double testDouble = m.GetIt<double>("TestDouble");
            string myString = m.GetIt<MyModel4>("Deep")?.MyString;

            System.Diagnostics.Debug.WriteLine($"Amount = {amount}.");
            System.Diagnostics.Debug.WriteLine($"Size = {size}.");
            System.Diagnostics.Debug.WriteLine($"ProductId = {productId}.");
            System.Diagnostics.Debug.WriteLine($"TestDouble = {testDouble}.");
            System.Diagnostics.Debug.WriteLine($"Deep.MyString = {myString}.");
        }

        private void BtnRemoveDc_Click(object sender, RoutedEventArgs e)
        {
            if(this.DataContext == null)
            {
                this.DataContext = GetNewViewModel();
            }
            else
            {
                this.DataContext = null;
            }
        }

        private void ReadWithMap(MyModel mm, ReferenceBindViewModelPB vm)
        {
            //var mapper = (PropBagMapperCustom<MyModel, ReferenceBindViewModelPB>)_autoMappers.GetMapperToUse(_mapperKey);

            var mapper = Mapper;

            ReferenceBindViewModelPB tt = (ReferenceBindViewModelPB)mapper.MapToDestination(mm, vm);

            // Now try creating a new one from mm.
            ReferenceBindViewModelPB test = (ReferenceBindViewModelPB)mapper.MapToDestination(mm);
        }

        private readonly string PERSON_VM_INSTANCE_KEY = "ReferenceBindViewModelPB";

        private IPropBagMapper<MyModel, ReferenceBindViewModelPB> _mapper;
        private IPropBagMapper<MyModel, ReferenceBindViewModelPB> Mapper
        {
            get
            {
                if (_mapper == null)
                {
                    _mapper = _autoMapperProvider.GetMapper<MyModel, ReferenceBindViewModelPB>(PERSON_VM_INSTANCE_KEY);
                }
                return _mapper;
            }
        }

        //private void DefineMappers()
        //{
        //    _autoMappers.Register(_mapperKey);
        //}

        //private void DefineMapingKeys(string instanceKey)
        //{
        //    BoundPropBag boundPB = _boundPropBags[instanceKey];

        //    _mapperKey
        //        = new PropBagMapperKey<MyModel, ReferenceBindViewModelPB>
        //        (boundPB.PropModel, boundPB.RtViewModelType, mappingStrategy: PropBagMappingStrategyEnum.ExtraMembers);

        //    //_mapperKey2
        //    //    = new PropBagMapperKey<MyModel2, ReferenceBindViewModelPB>
        //    //    (boundPB.PropModel, boundPB.RtViewModelType, mappingStrategy: PropBagMappingStrategyEnum.ExtraMembers);
        //}

        //private ConfiguredMappers GetAutoMappers(PropBagMappingStrategyEnum mappingStrategy)
        //{
        //    Func<Action<IMapperConfigurationExpression>, IConfigurationProvider> configBuilder
        //        = new MapperConfigurationProvider().BaseConfigBuilder;

        //    MapperConfigInitializerProvider mapperConfigExpression
        //        = new MapperConfigInitializerProvider(mappingStrategy);

        //    ConfiguredMappers result = new ConfiguredMappers(configBuilder, mapperConfigExpression);

        //    return result;
        //}

        private ReferenceBindViewModelPB GetNewViewModel()
        {
            PropModel pm = _propModelProvider.GetPropModel("ReferenceBindViewModelPB");
            IPropFactory pf = SettingsExtensions.ThePropFactory;

            ReferenceBindViewModelPB rbvm = new ReferenceBindViewModelPB(pm, pf);

            MyModel4 mod4 = new MyModel4() { MyString = "Start" };
            rbvm.SetIt<MyModel4>(mod4, "Deep");

            return rbvm;
        }

        private AutoMapperProvider InitializeAutoMappers(IPropModelProvider propModelProvider)
        {
            PropBagMappingStrategyEnum mappingStrategy = PropBagMappingStrategyEnum.ExtraMembers;

            Func<Action<IMapperConfigurationExpression>, IConfigurationProvider> configBuilder
                = new MapperConfigurationProvider().BaseConfigBuilder;

            MapperConfigInitializerProvider mapperConfigExpression = new MapperConfigInitializerProvider(mappingStrategy);

            ConfiguredMappers configuredMappers = new ConfiguredMappers(configBuilder, mapperConfigExpression);



            TypeDescriptionProvider typeDescriptionProvider = new TypeDescriptionProvider();

            IModuleBuilderInfoProvider x = new DefaultModuleBuilderInfoProvider();
            IModuleBuilderInfo mbi = x.ModuleBuilderInfo;

            AutoMapperProvider autoMapperProvider = new AutoMapperProvider(mappingStrategy, propModelProvider,
                configuredMappers, mbi, typeDescriptionProvider);

            return autoMapperProvider;
        }
    }




}
