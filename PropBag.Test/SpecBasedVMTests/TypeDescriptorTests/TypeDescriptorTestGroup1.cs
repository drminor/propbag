using DRM.TypeSafePropertyBag;
using DRM.TypeSafePropertyBag.Fundamentals;
using NUnit.Framework;
using PropBagLib.Tests.SpecBasedVMTests.BasicVM.Services;
using PropBagLib.Tests.SpecBasedVMTests.BasicVM.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PropBagLib.Tests.SpecBasedVMTests.TypeDescriptorTests
{
    using PropModelType = IPropModel<String>;

    [TestFixture(TestName = "CreateDestroyBasicVM")]
    [NonParallelizable]
    public class CreateDestroyBasicVM : TypeDescriptorSetup
    {
        PropModelType mainWindowPropModel = null;
        MainWindowViewModel mainWindowViewModel;
        //MainWindowViewModel mainWindowViewModel2;


        protected override Action EstablishContext()
        {
            ConfigPackageNameSuffix = "Emit";
            //ConfigPackageNameSuffix = "Extra";

            return base.EstablishContext();
        }

        protected override void Because_Of()
        {
            //mainWindowPropModel = PropModelCache.GetPropModel("MainWindowVM");

            string className = "MainWindowVM";
            string fcn = GetFullClassName(DefaultNamespace, className, ConfigPackageNameSuffix);

            if (PropModelCache.TryGetPropModel(fcn, out PropModelType mainWindowPropModel))
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fcn}.");
            }

            BaseMemTracker.CompactMeasureAndReport("After get mainWindow_PropModel.", "CreateDestroyBasicVM");

            // To see how much memory is not being cleaned up after one is created and then disposed.
            mainWindowViewModel = new MainWindowViewModel(mainWindowPropModel, PropStoreAccessService_Factory, AutoMapperProvider, propFactory: null, fullClassName: null);
            BaseMemTracker.CompactMeasureAndReport("After create the first mainWindowViewModel.", "CreateDestroyBasicVM");

            //mainWindowViewModel.Dispose();
            //BaseMemTracker.CompactMeasureAndReport("After dispose of the first mainWindowViewModel.", "CreateDestroyBasicVM");

            //mainWindowViewModel2 = new MainWindowViewModel(mainWindowPropModel, PropStoreAccessService_Factory, AutoMapperProvider, propFactory: null, fullClassName: null);
            //BaseMemTracker.CompactMeasureAndReport("After create the second mainWindowViewModel.", "CreateDestroyBasicVM");

            //// And here.
            //mainWindowViewModel2.Dispose();
            //BaseMemTracker.CompactMeasureAndReport("After dispose of the second mainWindowViewModel.", "CreateVM_CreateMainWindowVM_Run2");
        }

        [Test]
        public void CreateDestroyBasicVM_ThenGetPropModel()
        {
            Assert.That(mainWindowPropModel != null, "mainWindowPropModel is null.");
        }

        [Test]
        public void CreateDestroyBasicVM_MainWindowPropModelHasAllTheProps()
        {
            Assert.That(mainWindowPropModel.Count == 4, "mainWindowPropModel does not have 4 PropItems.");
        }

        [Test]
        public void CreateDestroyBasicVM_PersonVM_HasPropDescriptors()
        {
            PersonCollectionViewModel personCollectionViewModel = (PersonCollectionViewModel)mainWindowViewModel[typeof(PersonCollectionViewModel), "PersonCollectionVM"];
            Assert.That(personCollectionViewModel != null, "The personCollectionViewModel could not be found.");

            bool gotTheViewManager = mainWindowViewModel.TryGetViewManager("Business", typeof(PersonDAL), out IManageCViews cViewManager);

            Assert.That(gotTheViewManager, "TryGetViewManager failed for the Business property.");

            Assert.That(cViewManager != null, "The CollectionViewManager for the PersonListView property is null.");

            Type rtt = cViewManager.CollectionItemRunTimeType;

            IList<string> pNamesFromType = TypeInspectorUtility.GetPropertyNames(rtt);
            IList<string> pNamesFromInstance_WT;
            IList<string> pNamesFromInstance;


            ICollectionView cv = cViewManager.GetDefaultCollectionView();

            if(!cv.IsEmpty)
            {
                cv.MoveCurrentToFirst();
                object x = cv.CurrentItem;
                pNamesFromInstance_WT = TypeInspectorUtility.GetPropertyNames(rtt, x);
                pNamesFromInstance = TypeInspectorUtility.GetPropertyNames(x);
            }

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
            mainWindowViewModel.Dispose();
            CallContextDestroyer();
        }
    }



}
