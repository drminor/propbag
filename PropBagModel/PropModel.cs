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

            PropDoWhenChanged doWhenPrepped = PrepareDoWhenChangedField(pi.DoWhenChangedField, deferMethodRefResolution.Value, pi.Type);

            PropComparerField comparerPrepped = PrepareComparerField(pi.ComparerField);

            // Prepare the AddProp method call
            string formatString;
            object[] vals = new object[] {
                comparerPrepped.UseRefEquality ? "ObjComp" : null,
                null, // will eventually be null, "NoValue" or "NoStore"
                pi.Type,
                pi.Name,
                doWhenPrepped.DoWhenChanged,
                doWhenPrepped.DoAfterNotify ? "true" : "false",
                comparerPrepped.Comparer, "null",
                "null", // Extra Info -- if we ever use it.
                null // Initial Value
            };


            if(pi.HasStore)
            {
                PropIniialValueField initialValPrepped = PrepareInitialField(pi.InitalValueField,
                    pi.Name, requireExplicitInitialValue.Value);

                if (!initialValPrepped.SetToUndefined)
                {
                    // AddProp or AddPropObjComp

                    //  public IProp<T> AddProp<T>(string propertyName, 
                    //      Action<T, T> doIfChanged = null,
                    //      bool doAfterNotify = false,
                    //      IEqualityComparer<T> comparer = null,
                    //      object extraInfo = null,
                    //      T initalValue = default(T))

                    if (initialValPrepped.SetToDefault) // (pi.InitalValueField.SetToDefault)
                    {
                        if (comparerPrepped.UseRefEquality)
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5})";
                        }
                        else
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
                        }
                    }
                    else
                    {
                        vals[8] = GetStringRepForValue(initialValPrepped.InitialValue, pi.Type);

                        if (comparerPrepped.UseRefEquality)
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {7}, {8})";
                        }
                        else
                        {
                            formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6}, {7}, {8})";
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

                    if (comparerPrepped.UseRefEquality)
                    {
                        formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5})";
                    }
                    else
                    {
                        formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
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

                if (comparerPrepped.UseRefEquality)
                {
                    formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5})";
                }
                else
                {
                    formatString = "AddProp{0}{1}<{2}>(\"{3}\", {4}, {5}, {6})";
                }
            }

            return string.Format(formatString, vals);
        }

        private PropDoWhenChanged PrepareDoWhenChangedField(PropDoWhenChanged dwcf, bool deferMethodRefResolution, string propertyType)
        {
            if (dwcf == null) return new PropDoWhenChanged("null");

            string doWhenChanged;
            if (deferMethodRefResolution)
                // Wrap in a call to GetDelegate if non-null, otherwise return the string: "null"
                doWhenChanged = dwcf.DoWhenChanged == null ? "null" : WrapWithGetDelegate(dwcf.DoWhenChanged, propertyType);
            else
                // Return the string: "null", if there is no doWhenChanged action provided.
                doWhenChanged = dwcf.DoWhenChanged ?? "null";

            return new PropDoWhenChanged(doWhenChanged, dwcf.DoAfterNotify);
        }

        private PropComparerField PrepareComparerField(PropComparerField cf)
        {
            if (cf == null) return new PropComparerField("null");

            if (cf.UseRefEquality && cf.Comparer != null)
            {
                throw new ArgumentException("cf value of comparer must be null, if UseRefEquality is specified.");
            }

            return new PropComparerField(cf.Comparer ?? "null", cf.UseRefEquality);
        }

        private PropIniialValueField PrepareInitialField(PropIniialValueField pivf, string propertyName, bool requireExplicitInitialValue)
        {
            if (pivf == null)
            {
                if (requireExplicitInitialValue)
                {
                    string msg = "For property {0}: Property definitions that have false for the value of 'caller-provides-storage' "
                        + "must include an initial-value element.";
                    throw new ArgumentException(string.Format(msg, propertyName));
                }

                // This will result in the default value being used.
                return new PropIniialValueField(null, setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);
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

            //if (pivf.SetToDefault)
            //{
            //    return new PropIniialValueField("null", setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);
            //}

            //if (pivf.SetToUndefined)
            //{
            //    return new PropIniialValueField(null, setToDefault: false, setToUndefined: true, setToNull: false, setToEmptyString: false);
            //}

            if (pivf.SetToNull)
            {
                return new PropIniialValueField("null", setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);
            }

            if (pivf.SetToEmptyString)
            {
                return new PropIniialValueField("\"\"", setToDefault: false, setToUndefined: false, setToNull: false, setToEmptyString: true);

            }

            return pivf;
        }

        private string GetStringRepForValue(string value, string type)
        {
            // A Null value would result in no output from the string.format method,
            // replace with "null".
            return value ?? "null";
        }

        private string WrapWithGetDelegate(string delegateName, string type)
        {
            return string.Format("GetDelegate<{0}>(\"{1}\")", type, delegateName);
        }

    }
}
