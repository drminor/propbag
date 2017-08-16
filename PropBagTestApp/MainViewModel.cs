using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.PropBag;
using DRM.Inpcwv;
using DRM.PropBag.ControlModel;

namespace PropBagTestApp
{
    public class MainViewModel : PropBag
    {

        // Just for testing
        public bool MyStringWasUpdated { get; set; }

        public MainViewModel(byte flag)
        {
            if (flag != 0x0)
            {
                string ourClassName = this.GetType().ToString();
                throw new ApplicationException(string.Format("Unexpected value of flag when constructing the {0}.", ourClassName));
            }
            MyStringWasUpdated = false;
        }


        /// <summary>
        /// Constructor used by View to create with properties
        /// </summary>
        /// <param name="pm"></param>
        public MainViewModel(PropModel pm) : base(pm)
        {
            MyStringWasUpdated = false;
        }

        private void HandleMyStringUpdates(string oldVal, string newVal)
        {
            MyStringWasUpdated = true;
        }

        
    }
}
