using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using DRM.PropBag;
using DRM.PropBag.ControlModel;

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
        public void destroy()
        {
            mod1 = null;
        }


        [Test]
        public void Test1()
        {

            PropModel pm = new PropModel("CreateAtRunTimeModel", "PropBagLib.Tests", deriveFromPubPropBag: false, typeSafetyMode: PropBagTypeSafetyMode.AllPropsMustBeRegistered, deferMethodRefResolution: true);

            PropItem pi = new PropItem(typeof(string), "PropString", null, true, true, new PropInitialValueField("Initial Value"), null, null);

            pm.Props.Add(pi);

            mod1 = new CreateAtRunTimeModel(pm);

            Assert.That(mod1, Is.Not.EqualTo(null), "Expected the CreateAtRunTimeModel to have been created.");

            Assert.That(mod1.PropertyExists("PropString"), Is.True, "Expected the property with name = 'PropString' to have been create.");


        }

    }
}
