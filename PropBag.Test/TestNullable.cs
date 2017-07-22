using System;
using System.Collections.Generic;

using NUnit.Framework;

using PropBagLib;
using System.Collections.ObjectModel;

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

        [OneTimeSetUp]
        public void Create()
        {
            // Create
            mod1 = new NullableModel(PropBagTypeSafetyModeEnum.AllPropsMustBeRegistered);
        }

        [Test]
        public void ShouldSetAndRetrieveNullableInt()
        {
            Assert.That(mod1.PropNullableInt, Is.Null);

            mod1.ItGotUpdated = false; 
            mod1.PropNullableInt = 0;
            Assert.That(mod1.PropNullableInt, Is.EqualTo(0));
            Assert.That(mod1.ItGotUpdated, Is.EqualTo(true));

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

