using System;

namespace DRM.TypeSafePropertyBag.TypeDescriptors
{

    //    public delegate void AddValueChangedDelegate(object component, EventHandler handler);
    //    public delegate bool CanResetValueDelegate(object component);
    //    public delegate PropertyDescriptorCollection GetChildPropertiesDelegate(object instance, Attribute[] filter);
    //    public delegate object GetEditorDelegate(Type editorBaseType);
    //    public delegate object GetValueDelegate(object component);
    //    public delegate void RemoveValueChangedDelegate(object component, EventHandler handler);
    //    public delegate void ResetValueDelegate(object component);
    //    public delegate void SetValueDelegate(object component, object value);
    //    public delegate bool ShouldSerializeValueDelegate(object component);


    public struct TypeDescriptorConfig
    {
        # region Constructor

        public TypeDescriptorConfig(Attribute[] attributes, /*string category, */Type componentType, 
            //TypeConverter converter, string description, bool designTimeOnly, string displayName, 
           /* bool isBrowsable, bool isLocalizable,*/ bool isReadOnly, string name, 
            Type propertyType, bool supportsChangeEvents) 
            
        {
            Attributes = attributes;
            //Category = category;
            ComponentType = componentType;
            //Converter = converter;
            //Description = description;
            //DesignTimeOnly = designTimeOnly;
            //DisplayName = displayName;
            //IsBrowsable = isBrowsable;
            //IsLocalizable = isLocalizable;
            IsReadOnly = isReadOnly;
            Name = name;
            PropertyType = propertyType;
            SupportsChangeEvents = supportsChangeEvents;

            //AddValueChangedDel = addValueChangedDel;
            //CanResetValueDel = canResetValueDel;
            //GetChildPropertiesDel = getChildPropertiesDel;
            //GetEditorDel = getEditorDel;
            //GetValueDel = getValueDel;
            //RemoveValueChangedDel = removeValueChangedDel;
            //ResetValueDel = resetValueDel;
            //SetValueDel = setValueDel;
            //ShouldSerializeValueDel = shouldSerializeValueDel;
        }

        #endregion

        #region TypeDescriptor Properties

        public Attribute[] Attributes { get; }

        //public string Category { get; }

        public Type ComponentType { get; }

        //public TypeConverter Converter { get; }

        //public string Description { get; }

        //public bool DesignTimeOnly { get; }

        //public string DisplayName { get; }

        //public bool IsBrowsable { get; }

        //public bool IsLocalizable { get; }

        public bool IsReadOnly { get; }

        public string Name { get; }

        public Type PropertyType { get; }

        public bool SupportsChangeEvents { get; }



        //public AddValueChangedDelegate AddValueChangedDel { get; }

        //public CanResetValueDelegate CanResetValueDel { get; set;}

        //public GetChildPropertiesDelegate GetChildPropertiesDel { get; }

        //public GetEditorDelegate GetEditorDel { get; }

        //public GetValueDelegate GetValueDel { get; }

        //public RemoveValueChangedDelegate RemoveValueChangedDel { get; }

        //public ResetValueDelegate ResetValueDel { get; }

        //public SetValueDelegate SetValueDel { get; }

        //public ShouldSerializeValueDelegate ShouldSerializeValueDel { get; }

        #endregion

        #region Helper Methods

        #endregion
    }
}
