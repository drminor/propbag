using System;
using System.Collections.Generic;

using NUnit.Framework;


//using PropBagLib;
using DRM.PropBag;

namespace PropBagLib.Tests
{
    /// <summary>
    /// This class contains tests that directly use the PropBag class.
    /// Other classes contain tests that test classes that inherit from PropBag.
    /// 
    /// 
    /// </summary>

    [TestFixtureAttribute]
    public class TestSetAndGet
    {

        private SetAndGetModel mod1;

        [SetUp]
        public void Create()
        {
            // Create
            mod1 = new SetAndGetModel(PropBagTypeSafetyMode.OnlyTypedAccess);
        }

        [Test]
        public void ShouldSetAndRetrieveBool()
        {
            mod1.PropBool = true;
            Assert.That(mod1.PropBool, Is.EqualTo(true));
        }

        [Test]
        public void ShouldSetAndGetStringLiteral()
        {
            mod1.PropString = "Test";

            string temp = mod1.PropString;

            Assert.That(temp, Is.EqualTo("Test"));

            Assert.That(mod1.PropString, Is.EqualTo("Test"));
        }

        [Test]
        public void ShouldSetAndGetStringInterned()
        {
            string before = "Test1";
            string after = "Test1";

            mod1.PropString = before;
            Assert.That(mod1.PropString, Is.EqualTo(after));
        }

        [Test]
        public void ShouldSetAndGetStringBuilt()
        {
            string before = UnitTestHelpers.GetNewString("Hello");
            string after = UnitTestHelpers.GetNewString("Hello");

            mod1.PropString = before;
            Assert.That(mod1.PropString, Is.EqualTo(after));
        }

        [TearDown]
        public void destroy()
        {
            mod1 = null;
        }


    }
}
