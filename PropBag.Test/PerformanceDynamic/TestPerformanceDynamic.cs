using System;
using System.Collections.Generic;

using NUnit.Framework;

using DRM.PropBag; using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag;

namespace PropBagLib.Tests
{
    [TestFixtureAttribute]
    public class TestPerformanceDynamic
    {

        public PerformanceDynModel mod1;
        public dynamic mod1Dyn;

        bool varToEnsureWorkIsDone = false;

        const int InterationCount = 1000000;


        [SetUp]
        public void Create()
        {
        }

        [TearDown]
        public void Destroy()
        {
            mod1.ClearEventSubscribers();
            mod1 = null;
        }


        [Test]
        public void SetInt1000RegularDyn()
        {
            mod1Dyn = PerformanceDynModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1 = mod1Dyn as PerformanceDynModel;
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1.PropIntStandard = cntr;
            }
        }

        [Test]
        public void SetInt1000WithTypeDyn()
        {
            mod1Dyn = PerformanceDynModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1 = mod1Dyn as PerformanceDynModel;
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                // TODO: FIx Me.
                //mod1.PropIntND = cntr;
            }
        }

        [Test]
        public void SetInt1000WithDynProp()
        {
            mod1Dyn = PerformanceDynModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1 = mod1Dyn as PerformanceDynModel;
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1Dyn.PropInt = cntr;
            }
        }

        [Test]
        public void SetInt1000NoTypeDyn()
        {
            mod1Dyn = PerformanceDynModel.Create(PropBagTypeSafetyMode.None);
            mod1 = mod1Dyn as PerformanceDynModel;
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1[typeof(Int32), "PropInt"] = cntr;
            }
        }

        [Test]
        public void SetInt1000WithNoStoreDyn()
        {
            mod1Dyn = PerformanceDynModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1 = mod1Dyn as PerformanceDynModel;
            mod1.PropertyChanged += Mod1_PropertyChangedNoStore;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1.PropIntNoStore = cntr;
            }
        }

        #region Same Tests but with String type

        [Test]
        public void SetString1000RegularDyn()
        {
            mod1Dyn = PerformanceDynModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1 = mod1Dyn as PerformanceDynModel;
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            string val;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                val = cntr.ToString();
                mod1.PropStringStandard = val;
            }
        }

        [Test]
        public void SetString1000WithTypeDyn()
        {
            mod1Dyn = PerformanceDynModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1 = mod1Dyn as PerformanceDynModel;
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            string val;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                val = cntr.ToString();
                mod1[typeof(string), "PropString"] = val;
            }
        }


        [Test]
        public void SetString1000WithDynProp()
        {
            mod1Dyn = PerformanceDynModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1 = mod1Dyn as PerformanceDynModel;
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            string val;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                val = cntr.ToString();
                mod1Dyn.PropString = val;
            }
        }

        [Test]
        public void SetString1000IndexDyn()
        {
            mod1Dyn = PerformanceDynModel.Create(PropBagTypeSafetyMode.None);
            mod1 = mod1Dyn as PerformanceDynModel;
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            string val;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                val = cntr.ToString();
                mod1["PropString"] = val;
            }
        }


        [Test]
        public void SetString1000WithNoStoreDyn()
        {
            mod1Dyn = PerformanceDynModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1 = mod1Dyn as PerformanceDynModel;
            mod1.PropertyChanged += Mod1_PropertyChangedNoStore;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            string val;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                val = cntr.ToString();
                mod1.PropStringNoStore = val;
            }
        }

        #endregion


        void Mod1_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropInt")
            {
                varToEnsureWorkIsDone = !varToEnsureWorkIsDone;
            }
        }

        void Mod1_PropertyChangedNoStore(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropIntNoStore")
            {
                varToEnsureWorkIsDone = !varToEnsureWorkIsDone;
            }
        }

        void Mod1_PropertyChanged2(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropIntStandard")
            {
                varToEnsureWorkIsDone = !varToEnsureWorkIsDone;
            }
        }

    }
}
