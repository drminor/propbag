using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using System;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestOnlyTypedAccess
    {
        AutoMapperSupport.AutoMapperHelpers _amHelpers;
        OnlyTypedAccessModel mod1;

        private bool propString_WasUpdated;
        private string propStringOldVal;
        private string propStringNewVal;
        private bool doWhenStringChanged_WasCalled;

        //const string PROP_BOOL = "PropBool";
        //const string PROP_STRING = "PropString";
        //const string PROP_NEW = "PropNotDeclared";

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
        public void TestAllRegSetBool()
        {
            mod1 = new OnlyTypedAccessModel(PropBagTypeSafetyMode.OnlyTypedAccess, _amHelpers.PropFactory_V1);

            bool temp = mod1.PropBool;
            Assert.That(temp, Is.EqualTo(false),"Expecting the initial value of PropBool to be false.");

            mod1.PropBool = true;

            temp = mod1.PropBool;
            Assert.That(temp, Is.EqualTo(true), "Expecting the value of PropBool to be updated to true.");
        }

        [Test]
        public void TestAllRegSetString()
        {
            mod1 = new OnlyTypedAccessModel(PropBagTypeSafetyMode.OnlyTypedAccess, _amHelpers.PropFactory_V1);
            mod1.PropStringChanged += Mod1_PropStringChanged;

            string temp = mod1.PropString;
            Assert.That(temp, Is.Null, "Expecting the initial value of PropString to be null.");

            propStringOldVal = "u";
            propStringNewVal = "x";
            mod1.DoWhenStringChanged_WasCalled = false;
            mod1.DoWhenStringPropOldVal = "y";
            mod1.DoWhenStringPropNewVal = "z";

            mod1.PropString = "Water Colors";

            temp = mod1.PropString;
            Assert.That(temp, Is.EqualTo("Water Colors"), "Expecting the value of PropString to be updated to 'Water Colors.'");
            Assert.That(propString_WasUpdated, Is.True, "Expecting propStringWasUpdated to be true.");

            Assert.That(propStringOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(propStringNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");

            Assert.That(doWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWhenPropStringChanged to be called.");

            Assert.That(mod1.DoWhenStringPropOldVal, Is.Null, "Expecting the value of propStringOldVal to be 'y.'");
            Assert.That(mod1.DoWhenStringPropNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'z.'");
        }

        [Test]
        public void TestDoWhenPropStringChangedBefore()
        {
            mod1 = new OnlyTypedAccessModel(PropBagTypeSafetyMode.OnlyTypedAccess, _amHelpers.PropFactory_V1);

            mod1.PropStringChanged += Mod1_PropStringChanged;

            string temp = mod1.PropString;
            Assert.That(temp, Is.Null, "Expecting the initial value of PropString to be null.");

            propStringOldVal = "u";
            propStringNewVal = "x";
            mod1.DoWhenStringChanged_WasCalled = false;
            mod1.DoWhenStringPropOldVal = "y";
            mod1.DoWhenStringPropNewVal = "z";

            mod1.PropString = "Water Colors";

            temp = mod1.PropString;
            Assert.That(temp, Is.EqualTo("Water Colors"), "Expecting the value of PropString to be updated to 'Water Colors.'");
            Assert.That(propString_WasUpdated, Is.True, "Expecting propStringWasUpdated to be true.");

            Assert.That(propStringOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(propStringNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");

            Assert.That(doWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWWhenPropStringChanged to be called before the public event.");

            Assert.That(mod1.DoWhenStringPropOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(mod1.DoWhenStringPropNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");
            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWhenPropStringChanged not to have been called.");
        }

        [Test]
        public void TestDoWhenPropStringChangedAfter()
        {
            mod1 = new OnlyTypedAccessModel(PropBagTypeSafetyMode.OnlyTypedAccess, _amHelpers.PropFactory_V1);

            mod1.PropStringCallDoAfterChanged += Mod1_PropStringChanged;

            // TODO: Need to add test to check that values are really set to undefined.

            mod1.PropStringCallDoAfter = null;

            string temp = mod1.PropStringCallDoAfter;
            Assert.That(temp, Is.Null, "Expecting the initial value of PropString to be null.");

            propStringOldVal = "u";
            propStringNewVal = "x";
            mod1.DoWhenStringChanged_WasCalled = false;
            mod1.DoWhenStringPropOldVal = "y";
            mod1.DoWhenStringPropNewVal = "z";

            mod1.PropStringCallDoAfter = "Water Colors";

            temp = mod1.PropStringCallDoAfter;
            Assert.That(temp, Is.EqualTo("Water Colors"), "Expecting the value of PropString to be updated to 'Water Colors.'");
            Assert.That(propString_WasUpdated, Is.True, "Expecting propStringWasUpdated to be true.");

            Assert.That(propStringOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(propStringNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");

            // TODO: Fix This: The Order in which these are called in not yet suported.
            //Assert.That(doWhenStringChanged_WasCalled, Is.False, "Expecting internal DoWWhenPropStringChanged to not be called before the public event.");

            Assert.That(mod1.DoWhenStringPropOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(mod1.DoWhenStringPropNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");
            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWhenPropStringChanged not to have been called.");
        }
        
        #endregion

        #region Test Adding Unregistered Property

        [Test]
        public void TestAddNewProp()
        {
            mod1 = new OnlyTypedAccessModel(PropBagTypeSafetyMode.Tight, _amHelpers.PropFactory_V1);

            InvalidOperationException aa = new InvalidOperationException();

            Type tt = aa.GetType();

            Assert.Throws(tt, () => mod1["System.String","NewProperty"] = "This is a a test.");
        }

        #endregion

        #region Event Handlers

        void Mod1_PropStringChanged(object sender, PcTypedEventArgs<string> e)
        {
            propStringOldVal = e.OldValue;
            propStringNewVal = e.NewValue;
            propString_WasUpdated = true;

            // Get the value of the DoWhenStringChangedWasCalled at the time this event is being rasied.
            doWhenStringChanged_WasCalled = mod1.DoWhenStringChanged_WasCalled;
        }

        #endregion

    }
}
