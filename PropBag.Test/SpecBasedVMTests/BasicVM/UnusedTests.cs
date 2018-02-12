using NUnit.Framework;
using System;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM
{
    //[TestFixture(Ignore = "Not really a test, per se. It is used to see if the Base Resource Set can be allocated over and over without leaking memory.", TestName = "Regular Resource Set Memory Leak Test")]

    [TestFixture(TestName = "Regular Resource Set Memory Leak Test")]
    [NonParallelizable]
    public class AA_EmptyTest4 : BasicVM
    {
        protected override Action EstablishContext()
        {
            return base.EstablishContext();
        }

        protected override void Because_Of()
        {
        }

        [Test]
        public void AA_EmptyTest4_RegularResourceSet_Run0()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [Test]
        public void AA_EmptyTest4_RegularResourceSet_Run1()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [Test]
        public void AA_EmptyTest4_RegularResourceSet_Run2()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [Test]
        public void AA_EmptyTest4_RegularResourceSet_Run3()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [Test]
        public void AA_EmptyTest4_RegularResourceSet_Run4()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [Test]
        public void AA_EmptyTest4_RegularResourceSet_Run5()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [Test]
        public void AA_EmptyTest4_RegularResourceSet_Run6()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [Test]
        public void AA_EmptyTest4_RegularResourceSet_Run7()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [Test]
        public void AA_EmptyTest4_RegularResourceSet_Run8()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [Test]
        public void AA_EmptyTest4_RegularResourceSet_Run9()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        [SetUp]
        public void OneTimeSetup()
        {
            MemTrackerStartMessage = $"Starting Mem Tracking for {GetType().GetTestName()}.";
            CallContextEstablisher();
            CallBecause_Of();
        }

        [TearDown]
        public void OneTimeTearDown()
        {
            CallContextDestroyer();
        }

        public string TestName
        {
            get
            {
                return GetType().GetTestName();
            }
        }
    }

}
