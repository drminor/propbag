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

        [XmlAttribute(AttributeName = "derive-from-pub-prop-bag")]
        public bool DeriveFromPubPropBag { get; set; }

        [XmlAttribute (AttributeName="class-name")]
        public string ClassName { get; set; }

        [XmlAttribute(AttributeName = "namespace")]
        public string Namespace { get; set; }

        [XmlAttribute(AttributeName = "type-safety-mode")]
        public PropBagTypeSafetyMode TypeSafetyMode { get; set; }

        [XmlAttribute(AttributeName = "defer-method-ref-resolution")]
        public bool DeferMethodRefResolution { get; set; }

        [XmlAttribute(AttributeName = "require-explicit-initial-value")]
        public bool RequireExplicitInitialValue { get; set; }

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

        public PropModel() : this(null, null) { }

        public PropModel(string className, string namespaceName, 
            PropBagTypeSafetyMode typeSafetyMode = PropBagTypeSafetyMode.AllPropsMustBeRegistered, bool deferMethodRefResolution = true)
        {
            ClassName = className;
            Namespace = namespaceName;
            TypeSafetyMode = typeSafetyMode;
            DeferMethodRefResolution = deferMethodRefResolution;
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

            foreach (string s in RequiredNamespaces)
            {
                r.AppendLine(string.Format("using {0};", s));
            }

            foreach (string s in Namespaces)
            {
                r.AppendLine(string.Format("using {0};", s));
            }
           
            return r.AppendLine().ToString();
        }

        public string GetBaseClassName()
        {
            return DeriveFromPubPropBag ? "PubPropBag" : "PropBag";
        }

        public string GetAddPropMethodCallText(PropItem pi, 
            bool? deferMethodRefResolution = null, 
            bool? requireExplicitInitialValue = null)
        {
            // Use the global setting if not provided by the caller.
            if (!deferMethodRefResolution.HasValue) deferMethodRefResolution = this.DeferMethodRefResolution;
            if (!requireExplicitInitialValue.HasValue) requireExplicitInitialValue = this.RequireExplicitInitialValue;

            // DoWhenChaned Logic
            if (pi.DoWhenChangedField == null) pi.DoWhenChangedField = new PropDoWhenChanged(null, false);

            string doWhenChanged;
            if (deferMethodRefResolution.Value)
                // Wrap in a call to GetDelegate if non-null, otherwise return the string: "null"
                doWhenChanged = pi.DoWhenChangedField.DoWhenChanged == null ? "null" : WrapWithGetDelegate(pi.DoWhenChangedField.DoWhenChanged, pi.Type);
            else
                // Return the string: "null", if there is no doWhenChanged action provided.
                doWhenChanged = pi.DoWhenChangedField.DoWhenChanged ?? "null";

            string doAfterNotify = pi.DoWhenChangedField.DoAfterNotify ? "true" : "false";

            // Comparer Logic
            bool useRefEquality;
            string objComp;
            string comparer;

            if (pi.ComparerField != null) // It was included in the XML, but the value of the comparer string could be null.
            {
                if (pi.ComparerField.UseRefEquality)
                {
                    if (pi.ComparerField.Comparer != null)
                        throw new ArgumentException("The value of comparer must be null, if UseRefEquality is specified.");

                    objComp = "ObjComp";
                    useRefEquality = true;
                }
                else
                {
                    objComp = null;
                    useRefEquality = false;
                }

                comparer = pi.ComparerField.Comparer ?? "null";
            }
            else
            {
                objComp = null;
                useRefEquality = false;
                comparer = "null";
            }



            // Prepare the AddProp method call
            string methodName;
            string formatString;
            object[] vals;

            if(pi.HasStore)
            {
                methodName = "AddProp";
                string initVal;
                bool setToDefault;
                bool valueIsDefined = InitialValueIsDefined(pi.InitalValueField, pi.Name, 
                    requireExplicitInitialValue.Value, out initVal, out setToDefault);

                if (valueIsDefined)
                {
                    // AddProp or AddPropObjComp

                    //  public IProp<T> AddProp<T>(string propertyName, 
                    //      Action<T, T> doIfChanged = null,
                    //      bool doAfterNotify = false,
                    //      IEqualityComparer<T> comparer = null,
                    //      object extraInfo = null,
                    //      T initalValue = default(T))

                    if (setToDefault) // (pi.InitalValueField.SetToDefault)
                    {
                        if (useRefEquality)
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5})";
                            vals = new object[] {"ObjComp", null, pi.Type, pi.Name, doWhenChanged, doAfterNotify};

                        }
                        else
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
                            vals = new object[] {null, null, pi.Type, pi.Name, doWhenChanged, doAfterNotify, comparer};
                        }
                    }
                    else
                    {
                        initVal = GetStringRepForValue(initVal, pi.Type);

                        if(useRefEquality)
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6}, {7})";
                            vals = new object[] {"ObjComp", null, pi.Type, pi.Name, doWhenChanged, doAfterNotify, "null", initVal};
                        }
                        else
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6}, {7}, {8})";
                            vals = new object[] {null, null, pi.Type, pi.Name, doWhenChanged, doAfterNotify, comparer, "null", initVal};
                        }
                    }
                }
                else
                {
                    //AddPropNoValue or AddPropObjCompNoValue

                    //  public IProp<T> AddProp<T>(string propertyName, 
                    //      Action<T, T> doIfChanged = null,
                    //      bool doAfterNotify = false,
                    //      IEqualityComparer<T> comparer = null,
                    //      object extraInfo = null,

                    if (useRefEquality)
                    {
                        formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5})";
                        vals = new object[] {"ObjComp", "NoValue", pi.Type, pi.Name, doWhenChanged, doAfterNotify};

                    }
                    else
                    {
                        formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
                        vals = new object[] {null, "NoValue", pi.Type, pi.Name, doWhenChanged, doAfterNotify, comparer};

                    }
                }
            }
            else
            {
                // AddPropNoStore or AddPropNoStoreObjComp

                //  public void AddPropNoStore<T>(string propertyName,
                //      Action<T, T> doIfChanged,
                //      bool doAfterNotify = false,
                //      IEqualityComparer<T> comparer = null)

                if (useRefEquality)
                {
                    formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5})";
                    vals = new object[] {"ObjComp", "NoStore", pi.Type, pi.Name, doWhenChanged, doAfterNotify};

                }
                else
                {
                    formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
                    vals = new object[] {null, "NoStore", pi.Type, pi.Name, doWhenChanged, doAfterNotify, comparer };

                }
            }

            return string.Format(formatString, vals);
        }

        private bool InitialValueIsDefined(PropIniialValueField pivf,
            string propertyName,
            bool requireExplicitInitialValue,
            out string value,
            out bool useDefault)
        {
            if (pivf == null) 
            {
                if(requireExplicitInitialValue)
                {
                    string msg = "For property {0}: Property definitions that have false for the value of 'caller-provides-storage' "
                        + "must include an initial-value element.";
                    throw new ArgumentException(string.Format(msg, propertyName));
                }

                // This will result in the default value being used.
                value = null;
                useDefault = true;
                return true;
            }

            if (pivf.InitialValue == null && !pivf.SetToDefault && !pivf.SetToUndefined && !pivf.SetToNull && !pivf.SetToEmptyString)
            {
                string msg = "For property {0}: The initial-value must be specified if use-undefined, use-default, use-null and use-empty-string are all false.";
                throw new ArgumentException(string.Format(msg, propertyName));
            }

            if (pivf.SetToDefault && pivf.InitialValue != null)
            {
                string msg = "For property {0}: The initial-value has been specified, but use-default has also been set to true; "
                + "this is ambiguous.";
                throw new ArgumentException(string.Format(msg, propertyName));
            }

            if (pivf.SetToUndefined && pivf.InitialValue != null)
            {
                string msg = "For property {0}: he initial-value has been specified, but use-undefined has also been set to true; " 
                + "this is ambiguous.";
                throw new ArgumentException(string.Format(msg, propertyName));
            }

            if (pivf.SetToNull && pivf.InitialValue != null)
            {
                string msg = "For property {0}: he initial-value has been specified, but use-null has also been set to true; "
                + "this is ambiguous.";
                throw new ArgumentException(string.Format(msg, propertyName));
            }

            if (pivf.SetToEmptyString && pivf.InitialValue != null)
            {
                string msg = "For property {0}: he initial-value has been specified, but use-empty-string has also been set to true; "
                + "this is ambiguous.";
                throw new ArgumentException(string.Format(msg, propertyName));
            }

            if (pivf.SetToDefault)
            {
                value = "null";
                useDefault = true;
                return true;
            }

            if (pivf.SetToUndefined)
            {
                value = null;
                useDefault = false;
                return false;
            }

            if (pivf.SetToNull)
            {
                value = "null";
                useDefault = false;
                return true;
            }

            if (pivf.SetToEmptyString)
            {
                value = "\"\"";
                useDefault = false;
                return true;
            }

            value = pivf.InitialValue;
            useDefault = false;
            return true;
        }

        private string GetStringRepForValue(string value, string type)
        {
            //// The caller would have used "use-null", if they wanted it set to null.
            //// The caller must be specifing the empty string.
            //if (value == null && type == "string")
            //{
            //    return "\"\"";
            //}

            // Null results in no output from the string.format method, replace with "null".
            return value ?? "null";
        }

        private string WrapWithGetDelegate(string delegateName, string type)
        {
            return string.Format("GetDelegate<{0}>(\"{1}\")", type, delegateName);
        }

    }
}
