using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    //public delegate void PCObjEventHandler(object sender, PCObjectEventArgs e);
    //delegate void PCObjEventHandler(object @this, object sender, PcObjectEventArgs e);

    public class SubscriptionGen : ISubscription 
    {
        public ExKeyT OwnerPropId { get; protected set; }
        public Type PropertyType { get; }

        public SubscriptionKind SubscriptionKind { get; protected set; }
        public SubscriptionPriorityGroup SubscriptionPriorityGroup { get; protected set; }
        //public SubscriptionTargetKind SubscriptionTargetKind { get; protected set; }

        public WeakReference Target { get; protected set; }
        public string MethodName => HandlerProxy.Method.Name;

        //public EventHandlerProxy<PCGenEventArgs> GenHandlerProxy { get; protected set; }
        //public PCObjectEventAction ObjHandlerProxy { get; protected set; }
        //public EventHandlerProxy<PropertyChangedEventArgs> StandardHandlerProxy { get; protected set; }

        public Delegate HandlerProxy { get; }

        //public EventHandler<PCGenEventArgs> GenHandler { get; protected set; }
        //public EventHandler<PCObjectEventArgs> ObjHandler { get; protected set; }
        //public EventHandler<PropertyChangedEventArgs> StandardHandler { get; protected set; }

        public Action<object, object> GenDoWhenChanged { get; protected set; }
        public Action Action { get; protected set; }

        // Binding Subscription Members
        public LocalBindingInfo BindingInfo => throw new InvalidOperationException("SubscriptionGen cannot be used for BindingSubscriptions.");
        public object LocalBinderAsObject => throw new InvalidOperationException("SubscriptionGen cannot be used for BindingSubscriptions.");

        public SubscriptionGen(ISubscriptionKeyGen sKey)
        {
            if(sKey.HasBeenUsed)
            {
                throw new InvalidOperationException("The Key has already been used.");
            }

            OwnerPropId = sKey.OwnerPropId;
            PropertyType = sKey.PropertyType;

            SubscriptionKind = sKey.SubscriptionKind;
            SubscriptionPriorityGroup = sKey.SubscriptionPriorityGroup;
            //SubscriptionTargetKind = sKey.SubscriptionTargetKind;

            switch (SubscriptionKind)
            {
                //case SubscriptionKind.TypedHandler:
                //    break;

                case SubscriptionKind.GenHandler:
                    {
                        Target = new WeakReference(sKey.GenHandler.Target);

                        MethodInfo mi = sKey.GenHandler.Method;
                        Delegate proxyDelegate = mi.CreateDelegate(); // Uses static extension Method -- TODO: Provide Instance Library.
                        HandlerProxy = proxyDelegate;

                        break;
                    }
                case SubscriptionKind.ObjHandler:
                    {
                        Target = new WeakReference(sKey.ObjHandler.Target);

                        MethodInfo mi = sKey.ObjHandler.Method;
                        Delegate proxyDelegate = mi.CreateDelegate(); // Uses static extension Method -- TODO: Provide Instance Library.
                        HandlerProxy = proxyDelegate;

                        break;
                    }
                case SubscriptionKind.StandardHandler:
                    {
                        Target = new WeakReference(sKey.StandardHandler.Target);

                        MethodInfo mi = sKey.StandardHandler.Method;
                        Delegate proxyDelegate = mi.CreateDelegate(); // Uses static extension Method -- TODO: Provide Instance Library.
                        HandlerProxy = proxyDelegate;

                        break;
                    }

                //case SubscriptionKind.TypedAction:
                //    break;
                //case SubscriptionKind.ObjectAction:
                //    break;
                //case SubscriptionKind.ActionNoParams:
                //    break;
                //case SubscriptionKind.LocalBinding:
                //    break;
                default:
                    throw new InvalidOperationException($"The SubscriptionKind: {SubscriptionKind} is not recognized or is not supported.");
            }

            //GenDoWhenChanged = sKey.GenDoWhenChanged;
            //Action = sKey.Action;
        }

        void Main()
        {
            MethodInfo sayHelloMethod = typeof(Person).GetMethod("SayHello");
            OpenAction<Person, string> action =
                (OpenAction<Person, string>)
                    Delegate.CreateDelegate(
                        typeof(OpenAction<Person, string>),
                        null,
                        sayHelloMethod);

            Person joe = new Person { Name = "Joe" };
            action(joe, "Jack"); // Prints "Hello Jack, my name is Joe"
        }

        delegate void OpenAction<TThis, T>(TThis @this, T arg);


        class Person
        {
            public string Name { get; set; }

            public void SayHello(string name)
            {
                //Console.WriteLine("Hi {0}, my name is {1}", name, this.Name);
                System.Diagnostics.Debug.WriteLine($"Hi {name}, my name is {this.Name}");
            }
        }

        private void YYY()
        {

            //Type dType = sKey.ObjHandler.Method.GetDelegateType();

            //var temp3 = Delegate.CreateDelegate(dType, null, sKey.ObjHandler.Method);

            //var temp4 = Convert.ChangeType(temp3, dType);

            //Main();

            //PCObjEventHandler xx = (PCObjEventHandler)Delegate.CreateDelegate(typeof(PCObjEventHandler), null, mi);


            ////PCObjEventHandler temp = (PCObjEventHandler) Delegate.CreateDelegate(typeof(PCObjEventHandler), null, sKey.ObjHandler.Method);
            ////ObjHandler = temp;



            ////PCObjectEventAction temp2 = (PCObjectEventAction) Delegate.CreateDelegate(typeof(PCObjectEventAction), null, sKey.ObjHandler.Method);
            ////ObjHandlerProxy = temp4;
        }

    }


}
