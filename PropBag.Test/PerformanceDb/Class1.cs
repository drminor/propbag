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
using System.Linq;
using System.Threading;

namespace PropBagLib.Tests.PerformanceDb
{
    public class Class1
    {
        const int NUMBER_OF_PEOPLE = 1000;

        SimpleAutoMapperProvider _amp;
        AutoMapperHelpers _ourHelper;
        IPropFactory _propFactory_V1;
        PropModelHelpers _pmHelpers;

        public Class1()
        {

        }
        public void MapOcAndCleanUp()
        {
            _ourHelper = new AutoMapperHelpers();
            _propFactory_V1 = _ourHelper.GetNewPropFactory_V1();
            _amp = _ourHelper.GetAutoMapperSetup_V1();

            _pmHelpers = new PropModelHelpers();

            string configPackageName = "Extra_Members";

            ObservableCollTestObject oTester = new ObservableCollTestObject();
            oTester.CanMapObservableCollection(configPackageName, _ourHelper, _propFactory_V1, _amp, _pmHelpers, NUMBER_OF_PEOPLE);

            oTester.DoCleanup();
            oTester = null;

            Thread.Sleep(new TimeSpan(0, 0, 1));

            for (int tp = 0; tp < 5; tp++)
            {
                // Yield for 1 seconds.
                Thread.Sleep(new TimeSpan(0, 0, 1));
                GC.Collect(1, GCCollectionMode.Forced);
            }
        }
    }
}
