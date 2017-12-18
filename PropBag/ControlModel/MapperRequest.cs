using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.ControlModel
{
    public class MapperRequest : NotifyPropertyChangedBase, IEquatable<MapperRequest>
    {
        public string _cpn;
        public string ConfigPackageName { get { return _cpn; } set { this.SetIfDifferent<string>(ref _cpn, value); } }

        private Type _sourceType;
        public Type SourceType { get { return _sourceType; } set { _sourceType = value; } }

        private string _pmrk;
        public string PropModelResourceKey { get { return _pmrk; } set { this.SetIfDifferent<string>(ref _pmrk, value); } }

        public MapperRequest(Type sourceType, string propModelResourceKey, string configPackageName)
        {
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            PropModelResourceKey = propModelResourceKey ?? throw new ArgumentNullException(nameof(propModelResourceKey));
            ConfigPackageName = configPackageName ?? throw new ArgumentNullException(nameof(configPackageName));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MapperRequest);
        }

        public bool Equals(MapperRequest other)
        {
            return other != null &&
                   EqualityComparer<Type>.Default.Equals(SourceType, other.SourceType) &&
                   PropModelResourceKey == other.PropModelResourceKey &&
                   ConfigPackageName == other.ConfigPackageName;
        }

        public override int GetHashCode()
        {
            var hashCode = 435110090;
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(SourceType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PropModelResourceKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ConfigPackageName);
            return hashCode;
        }

        public static bool operator ==(MapperRequest request1, MapperRequest request2)
        {
            return EqualityComparer<MapperRequest>.Default.Equals(request1, request2);
        }

        public static bool operator !=(MapperRequest request1, MapperRequest request2)
        {
            return !(request1 == request2);
        }
    }
}
