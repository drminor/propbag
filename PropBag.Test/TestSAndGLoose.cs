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
    public class TestSAndGLoose
    {
        const string PROP_BOOL = "PropBool";
        const string PROP_STRING = "PropString";
        const string PROP_NEW = "PropNotDeclared";

        private SandGLoosetModel mod1;

        [OneTimeSetUp]
        //[SetUp]
        public void Create()
        {
            // Create
            mod1 = new SandGLoosetModel(PropBagTypeSafetyMode.Loose);
            mod1.SubscribeToPropStringChanged = this.DoWhenUpdatedExt;
        }



        [Test]
        public void ShouldSAndGLooseBool()
        {
            mod1[PROP_BOOL] = true;
            Assert.That(mod1[PROP_BOOL], Is.EqualTo(true));
        }

        [Test]
        public void ShouldSAndGLooseString()
        {
            mod1[PROP_STRING] = "Test2";

            string temp = (string) mod1[PROP_STRING];
            Assert.That(temp, Is.EqualTo("Test2"));
            Assert.That(mod1[PROP_STRING], Is.EqualTo("Test2"));
        }

        [Test]
        public void ShouldSAndGLooseUseNewProp()
        {
            mod1[PROP_NEW] = "string";
            Assert.That(mod1.GetTypeOfProperty(PROP_NEW), Is.EqualTo(typeof(string)));
            Assert.That(mod1[PROP_NEW], Is.EqualTo("string"));
        }

        [Test]
        public void ShouldSAndGLooseUseNew1stNullNotOk()
        {
            mod1[PROP_NEW] = null;
            Assert.That(mod1.GetTypeOfProperty(PROP_NEW), Is.EqualTo(typeof(object)));

            mod1[PROP_NEW] = "string";
            Assert.That(mod1.GetTypeOfProperty(PROP_NEW), Is.EqualTo(typeof(string)));
            Assert.That(mod1[PROP_NEW], Is.EqualTo("string"));
        }

        [Test]
        public void ShouldSAndGLooseUseNew2ndNullOk()
        {
            mod1[PROP_NEW] = "string";
            mod1[PROP_NEW] = null;
            Assert.That(mod1.GetTypeOfProperty(PROP_NEW), Is.EqualTo(typeof(string)));
            Assert.That(mod1[PROP_NEW], Is.Null);
        }

        [Test]
        public void ShouldGetKeyNotFoundEx()
        {
            object x;
            Assert.Throws<KeyNotFoundException>(() => x = mod1["x"]);
        }

        [Test]
        public void ShouldGetArgumentNullEx()
        {
            object x;
            Assert.Throws<ArgumentNullException>(() => x = mod1[null]);
        }

        [Test]
        public void SetInt1000WithType()
        {
            for (int cntr = 0; cntr < 999; cntr++)
            {
                mod1.PropInt = cntr;
            }
        }

        [Test]
        public void SetInt1000NoType()
        {
            for (int cntr = 0; cntr < 999; cntr++)
            {
                mod1["PropInt1"] = cntr;
            }
        }


        [OneTimeTearDown]
        //[TearDown]
        public void destroy()
        {
            mod1 = null;
        }

        private void DoWhenUpdatedExt(string oldVal, string newVal)
        {
            //ItGotUpdated = true;
        }

    }
}
