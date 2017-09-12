using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

using DRM.PropBag;
using DRM.Inpcwv;
using DRM.PropBag.ControlsWPF;
using DRM.PropBag.ControlModel;

using System.Drawing;

namespace PropBagTestApp
{
    public class DtoTestViewModel : PubPropBag, INotifyPropertyChanged, INotifyPropertyChanging
    {

        PropModel _pm;

        public DtoTestViewModel() { } // Shows that if no default constructor is available, the one that takes a single byte is use.

        // If it not desirable to provide a public, default, parameterless constructor, 
        // a consructor that takes a single byte can be used instead.
        // NOTE: Neither of these constructors is required if an instance of this class already exists from the proerty 
        // marked with the PropBagInstanceAttribute.
        // An instance of this class must be available so that we create an instance of a Action<T,T> delegate.
        public DtoTestViewModel(byte dummy) : base(dummy) {}

        /// <summary>
        /// Constructor used by View to create with properties
        /// </summary>
        /// <param name="pm"></param>
        public DtoTestViewModel(PropModel pm) : base(pm)
        {
            // Save a reference to the model used to defined our properties.
            _pm = pm;
        }

        private void DoWhenProductIdChanges(bool oldVal, bool newVal)
        {
            
        }

        public Guid ProductId
        {
            get
            {
                return (Guid)this["ProductId"];
            }
            set
            {
                this["ProductId"] = value;
            }
        }
        public int Amount { get; set; }
        public double Size { get; set; }

    }
}
