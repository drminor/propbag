using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using PropBagLib.Tests.BusinessModel;
using PropBagLib.Tests.SpecBasedVMTests.BasicVM.Services;
using PropBagLib.Tests.SpecBasedVMTests.BasicVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PropBagLib.Tests.SpecBasedVMTests.TypeDescriptorTests
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    [TestFixture(TestName = "CreateDestroyCreateBasicVM")]
    [NonParallelizable]
    public class CreateDestroyCreateBasicVM : TypeDescriptorSetup
    {
        PropModelType mainWindowPropModel;
        MainWindowViewModel mainWindowViewModel;

        protected override Action EstablishContext()
        {
            // Take Heap SnapShot here. (CreateVM_CreateMainWindowVM_Run1 has completed and the Context has been cleaned up.)

            //ConfigPackageNameSuffix = "Emit_Proxy";
            ConfigPackageNameSuffix = "Extra_Members";

            return base.EstablishContext();
        }

        protected override void Because_Of()
        {
            mainWindowPropModel = PropModelCache.GetPropModel("MainWindowVM");
            BaseMemTracker.CompactMeasureAndReport("After get mainWindow_PropModel.", "CreateVM_CreateMainWindowVM_Run2");

            // To see how much memory is not being cleaned up after one is created and then disposed.
            mainWindowViewModel = new MainWindowViewModel(mainWindowPropModel, PropStoreAccessService_Factory, AutoMapperProvider, propFactory: null, fullClassName: null);
            BaseMemTracker.CompactMeasureAndReport("After create the first mainWindowViewModel.", "CreateVM_CreateMainWindowVM_Run2");

            mainWindowViewModel.Dispose();
            BaseMemTracker.CompactMeasureAndReport("After dispose of the first mainWindowViewModel.", "CreateVM_CreateMainWindowVM_Run2");

            mainWindowViewModel = new MainWindowViewModel(mainWindowPropModel, PropStoreAccessService_Factory, AutoMapperProvider, propFactory: null, fullClassName: null);
            BaseMemTracker.CompactMeasureAndReport("After create the second mainWindowViewModel.", "CreateVM_CreateMainWindowVM_Run2");

            // And here.
            mainWindowViewModel.Dispose();
            BaseMemTracker.CompactMeasureAndReport("After dispose of the second mainWindowViewModel.", "CreateVM_CreateMainWindowVM_Run2");
        }

        [Test]
        public void CreateVM2_ThenGetPropModel()
        {
            Assert.That(mainWindowPropModel != null, "mainWindowPropModel is null.");
        }

        [Test]
        public void CreateVM2_MainWindowPropModelHasAllTheProps()
        {
            Assert.That(mainWindowPropModel.Count == 4, "mainWindowPropModel does not have 4 PropItems.");
        }

        [Test]
        public void CreateVM2_PersonCollectionViewModel_WasCreated()
        {
            //PersonCollectionViewModel personCollectionViewModel = (PersonCollectionViewModel)mainWindowViewModel[typeof(PersonCollectionViewModel), "PersonCollectionVM"];

            //Assert.That(personCollectionViewModel != null, "The personCollectionViewModel could not be found.");
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
            CallContextDestroyer();
        }
    }



}
