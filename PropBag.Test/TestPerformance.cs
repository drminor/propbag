using System;
using System.Collections.Generic;

using NUnit.Framework;


using DRM.PropBag;
using DRM.Ipnwv;

namespace PropBagLib.Tests
{
    /// <summary>
    /// This class contains tests that directly use the PropBag class.
    /// Other classes contain tests that test classes that inherit from PropBag.
    /// 
    /// 
    /// </summary>

    [TestFixtureAttribute]
    public class TestPerformance
    {
        //const string PROP_BOOL = "PropBool";
        //const string PROP_STRING = "PropString";
        //const string PROP_NEW = "PropNotDeclared";

        private LooseModel mod1;
        //bool PropStringChangeWasCalled = false;

        [OneTimeSetUp]
        //[SetUp]
        public void Create()
        {
            mod1 = new LooseModel();
            // Create
            //mod1 = new SandGLoosetModel(PropBagTypeSafetyMode.Loose);
            //mod1.SubscribeToPropStringChanged(DoWhenUpdatedExt);

            //mod1.PropStringChanged += mod1_PropStringChanged;

        }

        //[Test]
        //public void SetInt1000WithType()
        //{
        //    for (int cntr = 0; cntr < 999; cntr++)
        //    {
        //        mod1.PropInt = cntr;
        //    }
        //}

        //[Test]
        //public void SetInt1000NoType()
        //{
        //    for (int cntr = 0; cntr < 999; cntr++)
        //    {
        //        mod1["PropInt1"] = cntr;
        //    }
        //}


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
