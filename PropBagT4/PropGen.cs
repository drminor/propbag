
using System;
using System.Collections.Generic;
using PropBagLib.Tests;
using DRM.PropBag;
using DRM.Ipnwvc;


namespace PropBagLib.Tests
{
	public partial class PropGen : PropBag
	{
		public PropGen() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered) { }

		public PropGen(PropBagTypeSafetyMode typeSafetyMode) : base(typeSafetyMode)
		{
            //AddProp<object>("PropObject", null, false, null);
            //AddProp<string>("PropString", null, false, null);
            //AddPropNoStoreObjComp<bool>("PropBool", yyy, true, null);
            //AddPropNoStore<int>("PropInt", null, false, null);
            //AddPropExtStore<TimeSpan>("PropTimeSpan", null, null, DoWhenTimeSpanChanges, false, null);
            //AddPropObjComp<Uri>("PropUri", TestDelegate, false, null);
            //AddProp<Lazy<int>>("PropLazyInt", null, false, MyTestComparer, new Lazy<int>(() => 10));
		}

	#region Property Declarations
		  
		public object PropObject
		{
			get
			{
				return GetIt<object>();
			}
			set
			{
				SetIt<object>(value);
			}
		}  
	  
		public string PropString
		{
			get
			{
				return GetIt<string>();
			}
			set
			{
				SetIt<string>(value);
			}
		}  
	  
		public bool PropBool
		{
			get
			{
				return GetIt<bool>();
			}
			set
			{
				SetIt<bool>(value);
			}
		}  
	  
		public int PropInt
		{
			get
			{
				return GetIt<int>();
			}
			set
			{
				SetIt<int>(value);
			}
		}  
	  
		public TimeSpan PropTimeSpan
		{
			get
			{
				return GetIt<TimeSpan>();
			}
			set
			{
				SetIt<TimeSpan>(value);
			}
		}  
	  
		public Uri PropUri
		{
			get
			{
				return GetIt<Uri>();
			}
			set
			{
				SetIt<Uri>(value);
			}
		}  
	  
		public Lazy<int> PropLazyInt
		{
			get
			{
				return GetIt<Lazy<int>>();
			}
			set
			{
				SetIt<Lazy<int>>(value);
			}
		}  
	 
	#endregion

	#region PropetyChangedWithTVals Event Declarations
		  
			public event PropertyChangedWithTValsHandler<object> PropObjectChanged
			{
				add
				{
					AddToPropChanged<object>(value);
				}
				remove
				{
					RemoveFromPropChanged<object>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<string> PropStringChanged
			{
				add
				{
					AddToPropChanged<string>(value);
				}
				remove
				{
					RemoveFromPropChanged<string>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<bool> PropBoolChanged
			{
				add
				{
					AddToPropChanged<bool>(value);
				}
				remove
				{
					RemoveFromPropChanged<bool>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<int> PropIntChanged
			{
				add
				{
					AddToPropChanged<int>(value);
				}
				remove
				{
					RemoveFromPropChanged<int>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<TimeSpan> PropTimeSpanChanged
			{
				add
				{
					AddToPropChanged<TimeSpan>(value);
				}
				remove
				{
					RemoveFromPropChanged<TimeSpan>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<Uri> PropUriChanged
			{
				add
				{
					AddToPropChanged<Uri>(value);
				}
				remove
				{
					RemoveFromPropChanged<Uri>(value);
				}
			}
	  
			public event PropertyChangedWithTValsHandler<Lazy<int>> PropLazyIntChanged
			{
				add
				{
					AddToPropChanged<Lazy<int>>(value);
				}
				remove
				{
					RemoveFromPropChanged<Lazy<int>>(value);
				}
			}
	 
	#endregion

	} 
}
