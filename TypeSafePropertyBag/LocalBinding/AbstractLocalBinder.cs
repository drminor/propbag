using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public class AbstractLocalBinder<PropDataT> : IBindLocalProps<PropDataT> where PropDataT : IPropGen
    {
        private SimpleLevel2KeyMan _level2KeyManager;
        private SimpleCompKeyMan _compKeyManager;
        private readonly SimpleObjectIdDictionary<PropDataT> _propertyStore;

        public AbstractLocalBinder
            (
            SimpleObjectIdDictionary<PropDataT> propertyStore,
            SimpleCompKeyMan compKeyManager,
            SimpleLevel2KeyMan level2KeyManager
            ) 
        {
            _propertyStore = propertyStore;
            _compKeyManager = compKeyManager;
            _level2KeyManager = level2KeyManager;
        }

        public PropDataT GetPropData(SimpleExKey propId)
        {
            return default(PropDataT);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">The type of the binding's source property.</typeparam>
        /// <param name="targetPropId"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public int UpdateTarget<T>(SimpleExKey targetPropId, T oldValue, T newValue)
        {
            return 0;
        }

        public int UpdateTarget<T>(IPropBag sourceHost, BindingSubscription<T> bs, T oldValue, T newValue)
        {
            //bs.TypedDoWhenChanged.t
            return 0;
        }

    }
}
