
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using NUnit.Framework;

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
                typeToWrap: null, 
                propFactory: null,
                typeSafetyMode: PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true);

            PropItem pi = new PropItem(typeof(string), "PropString", true, true, PropKindEnum.Prop, null, new PropInitialValueField("Initial Value"), null, null, null);

            pm.Props.Add(pi);

            mod1 = new CreateAtRunTimeModel(pm);

            Assert.That(mod1, Is.Not.EqualTo(null), "Expected the CreateAtRunTimeModel to have been created.");

            Assert.That(mod1.PropertyExists("PropString"), Is.True, "Expected the property with name = 'PropString' to have been create.");


        }

    }
}
