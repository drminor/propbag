using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using System;

namespace PropBagLib.Tests.AutoMapperSupport
{
    [TestFixtureAttribute]
    public class TestAutoMapperBasic
    {
        AutoMapperProvider _amp;
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
            bool doesAmpHavePbTConversionServices = _amp.HasPbTConversionService;
        }

        [Test]
        public void CanRegisterMod3ToDestinationMapper()
        {
            PropModel propModel = GetPropModelForDestinationModel();
            Type typeToWrap = typeof(PropBag);
            IPropFactory propFactory = _propFactory_V1;
            string configPackageName = "Emit_Proxy";

            IPropBagMapperKey<MyModel3, DestinationModel> mapperRequest =
                _amp.RegisterMapperRequest<MyModel3, DestinationModel>
                (
                    propModel: propModel,
                    typeToWrap: typeToWrap,
                    propFactory: propFactory,
                    configPackageName: configPackageName
                    );

            Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");
        }



        private PropModel GetPropModelForDestinationModel()
        {
            PropModel result = new PropModel(className: "DestinationModel", instanceKey: "DestinationModel",
                namespaceName: "DummyNamespace", deriveFromPubPropBag: false, 
                typeSafetyMode: PropBagTypeSafetyMode.Tight, 
                deferMethodRefResolution: false, requireExplicitInitialValue: true);

            // ProductId (Guid - default)
            PropInitialValueField pivf = new PropInitialValueField(initialValue: null,
                setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);

            PropItem propItem = new PropItem(type: typeof(Guid), name: "ProductId",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                doWhenChanged: null, extraInfo: null, comparer: null, itemType: null);

            result.Props.Add(propItem);

            // Amount (int - default)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);

            propItem = new PropItem(type: typeof(int), name: "Amount",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                doWhenChanged: null, extraInfo: null, comparer: null, itemType: null);

            // Size (double - default)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);

            propItem = new PropItem(type: typeof(double), name: "Size",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                doWhenChanged: null, extraInfo: null, comparer: null, itemType: null);

            // Deep (MyModel4 - null)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            propItem = new PropItem(type: typeof(MyModel4), name: "Deep",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                doWhenChanged: null, extraInfo: null, comparer: null, itemType: null);

            return result;
        }


    }
}
