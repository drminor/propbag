using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport.ProxyEmit
{
    struct ProxyRequest : IEquatable<ProxyRequest>
    {
        public ProxyRequest(string propBagTemplate, Type dtViewModelType) : this()
        {
            PropBagTemplate = propBagTemplate;
            this.DtViewModelType = dtViewModelType;
        }

        public string PropBagTemplate { get; }
        public Type DtViewModelType { get; }

        public override bool Equals(object obj)
        {
            return obj is ProxyRequest && Equals((ProxyRequest)obj);
        }

        public bool Equals(ProxyRequest other)
        {
            return PropBagTemplate == other.PropBagTemplate &&
                   EqualityComparer<Type>.Default.Equals(DtViewModelType, other.DtViewModelType);
        }

        public override int GetHashCode()
        {
            var hashCode = 361171213;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PropBagTemplate);
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(DtViewModelType);
            return hashCode;
        }

        public static bool operator ==(ProxyRequest request1, ProxyRequest request2)
        {
            return request1.Equals(request2);
        }

        public static bool operator !=(ProxyRequest request1, ProxyRequest request2)
        {
            return !(request1 == request2);
        }
    }
}
