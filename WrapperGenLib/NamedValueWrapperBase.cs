using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Collections.ObjectModel;
using DRM.TypeSafePropertyBag;

namespace DRM.WrapperGenLib
{
    public class NamedValueWrapperBase : IWrapperBase
    {
        #region Private members

        Dictionary<string, ValPlusType> _namedValuesWithType;
        IDictionary<string, Type> _typeDefs;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new ProxyBase with an empty set of named Values.
        /// </summary>
        public NamedValueWrapperBase(IDictionary<string, Type> typeDefs)
        {
            _namedValuesWithType = new Dictionary<string, ValPlusType>();
            _typeDefs = typeDefs ?? throw new ArgumentNullException(nameof(typeDefs));
        }

        public NamedValueWrapperBase(Dictionary<string, ValPlusType> namedValuesToWrap, IDictionary<string, Type> typeDefs)
        {
            // TODO: Check to see if these two dictionary have conflicting contents.
            _namedValuesWithType = namedValuesToWrap;
            _typeDefs = typeDefs ?? throw new ArgumentNullException(nameof(typeDefs));
        }

        /// <summary>
        /// Creates a new ProxyBase with an inital set of Named Values copied
        /// from the provided list of named values.
        /// </summary>
        /// <param name="namedValues"></param>
        public NamedValueWrapperBase(IEnumerable<KeyValuePair<string, ValPlusType>> namedValues, IDictionary<string, Type> typeDefs)
        {
            foreach(KeyValuePair<string, ValPlusType> kvp in namedValues)
            {
                _namedValuesWithType.Add(kvp.Key, kvp.Value);
            }

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
            return _namedValuesWithType[s].Value;
        }

        public bool SetItWithType(object o, Type t, string s)
        {
            ValPlusType n = new ValPlusType(o, t);
            _namedValuesWithType.Add(s, n);
            return true;
        }

        public bool SetVal(string s, object o)
        {
            Type pType = GetTypeOfProperty(s);
            ValPlusType n = new ValPlusType(o, pType);
            _namedValuesWithType.Add(s, n);
            return true;
        }

        public Type GetTypeOfProperty(string s)
        {
            //if (!IsInitialized) throw new InvalidOperationException("ProxyBase is not initialize.");
            return _typeDefs[s];
        }

        #endregion
    }
}
