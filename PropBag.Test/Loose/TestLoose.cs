using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.Inpcwv;
using DRM.PropBag;

using NUnit.Framework;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestLoose
    {

        LooseModel mod1;

        const string PROP_NEW = "NewProp";

        private bool propString_WasUpdated;
        //private string propStringOldVal;
        //private string propStringNewVal;
        //private bool doWhenStringChanged_WasCalled;

        #region Setup and TearDown

        [OneTimeSetUp]
        public void Create()
        {
        }

        [OneTimeTearDown]
        public void destroy()
        {
            mod1 = null;
        }

        #endregion

        #region Tests

        [Test]
        public void ShouldUpdateBool()
        {
            mod1 = new LooseModel(PropBagTypeSafetyMode.Loose);

            mod1.PropBool = true;
            Assert.That(mod1.PropBool, Is.True, "PropBool did not get updated.");
        }

        [Test]
        public void ShouldUpdateString()
        {
            mod1 = new LooseModel(PropBagTypeSafetyMode.Loose);
            mod1.PropStringChanged += mod1_PropStringChanged;

            mod1.PropString = "Test2";

            string temp = mod1.PropString;
            Assert.That(temp, Is.EqualTo("Test2"));
            Assert.That(mod1.PropString, Is.EqualTo("Test2"));
            Assert.That(propString_WasUpdated, Is.EqualTo(true), "PropStringChangeWasCalled = false");
        }

        [Test]
        public void ShouldSAndGLooseBool()
        {
            mod1 = new LooseModel(PropBagTypeSafetyMode.Loose);

            mod1.PropBool = true;

            bool temp = mod1.PropBool;
            Assert.That(temp, Is.EqualTo(true));
            //Assert.That(PropStringChangeWasCalled, Is.EqualTo(true), "PropStringChangeWasCalled = false");
        }


        [Test]
        public void ShouldSAndGLooseUseNewProp()
        {
            mod1 = new LooseModel(PropBagTypeSafetyMode.Loose);

            mod1[PROP_NEW] = "string";
            Assert.That(mod1.GetTypeOfProperty(PROP_NEW), Is.EqualTo(typeof(string)));
            Assert.That(mod1[PROP_NEW], Is.EqualTo("string"));
        }

        [Test]
        public void ShouldSAndGLooseUseNew1stNullNotOk()
        {
            mod1 = new LooseModel(PropBagTypeSafetyMode.Loose);

            mod1[PROP_NEW] = null;
            Assert.That(mod1.GetTypeOfProperty(PROP_NEW), Is.EqualTo(typeof(object)));

            mod1[PROP_NEW] = "string";
            Assert.That(mod1.GetTypeOfProperty(PROP_NEW), Is.EqualTo(typeof(string)));
            Assert.That(mod1[PROP_NEW], Is.EqualTo("string"));
        }

        [Test]
        public void ShouldSAndGLooseUseNew2ndNullOk()
        {
            mod1 = new LooseModel(PropBagTypeSafetyMode.Loose);

            mod1[PROP_NEW] = "string";
            mod1[PROP_NEW] = null;
            Assert.That(mod1.GetTypeOfProperty(PROP_NEW), Is.EqualTo(typeof(string)));
            Assert.That(mod1[PROP_NEW], Is.Null);
        }

        [Test]
        public void ShouldGetKeyNotFoundEx()
        {
            mod1 = new LooseModel(PropBagTypeSafetyMode.Loose);

            object x;
            Assert.Throws<InvalidOperationException>(() => x = mod1["x"]);
        }

        [Test]
        public void ShouldGetArgumentNullEx()
        {
            mod1 = new LooseModel(PropBagTypeSafetyMode.Loose);

            object x;
            Assert.Throws<ArgumentNullException>(() => x = mod1[null]);
        }

        [Test]
        public void UpdateViaPropGenAccess()
        {
            mod1 = new LooseModel(PropBagTypeSafetyMode.Loose);

            IPropGen pg = mod1.GetProp("PropBool");

            IProp<bool> pt = (IProp<bool>) pg.TypedProp;

            //pt.Value = false;

            //pt.Value = true;

        }

        #endregion

        #region Event Handlers

        void mod1_PropStringChanged(object sender, PropertyChangedWithTValsEventArgs<string> e)
        {
            IProp<string> prop = (IProp<string>)sender;
            string oldVal = e.OldValue;
            string newVal = e.NewValue;
            propString_WasUpdated = true;
        }

        #endregion

    }
}
