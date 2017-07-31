﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.PropBag;

using NUnit.Framework;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestOnlyTypedAccess
    {

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
        }

        [OneTimeTearDown]
        public void destroy()
        {
            mod1 = null;
        }

        #endregion

        #region Tests

        [Test]
        public void TestAllRegSetBool()
        {
            mod1 = new OnlyTypedAccessModel(PropBagTypeSafetyMode.OnlyTypedAccess);

            bool temp = mod1.PropBool;
            Assert.That(temp, Is.EqualTo(false),"Expecting the initial value of PropBool to be false.");

            mod1.PropBool = true;

            temp = mod1.PropBool;
            Assert.That(temp, Is.EqualTo(true), "Expecting the value of PropBool to be updated to true.");
        }

        [Test]
        public void TestAllRegSetString()
        {
            mod1 = new OnlyTypedAccessModel(PropBagTypeSafetyMode.OnlyTypedAccess);
            mod1.PropStringChanged += mod1_PropStringChanged;

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

            Assert.That(doWhenStringChanged_WasCalled, Is.False, "Expecting internal DoWWhenPropStringChanged not to be called.");

            Assert.That(mod1.DoWhenStringPropOldVal, Is.EqualTo("y"), "Expecting the value of propStringOldVal to be 'y.'");
            Assert.That(mod1.DoWhenStringPropNewVal, Is.EqualTo("z"), "Expecting the value of propStringNewVal to be 'z.'");
        }

        [Test]
        public void TestAllRegDoWhenTiming()
        {
            TestDoWhenStringPropChangedTiming(false);
            TestDoWhenStringPropChangedTiming(true);
        }

        private void TestDoWhenStringPropChangedTiming(bool doAfterNotify)
        {
            mod1 = new OnlyTypedAccessModel(true, doAfterNotify);
            mod1.PropStringChanged += mod1_PropStringChanged;

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

            if (doAfterNotify)
            {
                Assert.That(doWhenStringChanged_WasCalled, Is.False, "Expecting internal DoWWhenPropStringChanged to be after before the public event.");
            }
            else
            {
                Assert.That(doWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWWhenPropStringChanged to be called before the public event.");
            }

            Assert.That(mod1.DoWhenStringPropOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(mod1.DoWhenStringPropNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");
        }

        #endregion

        #region Event Handlers

        void mod1_PropStringChanged(object sender, DRM.Ipnwv.PropertyChangedWithTValsEventArgs<string> e)
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
