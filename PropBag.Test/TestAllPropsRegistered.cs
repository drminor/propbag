using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using DRM.Ipnwv;
using DRM.PropBag;
using DRM.ReferenceEquality;


using NUnit.Framework;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestAllPropsRegistered
    {
        AllPropsRegisteredModel mod1 = null;

        private bool propStringWasUpdated;
        private string propStringOldVal;
        private string propStringNewVal;

        private bool doWhenStringChangedWasCalled;

        private bool propNullableInt_WasUpdated;
        private bool propICollectionInt_WasUpdated;

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

        #region Set Bool, Set String and DoWhenTiming

        [Test]
        public void TestAllRegSetBool()
        {
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);

            bool temp = mod1.PropBool;
            Assert.That(temp, Is.EqualTo(false),"Expecting the initial value of PropBool to be false.");

            mod1.PropBool = true;

            temp = mod1.PropBool;
            Assert.That(temp, Is.EqualTo(true), "Expecting the value of PropBool to be updated to true.");
        }

        [Test]
        public void TestAllRegSetString()
        {
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
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
            Assert.That(propStringWasUpdated, Is.True, "Expecting mod1_PropStringChanged to have been called.");

            Assert.That(propStringOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(propStringNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");

            Assert.That(doWhenStringChangedWasCalled, Is.False, "Expecting internal DoWhenPropStringChanged to not have been called.");

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
            mod1 = new AllPropsRegisteredModel(true, doAfterNotify);
            mod1.PropStringChanged += mod1_PropStringChanged;

            string temp = mod1.PropStringUseRefComp;
            Assert.That(temp, Is.Null, "Expecting the initial value of PropString to be null.");

            propStringOldVal = "u";
            propStringNewVal = "x";
            mod1.DoWhenStringChanged_WasCalled = false;
            mod1.DoWhenStringPropOldVal = "y";
            mod1.DoWhenStringPropNewVal = "z";

            mod1.PropString = "Water Colors";

            temp = mod1.PropString;
            Assert.That(temp, Is.EqualTo("Water Colors"), "Expecting the value of PropString to be updated to 'Water Colors.'");
            Assert.That(propStringWasUpdated, Is.True, "Expecting mod1_PropStringChanged to have been called.");

            Assert.That(propStringOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(propStringNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");

            if (doAfterNotify)
            {
                Assert.That(doWhenStringChangedWasCalled, Is.False, "Expecting internal DoWhenPropStringChanged to be after before the public event.");
            }
            else
            {
                Assert.That(doWhenStringChangedWasCalled, Is.True, "Expecting internal DoWhenPropStringChanged to be called before the public event.");
            }

            Assert.That(mod1.DoWhenStringPropOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(mod1.DoWhenStringPropNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");
            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWhenPropStringChanged not to have been called.");

        }

        #endregion

        #region Test Reference Equality Comparison

        [Test]
        public void TestStringRefComp()
        {
            mod1 = new AllPropsRegisteredModel(true, true);
            mod1.PropStringUseRefCompChanged += mod1_PropStringUseRefCompChanged;

            // Use our Reference Equality Comparer.
            IEqualityComparer<object> comparer = ReferenceEqualityComparer.Default;

            string temp = mod1.PropStringUseRefComp;
            Assert.That(temp, Is.Null, "Expecting the initial value of PropString to be null.");

            propStringWasUpdated = false;
            //propStringOldVal = "u";
            //propStringNewVal = "x";
            mod1.DoWhenStringChanged_WasCalled = false;
            //mod1.DoWhenStringPropOldVal = "y";
            //mod1.DoWhenStringPropNewVal = "z";

            mod1.PropStringUseRefComp = "Water Colors";

            temp = mod1.PropStringUseRefComp;

            // Use Standard Value Comparison to verify that the value is the same, although the reference is different.
            // We don't want to be fooled by actually sending a different value.
            bool theyAreTheSame = comparer.Equals("Water Colors", temp);
            Assert.That(theyAreTheSame, Is.True, "Temp should point to the same memory location as the string literal 'Water Colors.'");

            Assert.That(propStringWasUpdated, Is.True, "Expecting mod1_PropStringUseRefCompChanged to have been called.");
            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWhenPropStringChanged not to have been called.");


            propStringWasUpdated = false;
            mod1.DoWhenStringChanged_WasCalled = false;

            mod1.PropStringUseRefComp = "Water Colors";

            temp = mod1.PropStringUseRefComp;
            theyAreTheSame = comparer.Equals("Water Colors", temp);
            Assert.That(theyAreTheSame, Is.True, "Temp should point to the same memory location as the string literal 'Water Colors.'");

            Assert.That(propStringWasUpdated, Is.False, "Expecting mod1_PropStringUseRefCompChanged to have NOT been called.");
            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.False, "Expecting internal DoWhenPropStringChanged not to have been called.");

            propStringWasUpdated = false;
            mod1.DoWhenStringChanged_WasCalled = false;

            // Build a string with the same value, but at a different memory location.
            string difLoc = new StringBuilder("Water Colors").ToString();

            Assert.That(difLoc == "Water Colors", Is.True, "The value of difLoc should be 'Water Colors'.");

            mod1.PropStringUseRefComp = difLoc;

            temp = mod1.PropStringUseRefComp;
            theyAreTheSame = comparer.Equals("Water Colors", temp);
            Assert.That(theyAreTheSame, Is.False, "Temp should point to a different memory location as the string literal 'Water Colors.'");

            Assert.That(propStringWasUpdated, Is.True, "Expecting mod1_PropStringUseRefCompChanged to have been called.");
            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWhenPropStringChanged to have been called.");
        }

        #endregion

        #region Test Nullable<T> handling

        [Test]
        public void ShouldSetAndRetrieveNullableInt()
        {
            mod1 = new AllPropsRegisteredModel(true, false);
            mod1.PropNullableIntChanged += mod1_PropNullableIntChanged;

            Assert.That(mod1.PropNullableInt, Is.EqualTo(-1),"The intitalvalue should be -1");

            mod1.DoWhenNullIntChanged_WasCalled = false;

            mod1.PropNullableInt = 0;
            Assert.That(mod1.PropNullableInt, Is.EqualTo(0));
            Assert.That(mod1.DoWhenNullIntChanged_WasCalled, Is.EqualTo(true));
            Assert.That(propNullableInt_WasUpdated, Is.EqualTo(true), "propNullableIntChanged = false");

            mod1.DoWhenNullIntChanged_WasCalled = false;
            mod1.PropNullableInt = new Nullable<int>(1);
            Assert.That(mod1.PropNullableInt, Is.EqualTo(1));
            Assert.That(mod1.DoWhenNullIntChanged_WasCalled, Is.EqualTo(true));

            mod1.DoWhenNullIntChanged_WasCalled = false;
            mod1.PropNullableInt = null;
            Assert.That(mod1.PropNullableInt, Is.Null);
            Assert.That(mod1.DoWhenNullIntChanged_WasCalled, Is.EqualTo(true));

            int? test = new Nullable<int>();

            mod1.DoWhenNullIntChanged_WasCalled = false;
            mod1.PropNullableInt = test;
            Assert.That(mod1.PropNullableInt, Is.EqualTo(test));
            Assert.That(mod1.DoWhenNullIntChanged_WasCalled, Is.EqualTo(false));
        }

        [Test]
        public void ShouldSetAndRetrieveICollectionInt()
        {
            mod1 = new AllPropsRegisteredModel(true, false);
            mod1.PropICollectionIntChanged += mod1_PropICollectionIntChanged;
            mod1.PropNullableIntChanged += mod1_PropNullableIntChanged;


            mod1.DoWhenICollectionIntChanged_WasCalled = false;

            ICollection<int> before = new Collection<int>();

            mod1.PropICollectionInt = before;
            Assert.That(mod1.PropICollectionInt, Is.EqualTo(before));
            Assert.That(mod1.DoWhenICollectionIntChanged_WasCalled, Is.EqualTo(true));

            mod1.DoWhenICollectionIntChanged_WasCalled = false;
            mod1.PropICollectionInt = before;
            Assert.That(mod1.PropICollectionInt, Is.EqualTo(before));
            Assert.That(mod1.DoWhenICollectionIntChanged_WasCalled, Is.EqualTo(false));

            Collection<int> newVal = new Collection<int>();

            mod1.DoWhenICollectionIntChanged_WasCalled = false;
            mod1.PropICollectionInt = newVal;
            Assert.That(mod1.PropICollectionInt, Is.EqualTo(newVal));
            Assert.That(mod1.DoWhenICollectionIntChanged_WasCalled, Is.EqualTo(true));

            Assert.That(propICollectionInt_WasUpdated, Is.EqualTo(true));

            //TODO: This shouldn't be here : it is testing setting a property that has not been defined with AddProp.
            //mod1.DoWhenICollectionIntChanged_WasCalled = false;
            //mod1["PropICollectionInt"] = newVal;
            //Assert.That(mod1.PropICollectionInt, Is.EqualTo(newVal));
            //Assert.That(mod1.DoWhenICollectionIntChanged_WasCalled, Is.EqualTo(false));
        }

        #endregion

        #region Event Handlers

        void mod1_PropStringChanged(object sender, PropertyChangedWithTValsEventArgs<string> e)
        {
            propStringOldVal = e.OldValue;
            propStringNewVal = e.NewValue;
            propStringWasUpdated = true;

            // Get the value of the DoWhenStringChangedWasCalled at the time this event is being rasied.
            doWhenStringChangedWasCalled = mod1.DoWhenStringChanged_WasCalled;
        }

        void mod1_PropStringUseRefCompChanged(object sender, PropertyChangedWithTValsEventArgs<string> e)
        {
            propStringOldVal = e.OldValue;
            propStringNewVal = e.NewValue;
            propStringWasUpdated = true;

            // Get the value of the DoWhenStringChangedWasCalled at the time this event is being rasied.
            doWhenStringChangedWasCalled = mod1.DoWhenStringChanged_WasCalled;
        }

        void mod1_PropNullableIntChanged(object sender, PropertyChangedWithTValsEventArgs<int?> e)
        {
            Nullable<int> oldVal = e.OldValue;
            Nullable<int> newValue = e.NewValue;

            propNullableInt_WasUpdated = true;
        }

        void mod1_PropICollectionIntChanged(object sender, PropertyChangedWithTValsEventArgs<ICollection<int>> e)
        {
            ICollection<int> oldValue = e.OldValue;
            ICollection<int> newValue = e.NewValue;

            propICollectionInt_WasUpdated = true;
        }
        #endregion

    }
}
