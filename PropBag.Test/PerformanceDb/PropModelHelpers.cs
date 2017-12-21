using DRM.PropBag.ControlModel;
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
                deriveFrom: DeriveFromClassModeEnum.PropBag,
                targetType: null,
                propFactory: null,
                typeSafetyMode: PropBagTypeSafetyMode.Tight,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true
                );

            result.Namespaces.Add("System");
            result.Namespaces.Add("DRM.PropBag");
            result.Namespaces.Add("DRM.TypeSafePropertyBag");

            result.PropFactory = propFactory;

            // Id (Int - 0)
            PropInitialValueField pivf = new PropInitialValueField(initialValue: "0",
                setToDefault: false, setToUndefined: false, setToNull: false, setToEmptyString: false);

            PropItem propItem = new PropItem(type: typeof(int), name: "Id",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            // First Name (string - null)
            propItem = new PropItem(type: typeof(string), name: "FirstName",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            // Last Name (string - null)
            propItem = new PropItem(type: typeof(string), name: "LastName",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            // City Of Residence (string - null)
            propItem = new PropItem(type: typeof(string), name: "CityOfResidence",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: true, setToUndefined: false, setToNull: true, setToEmptyString: false);

            // Profession
            propItem = new PropItem(type: typeof(Profession), name: "Profession",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            return result;
        }

        public PropModel GetPropModelForModel5Dest(IPropFactory propFactory)
        {
            PropModel result = new PropModel
                (
                className: "DestinationModel5",
                namespaceName: "PropBagLib.Tests.PerformanceDb",
                deriveFrom: DeriveFromClassModeEnum.PropBag,
                targetType: null,
                propFactory: null,
                typeSafetyMode: PropBagTypeSafetyMode.Tight,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true
                );

            result.Namespaces.Add("System");
            result.Namespaces.Add("DRM.PropBag");
            result.Namespaces.Add("DRM.TypeSafePropertyBag");

            result.PropFactory = propFactory;

            // ProductId (Guid - default)
            PropInitialValueField pivf = new PropInitialValueField(initialValue: null,
                setToDefault: true, setToUndefined: false, setToNull: false, setToEmptyString: false);

            PropItem propItem = new PropItem(type: typeof(Guid), name: "ProductId",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);


            // Business (Business - null)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            propItem = new PropItem(type: typeof(Business), name: "Business",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);


            // PersonCollection (ObservableCollection<Person> - null)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            propItem = new PropItem(type: typeof(ObservableCollection<Person>), name: "PersonCollection",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.ObservableCollection,
                propTypeInfoField: null,
                initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: typeof(Person));
            result.Props.Add(propItem);

            return result;
        }

        public PropModel GetPropModelForModel6Dest(IPropFactory propFactory)
        {
            PropModel result = new PropModel
                (
                className: "DestinationModel6",
                namespaceName: "PropBagLib.Tests.PerformanceDb",
                deriveFrom: DeriveFromClassModeEnum.PropBag,
                targetType: null,
                propFactory: null,
                typeSafetyMode: PropBagTypeSafetyMode.Tight,
                deferMethodRefResolution: true,
                requireExplicitInitialValue: true
                );

            result.Namespaces.Add("System");
            result.Namespaces.Add("DRM.PropBag");
            result.Namespaces.Add("DRM.TypeSafePropertyBag");

            result.PropFactory = propFactory;

            // Business (Business - null)
            PropInitialValueField pivf = new PropInitialValueField(initialValue: null,
                setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            PropItem propItem = new PropItem(type: typeof(Business), name: "Business",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);


            //// PersonCollection (ObservableCollection<Person> - null)
            //pivf = new PropInitialValueField(initialValue: null,
            //    setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            //propItem = new PropItem(type: typeof(ObservableCollection<Person>), name: "PersonCollection",
            //    hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Collection,
            //    propTypeInfoField: null,
            //    initialValueField: pivf,
            //    doWhenChanged: null, extraInfo: null, comparer: null, itemType: typeof(Person));
            //result.Props.Add(propItem);

            // ChildVM (DestinationModel5 - null)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            propItem = new PropItem(type: typeof(DestinationModel5), name: "ChildVM",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null,
                initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: typeof(Person));
            result.Props.Add(propItem);

            // SelectedPerson (Business - null)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            propItem = new PropItem(type: typeof(Person), name: "SelectedPerson",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            // WMessage (String - null)
            pivf = new PropInitialValueField(initialValue: null,
                setToDefault: false, setToUndefined: false, setToNull: true, setToEmptyString: false);

            propItem = new PropItem(type: typeof(string), name: "WMessage",
                hasStore: true, typeIsSolid: true, propKind: PropKindEnum.Prop,
                propTypeInfoField: null, initialValueField: pivf,
                extraInfo: null, comparer: null, itemType: null);
            result.Props.Add(propItem);

            return result;
        }
    }
}
