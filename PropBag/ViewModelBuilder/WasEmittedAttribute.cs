using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ViewModelBuilder
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
    public class WasEmittedAttribute : Attribute
    {
        private DateTime dateUpdated;
        public DateTime DateUpdated
        {
            get
            {
                return dateUpdated;
            }
        }

        public WasEmittedAttribute(string theDate)
        {
            dateUpdated = DateTime.Parse(theDate);
        }
    }

}
