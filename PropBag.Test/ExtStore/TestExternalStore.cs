using System;
using System.Collections.Generic;

using NUnit.Framework;


using DRM.PropBag;
using DRM.Inpcwv;

namespace PropBagLib.Tests
{


    [TestFixtureAttribute]
    public class TestExternalStore
    {

        private ExtStoreModel mod1;

        bool varToEnsureWorkIsDone = false;

        int upCntr;



        [SetUp]
        public void Create()
        {
            upCntr = 0;
            object stuff = new object();
            PropExtStoreFactory factory = new PropExtStoreFactory(stuff);

            mod1 = ExtStoreModel.Create(factory);

        }

        [TearDown]
        public void destroy()
        {
            mod1 = null;
        }


        // TODO: Fix Me: You cannot set the value of a property that is created by the PropExtStoreFactory.
        //[Test]
        public void ExtStorePropsBasicSets()
        {

            mod1.PropIntChanged += mod1_PropIntChanged;
            mod1.PropStringChanged += mod1_PropStringChanged;

            mod1.PropInt = 2;
            mod1.PropInt = 10;

            mod1.PropString = "Hello";
            mod1.PropString = "Far out!";

            string temp = mod1.PropString;

            Assert.That(upCntr, Is.EqualTo(4), "Expected there to be 4 updates.");

        }

        void mod1_PropIntChanged(object sender, PropertyChangedWithTValsEventArgs<int> e)
        {
            varToEnsureWorkIsDone = !varToEnsureWorkIsDone;
            upCntr++;
        }

        void mod1_PropStringChanged(object sender, PropertyChangedWithTValsEventArgs<string> e)
        {
            varToEnsureWorkIsDone = !varToEnsureWorkIsDone;
            upCntr++;
        }
    }
}
