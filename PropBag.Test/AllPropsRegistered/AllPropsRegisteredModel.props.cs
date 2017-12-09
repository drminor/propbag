
using System.Reflection;
using DRM.PropBag;
using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;


namespace PropBagLib.Tests
{
	public partial class AllPropsRegisteredModel : PropBag
	{
		public AllPropsRegisteredModel() : this(PropBagTypeSafetyMode.AllPropsMustBeRegistered, null) { }

		public AllPropsRegisteredModel(PropBagTypeSafetyMode typeSafetyMode) : this(typeSafetyMode, null) { }

		public AllPropsRegisteredModel(PropBagTypeSafetyMode typeSafetyMode, IPropFactory factory) : base(typeSafetyMode, factory)
		{
	        AddProp<object>("PropObject", comparer:null);
		 
	        AddProp<string>("PropString", comparer:null);
		    //SubscribeToPropChanged<string>(GetDelegate<string>("DoWhenStringChanged"), "PropString"); 

	        AddPropNoValue<string>("PropStringCallDoAfter", comparer:EqualityComparer<string>.Default.Equals);
		    //SubscribeToPropChanged<string>(GetDelegate<string>("DoWhenStringChanged"), "PropStringCallDoAfter"); 

	        AddPropObjComp<string>("PropStringUseRefComp", extraInfo:null);
		    //<string>(GetDelegate<string>("DoWhenStringChanged"), "PropStringUseRefComp");
            
	        AddProp<bool>("PropBool", comparer:null);
		 
	        AddProp<int>("PropInt", comparer:null);
		 
	        AddProp<TimeSpan>("PropTimeSpan", comparer:null);
		 
	        AddProp<Uri>("PropUri", comparer:null);
		 
	        AddProp<Lazy<int>>("PropLazyInt", comparer:null);
		 
	        AddProp<Nullable<int>>("PropNullableInt", null, null, initialValue:-1);
		    //SubscribeToPropChanged<Nullable<int>>(GetDelegate<Nullable<int>>("DoWhenNullIntChanged"), "PropNullableInt");
            
	        AddProp<ICollection<int>>("PropICollectionInt", comparer:null);
		    //SubscribeToPropChanged<ICollection<int>>(GetDelegate<ICollection<int>>("DoWhenICollectionIntChanged"), "PropICollectionInt"); 
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
	  
		public string PropStringCallDoAfter
		{
			get
			{
				return GetIt<string>(nameof(PropStringCallDoAfter));
			}
			set
			{
				SetIt<string>(value, nameof(PropStringCallDoAfter));
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
		  
			public event EventHandler<PCTypedEventArgs<object>> PropObjectChanged
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
	  
			public event EventHandler<PCTypedEventArgs<string>> PropStringChanged
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
	  
			public event EventHandler<PCTypedEventArgs<string>> PropStringCallDoAfterChanged
			{
				add
				{
					AddToPropChanged<string>(value, nameof(PropStringCallDoAfterChanged));
				}
				remove
				{
					RemoveFromPropChanged<string>(value, nameof(PropStringCallDoAfterChanged));
				}
			}
	  
			public event EventHandler<PCTypedEventArgs<string>> PropStringUseRefCompChanged
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
	  
			public event EventHandler<PCTypedEventArgs<bool>> PropBoolChanged
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
	  
			public event EventHandler<PCTypedEventArgs<int>> PropIntChanged
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
	  
			public event EventHandler<PCTypedEventArgs<TimeSpan>> PropTimeSpanChanged
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
	  
			public event EventHandler<PCTypedEventArgs<Uri>> PropUriChanged
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
	  
			public event EventHandler<PCTypedEventArgs<Lazy<int>>> PropLazyIntChanged
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
	  
			public event EventHandler<PCTypedEventArgs<Nullable<int>>> PropNullableIntChanged
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
	  
			public event EventHandler<PCTypedEventArgs<ICollection<int>>> PropICollectionIntChanged
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
		EventHandler<PCTypedEventArgs<T>> GetDelegate<T>(string methodName)
		{
		    Type pp = this.GetType();
		    MethodInfo mi = pp.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		
		    if (mi == null) return null;
		
		    EventHandler<PCTypedEventArgs<T>> result = (EventHandler<PCTypedEventArgs<T>>)mi.CreateDelegate(typeof(EventHandler<PCTypedEventArgs<T>>), this);
		
		    return result;
		}


	} 
}
