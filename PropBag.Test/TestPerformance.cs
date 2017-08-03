using System;
using System.Collections.Generic;

using NUnit.Framework;


using DRM.PropBag;
using DRM.Ipnwvc;

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

        private PerformanceModel mod1;

        bool varToEnsureWorkIsDone = false;

        const int InterationCount = 1000000;


        [SetUp]
        public void Create()
        {
        }

        [TearDown]
        public void destroy()
        {
            mod1 = null;
        }


        [Test]
        public void SetInt1000Regular()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += mod1_PropertyChanged;
            mod1.PropertyChanged2 += mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1.PropIntStandard = cntr;
            }
        }

        [Test]
        public void SetInt1000WithType()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += mod1_PropertyChanged;
            mod1.PropertyChanged2 += mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1.PropInt = cntr;
            }
        }

        [Test]
        public void SetInt1000NoType()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += mod1_PropertyChanged;
            mod1.PropertyChanged2 += mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1["PropInt"] = cntr;
            }
        }

        [Test]
        public void SetInt1000WithNoStore()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += mod1_PropertyChangedNoStore;
            mod1.PropertyChanged2 += mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1.PropIntNoStore = cntr;
            }
        }

        #region Same Tests but with String type

        [Test]
        public void SetString1000Regular()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += mod1_PropertyChanged;
            mod1.PropertyChanged2 += mod1_PropertyChanged2;

            string val;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                val = cntr.ToString();
                mod1.PropStringStandard = val;
            }
        }

        [Test]
        public void SetString1000WithType()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += mod1_PropertyChanged;
            mod1.PropertyChanged2 += mod1_PropertyChanged2;

            string val;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                val = cntr.ToString();
                mod1.PropString = val;
            }
        }

        [Test]
        public void SetString1000NoType()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += mod1_PropertyChanged;
            mod1.PropertyChanged2 += mod1_PropertyChanged2;

            string val;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                val = cntr.ToString();
                mod1["PropString"] = val;
            }
        }

        [Test]
        public void SetString1000WithNoStore()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += mod1_PropertyChangedNoStore;
            mod1.PropertyChanged2 += mod1_PropertyChanged2;

            string val;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                val = cntr.ToString();
                mod1.PropStringNoStore = val;
            }
        }

        #endregion


        void mod1_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropInt")
            {
                varToEnsureWorkIsDone = !varToEnsureWorkIsDone;
            }
        }

        void mod1_PropertyChangedNoStore(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropIntNoStore")
            {
                varToEnsureWorkIsDone = !varToEnsureWorkIsDone;
            }
        }

        void mod1_PropertyChanged2(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropIntStandard")
            {
                varToEnsureWorkIsDone = !varToEnsureWorkIsDone;
            }
        }

    }
}
