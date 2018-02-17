using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using PropBagLib.Tests.BusinessModel;
using PropBagLib.Tests.SpecBasedVMTests.BasicVM.Services;
using PropBagLib.Tests.SpecBasedVMTests.BasicVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using DRM.ObjectIdDictionary;

namespace PropBagLib.Tests.SpecBasedVMTests.BasicVM
{
    [TestFixture(TestName = "NoResources_Run1")]
    [NonParallelizable]
    public class AA_EmptyTest1 : BasicVM
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
        public void AA_EmptyTest1_NoResources()
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


    [TestFixture(TestName = "NoResources_Run2")]
    [NonParallelizable]
    public class AA_EmptyTest2 : BasicVM
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
        public void AA_EmptyTest2_NoResources_Run2()
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


    [TestFixture(TestName = "Regular Resource Set")]
    [NonParallelizable]
    public class AA_EmptyTest3 : BasicVM
    {
        protected override Action EstablishContext()
        {
            return base.EstablishContext();
        }

        protected override void Because_Of()
        {
        }

        [Test]
        public void AA_EmptyTest3_RegularResourceSet()
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
    public class BaseLine : BasicVM
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


    [TestFixture(TestName = "Load Simple Model From PersonDal.")]
    [NonParallelizable]
    public class GetPropModel : BasicVM
    {
        IPropModel personVM_PropModel = null;

        protected override void Because_Of()
        {
            personVM_PropModel =  PropModelProvider.GetPropModel("PersonVM");
        }

        [Test]
        public void SimpleModel_ThenGetPropModel()
        {
            Assert.That(personVM_PropModel != null, "personVM_PropModel is null.");
        }

        [Test]
        public void SimpleModel_PersonPropModelHasAllTheProps()
        {
            Assert.That(personVM_PropModel.Props.Count == 5);
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


    [TestFixture(TestName = "CreateVM_CreateMainWindowVM.")]
    [NonParallelizable]
    public class CreateVM : BasicVM
    {
        IPropModel mainWindowPropModel;
        MainWindowViewModel mainWindowViewModel; 
        

        protected override void Because_Of()
        {
            //ConfigPackageNameSuffix = "Emit_Proxy";
            mainWindowPropModel = PropModelProvider.GetPropModel("MainWindowVM");

            mainWindowViewModel = new MainWindowViewModel(mainWindowPropModel, PropStoreAccessService_Factory, AutoMapperProvider, propFactory: null, fullClassName: null);
        }

        [Test]
        public void CreateVM_ThenGetPropModel()
        {
            Assert.That(mainWindowPropModel != null, "mainWindowPropModel is null.");
        }

        [Test]
        public void CreateVM_MainWindowPropModelHasAllTheProps()
        {
            Assert.That(mainWindowPropModel.Props.Count == 4, "mainWindowPropModel does not have 4 PropItems.");
        }

        [Test]
        public void CreateVM_PersonCollectionViewModel_WasCreated()
        {
            PersonCollectionViewModel personCollectionViewModel = (PersonCollectionViewModel)mainWindowViewModel[typeof(PersonCollectionViewModel), "PersonCollectionVM"];

            Assert.That(personCollectionViewModel != null, "The personCollectionViewModel could not be found.");
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
