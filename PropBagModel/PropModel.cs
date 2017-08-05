using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

using DRM.PropBag;

namespace DRM.PropBagModel
{
    [XmlRoot("model")]
    public class PropModel
    {

        const string PROP_BAG_NAME_SPACE = "DRM.PropBag";
        const string INPWV_NAME_SPACE = "DRM.Ipnwvc";
        const string REFLECTION_NAME_SPACE = "System.Reflection";

        [XmlAttribute (AttributeName="class-name")]
        public string ClassName { get; set; }

        [XmlAttribute(AttributeName = "namespace")]
        public string Namespace { get; set; }

        [XmlAttribute(AttributeName = "type-safety-mode")]
        public PropBagTypeSafetyMode TypeSafetyMode { get; set; }

        [XmlArray("namespaces")]
        [XmlArrayItem("namespace")]
        public string[] Namespaces { get; set; }

        [XmlArray("props")]
        [XmlArrayItem("prop")]
        public PropItem[] Props { get; set; }

        private List<string> requiredNameSpaces = new List<string>()
            {
                REFLECTION_NAME_SPACE, 
                PROP_BAG_NAME_SPACE, 
                INPWV_NAME_SPACE
            };

        public List<string> RequiredNamespaces
        {
            get { return requiredNameSpaces; } 
            set { requiredNameSpaces = value; }
        }

        public PropModel() : this(null, null)
        {
            TypeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered;
        }

        public PropModel(string className, string namespaceName, 
            PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered)
        {
            ClassName = className;
            Namespace = namespaceName;
            TypeSafetyMode = typeSafetyMode;
            Namespaces = null;
            Props = null;
        }

        public string SafetyModeString
        {
            get
            {
                return TypeSafetyMode.ToString();
            }
        }

        public string GetNamespaces()
        {
            StringBuilder r = new StringBuilder();
            foreach (string s in Namespaces)
            {
                r.AppendLine(string.Format("using {0};", s));
            }

            foreach (string s in RequiredNamespaces)
            {
                r.AppendLine(string.Format("using {0};", s));
            }
            
            return r.AppendLine().ToString();
        }

        public string GetAddPropMethodCallText(PropItem pi)
        {
            bool resolveDoWhenDelsAtRuntime = true;


            if (pi.DoWhenChangedField == null) pi.DoWhenChangedField = new PropDoWhenChanged(null, false);

            string doWhenChanged;
            if (resolveDoWhenDelsAtRuntime)
                doWhenChanged = pi.DoWhenChangedField.DoWhenChanged == null ? "null" : WrapWithGetDelegate(pi.DoWhenChangedField.DoWhenChanged, pi.Type);
            else
                doWhenChanged = pi.DoWhenChangedField.DoWhenChanged ?? "null";

            string doAfterNotify = pi.DoWhenChangedField.DoAfterNotify ? "true" : "false";

            //PropComparerField normPcf = GetNormalizedPcf(pi.ComparerField);
            //string objComp = normPcf.ComparerType == PropComparerType.typed ? null : "ObjComp";


            string objComp;
            string comparer;

            if (pi.ComparerField != null)
            {
                objComp = pi.ComparerField.UseRefEquality ? "ObjComp" : null;
                comparer = pi.ComparerField.Comparer ?? "null";
            }
            else
            {
                objComp = null;
                comparer = "null";
            }

            string methodName;
            string formatString;
            object[] vals;

            if(pi.HasStore)
            {
                //    public void AddProp<T>(string propertyName, 
                //        Action<T, T> doIfChanged, 
                //        bool doAfterNotify = false,
                //        IEqualityComparer<T> comparer = null,
                //        T initalValue = default(T))

                methodName = "AddProp";
                formatString = pi.InitialValue == null ? "{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})" : "{0}{1}<{2}>(\"{3}\", {4}, {5}, {6}, {7})";
                vals = new object[] { methodName, objComp, pi.Type, pi.Name, doWhenChanged, doAfterNotify, comparer, pi.InitialValue };
            }
            else
            {
                //  public void AddPropNoStore<T>(string propertyName,
                //      Action<T, T> doIfChanged,
                //      bool doAfterNotify = false,
                //      IEqualityComparer<T> comparer = null)

                methodName = "AddPropNoStore";
                formatString = "{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
                vals = new object[] { methodName, objComp, pi.Type, pi.Name, doWhenChanged, doAfterNotify, comparer };
            }


            //switch (pi.HasStore)
            //{
            //    case PropStorageType.@internal:
            //        {
            //            //    public void AddProp<T>(string propertyName, 
            //            //        Action<T, T> doIfChanged, 
            //            //        bool doAfterNotify = false,
            //            //        IEqualityComparer<T> comparer = null,
            //            //        T initalValue = default(T))

            //            methodName = "AddProp";
            //            formatString = pi.InitialValue == null ? "{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})" : "{0}{1}<{2}>(\"{3}\", {4}, {5}, {6}, {7})";
            //            vals = new object[] { methodName, objComp, pi.Type, pi.Name, doWhenChanged, doAfterNotify, comparer, pi.InitialValue };
            //            break;
            //        }
            //    case PropStorageType.none:
            //        {
            //            //  public void AddPropNoStore<T>(string propertyName,
            //            //      Action<T, T> doIfChanged,
            //            //      bool doAfterNotify = false,
            //            //      IEqualityComparer<T> comparer = null)

            //            methodName = "AddPropNoStore";
            //            formatString = "{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
            //            vals = new object[] { methodName, objComp, pi.Type, pi.Name, doWhenChanged, doAfterNotify, comparer };
            //            break;
            //        }
            //    case PropStorageType.external:
            //        {
            //            //  public Guid AddPropExtStore<T>(string propertyName,
            //            //      GetExtVal<T> getter, SetExtVal<T> setter, 
            //            //      Action<T, T> doIfChanged,
            //            //      bool doAfterNotify = false,
            //            //      IEqualityComparer<T> comparer = null)

            //            methodName = "AddPropExtStore";
            //            formatString = "{0}{1}<{2}>(\"{3}\", {4}, {5}, {6}, {7}, {8})";
            //            vals = new object[] { methodName, objComp, pi.Type, pi.Name, "null", "null", doWhenChanged, doAfterNotify, comparer };
            //            break;
            //        }
            //    default:
            //        {
            //            throw new ApplicationException("Unexpected value for PropStorageType.");
            //        }
            //}

            return string.Format(formatString, vals);
        }

        private string WrapWithGetDelegate(string delegateName, string type)
        {
            return string.Format("GetDelegate<{0}>(\"{1}\")", type, delegateName);
        }

        //private PropComparerField GetNormalizedPcf(PropComparerField pcf)
        //{
        //    if (pcf == null)
        //    {
        //        // This will result in the default comparer for the property's type to be use.
        //        return new PropComparerField(null, PropComparerType.typed);
        //    }

        //    if(pcf.ComparerType == PropComparerType.reference)
        //    {
        //        // This will result in our ReferenceEqualityComparer to be used.
        //        return new PropComparerField(null, PropComparerType.@object);
        //    }

        //    // The pcf has typed" or "@object" for the value of its PropComparerType
        //    // and either has a "real" value for the comparer delegate, or the null value:
        //    // in either case it will be handled correctly by the call to AddProp
        //    return pcf;

        //}
    }
}
