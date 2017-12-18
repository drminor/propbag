using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel
{
    public class PropBinderField : NotifyPropertyChangedBase, IEquatable<PropBinderField>
    {
        #region Private Properties
        //string targetProperty;
        string path;
        #endregion

        #region Public Properties

        //[XmlAttribute("target-property-name")]
        //public string TargetProperty { get { return targetProperty; } set { this.SetIfDifferent<string>(ref targetProperty, value); } }

        [XmlAttribute("path")]
        public string Path { get { return path; } set { this.SetIfDifferent<string>(ref path, value); } }

        public PropBinderField() : this(null) { }

        public PropBinderField(string pathToSource)
        {
            Path = pathToSource;
        }

        #endregion

        #region IEquatable Support and Object Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as PropBinderField);
        }

        public bool Equals(PropBinderField other)
        {
            return other != null &&
                   //TargetProperty == other.TargetProperty &&
                   Path == other.Path;
        }

        public override int GetHashCode()
        {
            var hashCode = -552951153;
            //hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TargetProperty);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            return hashCode;
        }

        public static bool operator ==(PropBinderField field1, PropBinderField field2)
        {
            return EqualityComparer<PropBinderField>.Default.Equals(field1, field2);
        }

        public static bool operator !=(PropBinderField field1, PropBinderField field2)
        {
            return !(field1 == field2);
        }

        #endregion
    }
}
