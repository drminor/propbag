using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;

//using PropBagLib;
using DRM.PropBag;


namespace PropBagLib.Tests
{
    /// <summary>
    /// 
    /// 
    /// </summary>

    [TestFixtureAttribute]
    public class TestNullable : PropBag
    {

        private NullableModel mod1;

        bool propNullableIntChanged = false;

        [OneTimeSetUp]
        public void Create()
        {
            // Create
            mod1 = new NullableModel(PropBagTypeSafetyMode.AllPropsMustBeRegistered);

            mod1.PropNullableIntChanged += mod1_PropNullableIntChanged;
        }

        void mod1_PropNullableIntChanged(object sender, DRM.Ipnwv.PropertyChangedWithTValsEventArgs<int?> e)
        {
            Nullable<int> oldVal = e.OldValue;
            Nullable<int> newValue = e.NewValue;

            propNullableIntChanged = true;
        }

        [Test]
        public void ShouldSetAndRetrieveNullableInt()
        {
            Assert.That(mod1.PropNullableInt, Is.Null);

            mod1.ItGotUpdated = false; 
            mod1.PropNullableInt = 0;
            Assert.That(mod1.PropNullableInt, Is.EqualTo(0));
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(true));
            Assert.That(propNullableIntChanged, Is.EqualTo(true), "propNullableIntChanged = false");

            mod1.ItGotUpdated = false;
            mod1.PropNullableInt = new Nullable<int>(1);
            Assert.That(mod1.PropNullableInt, Is.EqualTo(1));
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(true));

            mod1.ItGotUpdated = false; 
            mod1.PropNullableInt = null;
            Assert.That(mod1.PropNullableInt, Is.Null);
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(true));

            int? test = new Nullable<int>();

            mod1.ItGotUpdated = false; 
            mod1.PropNullableInt = test;
            Assert.That(mod1.PropNullableInt, Is.EqualTo(test));
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(false));
        }

        [Test]
        public void ShouldSetAndRetrieveICollectionInt()
        {
            //Assert.That(mod1.PropNullableInt, Is.Null);

            //mod1.ItGotUpdated = false;

            ICollection<int> before = new Collection<int>();

            mod1.PropICollectionInt = before;
            Assert.That(mod1.PropICollectionInt, Is.EqualTo(before));
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(true));

            mod1.ItGotUpdated = false;
            mod1.PropICollectionInt = before;
            Assert.That(mod1.PropICollectionInt, Is.EqualTo(before));
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(false));

            Collection<int> newVal = new Collection<int>();

            mod1.ItGotUpdated = false;
            mod1.PropICollectionInt = newVal;
            Assert.That(mod1.PropICollectionInt, Is.EqualTo(newVal));
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(true));

            //TODO: This shouldn't be here : it is testing setting a property that has not been defined with AddProp.
            mod1.ItGotUpdated = false;
            mod1["PropICollectionInt"] = newVal;
            Assert.That(mod1.PropICollectionInt, Is.EqualTo(newVal));
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(false));

        }

        [OneTimeTearDown]
        public void destroy()
        {
            mod1 = null;
        }

    }
}

