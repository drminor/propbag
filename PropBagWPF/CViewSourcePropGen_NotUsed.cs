using System.Windows.Data;

// This is perfectly fine, but not necessary, since the caller can easily specify CViewSourceProp<object> 
namespace DRM.PropBagWPF.Unused
{
    public class CViewSourcePropGen : CViewSourceProp
    {
        public CViewSourcePropGen(string propertyName, DataSourceProvider initialValue)
            : base(propertyName, initialValue)
        {
        }
    }
}
         