using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using System;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestLoose
    {
        AutoMapperSupport.AutoMapperHelpers _amHelpers;
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
            _amHelpers = new AutoMapperSupport.AutoMapperHelpers();
        }

        [OneTimeTearDown]
        public void Destroy()
        {
            mod1 = null;
        }

        #endregion

        #region Tests

        [Test]
        public void ShouldUpdateBool()
        {
            // TODO: AAA
            mod1 = new LooseModel(PropBagTypeSafetyMode.None, _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1)
            {
                PropBool = true
            };
            Assert.That(mod1.PropBool, Is.True, "PropBool did not get updated.");
        }

        [Test]
        public void ShouldUpdateString()
        {
            // TODO: AAA
            mod1 = new LooseModel(PropBagTypeSafetyMode.None, _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            mod1.PropStringChanged += Mod1_PropStringChanged;


            mod1.PropString = "Test2";

            string temp = mod1.PropString;

            // Added this statement on 11/9/2017.
            mod1.PropStringChanged -= Mod1_PropStringChanged;

            Assert.That(temp, Is.EqualTo("Test2"));
            Assert.That(mod1.PropString, Is.EqualTo("Test2"));
            Assert.That(propString_WasUpdated, Is.EqualTo(true), "PropStringChangeWasCalled = false");
        }



        [Test]
        public void ShouldSAndGLooseBool()
        {
            // TODO: AAA
            mod1 = new LooseModel(PropBagTypeSafetyMode.None, _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1)
            {
                PropBool = true
            };

            bool temp = mod1.PropBool;
            Assert.That(temp, Is.EqualTo(true));
            //Assert.That(PropStringChangeWasCalled, Is.EqualTo(true), "PropStringChangeWasCalled = false");
        }


        [Test]
        public void ShouldSAndGLooseUseNewProp()
        {
            // TODO: AAA
            mod1 = new LooseModel(PropBagTypeSafetyMode.None, _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            mod1["System.String", PROP_NEW] = "string";

            string s = (string)mod1["System.String", PROP_NEW];
            Type vType = mod1.GetTypeOfProperty(PROP_NEW);

            Assert.That(s, Is.EqualTo("string"));
            Assert.That(vType, Is.EqualTo(typeof(string)));
        }

        [Test]
        public void ShouldSAndGLooseUseNew1stNullNotOk()
        {
            // TODO: AAA
            mod1 = new LooseModel(PropBagTypeSafetyMode.None, _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            mod1[typeof(object), PROP_NEW] = 0;
            Assert.That(mod1.GetTypeOfProperty(PROP_NEW), Is.EqualTo(typeof(object)));

            Assert.Throws<ApplicationException>(() => mod1[typeof(string), PROP_NEW] = "0");
        }

        [Test]
        public void ShouldSAndGLooseUseNew2ndNullOk()
        {
            // TODO: AAA
            mod1 = new LooseModel(PropBagTypeSafetyMode.None, _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            mod1["System.String", PROP_NEW] = "string";
            mod1["System.String", PROP_NEW] = null;

            string s = (string)mod1["System.String", PROP_NEW];
            Type vType = mod1.GetTypeOfProperty(PROP_NEW);

            Assert.That(vType, Is.EqualTo(typeof(string)));
            Assert.That(s, Is.Null);
        }

        [Test]
        public void ShouldGetKeyNotFoundEx()
        {
            // TODO: AAA
            mod1 = new LooseModel(PropBagTypeSafetyMode.None, _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            object x;
            Assert.Throws<InvalidOperationException>(() => x = mod1["System.String", "x"]);
        }

        [Test]
        public void ShouldGetArgumentNullEx()
        {
            // TODO: AAA
            mod1 = new LooseModel(PropBagTypeSafetyMode.None, _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            object x;
            Assert.Throws<ArgumentNullException>(() => x = mod1["System.String", null]);
        }

        [Test]
        public void UpdateViaPropGenAccess()
        {
            // TODO: AAA
            mod1 = new LooseModel(PropBagTypeSafetyMode.None, _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            IPropData pg = mod1.GetPropGen("PropBool");

            IProp<bool> pt = (IProp<bool>)pg.TypedProp;

            //pt.Value = false;

            //pt.Value = true;

        }

        #endregion

        #region Event Handlers

        void Mod1_PropStringChanged(object sender, PcTypedEventArgs<string> e)
        {
            var x = sender;

            if(x is LooseModel lm)
            {
                System.Diagnostics.Debug.WriteLine("It is a LooseModel -- it works!");
            }

            string oldVal = e.OldValue;
            string newVal = e.NewValue;

            propString_WasUpdated = true;
        }

        #endregion

    }
}
