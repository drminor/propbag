using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace DRM.PropBag.ControlModel
{
    public class CollectionItem : NotifyPropertyChangedBase
    {
        string _propertyName; // The name that will be used for the property accessor on the ViewModel
        Type _propertyType; // The type of class that implements this INotifyCollectionChanged-based object.

        PropItem _collectionPropItem; // The type of object that each member of the collection will have.

        bool _useObservableOf_T;

        public CollectionItem(string propertyName, Type propertyType, PropItem collectionPropItem)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            CollectionPropItem = collectionPropItem ?? throw new ArgumentNullException(nameof(collectionPropItem));

            _useObservableOf_T = true;
        }

        public CollectionItem(string propertyName, Type propertyType, Type collectionItemType)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));

            Type itemType = collectionItemType ?? throw new ArgumentNullException(nameof(collectionItemType));

            CollectionPropItem = new PropItem(collectionItemType, $"{propertyName}_item");

            _useObservableOf_T = true;
        }

        public string PropertyName { get {return _propertyName;}  set { SetIfDifferent<string>(ref _propertyName, value); } }

        public Type PropertyType { get { return _propertyType; } set { _propertyType = value; } }

        public PropItem CollectionPropItem { get { return _collectionPropItem; } set { _collectionPropItem = value; } }


    }
}
