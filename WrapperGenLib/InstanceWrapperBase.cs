using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Collections.ObjectModel;
using DRM.TypeSafePropertyBag;

namespace DRM.WrapperGenLib
{
    public class InstanceWrapperBase : IWrapperBase
    {
        #region Private members

        ITypeSafePropBag _source;
        IDictionary<string, Type> _typeDefs;

        #endregion

        #region Constructors

        public InstanceWrapperBase(ITypeSafePropBag source, IDictionary<string, Type> typeDefs)
        {
            // TODO: Check to see if these two dictionary have conflicting contents.
            _source = source;
            _typeDefs = typeDefs ?? throw new ArgumentNullException(nameof(typeDefs));
        }

        #endregion

        #region Public properties

        //public Dictionary<string, ValPlusType> NamedValuesWithType => _namedValuesWithType;

        //public ReadOnlyDictionary<string, Type> TypeDefs
        //{
        //    get
        //    {
        //        // TODO: Consider cacheing this on first reference.
        //        return new ReadOnlyDictionary<string, Type>(_typeDefs);
        //    }
        //}

        //public bool IsInitialized => _typeDefs != null;

        #endregion

        #region Public methods

        public object GetVal(string s)
        {
            return _source.GetValWithType(s, GetTypeOfProperty(s));
        }

        //public bool SetItWithType(object o, Type t, string s)
        //{
        //    return _source.SetItWithType(o, t, s);
        //}

        public bool SetVal(string s, object o)
        {
            return _source.SetValWithType(s, GetTypeOfProperty(s), o);
        }

        public Type GetTypeOfProperty(string s)
        {
            return _typeDefs[s];
        }

        #endregion
    }
}
