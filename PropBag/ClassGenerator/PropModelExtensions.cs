using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.PropBag.XMLModel;

namespace DRM.PropBag.ClassGenerator
{
    /// <summary>
    /// Extends the DRM.PropBag.XMLModel.PropModel class.
    /// </summary>
    public static class PropModelExtensions
    {
        const string PROP_BAG_NAME_SPACE = "DRM.PropBag";
        const string INPWV_NAME_SPACE = "DRM.TypeSafePropertyBag";
        const string REFLECTION_NAME_SPACE = "System.Reflection";

        /// <summary>
        /// Returns the list of namespaces needed in class that derives from IPropBag
        /// to make calls to GetIt, SetIt and Add/RemovePropChanged.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static IList<string> GetRequiredNamespaces(this PropModel pm)
        {
            return new List<string> {
                REFLECTION_NAME_SPACE, 
                PROP_BAG_NAME_SPACE, 
                INPWV_NAME_SPACE
            };

        }

        public static PropDoWhenChanged PrepareDoWhenChangedField(this PropModel pm, PropItem pi)
        {
            PropDoWhenChanged dwcf = pi.DoWhenChangedField;
            if (dwcf == null) return new PropDoWhenChanged("null");

            string doWhenChanged;
            if (pm.DeferMethodRefResolution)
                // Wrap in a call to GetDelegate if non-null, otherwise return the string: "null"
                doWhenChanged = dwcf.DoWhenChanged == null ? "null" : WrapWithGetDelegate(dwcf.DoWhenChanged, pi.Type);
            else
                // Return the string: "null", if there is no doWhenChanged action provided.
                doWhenChanged = dwcf.DoWhenChanged ?? "null";

            return new PropDoWhenChanged(doWhenChanged, dwcf.DoAfterNotify);
        }

        public static PropComparerField PrepareComparerField(this PropModel pm, PropComparerField cf)
        {
            if (cf == null) return new PropComparerField("null");

            if (cf.UseRefEquality && cf.Comparer != null)
            {
                throw new ArgumentException("cf value of comparer must be null, if UseRefEquality is specified.");
            }

            return new PropComparerField(cf.Comparer ?? "null", cf.UseRefEquality);
        }

        public static PropInitialValueField PrepareInitialField(this PropModel pm, PropItem pi)
        {
            PropInitialValueField pivf = pi.InitalValueField;
            if (pivf == null)
            {
                if (pm.RequireExplicitInitialValue)
                {
                    string msg = "For property {0}: Property definitions that have false for the value of 'caller-provides-storage' "
                        + "must include an initial-value element.";
                    throw new ArgumentException(string.Format(msg, pi.Name));
                }

                // This will result in the default value being used.
                return new PropInitialValueField(null, setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);
            }

            if (pivf.InitialValue == null && !pivf.SetToDefault && !pivf.SetToUndefined && !pivf.SetToNull && !pivf.SetToEmptyString)
            {
                string msg = "For property {0}: The initial-value must be specified if use-undefined, use-default, use-null and use-empty-string are all false.";
                throw new ArgumentException(string.Format(msg, pi.Name));
            }

            if (pivf.SetToDefault && pivf.InitialValue != null)
            {
                string msg = "For property {0}: The initial-value has been specified, but use-default has also been set to true; "
                + "this is ambiguous.";
                throw new ArgumentException(string.Format(msg, pi.Name));
            }

            if (pivf.SetToUndefined && pivf.InitialValue != null)
            {
                string msg = "For property {0}: he initial-value has been specified, but use-undefined has also been set to true; "
                + "this is ambiguous.";
                throw new ArgumentException(string.Format(msg, pi.Name));
            }

            if (pivf.SetToNull && pivf.InitialValue != null)
            {
                string msg = "For property {0}: he initial-value has been specified, but use-null has also been set to true; "
                + "this is ambiguous.";
                throw new ArgumentException(string.Format(msg, pi.Name));
            }

            if (pivf.SetToEmptyString && pivf.InitialValue != null)
            {
                string msg = "For property {0}: he initial-value has been specified, but use-empty-string has also been set to true; "
                + "this is ambiguous.";
                throw new ArgumentException(string.Format(msg, pi.Name));
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
                return new PropInitialValueField("null", setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);
            }

            if (pivf.SetToEmptyString)
            {
                if (pi.Type == typeof(Guid).ToString())
                {
                    const string EMPTY_GUID = "00000000-0000-0000-0000-000000000000";
                    return new PropInitialValueField(EMPTY_GUID, setToDefault: false, setToUndefined: false, setToNull: false, setToEmptyString: true);
                }
                else
                {
                    return new PropInitialValueField("\"\"", setToDefault: false, setToUndefined: false, setToNull: false, setToEmptyString: true);
                }
            }

            return pivf;
        }

        public static string GetStringRepForValue(string value, string type)
        {
            // A Null value would result in no output from the string.format method,
            // replace with "null".
            return value ?? "null";
        }

        public static string WrapWithGetDelegate(string delegateName, string type)
        {
            return string.Format("GetDelegate<{0}>(\"{1}\")", type, delegateName);
        }
    }
}
