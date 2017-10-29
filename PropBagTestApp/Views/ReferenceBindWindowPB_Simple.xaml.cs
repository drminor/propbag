using AutoMapper;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ControlModel;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.ViewModelBuilder;
using PropBagTestApp.Models;
using PropBagTestApp.ViewModels;
using System;
using System.Windows;


namespace PropBagTestApp.View
{
    /// <summary>
    /// Interaction logic for ReferenceBindWindowPB_Simple.xaml
    /// </summary>
    public partial class ReferenceBindWindowPB_Simple : Window
    {

        IPropModelProvider _propModelProvider;
        //AutoMapperProvider _autoMapperProvider;

        //Dictionary<string, BoundPropBag> _boundPropBags;
        //PropBagMapperKey<MyModel, ReferenceBindViewModelPB> _mapperKey;
        ////PropBagMapperKey<MyModel2, DtoTestViewModelExtra> _mapperKey2;


        public ReferenceBindWindowPB_Simple()
        {
            IPropBagTemplateProvider propBagTemplateProvider = new PropBagTemplateProvider(Application.Current.Resources);
            _propModelProvider = new PropModelProvider(propBagTemplateProvider);

            //_autoMapperProvider = InitializeAutoMappers(_propModelProvider);
            _mapper = null;

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

            //if (vm == null)
            //{
            //    vm = GetNewViewModel();
            //    this.DataContext = vm;
            //}
        }

        private void BtnRead_Click_OLD(object sender, RoutedEventArgs e)
        {
            //CollectionViewSource aa = new CollectionViewSource();
            //ReferenceBindViewModelPB vm = (ReferenceBindViewModelPB)this.DataContext;

            //if (vm == null)
            //{
            //    vm = GetNewViewModel();
            //    this.DataContext = vm;
            //}

            //vm.SetIt<int>(21, "Amount");
            //int tttt = vm.GetIt<int>("Amount");
            ////m.Amount = 21;

            //vm.SetIt<double>(30.3, "Size");
            ////m.Size = 30.3;

            //vm.SetIt<Guid>(Guid.NewGuid(), "ProductId");
            ////m.ProductId = Guid.NewGuid();

            //vm.SetIt<double>(0.01, "TestDouble");
            ////m.TestDouble = 0.01;

            //MyModel4 r = vm.GetIt<MyModel4>("Deep");
            //if (r != null)
            //{
            //    r.MyString = "Jacob2";
            //}
            //else
            //{
            //    MyModel4 mod4 = new MyModel4() { MyString = "Jacob" };
            //    vm.SetIt<MyModel4>(mod4, "Deep");
            //}

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
                //this.DataContext = GetNewViewModel();
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
                    //_mapper = _autoMapperProvider.GetMapper<MyModel, ReferenceBindViewModelPB>(PERSON_VM_INSTANCE_KEY);
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

        //private ReferenceBindViewModelPB GetNewViewModel()
        //{
        //    //PropModel pm = _propModelProvider.GetPropModel("ReferenceBindViewModelPB");
        //    //IPropFactory pf = SettingsExtensions.ThePropFactory;

        //    //ReferenceBindViewModelPB rbvm = new ReferenceBindViewModelPB(pm, pf);

        //    //MyModel4 mod4 = new MyModel4() { MyString = "Start" };
        //    //rbvm.SetIt<MyModel4>(mod4, "Deep");

        //    //return rbvm;
        //}

        //private AutoMapperProvider InitializeAutoMappers(IPropModelProvider propModelProvider)
        //{
        //    // Select one of a few well-know strategies for 
        //    // 1. Createing new instances of IPropBag-based ViewModels
        //    // 2. It is used by AutoMapper support to inform how to
        //    //      a. Create new target instances,
        //    //      b. How to get Type info.
        //    //      c. How to create code that does the mapping.
        //    PropBagMappingStrategyEnum mappingStrategy = PropBagMappingStrategyEnum.ExtraMembers;

        //    // This allows us to keep a library of base Mapper configurations.
        //    Func<Action<IMapperConfigurationExpression>, IConfigurationProvider> configBuilder
        //        = new MapperConfigurationProvider().BaseConfigBuilder;

        //    // This is provided by the caller for a particular mapping application.
        //    MapperConfigInitializerProvider mapperConfigExpression = new MapperConfigInitializerProvider(mappingStrategy);

        //    // Provides a service to locate, cache mappers -- thereby optimizing the resources provided by and consumed by AutoMapping.
        //    ConfiguredMappers configuredMappers = new ConfiguredMappers(configBuilder, mapperConfigExpression);


        //    // Used only by some ModuleBuilders.
        //    TypeDescriptionProvider typeDescriptionProvider = new TypeDescriptionProvider();

        //    // Used by some ViewModel Activators to emit types, i.e., modules.
        //    IModuleBuilderInfoProvider x = new DefaultModuleBuilderInfoProvider();
        //    IModuleBuilderInfo mbi = x.ModuleBuilderInfo;

        //    // One big ball of AutoMapping services.
        //    AutoMapperProvider autoMapperProvider = new AutoMapperProvider(mappingStrategy, propModelProvider,
        //        configuredMappers, mbi, typeDescriptionProvider);

        //    //-------------------------

        //    // Other Dependencies that could be managed, but not part of AutoMapper, per say.
        //    // 1. Type Converter Cache used by most, if not all IPropFactory.
        //    // 2. DoSetDelegate Cache -- basically baked into IPropBag -- not many different options
        //    // 3. PropCreation Delegate Cache --  only used by IPropFactory this is not critical now, but could become so 
        //    // 4. Event Listeners that could managed better if done as a central service -- used by PropBag and the Binding engine.

        //    //------------------------ 

        //    //******************

        //    // Services that we need to focus on now.
        //    // 1. ViewModel creation 
        //    // 2. PropFactory boot strapping
        //    // 3. Proxy Model creation (we should be able to use 95% of the same services as those provided for ViewModel
        //    //          creation, in fact, ProxModel creation is probably the driver and ViewModel can benefit from
        //    //          novel techniqes explored here.
        //    // 4. Creating a language for ViewModel configuration.
        //    // 5. Creating services that allow for data flow behavior to be declared and executed without having
        //    //          to write code.
        //    // 6. Creating ViewModel sinks for data coming from the View dynamically, ReactiveUI has a 
        //    //          a way of doing this from the ViewModel to the View, can we build a facility to allow the reverse?
        //    // 
        //    // 7. Allowing the View to affect the behavior of the ViewModel dynamically.
        //    // 8. Design-time support including AutoMapper mapping configuration and testing.
        //    //******************

        //    // +++++++++++++++++++

        //    // Other services that should be addressed
        //    // 1. Building TypeDescriptors / Type Descriptions / PropertyInfo / Custom MetaData for reflection.
        //    // 2. XML Serialization services for saving / hydrating IPropBag objects.
        //    // 
        //    // +++++++++++++++++++

        //    return autoMapperProvider;
        //}
    }




}
