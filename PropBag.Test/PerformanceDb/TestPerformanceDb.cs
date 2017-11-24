using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using PropBagLib.Tests.AutoMapperSupport;
using PropBagLib.Tests.BusinessModel;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace PropBagLib.Tests.PerformanceDb
{
    [TestFixture]
    public class TestPerformanceDb
    {
        const int NUMBER_OF_PEOPLE = 1000;

        //SimpleAutoMapperProvider _amp;
        //AutoMapperHelpers _ourHelper;
        //IPropFactory propFactory_V1;

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

        [Test]
        public void ReadIntoNativeArray()
        {
            Business b = new Business();
            List<Person> personList = b.Get().ToList();
            Assert.That(personList.Count == 1000, $"The PersonList contains {personList.Count}, it should contain {NUMBER_OF_PEOPLE}.");
        }

        [Test]
        public void ReadIntoMappedArray() 
        {
            AutoMapperHelpers ourHelper = new AutoMapperHelpers();
            IPropFactory propFactory_V1 = ourHelper.GetNewPropFactory_V1();

            string fullClassName = "PropBagLib.Tests.PerformanceDb.DestinationModel1";
            List<DestinationModel1> destinationList = new List<DestinationModel1>();

            Assert.That(propFactory_V1.PropStoreAccessServiceProvider.AccessCounter == 0, "The Provider of PropStoreAccessServices has not had its Access Counter reset.");

            Business b = new Business();
            List<Person> personList = b.Get().ToList();
            foreach (Person p in personList)
            {
                DestinationModel1 dest = new DestinationModel1(PropBagTypeSafetyMode.Tight, propFactory_V1, fullClassName);

                dest.SetIt<int>(p.Id, "Id");
                dest.SetIt<string>(p.FirstName, "FirstName");
                dest.SetIt<string>(p.LastName, "LastName");
                dest.SetIt<string>(p.CityOfResidence, "CityOfResidence");
                dest.SetIt<Profession>(p.Profession, "Profession");

                destinationList.Add(dest);
            }
            Assert.That(destinationList.Count == 1000, $"The PersonList contains {destinationList.Count}, it should contain {NUMBER_OF_PEOPLE}.");

            int totalNumberOfGets = ourHelper.PropFactory_V1.PropStoreAccessServiceProvider.AccessCounter;
            Assert.That(totalNumberOfGets == NUMBER_OF_PEOPLE * 5, $"Total # of SetIt access operations is wrong: it should be {NUMBER_OF_PEOPLE * 5}, but instead it is {totalNumberOfGets}.");
        }

        [Test]
        public void CanMapObservableCollection_Proxy()
        {
            AutoMapperHelpers ourHelper = new AutoMapperHelpers();
            IPropFactory propFactory_V1 = ourHelper.GetNewPropFactory_V1();

            SimpleAutoMapperProvider _amp = ourHelper.GetAutoMapperSetup_V1();
            PropModelHelpers pmHelpers = new PropModelHelpers();

            Assert.That(propFactory_V1.PropStoreAccessServiceProvider.AccessCounter == 0, "The Provider of PropStoreAccessServices has not had its Access Counter reset.");

            // Setup Mapping between Model1 and Person
            PropModel propModel1 = pmHelpers.GetPropModelForModel1Dest(propFactory_V1);
            Type typeToWrap = typeof(DestinationModel1); // typeof(PropBag);
            string configPackageName = "Extra_Members"; //"Emit_Proxy"; // 

            IPropBagMapperKey<Person, DestinationModel1> mapperRequest =
                _amp.RegisterMapperRequest<Person, DestinationModel1>
                (
                    propModel: propModel1,
                    targetType: typeToWrap,
                    configPackageName: configPackageName
                );

            Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");

            IPropBagMapper<Person, DestinationModel1> mapper = _amp.GetMapper<Person, DestinationModel1>(mapperRequest);

            Assert.That(mapper, Is.Not.Null, "mapper should be non-null");

            //MyModel5 testMainVM = new MyModel5
            //{
            //    ProductId = Guid.NewGuid(),
            //    Business = new Business()
            //};


            PropModel propModel5 = pmHelpers.GetPropModelForModel5Dest(propFactory_V1);

            string fullClassName = null; // Don't override the value from the PropModel.
            DestinationModel5 testMainVM = new DestinationModel5(propModel5, fullClassName, propFactory_V1);

            Business b = new Business();
            testMainVM.SetIt(b, "Business"); // THIS IS A SET ACESSS OPERATION.
            // ToDo: try using IEnumerable<Person> instead.

            b = testMainVM.GetIt<Business>("Business");

            List<Person> unMappedPeople = b.Get();

            IEnumerable<DestinationModel1> mappedPeople = mapper.MapToDestination(unMappedPeople);
            unMappedPeople = null;
            ObservableCollection<DestinationModel1> readyForTheView = new System.Collections.ObjectModel.ObservableCollection<DestinationModel1>(mappedPeople);

            mappedPeople = null;

            // Each time a item is mapped, it is first created. (5 sets during consruction, and another 5 for the actual mapping.)
            int totalNumberOfGets = ourHelper.PropFactory_V1.PropStoreAccessServiceProvider.AccessCounter;

            if(configPackageName == "Extra_Members")
            {
                Assert.That(totalNumberOfGets == 1, $"Total # of SetIt access operations is wrong: it should be {1}, but instead it is {totalNumberOfGets}.");
            }
            else
            {
                Assert.That(totalNumberOfGets == 1 + (NUMBER_OF_PEOPLE * 5), $"Total # of SetIt access operations is wrong: it should be {1 + NUMBER_OF_PEOPLE * 5}, but instead it is {totalNumberOfGets}.");
            }

            int currentNumRootPropBags = ourHelper.PropFactory_V1.PropStoreAccessServiceProvider.NumberOfRootPropBagsInPlay;
            int totalRootPropBagsCreated = ourHelper.PropFactory_V1.PropStoreAccessServiceProvider.TotalNumberOfAccessServicesCreated;

            //Thread.Sleep(new TimeSpan(0, 0, 1));

            //foreach(DestinationModel1 pp in readyForTheView)
            //{
            //    pp.Dispose();
            //}
            //readyForTheView = null;

            //testMainVM.SetIt<Business>(null, "Business");
            //b.Dispose();
            //b = null;

            //testMainVM.Dispose();
            //testMainVM = null;

            //Thread.Sleep(new TimeSpan(0, 0, 1));

            //// Test the PropStoreAccessProvider prune store feature.
            //// Do nothing for 1 hour, 24 minutes, in increments of 5 seconds.
            //for (int tp = 0; tp < 1000; tp++)
            //{
            //    // Yield for 20 seconds.
            //    Thread.Sleep(new TimeSpan(0, 0, 5));
            //    GC.Collect(4, GCCollectionMode.Forced);
            //}
        }

        [Test]
        public void ZBindParent()
        {
            AutoMapperHelpers ourHelper = new AutoMapperHelpers();
            IPropFactory propFactory_V1 = ourHelper.GetNewPropFactory_V1();
            Assert.That(propFactory_V1.PropStoreAccessServiceProvider.AccessCounter == 0, "The Provider of PropStoreAccessServices has not had its Access Counter reset.");

            List<DestinationModel1> destinationList = new List<DestinationModel1>();

            PropModelHelpers pmHelpers = new PropModelHelpers();

            // Set up Child VM (Using Model 5)
            PropModel propModel5 = pmHelpers.GetPropModelForModel5Dest(propFactory_V1);
            DestinationModel5 testChildVM = new DestinationModel5(propModel5, null, propFactory_V1);

            Business b = new Business();
            testChildVM.SetIt(b, "Business");
            testChildVM.RegisterBinding<Business>("Business", "../Business");

            List<Person> personList = b.Get().ToList();
            ObservableCollection<Person> personList2 = new ObservableCollection<Person>(personList);
            testChildVM.SetIt(personList2, "PersonCollection");


            // Set up MainVM (Using Model 6)
            PropModel propModel6 = pmHelpers.GetPropModelForModel6Dest(propFactory_V1);
            DestinationModel6 testMainVM = new DestinationModel6(propModel6, null, propFactory_V1);

            Business b2 = new Business();
            testMainVM.SetIt(b, "Business");

            testMainVM.SetIt<DestinationModel5>(testChildVM, "ChildVM");
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
