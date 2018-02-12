using NUnit.Framework;
using PropBagLib.Tests.BusinessModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM
{
    [TestFixture(TestName = "NoResources_Run1")]
    [NonParallelizable]
    public class AA_EmptyTest1 : BasicVM
    {
        // Override the base EstablishContext so that no resources are used (and the cleanup action is null.)
        protected override Action EstablishContext()
        {
            return null;
        }

        protected override void Because_Of()
        {
        }

        [Test]
        public void AA_EmptyTest1_NoResources()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        //protected void DoOn

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            MemTrackerStartMessage = $"Starting Mem Tracking for {GetType().GetTestName()}.";
            CallContextEstablisher();
            CallBecause_Of();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            CallContextDestroyer();
        }
    }

    [TestFixture(TestName = "NoResources_Run2")]
    [NonParallelizable]
    public class AA_EmptyTest2 : BasicVM
    {
        // Override the base EstablishContext so that no resources are used (and the cleanup action is null.)
        protected override Action EstablishContext()
        {
            return null;
        }

        protected override void Because_Of()
        {
        }

        [Test]
        public void AA_EmptyTest2_NoResources_Run2()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            MemTrackerStartMessage = $"Starting Mem Tracking for {GetType().GetTestName()}.";
            CallContextEstablisher();
            CallBecause_Of();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            CallContextDestroyer();
        }
    }

    [TestFixture(TestName = "Regular Resource Set")]
    [NonParallelizable]
    public class AA_EmptyTest3 : BasicVM
    {
        protected override Action EstablishContext()
        {
            return base.EstablishContext();
        }

        protected override void Because_Of()
        {
        }

        [Test]
        public void AA_EmptyTest3_RegularResourceSet()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            MemTrackerStartMessage = $"Starting Mem Tracking for {GetType().GetTestName()}.";
            CallContextEstablisher();
            CallBecause_Of();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            CallContextDestroyer();
        }
    }


    [TestFixture(TestName = "BaseLine (New Business, Get 200 Person Records.)")]
    [NonParallelizable]
    public class BaseLine : BasicVM
    {
        Business b;
        List<Person> personList;

        protected override void Because_Of()
        {
            // This is run first, to get the database "fired up."
            b = new Business();
            personList = b.Get(200).ToList();
        }

        [Test]
        public void BaseLine_ThenTheDALIsWorking()
        {
            Assert.That(b != null, "Business is null.");
            Assert.That(personList != null, "The PersonList is null.");
            Assert.That(personList.Count >= 200, "The PersonList contains fewer that 200 items.");
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            MemTrackerStartMessage = $"Starting Mem Tracking for {GetType().GetTestName()}.";
            CallContextEstablisher();
            CallBecause_Of();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            CallContextDestroyer();
        }
    }

}
