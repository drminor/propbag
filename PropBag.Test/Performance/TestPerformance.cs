using System;
using System.Collections.Generic;

using NUnit.Framework;


using DRM.PropBag;
using DRM.TypeSafePropertyBag;

namespace PropBagLib.Tests
{


    [TestFixtureAttribute]
    public class TestPerformance
    {

        public PerformanceModel mod1;

        bool varToEnsureWorkIsDone = false;

        const int InterationCount = 100000;


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
        public void SetInt1000Regular()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1.PropIntStandard = cntr;
            }
        }

        [Test]
        public void SetInt1000WithType()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1.PropInt = cntr;
            }
        }

        [Test]
        public void SetInt1000Index()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.Loose);
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1["System.Int32","PropInt"] = cntr;
            }
        }

        [Test]
        public void SetInt1000WithNoStore()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += Mod1_PropertyChangedNoStore;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                mod1.PropIntNoStore = cntr;
            }
        }

        [Test]
        public void SetInt1000UsingTypeProp()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += Mod1_PropertyChangedNoStore;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            IProp<int> typedProp = mod1.GetTypedProp<int>("PropInt");

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                typedProp.TypedValue = cntr;
            }
        }

        #region Same Tests but with String type

        [Test]
        public void SetString1000Regular()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
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
        public void SetString1000WithType()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

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
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.Loose);
            mod1.PropertyChanged += Mod1_PropertyChanged;
            mod1.PropertyChanged2 += Mod1_PropertyChanged2;

            string val;

            for (int cntr = 0; cntr < InterationCount - 1; cntr++)
            {
                val = cntr.ToString();
                mod1["System.String","PropString"] = val;
            }
        }

        [Test]
        public void SetString1000WithNoStore()
        {
            mod1 = PerformanceModel.Create(PropBagTypeSafetyMode.AllPropsMustBeRegistered);
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
