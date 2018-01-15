
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using PropBagLib.Tests.AutoMapperSupport;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestCreateAtRunTime
    {
        public CreateAtRunTimeModel mod1;

        [SetUp]
        public void Create()
        {
        }

        [TearDown]
        public void Destroy()
        {
            mod1 = null;
        }


        [Test]
        public void Test1()
        {

            PropModel pm = new PropModel
                (
                className: "CreateAtRunTimeModel",
                namespaceName: "PropBagLib.Tests",
                deriveFrom: DeriveFromClassModeEnum.PropBag,
                targetType: null,
                propStoreServiceProviderType: null,
                propFactory: null,
                typeSafetyMode: PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true);

            PropItem pi = new PropItem(typeof(string), "PropString", PropStorageStrategyEnum.Internal, true, PropKindEnum.Prop, null, new PropInitialValueField("Initial Value"), null, null, null);

            pm.Props.Add(pi);

            AutoMapperHelpers ourHelper = new AutoMapperHelpers();
            IPropFactory propFactory_V1 = ourHelper.GetNewPropFactory_V1();

            // TODO: AAA
            mod1 = new CreateAtRunTimeModel(pm, ourHelper.StoreAccessCreator, propFactory_V1);

            Assert.That(mod1, Is.Not.EqualTo(null), "Expected the CreateAtRunTimeModel to have been created.");

            Assert.That(mod1.PropertyExists("PropString"), Is.True, "Expected the property with name = 'PropString' to have been create.");


        }

    }
}
