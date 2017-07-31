using System;
using System.Collections.Generic;
using DRM.Ipnwv;
using DRM.PropBag;

namespace PropBagLib.Tests
{
	public partial class TestNullable : PropBag
	{
		public TestNullable() : this(PropBagTypeSafetyMode.Loose) { }

		public TestNullable(PropBagTypeSafetyMode typeSafetyMode) : base(typeSafetyMode)
		{
				AddProp<Nullable<int>>("PropNullableInt");
			AddProp<ICollection<int>>("PropICollectionInt");
		}

	#region Property Declarations
		  
		public Nullable<int> PropNullableInt
		{
			get
			{
				return GetIt<Nullable<int>>();
			}
			set
			{
				SetIt<Nullable<int>>(value);
			}
		}  
	  
		public ICollection<int> PropICollectionInt
		{
			get
			{
				return GetIt<ICollection<int>>();
			}
			set
			{
				SetIt<ICollection<int>>(value);
			}
		}  
	 
	#endregion

	#region PropetyChangedWithTVals Event Declarations
		  
			public event PropertyChangedWithTValsHandler<Nullable<int>> PropNullableIntChanged
			{
				add
				{
					AddToPropChanged<Nullable<int>>(value);
				}
				remove
				{
					RemoveFromPropChanged<Nullable<int>>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<ICollection<int>> PropICollectionIntChanged
			{
				add
				{
					AddToPropChanged<ICollection<int>>(value);
				}
				remove
				{
					RemoveFromPropChanged<ICollection<int>>(value);
				}
			}
	 
	#endregion

	} 
}
