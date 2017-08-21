using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel
{
    public class PropModel : NotifyPropertyChangedBase
    {

        bool dfppb;
        string cn;
        string ik;
        string ns;
        PropBagTypeSafetyMode tsm;
        bool dmrr;
        bool reiv;
        AbstractPropFactory pf;

        ObservableCollection<string> _namespaces;
        ObservableCollection<PropItem> _props;

        [XmlAttribute(AttributeName = "derive-from-pub-prop-bag")]
        public bool DeriveFromPubPropBag { get { return dfppb; } set { SetIfDifferent<bool>(ref dfppb, value); } }

        [XmlAttribute(AttributeName = "class-name")]
        public string ClassName { get { return cn; } set { SetIfDifferent<string>(ref cn, value); } }

        [XmlIgnore]
        public string InstanceKey { get { return ik; } set { SetIfDifferent<string>(ref ik, value); } }

        [XmlAttribute(AttributeName = "output-namespace")]
        public string Namespace { get { return ns; } set { SetIfDifferent<string>(ref ns, value); } }

        [XmlAttribute(AttributeName = "type-safety-mode")]
        public PropBagTypeSafetyMode TypeSafetyMode
        { get { return tsm; } set { SetIfDifferentVT<PropBagTypeSafetyMode>(ref tsm, value); } }


        [XmlAttribute(AttributeName = "defer-method-ref-resolution")]
        public bool DeferMethodRefResolution { get { return dmrr; } set { SetIfDifferent<bool>(ref dmrr, value); } }

        [XmlAttribute(AttributeName = "require-explicit-initial-value")]
        public bool RequireExplicitInitialValue { get { return reiv; } set { SetIfDifferent<bool>(ref reiv, value); } }


        // TODO: This is not Serializable, consider providing string representation as a proxy
        // Perhaps we should simply not serialize instances of PropBag Control Models.
        [XmlIgnore]
        public AbstractPropFactory PropFactory { get { return pf; }  set {SetAlways<AbstractPropFactory>(ref pf, value); } }

        [XmlArray("namespaces")]
        [XmlArrayItem("namespace")]
        public ObservableCollection<string> Namespaces
        {
            get { return _namespaces; }
            set { this.SetCollection<ObservableCollection<string>, string>(ref _namespaces, value);  }
        }

        [XmlArray("props")]
        [XmlArrayItem("prop")]
        public ObservableCollection<PropItem> Props
        {
            get { return _props; }
            set { this.SetCollection<ObservableCollection<PropItem>, PropItem>(ref _props, value); }
        }


        public PropModel() : this("UndefinedClassName", null, "UndefinedNameSpace") { }

        public PropModel(string className, string instanceKey, string namespaceName,
            bool deriveFromPubPropBag = false,
            PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered,
            bool deferMethodRefResolution = true,
            bool requireExplicitInitialValue = true)
        {
            DeriveFromPubPropBag = deriveFromPubPropBag;
            ClassName = className;
            InstanceKey = instanceKey;
            Namespace = namespaceName;
            TypeSafetyMode = typeSafetyMode;
            DeferMethodRefResolution = deferMethodRefResolution;
            RequireExplicitInitialValue = requireExplicitInitialValue;

            Namespaces = new ObservableCollection<string>();
            Props = new ObservableCollection<PropItem>();
        }
    }
}
