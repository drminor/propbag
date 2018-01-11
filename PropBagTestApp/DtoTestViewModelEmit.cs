using DRM.PropBag;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;
using PropBagTestApp.Models;
using System.Windows;

namespace PropBagTestApp
{
    public class DtoTestViewModelEmit : PropBag
    {
        PropModel _pm;

        public DtoTestViewModelEmit() : base(PropBagTypeSafetyMode.None, null, null) { } // Shows that if no default constructor is available, the one that takes a single byte is use.

        //// If it not desirable to provide a public, default, parameterless constructor, 
        //// a consructor that takes a single byte can be used instead.
        //// NOTE: Neither of these constructors is required if an instance of this class already exists from the proerty 
        //// marked with the PropBagInstanceAttribute.
        //// An instance of this class must be available so that we create an instance of a Action<T,T> delegate.
        //public DtoTestViewModelEmit(byte dummy) : base(dummy) {}

        public DtoTestViewModelEmit(PropBagTypeSafetyMode safetyMode) : base(safetyMode, null, null) { }

        /// <summary>
        /// Constructor used by View to create with properties
        /// </summary>
        /// <param name="pm"></param>
        
        // TODO: AAA
        public DtoTestViewModelEmit(PropModel pm) : base(pm, null, null, null)
        {
            // Save a reference to the model used to defined our properties.
            _pm = pm;
        }

        private void DoWhenProductIdChanges(bool oldVal, bool newVal)
        {
            System.Diagnostics.Debug.WriteLine("ProductId was changed.");
        }

        //int _testP;
        //public int TestP
        //{
        //   get { return _testP; } 
        //    set
        //    {
        //        _testP = value;
        //        OnPropertyChanged("TestP");
        //    }
        //}

        //double _testDouble;
        //public double TestDouble
        //{
        //    get { return _testDouble; }
        //    set
        //    {
        //        _testDouble = value;
        //        OnPropertyChanged("TestDouble");
        //    }
        //}

        //MyModel4 _deep2;
        //public MyModel4 Deep2
        //{
        //    get { return _deep2; }
        //    set
        //    {
        //        _deep2 = value;
        //        OnPropertyChanged("Deep2");
        //    }
        //}


    }
}
