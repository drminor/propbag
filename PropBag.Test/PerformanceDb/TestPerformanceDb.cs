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

namespace PropBagLib.Tests.PerformanceDb
{
    [TestFixture]
    public class TestPerformanceDb
    {
        const int NUMBER_OF_PEOPLE = 1000;

        SimpleAutoMapperProvider _amp;
        AutoMapperHelpers _ourHelper;
        IPropFactory _propFactory_V1;

        List<Person> _personList;

        [OneTimeSetUp]
        public void SetUpOneTime()
        {
            _ourHelper = new AutoMapperHelpers();


            Business b = new Business();
            _personList = b.Get(1).ToList();

            if(_personList.Count == 0)
            {
                PopDatabase(NUMBER_OF_PEOPLE);
            }

            _personList = b.Get().ToList();
        }

        [SetUp]
        public void SetUpForEach()
        {
            // Get a brand new propFactory instance for each test.
            _propFactory_V1 = _ourHelper.GetNewPropFactory_V1();
        }

        [Test]
        public void ReadIntoNativeArray()
        {
            Assert.That(_personList.Count == 1000, $"The PersonList contains {_personList.Count}, it should contain {NUMBER_OF_PEOPLE}.");
        }

        [Test]
        public void ReadIntoMappedArray() 
        {
            string fullClassName = "PropBagLib.Tests.PerformanceDb.DestinationModel1";
            List<DestinationModel1> destinationList = new List<DestinationModel1>();

            foreach(Person p in _personList)
            {
                DestinationModel1 dest = new DestinationModel1(PropBagTypeSafetyMode.Tight, _propFactory_V1, fullClassName);

                dest.SetIt<int>(p.Id, "Id");
                dest.SetIt<string>(p.FirstName, "FirstName");
                dest.SetIt<string>(p.LastName, "LastName");
                dest.SetIt<string>(p.CityOfResidence, "CityOfResidence");
                dest.SetIt<Profession>(p.Profession, "Profession");

                destinationList.Add(dest);
            }
            Assert.That(destinationList.Count == 1000, $"The PersonList contains {destinationList.Count}, it should contain {NUMBER_OF_PEOPLE}.");

            DestinationModel1 lastDestItem = destinationList[NUMBER_OF_PEOPLE - 1];

            int totalNumberOfGets = lastDestItem.OurStoreAccessor.AccessCounter;
            Assert.That(totalNumberOfGets == NUMBER_OF_PEOPLE * 5, $"Total # of SetIt access operations is wrong: it should be {NUMBER_OF_PEOPLE * 5}, but instead it is {totalNumberOfGets}.");
        }

        [Test]
        public void CanMapObservableCollection_Proxy()
        {
            _amp = _ourHelper.GetAutoMapperSetup_V1();
            IPropFactory propFactory = _propFactory_V1;
            PropModelHelpers pmHelpers = new PropModelHelpers();

            Assert.That(_propFactory_V1.PropStoreAccessServiceProvider.AccessCounter == 0, "The Provider of PropStoreAccessServices has not had its Access Counter reset.");

            // Setup Mapping between Model1 and Person
            PropModel propModel1 = pmHelpers.GetPropModelForModel1Dest(propFactory);
            Type typeToWrap = typeof(DestinationModel1); // typeof(PropBag);
            string configPackageName = "Emit_Proxy"; // "Extra_Members"; //

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


            PropModel propModel5 = pmHelpers.GetPropModelForModel5Dest(propFactory);

            string fullClassName = null; // Don't override the value from the PropModel.
            DestinationModel5 testMainVM = new DestinationModel5(propModel5, fullClassName, _propFactory_V1);

            Business b = new Business();
            testMainVM.SetIt(b, "Business"); // THIS IS A SET ACESSS OPERATION.
            // ToDo: try using IEnumerable<Person> instead.
            List<Person> unMappedPeople = testMainVM.GetIt<Business>("Business").Get();

            IEnumerable<DestinationModel1> mappedPeople = mapper.MapToDestination(unMappedPeople);

            ObservableCollection<DestinationModel1> readyForTheView = new System.Collections.ObjectModel.ObservableCollection<DestinationModel1>(mappedPeople);

            // Each time a item is mapped, it is first created. (5 sets during consruction, and another 5 for the actual mapping.)
            DestinationModel1 lastDestItem = mappedPeople.Last();
            int totalNumberOfGets = lastDestItem.OurStoreAccessor.AccessCounter;
            Assert.That(totalNumberOfGets == 1 + (NUMBER_OF_PEOPLE * 10), $"Total # of SetIt access operations is wrong: it should be {NUMBER_OF_PEOPLE * 5}, but instead it is {totalNumberOfGets}.");

            int currentNumRootPropBags = _ourHelper.PropFactory_V1.PropStoreAccessServiceProvider.NumberOfRootPropBagsInPlay;
            int totalRootPropBagsCreated = _ourHelper.PropFactory_V1.PropStoreAccessServiceProvider.TotalNumberOfAccessServicesCreated;
        }


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

            _personList = b.Get().ToList();

            foreach (Person p in _personList)
            {
                b.Delete(p);
            }
        }


    }
}
