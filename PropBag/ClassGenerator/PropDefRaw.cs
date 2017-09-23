using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ClassGenerator
{

    public enum PropCreateMethodEnum
    {
        value, // Initial value has been provided.
        useDefault, // Need to instantiate default value.
        noValue // Value is not needed or value will be initialized to undefined.
    }

    public class PropDefRaw
    {
        public PropCreateMethodEnum CreateType { get; set; }
        public bool HasStore { get; set; }
        public bool TypeIsSolid { get; set; }
        public bool UseRefEquality { get; set; }
        public string PropType { get; set; }
        public string PropName { get; set; }
        public string DoWhenChanged { get; set; }
        public bool DoAfterNotify { get; set; }
        public string Comparer { get; set; }
        public string ExtraInfo { get; set; }
        public string InitialValue { get; set; }


        public PropDefRaw(PropCreateMethodEnum ct,
            bool hasStore, bool typeIsSolid,
            bool useRefEquality,
            string type, string name, string doWhenChanged, bool doAfterNotify, string comparer,
            string extraInfo, string initialValue = null)
        {
            CreateType = ct;
            HasStore = hasStore;
            TypeIsSolid = typeIsSolid;
            UseRefEquality = useRefEquality;
            PropType = type;
            PropName = name;
            DoWhenChanged = DoWhenChanged;
            DoAfterNotify = doAfterNotify;
            Comparer = comparer;
            ExtraInfo = extraInfo;
            InitialValue = initialValue;
        }


    }


}
