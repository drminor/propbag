using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    [TestFixtureAttribute]
    public class TestAutoMapperBasic
    {
        IProvideAutoMappers _amp;
        IPropFactory _propFactory_V1;

        [OneTimeSetUp]
        public void SetupAutoMapperSupport_V1()
        {
            AutoMapperHelpers ourHelper = new AutoMapperHelpers();

            _propFactory_V1 = ourHelper.PropFactory_V1;
            _amp = ourHelper.GetAutoMapperSetup_V1();
        }

        [TearDown]
        public void Destroy()
        {
        }

        [Test]
        public void AutoMapperSupport_V1_ShouldBeSetup()
        {
            bool doesAmpHavePbTConversionServices = _amp.HasPropModelLookupService;
        }

        [Test]
        public void CanRegisterMod3ToDestinationMapper_Proxy()
        {
            IPropFactory propFactory = _propFactory_V1;

            PropModel propModel = GetPropModelForModel3Dest(propFactory);
            Type typeToWrap = typeof(PropBag);
            string configPackageName = "Emit_Proxy";

            IPropBagMapperKey<MyModel3, DestinationModel3> mapperRequest =
                _amp.RegisterMapperRequest<MyModel3, DestinationModel3>
                (
                    propModel: propModel,
                    typeToWrap: typeToWrap,
                    configPackageName: configPackageName
                );

            Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");
        }

        [Test]
        public void CanGetMapperForMod3ToDestination_Proxy()
        {
            IPropFactory propFactory = _propFactory_V1;

            PropModel propModel = GetPropModelForModel3Dest(propFactory);
            Type typeToWrap = typeof(PropBag);
            string configPackageName = "Emit_Proxy";

            IPropBagMapperKey<MyModel3, DestinationModel3> mapperRequest =
                _amp.RegisterMapperRequest<MyModel3, DestinationModel3>
                (
                    propModel: propModel,
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
                _amp.RegisterMapperRequest<MyModel3, DestinationModel3>
                (
                    propModel: propModel,
                    typeToWrap: typeToWrap,
                    configPackageName: configPackageName
                );

            IPropBagMapper<MyModel3, DestinationModel3> mapper2 = _amp.GetMapper<MyModel3, DestinationModel3>(mapperRequest2);

            DestinationModel3 testDest2 = mapper.MapToDestination(testSource);

        }

        [Test]
        public void CanGetMapperForMod3ToDestination_Extra()
        {
            IPropFactory propFactory = _propFactory_V1;

            PropModel propModel = GetPropModelForModel3Dest(propFactory);
            Type typeToWrap = typeof(DestinationModel3); // typeof(PropBag);
            string configPackageName = "Extra_Members"; // "Emit_Proxy";

            IPropBagMapperKey<MyModel3, DestinationModel3> mapperRequest =
                _amp.RegisterMapperRequest<MyModel3, DestinationModel3>
                (
                    propModel: propModel,
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
                _amp.RegisterMapperRequest<MyModel3, DestinationModel3>
                (
                    propModel: propModel,
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
                deriveFrom: DeriveFromClassModeEnum.PropBag,
                targetType: null,
                propStoreServiceProviderType: null,
                propFactory: null,
                typeSafetyMode: PropBagTypeSafetyMode.Tight,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true);

            //result.Namespaces.Add("System");
            result.Namespaces.Add("DRM.PropBag");
            result.Namespaces.Add("DRM.TypeSafePropertyBag");

            result.PropFactory = propFactory;

            // ProductId (Guid - default)
            PropInitialValueField pivf = new PropInitialValueField(initialValue: null,
                setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);

            PropItemModel propItem = new PropItemModel(type: typeof(Guid), name: "ProductId",
                storageStrategy: PropStorageStrategyEnum.Internal, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            // Amount (int - default)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);

            propItem = new PropItemModel(type: typeof(int), name: "Amount",
                storageStrategy: PropStorageStrategyEnum.Internal, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);


            // Size (double - default)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);

            propItem = new PropItemModel(type: typeof(double), name: "Size",
                storageStrategy: PropStorageStrategyEnum.Internal, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            // Deep (MyModel4 - null)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            propItem = new PropItemModel(type: typeof(MyModel4), name: "Deep",
                storageStrategy: PropStorageStrategyEnum.Internal, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            return result;
        }

        #endregion

        #region GET SIZE

        private void GetSizeX()
        {
            long StopBytes = 0;
            object myFoo;

            long StartBytes = System.GC.GetTotalMemory(true);
            myFoo = new object();
            StopBytes = System.GC.GetTotalMemory(true);

            string result = "Size is " + ((long)(StopBytes - StartBytes)).ToString();

            GC.KeepAlive(myFoo); // This ensure a reference to object keeps object in memory
        }

        private void GetSizeY()
        {

            //long size = 0;
            //object o = new object();
            //using (Stream s = new MemoryStream())
            //{
            //    BinaryFormatter formatter = new BinaryFormatter();
            //    formatter.Serialize(s, o);
            //    size = s.Length;
            //}
        }

        #endregion


    }
}
