using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using PropBagLib.Tests.BusinessModel;
using PropBagLib.Tests.SpecBasedVMTests.BasicVM.Services;
using PropBagLib.Tests.SpecBasedVMTests.BasicVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;


    [TestFixture(TestName = "AAA No Resources Run1")]
    [NonParallelizable]
    public class AAEmptyTest1 : BasicVMSetup
    {
        // Override the base EstablishContext so that no resources are used (and the cleanup action is null.)
        protected override Action EstablishContext()
        {
            return null;
        }

        protected override void Because_Of()
        {
        }

        [Test]
        public void AAEmptyTest1_NoResources()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        //protected void DoOn

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

    [TestFixture(TestName = "ZZZ No Resources End Run")]
    [NonParallelizable]
    public class ZZEmptyTest1 : BasicVMSetup
    {
        // Override the base EstablishContext so that no resources are used (and the cleanup action is null.)
        protected override Action EstablishContext()
        {
            // Take Heap SnapShot here. (CreateVM_CreateMainWindowVM_Run2 and Run3 have completed, since the first SnapShot and the Context has been cleaned up.)

            BaseMemTracker.CompactMeasureAndReport("Here at ZZZ No Resources End Run -- EstablishContext.", "CreateVM_CreateMainWindowVM_Run1");

            return null;
        }

        protected override void Because_Of()
        {
        }

        [Test]
        public void ZZEmptyTest1_NoResources()
        {
            Assert.That(1 == 1, "One does not equal one.");
        }

        //protected void DoOn

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


    [TestFixture(TestName = "AAA No Resources Run2")]
    [NonParallelizable]
    public class AAEmptyTest2 : BasicVMSetup
    {
        // Override the base EstablishContext so that no resources are used (and the cleanup action is null.)
        protected override Action EstablishContext()
        {
            return null;
        }

        protected override void Because_Of()
        {
        }

        [Test]
        public void AAEmptyTest2_NoResources_Run2()
        {
            Assert.That(1 == 1, "One does not equal one.");
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


    [TestFixture(TestName = "AAA Regular Resource Set")]
    [NonParallelizable]
    public class AAEmptyTest3 : BasicVMSetup
    {
        protected override Action EstablishContext()
        {
            return base.EstablishContext();
        }

        protected override void Because_Of()
        {
        }

        [Test]
        public void AAEmptyTest3_RegularResourceSet()
        {
            Assert.That(1 == 1, "One does not equal one.");
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


    [TestFixture(TestName = "BaseLine (New Business, Get 200 Person Records.)")]
    [NonParallelizable]
    public class BaseLine : BasicVMSetup
    {
        PersonDAL dal = null;
        List<Person> personList = null;

        protected override void Because_Of()
        {
            // This is run first, to get the database "fired up."
            dal = new PersonDAL();
            personList = dal.Get(200).ToList();
        }

        [Test]
        public void BaseLine_ThenTheDALIsWorking()
        {
            Assert.That(dal != null, $"dal of type: {dal.GetType()} is null.");
            Assert.That(personList != null, "The PersonList is null.");
            Assert.That(personList.Count >= 200, "The PersonList contains fewer that 200 items.");
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


    [TestFixture(TestName = "Load Simple Model From PersonDal")]
    [NonParallelizable]
    public class BBGetPropModel : BasicVMSetup
    {
        PropModelType personVM_PropModel = null;

        protected override void Because_Of()
        {
            //personVM_PropModel = PropModelCache.GetPropModel("PersonVM");

            string className = "PersonVM";
            string fcn = GetResourceKeyWithSuffix(className, ConfigPackageNameSuffix);

            if (PropModelCache.TryGetPropModel(fcn, out PropModelType personVM_PropModel))
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fcn}.");
            }

        }

        [Test]
        public void BBGetPropModel_ThenWeHaveThePropModel()
        {
            Assert.That(personVM_PropModel != null, "personVM_PropModel is null.");
        }

        [Test]
        public void BBGetPropModel_PersonPropModelHasAllTheProps()
        {
            Assert.That(personVM_PropModel.Count == 5);
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


    [TestFixture(TestName = "CreateVM_CreateMainWindowVM_Run1")]
    [NonParallelizable]
    public class CreateVM : BasicVMSetup
    {
        PropModelType mainWindowPropModel = null;
        MainWindowViewModel mainWindowViewModel;

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
            string fcn = GetResourceKeyWithSuffix(className, ConfigPackageNameSuffix);

            if (PropModelCache.TryGetPropModel(fcn, out PropModelType mainWindowPropModel))
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fcn}.");
            }

            BaseMemTracker.CompactMeasureAndReport("After get mainWindow_PropModel.", "CreateVM_CreateMainWindowVM_Run1");

            mainWindowViewModel = new MainWindowViewModel(mainWindowPropModel, PropStoreAccessService_Factory, AutoMapperProvider, propFactory: null, fullClassName: null);
            BaseMemTracker.CompactMeasureAndReport("After create the mainWindowViewModel.", "CreateVM_CreateMainWindowVM_Run1");

            mainWindowViewModel.Dispose();
            BaseMemTracker.CompactMeasureAndReport("After dispose of the mainWindowViewModel.", "CreateVM_CreateMainWindowVM_Run1");
        }

        [Test]
        public void CreateVM_ThenGetPropModel()
        {
            Assert.That(mainWindowPropModel != null, "mainWindowPropModel is null.");
        }

        [Test]
        public void CreateVM_MainWindowPropModelHasAllTheProps()
        {
            Assert.That(mainWindowPropModel.Count == 4, "mainWindowPropModel does not have 4 PropItems.");
        }

        //[Test]
        //public void CreateVM_PersonCollectionViewModel_WasCreated()
        //{
        //    PersonCollectionViewModel personCollectionViewModel = (PersonCollectionViewModel)mainWindowViewModel[typeof(PersonCollectionViewModel), "PersonCollectionVM"];

        //    Assert.That(personCollectionViewModel != null, "The personCollectionViewModel could not be found.");
        //}

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

    [TestFixture(TestName = "CreateVM_CreateMainWindowVM_Run2")]
    [NonParallelizable]
    public class CreateVM2 : BasicVMSetup
    {
        PropModelType mainWindowPropModel = null;
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
            //mainWindowPropModel = PropModelCache.GetPropModel("MainWindowVM");

            string className = "MainWindowVM";
            string fcn = GetResourceKeyWithSuffix(className, ConfigPackageNameSuffix);

            if (PropModelCache.TryGetPropModel(fcn, out PropModelType mainWindowPropModel))
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fcn}.");
            }
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

    [TestFixture(TestName = "CreateVM_CreateMainWindowVM_Run3")]
    [NonParallelizable]
    public class CreateVM3 : BasicVMSetup
    {
        PropModelType mainWindowPropModel = null;
        MainWindowViewModel mainWindowViewModel;

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
            string fcn = GetResourceKeyWithSuffix(className, ConfigPackageNameSuffix);

            if (PropModelCache.TryGetPropModel(fcn, out PropModelType mainWindowPropModel))
            {
                throw new KeyNotFoundException($"Could not find a PropModel with Full Class Name = {fcn}.");
            }
            BaseMemTracker.CompactMeasureAndReport("After get mainWindow_PropModel.", "CreateVM_CreateMainWindowVM_Run1");

            mainWindowViewModel = new MainWindowViewModel(mainWindowPropModel, PropStoreAccessService_Factory, AutoMapperProvider, propFactory: null, fullClassName: null);
            BaseMemTracker.CompactMeasureAndReport("After create the mainWindowViewModel.", "CreateVM_CreateMainWindowVM_Run1");

            mainWindowViewModel.Dispose();
            BaseMemTracker.CompactMeasureAndReport("After dispose of the mainWindowViewModel.", "CreateVM_CreateMainWindowVM_Run1");
        }

        [Test]
        public void CreateVM3_ThenGetPropModel()
        {
            Assert.That(mainWindowPropModel != null, "mainWindowPropModel is null.");
        }

        [Test]
        public void CreateVM3_MainWindowPropModelHasAllTheProps()
        {
            Assert.That(mainWindowPropModel.Count == 4, "mainWindowPropModel does not have 4 PropItems.");
        }

        [Test]
        public void CreateVM3_PersonCollectionViewModel_WasCreated()
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
