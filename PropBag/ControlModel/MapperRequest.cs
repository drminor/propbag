using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    public class MapperRequest : NotifyPropertyChangedBase, IEquatable<IMapperRequest>, IMapperRequest
    {
        public string _cpn;
        public string ConfigPackageName { get { return _cpn; } set { this.SetIfDifferent<string>(ref _cpn, value); } }

        private Type _sourceType;
        public Type SourceType { get { return _sourceType; } set { _sourceType = value; } }

        private string _pmrk;
        public string PropModelResourceKey { get { return _pmrk; } set { this.SetIfDifferent<string>(ref _pmrk, value); } }

        private PropModelType _pm;
        public PropModelType PropModel { get { return _pm; } set
            {
                this.SetAlways<PropModelType>(ref _pm, value);
            }
        }

        public MapperRequest(Type sourceType, string propModelResourceKey, string configPackageName)
        {
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            PropModelResourceKey = propModelResourceKey ?? throw new ArgumentNullException(nameof(propModelResourceKey));
            ConfigPackageName = configPackageName ?? throw new ArgumentNullException(nameof(configPackageName));
            PropModel = null;
        }

        public MapperRequest(Type sourceType, PropModelType propModel, string configPackageName)
        {
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            PropModelResourceKey = null;
            ConfigPackageName = configPackageName ?? throw new ArgumentNullException(nameof(configPackageName));
            PropModel = propModel ?? throw new ArgumentNullException(nameof(propModel));
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IMapperRequest);
        }

        public bool Equals(IMapperRequest other)
        {
            if (other == null) return false;

            bool result;
            if (PropModel == null)
            {
                result = EqualityComparer<Type>.Default.Equals(SourceType, other.SourceType) &&
                   PropModelResourceKey == other.PropModelResourceKey &&
                   ConfigPackageName == other.ConfigPackageName;
            }
            else
            {
                result = EqualityComparer<Type>.Default.Equals(SourceType, other.SourceType) &&
                   PropModel == other.PropModel &&
                   ConfigPackageName == other.ConfigPackageName;
            }
            return result;

        }

        public override int GetHashCode()
        {
            var hashCode = 435110090;
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(SourceType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PropModelResourceKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ConfigPackageName);
            return hashCode;
        }

        public object Clone()
        {
            MapperRequest result = new MapperRequest(SourceType, PropModelResourceKey, ConfigPackageName);
            result.PropModel = PropModel;
            return result;
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
