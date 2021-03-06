﻿
using DRM.PropBag;
using DRM.PropBag.Caches;
using DRM.TypeSafePropertyBag; using Swhp.Tspb.PropBagAutoMapperService;

using NUnit.Framework;
using System;

namespace PropBagLib.Tests
{
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

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
            AutoMapperSupport.AutoMapperHelpers mapperHelpers = new AutoMapperSupport.AutoMapperHelpers();

            IPropFactory standardPropFactory = mapperHelpers.PropFactory_V1;

            IPropFactory factory = mapperHelpers.PropFactoryExt_V1;

            PSAccessServiceCreatorInterface storeAccessCreator = mapperHelpers.StoreAccessCreator;

            mod1 = ExtStoreModel.Create(storeAccessCreator, factory);
        }

        [TearDown]
        public void Destroy()
        {
            mod1 = null;
        }


        // TODO: Fix Me: You cannot set the value of a property that is created by the PropExtStoreFactory.
        //[Test]
        public void ExtStorePropsBasicSets()
        {

            mod1.PropIntChanged += Mod1_PropIntChanged;
            mod1.PropStringChanged += Mod1_PropStringChanged;

            mod1.PropInt = 2;
            mod1.PropInt = 10;

            mod1.PropString = "Hello";
            mod1.PropString = "Far out!";

            string temp = mod1.PropString;

            Assert.That(upCntr, Is.EqualTo(4), "Expected there to be 4 updates.");

        }

        void Mod1_PropIntChanged(object sender, PcTypedEventArgs<int> e)
        {
            varToEnsureWorkIsDone = !varToEnsureWorkIsDone;
            upCntr++;
        }

        void Mod1_PropStringChanged(object sender, PcTypedEventArgs<string> e)
        {
            varToEnsureWorkIsDone = !varToEnsureWorkIsDone;
            upCntr++;
        }
    }
}
