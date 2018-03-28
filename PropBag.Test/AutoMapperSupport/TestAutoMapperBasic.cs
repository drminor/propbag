using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    using PropModelCacheInterface = ICachePropModels<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    [TestFixture]
    public class TestAutoMapperBasic
    {
        IAutoMapperService _amp;
        IPropFactory _propFactory_V1;
        ICreateWrapperTypes _wrapperTypeCreator_V1;
        ViewModelFactoryInterface _viewModelFactory;

        [OneTimeSetUp]
        public void SetupAutoMapperSupport_V1()
        {
            AutoMapperHelpers ourHelper = new AutoMapperHelpers();

            _propFactory_V1 = ourHelper.PropFactory_V1;
            _amp = ourHelper.GetAutoMapperSetup_V1();
            _wrapperTypeCreator_V1 = ourHelper.GetWrapperTypeCreator_V1();
            PropModelCacheInterface _propModelCache = ourHelper.GetPropModelCache_V1();

            _viewModelFactory = ourHelper.ViewModelFactory;
        }

        [TearDown]
        public void Destroy()
        {
        }

        [Test]
        public void AutoMapperSupport_V1_ShouldBeSetup()
        {
            Assert.That(_amp, Is.Not.Null, "AutoMapper Support was not Setup.");
        }

        [Test]
        public void CanRegisterMod3ToDestinationMapper_Proxy()
        {
            IPropFactory propFactory = _propFactory_V1;
            ViewModelFactoryInterface viewModelFactory = _viewModelFactory;

            PropModel propModel = GetPropModelForModel3Dest(propFactory);
            Type typeToWrap = typeof(PropBag);
            string configPackageName = "Emit_Proxy";

            IPropBagMapperKey<MyModel3, DestinationModel3> mapperRequest =
                _amp.SubmitMapperRequest<MyModel3, DestinationModel3>
                (
                    propModel: propModel,
                    viewModelFactory: viewModelFactory,
                    typeToWrap: typeToWrap,
                    configPackageName: configPackageName
                );

            Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");
        }

        [Test]
        public void CanGetMapperForMod3ToDestination_Proxy()
        {
            IPropFactory propFactory = _propFactory_V1;
            ViewModelFactoryInterface viewModelFactory = _viewModelFactory;


            PropModel propModel = GetPropModelForModel3Dest(propFactory);

            viewModelFactory.PropModelCache.Add(propModel);


            Type typeToWrap = typeof(PropBag);
            string configPackageName = "Emit_Proxy";

            IMapperRequest mapperRequest = new MapperRequest(typeof(MyModel3), propModel, configPackageName);

            Type et = _wrapperTypeCreator_V1.GetWrapperType(propModel, typeToWrap);

            propModel.NewEmittedType = et;

            IPropBagMapperKeyGen mapperKey = _amp.SubmitMapperRequest(mapperRequest.PropModel, _viewModelFactory,  mapperRequest.SourceType, mapperRequest.ConfigPackageName, _amp);

            // Get the AutoMapper mapping function associated with the mapper request already submitted.
            IPropBagMapperGen genMapper = _amp.GetMapper(mapperKey);


            //IPropBagMapperKey<MyModel3, DestinationModel3> mapperRequest =
            //    _amp.SubmitMapperRequest<MyModel3, DestinationModel3>
            //    (
            //        propModel: propModel,
            //        typeToWrap: et, //typeToWrap
            //        configPackageName: configPackageName
            //    );

            Assert.That(mapperKey, Is.Not.Null, "mapperRequest should be non-null.");

            //IPropBagMapper<MyModel3, DestinationModel3> mapper = _amp.GetMapper<MyModel3, DestinationModel3>(mapperRequest);

            Assert.That(genMapper, Is.Not.Null, "mapper should be non-null");

            MyModel4 dp = new MyModel4
            {
                MyString = "This is a good thing."
            };

            MyModel3 testSource = new MyModel3
            {
                Amount = 11,
                Size = 22.22,
                ProductId = Guid.Empty,
                Deep = dp
            };


            var testDest = genMapper.MapToDestination(testSource);

            //IPropBagMapperKey<MyModel3, DestinationModel3> mapperRequest2 =
            //    _amp.SubmitMapperRequest<MyModel3, DestinationModel3>
            //    (
            //        propModel: propModel,
            //        typeToWrap: typeToWrap,
            //        configPackageName: configPackageName
            //    );

            //IPropBagMapper<MyModel3, DestinationModel3> mapper2 = _amp.GetMapper<MyModel3, DestinationModel3>(mapperRequest2);

            //DestinationModel3 testDest2 = mapper.MapToDestination(testSource);

        }

        [Test]
        public void CanGetMapperForMod3ToDestination_Extra()
        {
            IPropFactory propFactory = _propFactory_V1;
            ViewModelFactoryInterface viewModelFactory = _viewModelFactory;



            PropModel propModel = GetPropModelForModel3Dest(propFactory);
            viewModelFactory.PropModelCache.Add(propModel);

            Type typeToWrap = typeof(DestinationModel3); // typeof(PropBag);
            string configPackageName = "Extra_Members"; // "Emit_Proxy";

            IPropBagMapperKey<MyModel3, DestinationModel3> mapperRequest =
                _amp.SubmitMapperRequest<MyModel3, DestinationModel3>
                (
                    propModel: propModel,
                    viewModelFactory: viewModelFactory,
                    typeToWrap: typeToWrap,
                    configPackageName: configPackageName
                );

            Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");

            IPropBagMapper<MyModel3, DestinationModel3> mapper = _amp.GetMapper<MyModel3, DestinationModel3>(mapperRequest);

            Assert.That(mapper, Is.Not.Null, "mapper should be non-null");

            MyModel4 dp = new MyModel4
            {
                MyString = "This is a good thing."
            };

            MyModel3 testSource = new MyModel3
            {
                Amount = 11,
                Size = 22.22,
                ProductId = Guid.Empty,
                Deep = dp
            };


            DestinationModel3 testDest = mapper.MapToDestination(testSource);

            IPropBagMapperKey<MyModel3, DestinationModel3> mapperRequest2 =
                _amp.SubmitMapperRequest<MyModel3, DestinationModel3>
                (
                    propModel: propModel,
                    viewModelFactory: viewModelFactory,
                    typeToWrap: typeToWrap,
                    configPackageName: configPackageName
                );

            IPropBagMapper<MyModel3, DestinationModel3> mapper2 = _amp.GetMapper<MyModel3, DestinationModel3>(mapperRequest2);

            DestinationModel3 testDest2 = mapper.MapToDestination(testSource);
        }

        #region Private Support Methods

        private PropModel GetPropModelForModel3Dest(IPropFactory propFactory)
        {

            PropModel result = new PropModel
                (
                className: "DestinationModel3",
                namespaceName: "PropBagLib.Tests.AutoMapperSupport",
                deriveFrom: DeriveFromClassModeEnum.Custom,
                targetType: typeof(DestinationModel3),
                propFactory: propFactory,
                propFactoryType: null,
                propModelCache: null,
                typeSafetyMode: PropBagTypeSafetyMode.Tight,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true
                );

            //result.Namespaces.Add("System");
            result.Namespaces.Add("DRM.PropBag");
            result.Namespaces.Add("DRM.TypeSafePropertyBag");

            // ProductId (Guid - default)
            IPropInitialValueField pivf = PropInitialValueField.UseDefault;

            PropItemModel propItem = new PropItemModel(type: typeof(Guid), name: "ProductId",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            // Amount (int - default)
            propItem = new PropItemModel(type: typeof(int), name: "Amount",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);


            // Size (double - default)
            propItem = new PropItemModel(type: typeof(double), name: "Size",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            // Deep (MyModel4 - null)
            pivf = PropInitialValueField.UseNull;

            propItem = new PropItemModel(type: typeof(MyModel4), name: "Deep",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            return result;
        }

        #endregion
    }
}
