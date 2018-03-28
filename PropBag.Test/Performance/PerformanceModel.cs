using DRM.PropBag;
using DRM.PropBag.ViewModelTools;
using DRM.TypeSafePropertyBag;
using PropBagLib.Tests.AutoMapperSupport;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PropBagLib.Tests
{
    using PropModelType = IPropModel<String>;
    using ViewModelFactoryInterface = IViewModelFactory<UInt32, String>;

    public partial class PerformanceModel : PropBag
    {

        public PerformanceModel(PropModelType propModel, ViewModelFactoryInterface viewModelFactory) :
            base(propModel, viewModelFactory, null)
        {

        }

        static public PerformanceModel Create(PropBagTypeSafetyMode safetyMode, ViewModelFactoryInterface viewModelFactory)
        {
            AutoMapperHelpers ourHelper = new AutoMapperHelpers();
            IPropFactory propFactory_V1 = ourHelper.GetNewPropFactory_V1();

            PropModelType propModel = new PropModel
                (
                "PerformanceModel",
                "PropBagLib.Tests",
                DeriveFromClassModeEnum.Custom,
                typeof(PerformanceModel),
                propFactory_V1,
                propFactoryType: null,
                propModelCache: null,
                typeSafetyMode: safetyMode,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true
                );

            long generationId = viewModelFactory.PropModelCache.Add(propModel);

            PerformanceModel pmViewModel = new PerformanceModel(propModel, viewModelFactory);

            pmViewModel.AddPropNoStore<int>("PropIntNoStore");
            pmViewModel.AddPropNoStore<string>("PropStringNoStore");
            pmViewModel.AddProp<int>("PropInt");
            pmViewModel.AddProp<string>("PropString");

            pmViewModel.FixProps();

            return pmViewModel;
        }

        public void FixProps()
        {
            base.TryFixPropSet();
        }

        new public IProp<T> GetTypedProp<T>(string propertyName, bool mustBeRegistered, bool neverCreate)
        {
            IProp<T> result = base.GetTypedProp<T>(propertyName, mustBeRegistered, neverCreate); 
            return result;
        }

        // Regular Property Definitions Used as a control
        public event PropertyChangedEventHandler PropertyChanged2;

        int _propInt;
        public int PropIntStandard
        {
            get { return _propInt; }

            set
            {
                if (_propInt != value)
                {

                    _propInt = value;
                    OnPropertyChanged2();
                }
            }
        }


        string _propString;
        public string PropStringStandard
        {
            get { return _propString; }

            set
            {
                if (_propString != value)
                {

                    _propString = value;
                    OnPropertyChanged2();
                }
            }
        }

        int _propIntNoStore;
        public int PropIntNoStore
        {
            get
            {
                return _propIntNoStore;
            }
            set
            {
                SetIt<int>(value, ref _propIntNoStore, nameof(PropIntNoStore));
            }
        }  

        
        string _propStringNoStore;
        public string PropStringNoStore
        {
            get
            {
                return _propStringNoStore;
            }
            set
            {
                SetIt(value, ref _propStringNoStore, nameof(PropStringNoStore));
            }
        }

        protected void OnPropertyChanged2([CallerMemberName]string propertyName = null)
        {
            Interlocked.CompareExchange(ref PropertyChanged2, null, null)?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

        }
    }
}
