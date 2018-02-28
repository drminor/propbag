using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;
    using PropItemSetInterface = IPropItemSet<String>;

    public class PropItemSet : PropItemSetInterface/*, IEquatable<PropItemSet>*/
    {
        #region Private Members

        Dictionary<PropNameType, IPropTemplate> _propItems;

        object _sync = new object();

        #endregion

        #region Constructor

        public PropItemSet(/*int maxPropsPerObject*/)
        {
            //MaxPropsPerObject = maxPropsPerObject;
            _propItems = new Dictionary<PropNameType, IPropTemplate>();
        }

        #endregion

        #region Public Members

        public int Count => _propItems.Count;

        //public int MaxPropsPerObject { get; }

        //private bool _isFixed;
        //public bool IsFixed
        //{
        //    get
        //    {
        //        return _isFixed;
        //    }
        //}

        //public void Fix()
        //{
        //    lock(_sync)
        //    {
        //        _hashCode = ComputeHashCode();
        //        _isFixed = true;
        //    }
        //}


        public bool Contains(PropNameType propertyName)
        {
            bool result = _propItems.ContainsKey(propertyName);
            return result;
        }

        public bool Contains(IPropTemplate propTemplate)
        {
            bool result = _propItems.ContainsValue(propTemplate);
            return result;
        }

        public void Add(PropNameType propertyName, IPropTemplate propTemplate)
        {
            lock (_sync)
            {
                //if (_isFixed)
                //{
                //    throw new InvalidOperationException("Cannot Add PropTemplates to a Fixed PropItemSet.");
                //}

                _propItems.Add(propertyName, propTemplate);
            }
        }

        public bool TryGetPropTemplate(PropNameType propertyName, out IPropTemplate propTemplate)
        {
            if (_propItems.TryGetValue(propertyName, out propTemplate))
            {
                return true;
            }
            else
            {
                propTemplate = null;
                return false;
            }
        }

        public bool TryRemove(PropNameType propertyName, out IPropTemplate propTemplate)
        {
            lock (_sync)
            {
                //if (_isFixed)
                //{
                //    throw new InvalidOperationException("PropTemplates cannot be remvoed from a fixed PropItemSet.");
                //}

                if (_propItems.TryGetValue(propertyName, out propTemplate))
                {
                    _propItems.Remove(propertyName);
                    return true;
                }
            }

            propTemplate = null;
            return false;
        }


        // TODO: Cannot clear, must delete from Cache.
        public void Clear()
        {
            lock (_sync)
            {
                //if(_isFixed)
                //{
                //    throw new InvalidOperationException("A fixed PropItemSet cannot be cleared.");
                //}

                _propItems.Clear();
            }
        }

        #endregion

        #region Collection Support

        public IEnumerable<PropNameType> GetPropNames()
        {
            IEnumerable<PropNameType> result = _propItems.Select(x => x.Key);
            return result;
        }

        public IEnumerable<IPropTemplate> GetPropTemplates()
        {
            IEnumerable<IPropTemplate> result = _propItems.Select(x => x.Value);
            return result;
        }

        #endregion

        #region Object Overrides and IEquatable Support

        public override string ToString()
        {
            //string @fixed = "IsFixed ? "Fixed" : "Open";
            //return $"{@fixed} PropItemSet with {Count} items.

            return $"PropItemSet with {Count} items.";
        }

        #endregion
    }
}
