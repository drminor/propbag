
using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using PropBagLib.Tests.AutoMapperSupport;

namespace PropBagLib.Tests
{
    [TestFixture]
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
            AutoMapperHelpers ourHelper = new AutoMapperHelpers();
            IPropFactory propFactory_V1 = ourHelper.GetNewPropFactory_V1();

            PropModel pm = new PropModel
                (
                className: "CreateAtRunTimeModel",
                namespaceName: "PropBagLib.Tests",
                deriveFrom: DeriveFromClassModeEnum.PropBag,
                targetType: null,
                propFactory: propFactory_V1,
                propFactoryType: null,
                propModelCache: null,
                typeSafetyMode: PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true);

            PropItemModel pi = new PropItemModel(typeof(string), "PropString",
                PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop, initialValueField: new PropInitialValueField("Initial Value"));

            pm.Add(pi.PropertyName, pi);


            mod1 = new CreateAtRunTimeModel(pm, ourHelper.StoreAccessCreator);

            Assert.That(mod1, Is.Not.EqualTo(null), "Expected the CreateAtRunTimeModel to have been created.");

            Assert.That(mod1.PropertyExists("PropString"), Is.True, "Expected the property with name = 'PropString' to have been create.");


        }

    }
}
