using DRM.PropBag;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using PropBagLib.Tests.AutoMapperSupport;
using PropBagLib.Tests.BusinessModel;
using Swhp.Tspb.PropBagAutoMapperService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PropBagLib.Tests.PerformanceDb
{
    using PropModelCacheInterface = ICachePropModels<String>;
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    [TestFixture]
    public class TestPerformanceDb
    {
        const int NUMBER_OF_PEOPLE = 1000;

        IPropBagMapperService _amp;
        AutoMapperHelpers _ourHelper;
        IPropFactory _propFactory_V1;
        PropModelCacheInterface _propModelCache;

        PropModelHelpers _pmHelpers;


        [OneTimeSetUp]
        public void SetUpOneTime()
        {
            //_ourHelper = new AutoMapperHelpers();
            
            //Business b = new Business();
            //List<Person> personList = b.Get(1).ToList();

            //if(personList.Count == 0)
            //{
            //    PopDatabase(NUMBER_OF_PEOPLE);
            //}
        }

        [SetUp]
        public void SetUpForEach()
        {
            // Get a brand new propFactory instance for each test.
            //AutoMapperHelpers  _ourHelper = new AutoMapperHelpers();
            //_propFactory_V1 = _ourHelper.GetNewPropFactory_V1();
        }

        //[Test]
        //public void Can_ConvertIObsColl_to_ObservableColl()
        //{
        //    ObservableCollection<int> ocIn = new ObservableCollection<int>();

        //    IObsCollection<int> dest;

        //    dest = (IObsCollection<int>) ocIn;

        //    ObservableCollection<int> ocOut = (ObservableCollection<int>) dest;
        //}

        [Test]
        public void A_ReadIntoMappedArray()
        {
            // This is run first, to get the database "fired up."
            Business b = new Business();
            List<Person> personList = b.Get(1000).ToList();
        }

        [Test]
        public void ReadIntoNativeArray()
        {
            Business b = new Business();

            //List<Person> personList = b.Get(1000).ToList();
            //Assert.That(personList.Count == 1000, $"The PersonList contains {personList.Count}, it should contain {NUMBER_OF_PEOPLE}.");

            IEnumerable<Person> personList = b.Get(1000);

            int cnt = 0;
            foreach(Person p in personList)
            {
                Person np = new Person()
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    CityOfResidence = p.CityOfResidence,
                    Profession = p.Profession
                };
                cnt++;
            }

            Assert.That(cnt == NUMBER_OF_PEOPLE, $"The PersonList contains {cnt}, it should contain {NUMBER_OF_PEOPLE}.");

        }

        [Test]
        public void ReadIntoMappedArray() 
        {
            AutoMapperHelpers ourHelper = new AutoMapperHelpers();
            IPropFactory propFactory_V1 = ourHelper.GetNewPropFactory_V1();

            string fullClassName = "PropBagLib.Tests.PerformanceDb.DestinationModel1";
            List<DestinationModel1> destinationList = new List<DestinationModel1>();

            Assert.That(ourHelper.StoreAccessCreator.AccessCounter == 0, "The Provider of PropStoreAccessServices has not had its Access Counter reset.");

            Business b = new Business();
            List<Person> personList = b.Get(1000).ToList();
            foreach (Person p in personList)
            {
                // TODO: AAA
                DestinationModel1 dest = new DestinationModel1(PropBagTypeSafetyMode.Tight, ourHelper.StoreAccessCreator, propFactory_V1, fullClassName);

                dest.SetIt<int>(p.Id, "Id");
                dest.SetIt<string>(p.FirstName, "FirstName");
                dest.SetIt<string>(p.LastName, "LastName");
                dest.SetIt<string>(p.CityOfResidence, "CityOfResidence");
                dest.SetIt<Profession>(p.Profession, "Profession");

                destinationList.Add(dest);
            }
            Assert.That(destinationList.Count == 1000, $"The PersonList contains {destinationList.Count}, it should contain {NUMBER_OF_PEOPLE}.");

            int totalNumberOfGets = ourHelper.StoreAccessCreator.AccessCounter;
            Assert.That(totalNumberOfGets == NUMBER_OF_PEOPLE * 5, $"Total # of SetIt access operations is wrong: it should be {NUMBER_OF_PEOPLE * 5}, but instead it is {totalNumberOfGets}.");

            PropBag test = (PropBag)destinationList[0];

            int howManyDoSetDelegatesGotCreated =  test.NumOfDoSetDelegatesInCache;
        }

        [Test]
        public void CanMapObservableCollection_Extra()
        {
            _ourHelper = new AutoMapperHelpers();
            _propFactory_V1 = _ourHelper.GetNewPropFactory_V1();
            _propModelCache = _ourHelper.GetPropModelCache_V1();

            _amp = _ourHelper.GetAutoMapperSetup_V1();

            _pmHelpers = new PropModelHelpers();

            string configPackageName = "Extra_Members";

            ObservableCollTestObject oTester = new ObservableCollTestObject();
            oTester.CanMapObservableCollection(configPackageName, _ourHelper, _propFactory_V1, _propModelCache, _amp, _pmHelpers, NUMBER_OF_PEOPLE);
            oTester.DoCleanup();
        }

        [Test]
        public void CanMapObservableCollection_Extra_AfterSetup()
        {
            string configPackageName = "Extra_Members";
            ObservableCollTestObject oTester = new ObservableCollTestObject();
            oTester.CanMapObservableCollection(configPackageName, _ourHelper, _propFactory_V1, _propModelCache, _amp, _pmHelpers, NUMBER_OF_PEOPLE);
            oTester.DoCleanup();
        }

        [Test]
        public void CanMapObservableCollection_AProxy()
        {
            _ourHelper = new AutoMapperHelpers();
            _propFactory_V1 = _ourHelper.GetNewPropFactory_V1();
            _amp = _ourHelper.GetAutoMapperSetup_V1();
            _propModelCache = _ourHelper.GetPropModelCache_V1();

            _pmHelpers = new PropModelHelpers();

            string configPackageName = "Emit_Proxy";

            ObservableCollTestObject oTester = new ObservableCollTestObject();
            oTester.CanMapObservableCollection(configPackageName, _ourHelper, _propFactory_V1, _propModelCache, _amp, _pmHelpers, NUMBER_OF_PEOPLE);
            //oTester.DoCleanup();
        }

        [Test]
        public void CanMapObservableCollection_AProxy_AfterSetup()
        {
            string configPackageName = "Emit_Proxy";
            ObservableCollTestObject oTester = new ObservableCollTestObject();
            oTester.CanMapObservableCollection(configPackageName, _ourHelper, _propFactory_V1, _propModelCache, _amp, _pmHelpers, NUMBER_OF_PEOPLE);
            oTester.DoCleanup();
        }

        [Test]
        public void Z_BindParent()
        {
            AutoMapperHelpers ourHelper = new AutoMapperHelpers();

            IPropBagMapperService autoMapperService = ourHelper.GetAutoMapperSetup_V1();

            IPropFactory propFactory_V1 = ourHelper.GetNewPropFactory_V1();
            ICreateWrapperTypes wrapperTypeCreator = ourHelper.GetWrapperTypeCreator_V1();

            ViewModelFactoryInterface viewModelFactory = ourHelper.ViewModelFactory;
            //viewModelFactory.PropModelCache.Add(propModel5);
            _propModelCache = viewModelFactory.PropModelCache;


            Assert.That(ourHelper.StoreAccessCreator.AccessCounter == 0, "The Provider of PropStoreAccessServices has not had its Access Counter reset.");

            List<DestinationModel1> destinationList = new List<DestinationModel1>();



            PropModelHelpers pmHelpers = new PropModelHelpers();

            // Set up Child VM (Using Model 5)
            PropModelType propModel5 = pmHelpers.GetPropModelForModel5Dest(propFactory_V1, _propModelCache);


            DestinationModel5 testChildVM = new DestinationModel5(pm: propModel5, viewModelFactory: viewModelFactory, autoMapperService: autoMapperService, propFactory: propFactory_V1, fullClassName: "PropBagLib.Tests.PerformanceDb.DestinationModel5");

            Business b = new Business();
            testChildVM.SetIt(b, "Business");
            testChildVM.RegisterBinding<Business>("Business", "../Business");

            //List<Person> personList = b.Get().ToList();
            //ObservableCollection<Person> personList2 = new ObservableCollection<Person>(personList);
            //testChildVM.SetIt(personList2, "PersonCollection");


            // Set up MainVM (Using Model 6)
            PropModelType propModel6 = pmHelpers.GetPropModelForModel6Dest(propFactory_V1, _propModelCache);
            DestinationModel6 testMainVM = new DestinationModel6(propModel6, viewModelFactory, autoMapperService, propFactory_V1, null);

            Business b2 = new Business();
            testMainVM.SetIt(b2, "Business");


            testMainVM.SetIt<DestinationModel5>(testChildVM, "ChildVM");
            testMainVM.RegisterBinding<Business>("Business", "./ChildVM/Business");

            b2 = new Business();
            testMainVM.SetIt(b2, "Business");
        }

        [Test]
        public void MapOcAndCleanUp()
        {
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();

            _ourHelper = new AutoMapperHelpers();
            ICreateWrapperTypes wrapperTypeCreator = _ourHelper.GetWrapperTypeCreator_V1();

            _propFactory_V1 = _ourHelper.GetNewPropFactory_V1();
            _propModelCache = _ourHelper.GetPropModelCache_V1();

            _amp = _ourHelper.GetAutoMapperSetup_V1();

            _pmHelpers = new PropModelHelpers();

            string configPackageName = "Extra_Members";

            ObservableCollTestObject oTester = new ObservableCollTestObject();
            oTester.CanMapObservableCollection(configPackageName, _ourHelper, _propFactory_V1, _propModelCache, _amp, _pmHelpers, NUMBER_OF_PEOPLE);

            oTester.DoCleanup();
            oTester = null;

            //Thread.Sleep(new TimeSpan(0, 0, 1));

            //for (int tp = 0; tp < 5; tp++)
            //{
            //    // Yield for 1 seconds.
            //    Thread.Sleep(new TimeSpan(0, 0, 1));
            //    GC.WaitForPendingFinalizers();
            //    GC.WaitForFullGCComplete();
            //}

            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();

        }

        #region DB Helper Methods

        private void PopDatabase(int num)
        {
            Business b = new Business();
            for (int i = 0; i < num; i++)
            {
                Person p = new Person()
                {
                    Profession = Profession.SoftwareEngineer,
                    FirstName = $"TestFirst{i}",
                    LastName = $"TestLast{i}",
                    CityOfResidence = $"Test City{i}",
                    Id = 0
                };
                b.Update(p);
            }
        }

        private void ClearDatabase()
        {
            Business b = new Business();

            List<Person> personList = b.Get().ToList();

            foreach (Person p in personList)
            {
                b.Delete(p);
            }
        }

        #endregion
    }
}
