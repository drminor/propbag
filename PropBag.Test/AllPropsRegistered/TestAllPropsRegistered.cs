using DRM.PropBag;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using DRM.TypeSafePropertyBag;

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
        public void Destroy()
        {
            mod1.ClearEventSubscribers();
            mod1 = null;
        }

        #endregion

        #region Set Bool, Set String and DoWhenTiming

        [Test]
        public void TestAllRegSetBool()
        {
            ////Dictionary<int, int> testD = null;

            //Type typDestDType = typeof(Dictionary<int, int>);

            //string strDestDType = typDestDType.FullName;

            //strDestDType = "System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]";
            //strDestDType = "System.Collections.Generic.Dictionary`2[[System.Int32],[System.Int32]]";

            //Type tt = Type.GetType(strDestDType);


            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);

            //mod1.reg

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
            Assert.That(propStringWasUpdated, Is.True, "Expecting mod1_PropStringChanged to have been called.");

            Assert.That(propStringOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(propStringNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");

            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWhenPropStringChanged to not have been called.");

            Assert.That(mod1.DoWhenStringPropOldVal, Is.Null, "Expecting the value of propStringOldVal to be 'y.'");
            Assert.That(mod1.DoWhenStringPropNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'z.'");
        }

        [Test]
        public void TestDoWhenPropStringChangedBefore()
        {
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
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
            Assert.That(propStringWasUpdated, Is.True, "Expecting mod1_PropStringChanged to have been called.");

            Assert.That(propStringOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(propStringNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");

            Assert.That(doWhenStringChangedWasCalled, Is.True, "Expecting internal DoWhenPropStringChanged to be called before the public event.");

            Assert.That(mod1.DoWhenStringPropOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(mod1.DoWhenStringPropNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");
            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWhenPropStringChanged not to have been called.");
        }

        // TODO: The IProp<T> implementations, now take the DoWhenChanged and add it
        // to the "regular" list of event subscribers. There is no longer any support for
        // DoAfterNotify. Fix or remove the whole DoAfterNotify "business."
        [Test]
        public void TestDoWhenPropStringChangedAfter()
        {
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);

            string temp;

            // TODO: The first part of this needs to be moved to its own separate test.
            //InvalidOperationException ioe = new InvalidOperationException();
            //Type tt = ioe.GetType();
            //Assert.Throws(tt, () => temp = mod1.PropStringCallDoAfter, "Expecting the value to be undefined.");

            mod1.PropStringCallDoAfterChanged += Mod1_PropStringChanged;
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChangedWithGenVals += Mod1_PropertyChangedWithVals;
            mod1.PropertyChanging += Mod1_PropertyChanging;

            // TODO: Need to also test "mod1.PropStringCallDoAfterChanged += mod1_PropStringChanged;"
            // When the mode is loose, or OnlyTypedAccess.

            mod1.PropStringCallDoAfter = null;
    
            // Original Test starts here.
            temp = mod1.PropStringCallDoAfter;
            Assert.That(temp, Is.Null, "Expecting the initial value of PropString to be null.");

            propStringOldVal = "u";
            propStringNewVal = "x";
            mod1.DoWhenStringChanged_WasCalled = false;
            mod1.DoWhenStringPropOldVal = "y";
            mod1.DoWhenStringPropNewVal = "z";

            mod1.PropStringCallDoAfter = "Water Colors";

            temp = mod1.PropStringCallDoAfter;
            Assert.That(temp, Is.EqualTo("Water Colors"), "Expecting the value of PropString to be updated to 'Water Colors.'");
            Assert.That(propStringWasUpdated, Is.True, "Expecting mod1_PropStringChanged to have been called.");

            Assert.That(propStringOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(propStringNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");

            Assert.That(doWhenStringChangedWasCalled, Is.False, "Expecting internal DoWhenPropStringChanged to not be called before the public event.");

            Assert.That(mod1.DoWhenStringPropOldVal, Is.Null, "Expecting the value of propStringOldVal to be null.");
            Assert.That(mod1.DoWhenStringPropNewVal, Is.EqualTo("Water Colors"), "Expecting the value of propStringNewVal to be 'Water Colors.'");
            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWhenPropStringChanged not to have been called.");
        }

        void Mod1_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
        {
        }

        void Mod1_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Test Reference Equality Comparison

        [Test]
        public void TestStringRefComp()
        {
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropStringUseRefCompChanged += Mod1_PropStringUseRefCompChanged;

            

            // Use our Reference Equality Comparer.
            IEqualityComparer<string> comparer = RefEqualityComparer<string>.Default;

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
            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.True, "Expecting internal DoWhenPropStringChanged to have been called.");


            propStringWasUpdated = false;
            mod1.DoWhenStringChanged_WasCalled = false;

            mod1.PropStringUseRefComp = "Water Colors";

            temp = mod1.PropStringUseRefComp;
            theyAreTheSame = comparer.Equals("Water Colors", temp);
            Assert.That(theyAreTheSame, Is.True, "Temp should point to the same memory location as the string literal 'Water Colors.'");

            Assert.That(propStringWasUpdated, Is.False, "Expecting mod1_PropStringUseRefCompChanged to have NOT been called.");
            Assert.That(mod1.DoWhenStringChanged_WasCalled, Is.False, "Expecting internal DoWhenPropStringChanged to not have been called.");

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
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropNullableIntChanged += Mod1_PropNullableIntChanged;

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
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropICollectionIntChanged += Mod1_PropICollectionIntChanged;


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

        }

        #endregion

        #region Test Adding Unregistered Property

        [Test]
        public void TestAddNewProp()
        {
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);

            InvalidOperationException kk = new InvalidOperationException();

            Type tt = kk.GetType();

            Assert.Throws(tt, () => mod1["System.String", "NewProperty"] = "This is a a test.");
        }


        #endregion

        #region Test Subscribing to the generic event 

        object genObjOldVal = -1;
        object genObjNewVal = -2;

        [Test]
        public void TestPropertyChangedWithVals()
        {
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChangedWithGenVals += Mod1_PropertyChangedWithVals;

            mod1.PropInt = 0;
            mod1.PropInt = 1;

            Assert.That(genObjOldVal, Is.EqualTo(0), "The old value should have been 0.");
            Assert.That(genObjNewVal, Is.EqualTo(1), "The old value should have been 1.");
        }

        void Mod1_PropertyChangedWithVals(object sender, PCGenEventArgs e)
        {
            object sendr = sender;
            string prpName = e.PropertyName;
            if (prpName == "PropInt")
            {
                genObjOldVal = e.OldValue;
                genObjNewVal = e.NewValue;
            }
        }

        #endregion

        #region Test Subscribing to events for a particular property

        [Test]
        public void TestSubscribePropChangedGen()
        {
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.SubscribeToPropChanged(handler: DoWhenPropIntChangesGen, propertyName: "PropInt", propertyType: typeof(int));

            mod1.PropInt = 0;
            mod1.PropInt = 1;

            Assert.That(genObjOldVal, Is.EqualTo(0), "The old value should have been 0.");
            Assert.That(genObjNewVal, Is.EqualTo(1), "The new value should have been 1.");

            mod1.UnSubscribeToPropChanged(eventHandler: DoWhenPropIntChangesGen, propertyName: "PropInt", propertyType: typeof(int));
            mod1.PropInt = 2;

            Assert.That(genObjOldVal, Is.EqualTo(0), "The old value should have been 0. The action did not get unscribed.");
            Assert.That(genObjNewVal, Is.EqualTo(1), "The new value should have been 1.");
        }

        void DoWhenPropIntChangesGen(object oldVal, object newValue)
        {
            genObjOldVal = oldVal;
            genObjNewVal = newValue;
        }

        int typedOldVal;
        int typedNewVal;

        [Test]
        public void TestSubscribePropChangedTyped()
        {
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.SubscribeToPropChanged<int>(DoWhenPropIntChangesTyped, "PropInt");

            mod1.PropInt = 0;
            mod1.PropInt = 1;

            Assert.That(typedOldVal, Is.EqualTo(0), "The old value should have been 0.");
            Assert.That(typedNewVal, Is.EqualTo(1), "The new value should have been 1.");

            mod1.UnSubscribeToPropChanged<int>(DoWhenPropIntChangesTyped, "PropInt");
            mod1.PropInt = 2;

            Assert.That(typedOldVal, Is.EqualTo(0), "The old value should have been 0. The action did not get unscribed.");
            Assert.That(typedNewVal, Is.EqualTo(1), "The new value should have been 1.");
        }

        void DoWhenPropIntChangesTyped(object sender, PCTypedEventArgs<int> e)
        {
            typedOldVal = e.OldValue;
            typedNewVal = e.NewValue;
        }

        #endregion

        #region Test PubPropBag

        [Test]
        public void TestPublicPropBag()
        {
            PubPropBag ppb = new PubPropBag(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            ppb.AddProp<int>("PropInt");
        }
        #endregion

        #region Test Enhance Add Prop Calls

        // TODO: Improve and uncomment
        //[Test]
        //public void TestFindMethod()
        //{
        //    mod1 = new AllPropsRegisteredModel();
        //    bool methExists = mod1.DoesMethodExist("DoWhenNullIntChanged");

        //    Assert.That(methExists, Is.True, "The method should exist.");
        //}

        #endregion

        #region Event Handlers

        void Mod1_PropStringChanged(object sender, PCTypedEventArgs<string> e)
        {
            propStringOldVal = e.OldValue;
            propStringNewVal = e.NewValue;
            propStringWasUpdated = true;

            // Get the value of the DoWhenStringChangedWasCalled at the time this event is being rasied.
            doWhenStringChangedWasCalled = mod1.DoWhenStringChanged_WasCalled;
        }

        void Mod1_PropStringUseRefCompChanged(object sender, PCTypedEventArgs<string> e)
        {
            propStringOldVal = e.OldValue;
            propStringNewVal = e.NewValue;
            propStringWasUpdated = true;

            // Get the value of the DoWhenStringChangedWasCalled at the time this event is being rasied.
            doWhenStringChangedWasCalled = mod1.DoWhenStringChanged_WasCalled;
        }

        void Mod1_PropNullableIntChanged(object sender, PCTypedEventArgs<int?> e)
        {
            Nullable<int> oldVal = e.OldValue;
            Nullable<int> newValue = e.NewValue;

            propNullableInt_WasUpdated = true;
        }

        void Mod1_PropICollectionIntChanged(object sender, PCTypedEventArgs<ICollection<int>> e)
        {
            ICollection<int> oldValue = e.OldValue;
            ICollection<int> newValue = e.NewValue;

            propICollectionInt_WasUpdated = true;
        }
        #endregion

    }
}
