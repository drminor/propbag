# Markdown file

    PropItem:
    Attribute: PropertyType(Type)
    Attribute: PropKind(PropKindEnum)
    
    Child Element: TypeInfoField
    
    For Simple Types:
    1. Use PropertyType on PropItem and leave PropKind blank.
    or
    2. Use PropKind = 'Prop' and Use TypeInfoField
    
    For DataTable:
    1. Use PropKind = 'DataTable' and use DataTableInfoField
    
    
    For Complex, non-collection
    1. Use PropKind = 'Prop' and Use TypeInfoField
    
    For Collection:
    1. Use PropKind = 'Collection' and use TypeInfoField
    
    
    TypeInfoField
        Attribute: PropertyType (Type)
        Attribute: TypeName (string)
        Attribute: FullyQualifiedTypeName: (string)
        Attribute: CollectionType: (WellKnownCollectionTypeEnum)
        Attribute: TypeParameter1 (Type)
        Attribute: TypeParameter2 (Type)
        Attribute: TypeParameter3 (Type)
    
        Child Elements:
        TypeInfoField
    
    
    DataTableInfoField
    
     More to come.