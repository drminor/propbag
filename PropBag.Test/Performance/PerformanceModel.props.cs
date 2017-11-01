
using System.Reflection;
using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using PropBagLib.Tests;


namespace PropBagLib.Tests
{
	public partial class PerformanceModel : PropBag
	{
		public PerformanceModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, null) { }

		public PerformanceModel(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, null) { }

		public PerformanceModel(PropBagTypeSafetyMode typeSafetyMode, IPropFactory factory) : base(typeSafetyMode, factory)
		{
	        AddProp<object>("PropObject", null, false, null);
	        AddProp<string>("PropString", null, false, null, null, "");
	        AddPropObjComp<string>("PropStringUseRefComp", null, false);
	        AddProp<bool>("PropBool", null, false, null, null, false);
	        AddProp<int>("PropInt", null, false, null);
	        AddProp<TimeSpan>("PropTimeSpan", null, false, null);
	        AddProp<Uri>("PropUri", null, false, null);
	        AddProp<Lazy<int>>("PropLazyInt", null, false, null);
	        AddProp<Nullable<int>>("PropNullableInt", null, false, null, null, -1);
	        AddProp<ICollection<int>>("PropICollectionInt", null, false, null, null, null);
		}

	#region Property Declarations
		  
		public object PropObject
		{
			get
			{
				return GetIt<object>(nameof(PropObject));
			}
			set
			{
				SetIt<object>(value, nameof(PropObject));
			}
		}  
	  
		public string PropString
		{
			get
			{
				return GetIt<string>(nameof(PropString));
			}
			set
			{
				SetIt<string>(value, nameof(PropString));
			}
		}  
	  
		public string PropStringUseRefComp
		{
			get
			{
				return GetIt<string>(nameof(PropStringUseRefComp));
			}
			set
			{
				SetIt<string>(value, nameof(PropStringUseRefComp));
			}
		}  
	  
		public bool PropBool
		{
			get
			{
				return GetIt<bool>(nameof(PropBool));
			}
			set
			{
				SetIt<bool>(value, nameof(PropBool));
			}
		}  
	  
		public int PropInt
		{
			get
			{
				return GetIt<int>(nameof(PropInt));
			}
			set
			{
				SetIt<int>(value, nameof(PropInt));
			}
		}  
	  
		public TimeSpan PropTimeSpan
		{
			get
			{
				return GetIt<TimeSpan>(nameof(PropTimeSpan));
			}
			set
			{
				SetIt<TimeSpan>(value, nameof(PropTimeSpan));
			}
		}  
	  
		public Uri PropUri
		{
			get
			{
				return GetIt<Uri>(nameof(PropUri));
			}
			set
			{
				SetIt<Uri>(value, nameof(PropUri));
			}
		}  
	  
		public Lazy<int> PropLazyInt
		{
			get
			{
				return GetIt<Lazy<int>>(nameof(PropLazyInt));
			}
			set
			{
				SetIt<Lazy<int>>(value, nameof(PropLazyInt));
			}
		}  
	  
		public Nullable<int> PropNullableInt
		{
			get
			{
				return GetIt<Nullable<int>>(nameof(PropNullableInt));
			}
			set
			{
				SetIt<Nullable<int>>(value, nameof(PropNullableInt));
			}
		}  
	  
		public ICollection<int> PropICollectionInt
		{
			get
			{
				return GetIt<ICollection<int>>(nameof(PropICollectionInt));
			}
			set
			{
				SetIt<ICollection<int>>(value, nameof(PropICollectionInt));
			}
		}  
	 
	#endregion

	#region PropetyChangedWithTVals Event Declarations
		  
			public event PropertyChangedWithTValsHandler<object> PropObjectChanged
			{
				add
				{
					AddToPropChanged<object>(value, nameof(PropObjectChanged));
				}
				remove
				{
					RemoveFromPropChanged<object>(value, nameof(PropObjectChanged));
				}
			}
	  
			public event PropertyChangedWithTValsHandler<string> PropStringChanged
			{
				add
				{
					AddToPropChanged<string>(value, nameof(PropStringChanged));
				}
				remove
				{
					RemoveFromPropChanged<string>(value, nameof(PropStringChanged));
				}
			}
	  
			public event PropertyChangedWithTValsHandler<string> PropStringUseRefCompChanged
			{
				add
				{
					AddToPropChanged<string>(value, nameof(PropStringUseRefCompChanged));
				}
				remove
				{
					RemoveFromPropChanged<string>(value, nameof(PropStringUseRefCompChanged));
				}
			}
	  
			public event PropertyChangedWithTValsHandler<bool> PropBoolChanged
			{
				add
				{
					AddToPropChanged<bool>(value, nameof(PropBoolChanged));
				}
				remove
				{
					RemoveFromPropChanged<bool>(value, nameof(PropBoolChanged));
				}
			}
	  
			public event PropertyChangedWithTValsHandler<int> PropIntChanged
			{
				add
				{
					AddToPropChanged<int>(value, nameof(PropIntChanged));
				}
				remove
				{
					RemoveFromPropChanged<int>(value, nameof(PropIntChanged));
				}
			}
	  
			public event PropertyChangedWithTValsHandler<TimeSpan> PropTimeSpanChanged
			{
				add
				{
					AddToPropChanged<TimeSpan>(value, nameof(PropTimeSpanChanged));
				}
				remove
				{
					RemoveFromPropChanged<TimeSpan>(value, nameof(PropTimeSpanChanged));
				}
			}
	  
			public event PropertyChangedWithTValsHandler<Uri> PropUriChanged
			{
				add
				{
					AddToPropChanged<Uri>(value, nameof(PropUriChanged));
				}
				remove
				{
					RemoveFromPropChanged<Uri>(value, nameof(PropUriChanged));
				}
			}
	  
			public event PropertyChangedWithTValsHandler<Lazy<int>> PropLazyIntChanged
			{
				add
				{
					AddToPropChanged<Lazy<int>>(value, nameof(PropLazyIntChanged));
				}
				remove
				{
					RemoveFromPropChanged<Lazy<int>>(value, nameof(PropLazyIntChanged));
				}
			}
	  
			public event PropertyChangedWithTValsHandler<Nullable<int>> PropNullableIntChanged
			{
				add
				{
					AddToPropChanged<Nullable<int>>(value, nameof(PropNullableIntChanged));
				}
				remove
				{
					RemoveFromPropChanged<Nullable<int>>(value, nameof(PropNullableIntChanged));
				}
			}
	  
			public event PropertyChangedWithTValsHandler<ICollection<int>> PropICollectionIntChanged
			{
				add
				{
					AddToPropChanged<ICollection<int>>(value, nameof(PropICollectionIntChanged));
				}
				remove
				{
					RemoveFromPropChanged<ICollection<int>>(value, nameof(PropICollectionIntChanged));
				}
			}
	 
	#endregion

		/// <summary>
		/// If the delegate exists, the original name is returned,
		/// otherwise null is returned.
		/// </summary>
		/// <param name="methodName">Some public or non-public instance method in this class.</param>
		/// <returns>The name, unchanged, if the method exists, otherwise null.</returns>
		private Action<T, T> GetDelegate<T>(string methodName)
		{
		    Type pp = this.GetType();
		    MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		
		    if (mi == null) return null;
		
		    Action<T, T> result = (Action<T, T>)mi.CreateDelegate(typeof(Action<T, T>), this);
		
		    return result;
		}


	} 
}
