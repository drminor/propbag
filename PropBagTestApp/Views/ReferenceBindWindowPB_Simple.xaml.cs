using AutoMapper;
using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using PropBagTestApp.Infra;
using PropBagTestApp.Models;
using PropBagTestApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PropBagTestApp.View
{
    using PropModelType = IPropModel<String>;
    using PropModelCacheInterface = ICachePropModels<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    /// <summary>
    /// Interaction logic for ReferenceBindWindowPB_Simple.xaml
    /// </summary>
    public partial class ReferenceBindWindowPB_Simple : Window
    {

        public ReferenceBindWindowPB_Simple()
        {
            _mapper = null;

            InitializeComponent();

            ReferenceBindViewModelPB rbvm = (ReferenceBindViewModelPB)this.DataContext;

            MyModel4 mod4 = new MyModel4() { MyString = "Start" };
            rbvm.SetIt<MyModel4>(mod4, "Deep");
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
            var mapper = Mapper; 

            ReferenceBindViewModelPB tt = (ReferenceBindViewModelPB)mapper.MapToDestination(mm, vm);

            // Now try creating a new one from mm.
            ReferenceBindViewModelPB test = (ReferenceBindViewModelPB)mapper.MapToDestination(mm);
        }

        private readonly string REFERENCE_BIND_VM_RES_KEY = "ReferenceBindViewModelPB";

        private IPropBagMapper<MyModel, ReferenceBindViewModelPB> _mapper;
        private IPropBagMapper<MyModel, ReferenceBindViewModelPB> Mapper
        {
            get
            {
                if (_mapper == null)
                {
                    PropModelCacheInterface propModelCache = PropStoreServicesForThisApp.PropModelCache;

                    string fcn = REFERENCE_BIND_VM_RES_KEY;
                    if (propModelCache.TryGetPropModel(fcn, out PropModelType propModel))
                    {
                        throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fcn}.");
                    }

                    IPropBagMapperService propBagMapperService = PropStoreServicesForThisApp.PropBagMapperService;

                    IMapperRequest mr = new MapperRequest(typeof(MyModel), propModel, "emit_proxy");

                    IPropBagMapper<MyModel, ReferenceBindViewModelPB> propBagMapper = PropStoreServicesForThisApp.GetAutoMapper<MyModel, ReferenceBindViewModelPB>
                        (
                        mr,
                        propBagMapperService,
                        out IPropBagMapperKey<MyModel, ReferenceBindViewModelPB> propBagMapperKey
                        );

                    //IPropBagMapperKey<MyModel, ReferenceBindViewModelPB> mapperRequest
                    //    = PropStoreServicesForThisApp.AutoMapperService.SubmitRawAutoMapperRequest<MyModel, ReferenceBindViewModelPB>
                    //    (
                    //    propModel,
                    //    //viewModelFactory,
                    //    typeof(ReferenceBindViewModelPB),
                    //    configPackageName: "emit_proxy"
                    //    );



                    //_mapper = PropStoreServicesForThisApp.AutoMapperService.GetMapper(mapperRequest);

                    _mapper = propBagMapper;
                }
                return _mapper;
            }
        }

        //private IMapper GetAutoMapper<TSource, TDestination>
        //    (
        //    IMapperRequest mapperRequest,
        //    IAutoMapperService autoMapperService,
        //    out IPropBagMapperKey<TSource, TDestination> propBagMapperKey
        //    )
        //    where TDestination : class, IPropBag
        //{
        //    // This is where the PropModel is used to define the Mapper 

        //    // TODO: See if we can submit the request earlier; perhaps when the mapper request is created.

        //    Type typeToWrap = mapperRequest.PropModel.TypeToWrap;

        //    // Submit the Mapper Request.
        //    propBagMapperKey = autoMapperService.SubmitRawAutoMapperRequest<TSource, TDestination>
        //        (mapperRequest.PropModel/*, viewModelFactory*/, typeToWrap, mapperRequest.ConfigPackageName);

        //    // Get the AutoMapper mapping function associated with the mapper request just submitted.
        //    //IPropBagMapperGen genMapper = _autoMapperService.GetMapper(mapperKey);

        //    IMapper rawAutoMapper = autoMapperService.GetRawAutoMapper(propBagMapperKey);
        //    return rawAutoMapper;
        //}



    }
}
