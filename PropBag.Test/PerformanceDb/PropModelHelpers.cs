using DRM.PropBag;
using DRM.TypeSafePropertyBag;

using PropBagLib.Tests.BusinessModel;

using System;
using System.Collections.ObjectModel;

namespace PropBagLib.Tests.PerformanceDb
{
    public class PropModelHelpers
    {
        public PropModel GetPropModelForModel1Dest(IPropFactory propFactory)
        {
            PropModel result = new PropModel
                (
                className: "DestinationModel1",
                namespaceName: "PropBagLib.Tests.PerformanceDb",
                deriveFrom: DeriveFromClassModeEnum.Custom,
                targetType: typeof(DestinationModel1),
                propFactory: propFactory,
                propFactoryType: null,
                propModelProvider: null,
                typeSafetyMode: PropBagTypeSafetyMode.Tight,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true
                );

            result.Namespaces.Add("System");
            result.Namespaces.Add("DRM.PropBag");
            result.Namespaces.Add("DRM.TypeSafePropertyBag");

            // Id (Int - 0)
            IPropInitialValueField pivf = new PropInitialValueField(initialValue: "0");

            PropItemModel propItem = new PropItemModel(type: typeof(int), name: "Id",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            pivf = PropInitialValueField.UseNull;

            // First Name (string - null)
            propItem = new PropItemModel(type: typeof(string), name: "FirstName",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            // Last Name (string - null)
            propItem = new PropItemModel(type: typeof(string), name: "LastName",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            // City Of Residence (string - null)
            propItem = new PropItemModel(type: typeof(string), name: "CityOfResidence",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            pivf = PropInitialValueField.UseDefault;

            // Profession
            propItem = new PropItemModel(type: typeof(Profession), name: "Profession",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            return result;
        }

        public PropModel GetPropModelForModel5Dest(IPropFactory propFactory)
        {
            PropModel result = new PropModel
                (
                className: "DestinationModel5",
                namespaceName: "PropBagLib.Tests.PerformanceDb",
                deriveFrom: DeriveFromClassModeEnum.Custom,
                targetType: typeof(DestinationModel5),
                propFactory: propFactory,
                propFactoryType: null,
                propModelProvider: null,
                typeSafetyMode: PropBagTypeSafetyMode.Tight,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true
                );

            result.Namespaces.Add("System");
            result.Namespaces.Add("DRM.PropBag");
            result.Namespaces.Add("DRM.TypeSafePropertyBag");

            // ProductId (Guid - default)
            IPropInitialValueField pivf = PropInitialValueField.UseDefault;

            PropItemModel propItem = new PropItemModel(type: typeof(Guid), name: "ProductId",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);


            // Business (Business - null)
            pivf = PropInitialValueField.UseNull;

            propItem = new PropItemModel(type: typeof(Business), name: "Business",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);


            // PersonCollection (ObservableCollection<Person> - null)
            propItem = new PropItemModel(type: typeof(ObservableCollection<Person>), name: "PersonCollection",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.ObservableCollection,
                initialValueField: pivf, itemType: typeof(Person));
            result.Add(propItem.PropertyName, propItem);

            return result;
        }

        public PropModel GetPropModelForModel6Dest(IPropFactory propFactory)
        {
            PropModel result = new PropModel
                (
                className: "DestinationModel6",
                namespaceName: "PropBagLib.Tests.PerformanceDb",
                deriveFrom: DeriveFromClassModeEnum.Custom,
                targetType: typeof(DestinationModel6),
                propFactory: propFactory,
                propFactoryType: null,
                propModelProvider: null,
                typeSafetyMode: PropBagTypeSafetyMode.Tight,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true
                );

            result.Namespaces.Add("System");
            result.Namespaces.Add("DRM.PropBag");
            result.Namespaces.Add("DRM.TypeSafePropertyBag");

            // Business (Business - null)
            IPropInitialValueField pivf = PropInitialValueField.UseNull;

            PropItemModel propItem = new PropItemModel(type: typeof(Business), name: "Business",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            //// PersonCollection (ObservableCollection<Person> - null)
            //pivf = new PropInitialValueField(initialValue: null,
            //    setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            //propItem = new PropItem(type: typeof(ObservableCollection<Person>), name: "PersonCollection",
            //    hasStore: true, propKind: PropKindEnum.Collection,
            //    propTypeInfoField: null,
            //    initialValueField: pivf,
            //    doWhenChanged: null, extraInfo: null, comparer: null, itemType: typeof(Person));
            //result.Props.Add(propItem);

            // ChildVM (DestinationModel5 - null)
            propItem = new PropItemModel(type: typeof(DestinationModel5), name: "ChildVM",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            // SelectedPerson (Business - null)
            propItem = new PropItemModel(type: typeof(Person), name: "SelectedPerson",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            // WMessage (String - null)
            propItem = new PropItemModel(type: typeof(string), name: "WMessage",
                storageStrategy: PropStorageStrategyEnum.Internal, propKind: PropKindEnum.Prop,
                initialValueField: pivf);
            result.Add(propItem.PropertyName, propItem);

            return result;
        }
    }
}
