using DRM.PropBag;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using DRM.TypeSafePropertyBag; using Swhp.Tspb.PropBagAutoMapperService;
using System.Reflection;


namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestAllPropsRegistered
    {
        AutoMapperSupport.AutoMapperHelpers _amHelpers;

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
            _amHelpers = new AutoMapperSupport.AutoMapperHelpers();
        }

        [OneTimeTearDown]
        public void Destroy()
        {
            mod1.Dispose();
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

            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

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
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

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
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

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
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

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

            // TODO: Fix This: The Order in which these are called in not yet suported.
            //Assert.That(doWhenStringChangedWasCalled, Is.False, "Expecting internal DoWhenPropStringChanged to not be called before the public event.");

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
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

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
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

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
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

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
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

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
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            //mod1.SubscribeToPropChanged(Mod1_PropertyChangedWithVals, "PropInt", typeof(int));
            mod1.PropertyChangedWithGenVals += Mod1_PropertyChangedWithVals;
            mod1.PropertyChanging += Mod1_PropertyChanging1;
            mod1.PropertyChanged += Mod1_PropertyChanged1;

            mod1.PropertyChangedWithObjectVals += Mod1_PropertyChangedWithObjectVals;

            mod1.PropInt = 0;
            mod1.PropInt = 1;

            Assert.That(genObjOldVal, Is.EqualTo(0), "The old value should have been 0.");
            Assert.That(genObjNewVal, Is.EqualTo(1), "The old value should have been 1.");
        }

        private void Mod1_PropertyChanged1(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string test = e.PropertyName;
        }

        private void Mod1_PropertyChanging1(object sender, System.ComponentModel.PropertyChangingEventArgs e)
        {
            string test = e.PropertyName;
        }

        public void Mod1_PropertyChangedWithObjectVals(object sender, PcObjectEventArgs e)
        {
            object sendr = sender;
            string prpName = e.PropertyName;

            //genObjOldVal = e.OldValue;
            //genObjNewVal = e.NewValue;

            if (prpName == "PropInt")
            {
                genObjOldVal = e.OldValue;
                genObjNewVal = e.NewValue;
            }
        }

        void Mod1_PropertyChangedWithVals(object sender, PcGenEventArgs e)
        {
            object sendr = sender;
            string prpName = e.PropertyName;

            //genObjOldVal = e.OldValue;
            //genObjNewVal = e.NewValue;

            if (prpName == "PropInt")
            {
                genObjOldVal = e.OldValue;
                genObjNewVal = e.NewValue;
            }
        }

        public void TestHandler(object sender, EventArgs e)
        {
            object sendr = sender;
        }

        #endregion

        #region Test Subscribing to events for a particular property

        [Test]
        public void TestSubscribePropChangedGen()
        {
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            mod1.SubscribeToPropChanged(eventHandler: DoWhenPropIntChangesGen, propertyName: "PropInt", propertyType: typeof(int));

            mod1.PropInt = 0;
            mod1.PropInt = 1;

            Assert.That(genObjOldVal, Is.EqualTo(0), "The old value should have been 0.");
            Assert.That(genObjNewVal, Is.EqualTo(1), "The new value should have been 1.");

            mod1.UnSubscribeToPropChanged(eventHandler: DoWhenPropIntChangesGen, propertyName: "PropInt", propertyType: typeof(int));
            mod1.PropInt = 2;

            Assert.That(genObjOldVal, Is.EqualTo(0), "The old value should have been 0. The action did not get unsubcribed.");
            Assert.That(genObjNewVal, Is.EqualTo(1), "The new value should have been 1.");
        }

        void DoWhenPropIntChangesGen(object sender, PcGenEventArgs e)
        {
            genObjOldVal = e.OldValue;
            genObjNewVal = e.NewValue;
        }

        int typedOldVal;
        int typedNewVal;

        [Test]
        public void TestSubscribePropChangedTyped()
        {
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            mod1.SubscribeToPropChanged<int>(DoWhenPropIntChangesTyped, "PropInt");

            mod1.PropInt = 0;
            mod1.PropInt = 1;

            Assert.That(typedOldVal, Is.EqualTo(0), "The old value should have been 0.");
            Assert.That(typedNewVal, Is.EqualTo(1), "The new value should have been 1.");

            mod1.UnSubscribeToPropChanged<int>(DoWhenPropIntChangesTyped, "PropInt");
            mod1.PropInt = 2;

            Assert.That(typedOldVal, Is.EqualTo(0), "The old value should have been 0. The action did not get unsubscribed.");
            Assert.That(typedNewVal, Is.EqualTo(1), "The new value should have been 1.");
        }

        void DoWhenPropIntChangesTyped(object sender, PcTypedEventArgs<int> e)
        {
            typedOldVal = e.OldValue;
            typedNewVal = e.NewValue;
        }

        delegate void PCObjEventHandler(object @this, object sender, EventArgs e);


        [Test]
        public void TestPropertyChangedObject()
        {
            // TODO: AAA
            mod1 = new AllPropsRegisteredModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1);

            //Action<object, EventArgs> objHandler = this.TestHandler;

            //WeakReference Target = new WeakReference(objHandler.Target);

            //MethodInfo mi = objHandler.Method;

            ////Delegate ttt = mi.CreateDelegate();

            //Type dType = objHandler.Method.GetDelegateType();

            //var temp3 = Delegate.CreateDelegate(dType, null, objHandler.Method);

            //var temp4 = Convert.ChangeType(temp3, dType);

            ////Main();


            //Action<TestAllPropsRegistered, object, EventArgs> xx = (Action<TestAllPropsRegistered, object, EventArgs>)Delegate.CreateDelegate(typeof(Action<TestAllPropsRegistered, object, EventArgs>), null, mi);

            ////PCObjEventHandler temp = (PCObjEventHandler) Delegate.CreateDelegate(typeof(PCObjEventHandler), null, sKey.ObjHandler.Method);
            ////ObjHandler = temp;

            ////PCObjectEventAction temp2 = (PCObjectEventAction) Delegate.CreateDelegate(typeof(PCObjectEventAction), null, sKey.ObjHandler.Method);
            ////ObjHandlerProxy = temp4;

            //string methodName = objHandler.Method.Name;

            mod1.SubscribeToPropChanged(Mod1_PropertyChangedWithObjectVals, "PropInt");

            mod1.PropInt = 0;
            mod1.PropInt = 1;

            Assert.That(genObjOldVal, Is.EqualTo(0), "The old value should have been 0.");
            Assert.That(genObjNewVal, Is.EqualTo(1), "The old value should have been 1.");
        }


        #endregion

        void Main()
        {
            MethodInfo sayHelloMethod = typeof(Person).GetMethod("SayHello");
            OpenAction<Person, string> action =
                (OpenAction<Person, string>)
                    Delegate.CreateDelegate(
                        typeof(OpenAction<Person, string>),
                        null,
                        sayHelloMethod);

            Person joe = new Person { Name = "Joe" };
            action(joe, "Jack"); // Prints "Hello Jack, my name is Joe"
        }

        delegate void OpenAction<TThis, T>(TThis @this, T arg);


        class Person
        {
            public string Name { get; set; }

            public void SayHello(string name)
            {
                //Console.WriteLine("Hi {0}, my name is {1}", name, this.Name);
                System.Diagnostics.Debug.WriteLine($"Hi {name}, my name is {this.Name}");
            }
        }

        #region Test PubPropBag

        [Test]
        public void TestPublicPropBag()
        {
            // TODO: AAA
            PubPropBag ppb = new PubPropBag(PropBagTypeSafetyMode.AllPropsMustBeRegistered,
                _amHelpers.StoreAccessCreator, _amHelpers.PropFactory_V1, "PropBagLib.Tests.PubPropBag");

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

        void Mod1_PropStringChanged(object sender, PcTypedEventArgs<string> e)
        {
            propStringOldVal = e.OldValue;
            propStringNewVal = e.NewValue;
            propStringWasUpdated = true;

            // Get the value of the DoWhenStringChangedWasCalled at the time this event is being rasied.
            doWhenStringChangedWasCalled = mod1.DoWhenStringChanged_WasCalled;
        }

        void Mod1_PropStringUseRefCompChanged(object sender, PcTypedEventArgs<string> e)
        {
            propStringOldVal = e.OldValue;
            propStringNewVal = e.NewValue;
            propStringWasUpdated = true;

            // Get the value of the DoWhenStringChangedWasCalled at the time this event is being rasied.
            doWhenStringChangedWasCalled = mod1.DoWhenStringChanged_WasCalled;
        }

        void Mod1_PropNullableIntChanged(object sender, PcTypedEventArgs<int?> e)
        {
            Nullable<int> oldVal = e.OldValue;
            Nullable<int> newValue = e.NewValue;

            propNullableInt_WasUpdated = true;
        }

        void Mod1_PropICollectionIntChanged(object sender, PcTypedEventArgs<ICollection<int>> e)
        {
            ICollection<int> oldValue = e.OldValue;
            ICollection<int> newValue = e.NewValue;

            propICollectionInt_WasUpdated = true;
        }
        #endregion

    }
}
