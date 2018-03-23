using DRM.PropBag;
using DRM.PropBag.AutoMapperSupport;
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
    using PropNameType = String;
    using PropModelType = IPropModel<String>;
    using PropModelCacheInterface = ICachePropModels<String>;

    public class ObservableCollTestObject
    {
        private DestinationModel5 _testMainVM { get; set; }
        
        //private ObservableCollection<DestinationModel1> _readyForTheView { get; set; }
        private ObservableCollection<object> _readyForTheView { get; set; }

        public void CanMapObservableCollection(
            string configPackageName,
            AutoMapperHelpers ourHelper,
            IPropFactory propFactory_V1,
            PropModelCacheInterface propModelCache,
            IProvideAutoMappers amp,
            PropModelHelpers pmHelpers,
            int numberOfItemsToLoad
            )
        {
            ourHelper.StoreAccessCreator.ResetAccessCounter();

            Assert.That(ourHelper.StoreAccessCreator.AccessCounter == 0, "The Provider of PropStoreAccessServices did not have its Access Counter reset.");

            // Setup Mapping between Model1 and Person
            PropModelType propModel1 = pmHelpers.GetPropModelForModel1Dest(propFactory_V1, propModelCache);
            //Type typeToWrap = typeof(DestinationModel1);

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

            DestinationModel1 test2 = new DestinationModel1(propModel1, ourHelper.StoreAccessCreator, null, ourHelper.PropFactory_V1, null);
            
            if (configPackageName == "Emit_Proxy")
            {
                DestinationModel1 test2Copy = new DestinationModel1(test2);
            }
            else
            {
                DestinationModel1 test2Copy = (DestinationModel1)test2.Clone();
            }

            Type typeToWrap = typeof(PropBag);

            IPropBagMapperGen genMapper = null;
            IPropBagMapper<Person, DestinationModel1> mapper = null;

            if (configPackageName == "Emit_Proxy")
            {
                IMapperRequest localMr = new MapperRequest(typeof(Person), propModel1, configPackageName);
                Type et = amp.WrapperTypeCreator.GetWrapperType(propModel1, typeToWrap);
                propModel1.NewEmittedType = et;

                IPropBagMapperKeyGen mapperRequest = amp.SubmitMapperRequest(localMr.PropModel, localMr.SourceType, localMr.ConfigPackageName);

                Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");

                // Get the AutoMapper mapping function associated with the mapper request just submitted.
                genMapper = amp.GetMapper(mapperRequest);
                Assert.That(genMapper, Is.Not.Null, "mapper should be non-null");
            }
            else
            {
                IPropBagMapperKey<Person, DestinationModel1> mapperRequest = amp.SubmitMapperRequest<Person, DestinationModel1>
                    (
                        propModel: propModel1,
                        typeToWrap: typeToWrap,
                        configPackageName: configPackageName
                    );

                Assert.That(mapperRequest, Is.Not.Null, "mapperRequest should be non-null.");

                mapper = amp.GetMapper<Person, DestinationModel1>(mapperRequest);
                Assert.That(mapper, Is.Not.Null, "mapper should be non-null");
            }

            PropModelType propModel5 = pmHelpers.GetPropModelForModel5Dest(propFactory_V1);

            string fullClassName = null; // Don't override the value from the PropModel.
            _testMainVM = new DestinationModel5(propModel5, ourHelper.StoreAccessCreator, amp, propFactory_V1, fullClassName);

            Business b = new Business();
            _testMainVM.SetIt(b, "Business"); // THIS IS A SET ACESSS OPERATION.

            b = _testMainVM.GetIt<Business>("Business");

            // TODO: try using IEnumerable<Person> instead.
            List<Person> unMappedPeople = b.Get(1000);

            //IEnumerable<DestinationModel1> mappedPeople;
            IEnumerable<object> mappedPeople;

            if (configPackageName == "Emit_Proxy")
            {
                mappedPeople = genMapper.MapToDestination(unMappedPeople)/*.Cast<DestinationModel1>()*/;

            }
            else
            {
                mappedPeople = mapper.MapToDestination(unMappedPeople);
            }


            _readyForTheView = new ObservableCollection<object>(mappedPeople);

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
            //int howManyCreateWithNoVal = sampleItem.CreatePropWithNoValCacheCount;

            //Thread.Sleep(new TimeSpan(0, 0, 1));
        }

        public void DoCleanup()
        {
            //foreach (DestinationModel1 pp in _readyForTheView)
            //{
            //    pp.Dispose();
            //}

            foreach (object pp in _readyForTheView)
            {
                if (pp is IDisposable disable)
                    disable.Dispose();
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
