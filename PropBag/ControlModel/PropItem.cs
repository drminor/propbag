using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel
{
    public class PropItem : NotifyPropertyChangedBase
    {
        string pn;
        Type pt;
        PropInitialValueField ivf;
        PropComparerField cf;
        PropDoWhenChangedField dwc;

        bool hs;
        bool tis;
        string ei;

        [XmlElement("name")]
        public string PropertyName { get {return pn;}  set { SetIfDifferent<string>(ref pn, value); } }

        [XmlElement("type")]
        public Type PropertyType { get { return pt; } set { pt = value; } }

        [XmlElement("initial-value")]
        public PropInitialValueField InitialValueField { get { return ivf; }
            set { SetIfDifferent<PropInitialValueField>(ref ivf, value); }
        }

        [XmlElement("comparer")]
        public PropComparerField ComparerField { get { return cf; }
            set { SetIfDifferent<PropComparerField>(ref cf, value); }
        }

        [XmlElement("do-when-changed")]
        public PropDoWhenChangedField DoWhenChangedField { get { return dwc; }
            set { SetIfDifferent<PropDoWhenChangedField>(ref dwc, value); }
        }

        [XmlAttribute(AttributeName = "caller-provides-storage")]
        public bool HasStore { get { return hs; } set { SetIfDifferent<bool>(ref hs, value); } }

        [XmlIgnore]
        public bool TypeIsSolid { get { return tis; } set { SetIfDifferent<bool>(ref tis, value); } }

        [XmlIgnore]
        public string ExtraInfo { get { return ei; } set { SetIfDifferent<string>(ref ei, value); } }

        public PropItem() : this(null, null) { }

        public PropItem(Type type, string name,
            string extraInfo = null, bool hasStore = true,
            bool typeIsSolid = true,
            PropInitialValueField initialValueField = null,
            PropDoWhenChangedField doWhenChanged = null, PropComparerField comparer = null)
        {
            PropertyType = type;
            PropertyName = name;
            ExtraInfo = extraInfo;
            HasStore = hasStore;
            TypeIsSolid = typeIsSolid;
            InitialValueField = initialValueField;
            ComparerField = comparer;
            DoWhenChangedField = doWhenChanged ?? new PropDoWhenChangedField();

        }
    }
}
