﻿using DRM.PropBag;
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
            IPropFactory propFactory = _propFactory_V1;

            PropModel propModel = GetPropModelForDestinationModel(propFactory);
            Type typeToWrap = typeof(PropBag);
            string configPackageName = "Emit_Proxy";

            IPropBagMapperKey<MyModel3, DestinationModel> mapperRequest =
                _amp.RegisterMapperRequest<MyModel3, DestinationModel>
                (
                    propModel: propModel,
                    typeToWrap: typeToWrap,
                    configPackageName: configPackageName
                );

            Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");
        }

        [Test]
        public void CanGetMapperForMod3ToDestination()
        {
            IPropFactory propFactory = _propFactory_V1;

            PropModel propModel = GetPropModelForDestinationModel(propFactory);
            Type typeToWrap = typeof(PropBag);
            string configPackageName = "Emit_Proxy";

            IPropBagMapperKey<MyModel3, DestinationModel> mapperRequest =
                _amp.RegisterMapperRequest<MyModel3, DestinationModel>
                (
                    propModel: propModel,
                    typeToWrap: typeToWrap,
                    configPackageName: configPackageName
                );

            Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");

            IPropBagMapper<MyModel3, DestinationModel> mapper = _amp.GetMapper<MyModel3, DestinationModel>(mapperRequest);

            Assert.That(mapper, Is.Not.Null, "mapper should be non-null");


        }


        #region Private Support Methods

        private PropModel GetPropModelForDestinationModel(IPropFactory propFactory)
        {
            PropModel result = new PropModel(className: "DestinationModel", instanceKey: "DestinationModel",
                namespaceName: "DummyNamespace",
                propFactory: propFactory,
                deriveFromPubPropBag: false, 
                typeSafetyMode: PropBagTypeSafetyMode.Tight, 
                deferMethodRefResolution: false, requireExplicitInitialValue: true);

            //result.Namespaces.Add("System");
            result.Namespaces.Add("DRM.PropBag");
            result.Namespaces.Add("DRM.TypeSafePropertyBag");

            result.PropFactory = propFactory;

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
            result.Props.Add(propItem);


            // Size (double - default)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);

            propItem = new PropItem(type: typeof(double), name: "Size",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                doWhenChanged: null, extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            // Deep (MyModel4 - null)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            propItem = new PropItem(type: typeof(MyModel4), name: "Deep",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                doWhenChanged: null, extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            return result;
        }

        #endregion

    }
}
