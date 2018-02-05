using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.TypeSafePropertyBag;
using NUnit.Framework;
using PropBagLib.Tests.AutoMapperSupport;
using PropBagLib.Tests.BusinessModel;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PropBagLib.Tests.PerformanceDb
{
    public class ObservableCollTestObject
    {
        private DestinationModel5 _testMainVM { get; set; }
        private ObservableCollection<DestinationModel1> _readyForTheView { get; set; }

        public void CanMapObservableCollection(
            string configPackageName,
            AutoMapperHelpers ourHelper,
            IPropFactory propFactory_V1,
            IProvideAutoMappers amp,
            PropModelHelpers pmHelpers,
            int numberOfItemsToLoad
            )
        {
            ourHelper.StoreAccessCreator.ResetAccessCounter();

            Assert.That(ourHelper.StoreAccessCreator.AccessCounter == 0, "The Provider of PropStoreAccessServices did not have its Access Counter reset.");

            // Setup Mapping between Model1 and Person
            PropModel propModel1 = pmHelpers.GetPropModelForModel1Dest(propFactory_V1);
            Type typeToWrap = typeof(DestinationModel1);

            // Make sure we can activate (or clone as appropriate) the destination type.
            DestinationModel1 test = new DestinationModel1(PropBagTypeSafetyMode.AllPropsMustBeRegistered, ourHelper.StoreAccessCreator, null, ourHelper.PropFactory_V1);
            if (configPackageName == "Emit_Proxy")
            {
                DestinationModel1 testCopy = new DestinationModel1(test);
            }
            else
            {
                DestinationModel1 testCopy = (DestinationModel1) test.Clone();
            }

            IPropBagMapperKey<Person, DestinationModel1> mapperRequest = amp.RegisterMapperRequest<Person, DestinationModel1>
                (
                    propModel: propModel1,
                    typeToWrap: typeToWrap,
                    configPackageName: configPackageName
                );

            Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");

            IPropBagMapper<Person, DestinationModel1> mapper = amp.GetMapper<Person, DestinationModel1>(mapperRequest);
            Assert.That(mapper, Is.Not.Null, "mapper should be non-null");

            PropModel propModel5 = pmHelpers.GetPropModelForModel5Dest(propFactory_V1);

            string fullClassName = null; // Don't override the value from the PropModel.
            // TODO: AAA
            _testMainVM = new DestinationModel5(propModel5, ourHelper.StoreAccessCreator, propFactory_V1, fullClassName);

            Business b = new Business();
            _testMainVM.SetIt(b, "Business"); // THIS IS A SET ACESSS OPERATION.

            b = _testMainVM.GetIt<Business>("Business");

            // TODO: try using IEnumerable<Person> instead.
            List<Person> unMappedPeople = b.Get();

            IEnumerable<DestinationModel1> mappedPeople = mapper.MapToDestination(unMappedPeople);

            _readyForTheView = new System.Collections.ObjectModel.ObservableCollection<DestinationModel1>(mappedPeople);

            // Each time a item is mapped, it is first created. (5 sets during consruction, and another 5 for the actual mapping.)
            int totalNumberOfGets = ourHelper.StoreAccessCreator.AccessCounter;

            if (configPackageName == "Extra_Members")
            {
                Assert.That(totalNumberOfGets == 1, $"Total # of SetIt access operations is wrong: it should be {1}, but instead it is {totalNumberOfGets}.");
            }
            else
            {
                Assert.That(totalNumberOfGets == 1 + (numberOfItemsToLoad * 5), $"Total # of SetIt access operations is wrong: it should be {1 + numberOfItemsToLoad * 5}, but instead it is {totalNumberOfGets}.");
            }

            int currentNumRootPropBags = ourHelper.StoreAccessCreator.NumberOfRootPropBagsInPlay;
            int totalRootPropBagsCreated = ourHelper.StoreAccessCreator.TotalNumberOfAccessServicesCreated;

            PropBag sampleItem = (PropBag)_readyForTheView[0];

            int howManyDoSetDelegatesGotCreated = sampleItem.NumOfDoSetDelegatesInCache;
            int howManyCreateFromString = sampleItem.CreatePropFromStringCacheCount;
            int howManyCreateWithNoVal = sampleItem.CreatePropWithNoValCacheCount;

            //Thread.Sleep(new TimeSpan(0, 0, 1));
        }

        public void DoCleanup()
        {
            foreach (DestinationModel1 pp in _readyForTheView)
            {
                pp.Dispose();
            }

            Business b = _testMainVM.GetIt<Business>("Business");
            b.Dispose();

            _testMainVM.Dispose();

            _testMainVM = null;
            _readyForTheView = null;

            //_mapperRequest = null;

        }

    }
}
