using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
using DRM.PropBag.ControlModel;
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
            SimpleAutoMapperProvider amp,
            PropModelHelpers pmHelpers,
            int numberOfItemsToLoad
            )
        {
            propFactory_V1.PropStoreAccessServiceProvider.ResetAccessCounter();

            Assert.That(propFactory_V1.PropStoreAccessServiceProvider.AccessCounter == 0, "The Provider of PropStoreAccessServices did not have its Access Counter reset.");

            // Setup Mapping between Model1 and Person
            PropModel propModel1 = pmHelpers.GetPropModelForModel1Dest(propFactory_V1);
            Type typeToWrap = typeof(DestinationModel1);

            IPropBagMapperKey<Person, DestinationModel1> mapperRequest = amp.RegisterMapperRequest<Person, DestinationModel1>
                (
                    propModel: propModel1,
                    targetType: typeToWrap,
                    configPackageName: configPackageName
                );

            Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");

            IPropBagMapper<Person, DestinationModel1> mapper = amp.GetMapper<Person, DestinationModel1>(mapperRequest);
            Assert.That(mapper, Is.Not.Null, "mapper should be non-null");

            PropModel propModel5 = pmHelpers.GetPropModelForModel5Dest(propFactory_V1);

            string fullClassName = null; // Don't override the value from the PropModel.
            _testMainVM = new DestinationModel5(propModel5, fullClassName, propFactory_V1);

            Business b = new Business();
            _testMainVM.SetIt(b, "Business"); // THIS IS A SET ACESSS OPERATION.

            b = _testMainVM.GetIt<Business>("Business");

            // TODO: try using IEnumerable<Person> instead.
            List<Person> unMappedPeople = b.Get();

            IEnumerable<DestinationModel1> mappedPeople = mapper.MapToDestination(unMappedPeople);

            _readyForTheView = new System.Collections.ObjectModel.ObservableCollection<DestinationModel1>(mappedPeople);

            // Each time a item is mapped, it is first created. (5 sets during consruction, and another 5 for the actual mapping.)
            int totalNumberOfGets = ourHelper.PropFactory_V1.PropStoreAccessServiceProvider.AccessCounter;

            if (configPackageName == "Extra_Members")
            {
                Assert.That(totalNumberOfGets == 1, $"Total # of SetIt access operations is wrong: it should be {1}, but instead it is {totalNumberOfGets}.");
            }
            else
            {
                Assert.That(totalNumberOfGets == 1 + (numberOfItemsToLoad * 5), $"Total # of SetIt access operations is wrong: it should be {1 + numberOfItemsToLoad * 5}, but instead it is {totalNumberOfGets}.");
            }

            int currentNumRootPropBags = ourHelper.PropFactory_V1.PropStoreAccessServiceProvider.NumberOfRootPropBagsInPlay;
            int totalRootPropBagsCreated = ourHelper.PropFactory_V1.PropStoreAccessServiceProvider.TotalNumberOfAccessServicesCreated;

            PropBag test = (PropBag)_readyForTheView[0];

            int howManyDoSetDelegatesGotCreated = test.NumOfDoSetDelegatesInCache;
            int howManyCreateFromString = test.CreatePropFromStringCacheCount;
            int howManyCreateWithNoVal = test.CreatePropWithNoValCacheCount;

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
