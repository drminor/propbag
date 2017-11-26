using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag.LocalBinding.Engine
{
    public class OSCollection<T> : List<ObservableSource<T>>
    {
        public OSCollection()
        {
        }

        public new void Add(ObservableSource<T> os)
        {
            if (os == null) throw new InvalidOperationException("Items of an OSCollection cannot be null.");
            base.Add(os);
        }

        public new ObservableSource<T> this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                base[index] = value ?? throw new InvalidOperationException("Items of an OSCollection cannot be null.");
            }
    
        }

        public string Path
        {
            get
            {
                string result = null;

                for (int nPtr = 1; nPtr < Count; nPtr++)
                {
                    if (result != null) result += "/";
                    result += this[nPtr].PathElement;
                }

                return result ?? ".";
            }
        }

        private string GetConnectedPathElement(bool isFirst, PathConnectorTypeEnum po, string pathElement)
        {
            if (isFirst)
            {
                switch (po)
                {
                    case PathConnectorTypeEnum.Dot: return pathElement;
                    //case PathConnectorTypeEnum.Slash: return pathElement ?? ".";
                    //case PathConnectorTypeEnum.DotIndexer: return $"[{pathElement}]";
                    //case PathConnectorTypeEnum.SlashIndexer: return $"[{pathElement}]";
                    default:
                        {
                            throw new InvalidOperationException($"The value {po} is not a supported or recognized PathOperatorEnum.");
                        }
                }
            }
            else
            {
                //if (pathElement == null)
                //{
                //    throw new ArgumentNullException("The PathElement cannot be null except for paths with only one component.");
                //}

                //pathElement = pathElement ?? "Not Yet Determined.";

                switch (po)
                {
                    case PathConnectorTypeEnum.Dot: return $"/{pathElement}";
                    //case PathConnectorTypeEnum.Slash: return $"/{pathElement}";
                    //case PathConnectorTypeEnum.DotIndexer: return $".[{pathElement}]";
                    //case PathConnectorTypeEnum.SlashIndexer: return $"/[{pathElement}]";
                    default:
                        {
                            throw new InvalidOperationException($"The value {po} is not a supported or recognized PathOperatorEnum.");
                        }
                }
            }
        }

        //public void ResetListeners(int startIndex, EventHandler<DataSourceChangedEventArgs> dsSubscription)
        //{
        //    for (int nPtr = startIndex; nPtr < Count; nPtr++)
        //    {
        //        this[nPtr].Reset(dsSubscription);
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathListeners"></param>
        /// <param name="index"></param>
        /// <param name="newListenerProvider"></param>
        /// <param name="dsSubscription"></param>
        /// <param name="newListener"></param>
        /// <returns>True if the path listener is ready to start listening to property and/or collection change events.</returns>
        //public ObservableSourceStatusEnum ReplaceListener(int index, ObservableSource<T> newListener,
        //    EventHandler<DataSourceChangedEventArgs> dsSubscription, EventHandler<PCTypedEventArgs<T>> typedSubscription)
        //{
        //    if (index == Count - 1)
        //    {
        //        System.Diagnostics.Debug.Assert(index != Count - 1,
        //            "We are replacing the terminal listener. This is not good.");
        //    }

        //    ObservableSource<T> oldListener = this[index];

        //    bool wasListening = oldListener?.IsListeningForNewDC == true;

        //    if (newListener?.IsListeningForNewDC != true)
        //    {
        //        // The new ObservableSource<T> is not responding to any change events.
        //        if (index == 0)
        //        {
        //            System.Diagnostics.Debug.WriteLine("The Source Root is not listening.");
        //        }
        //        else if (wasListening)
        //        {
        //            System.Diagnostics.Debug.WriteLine($"The ObservableSource<T> at node {index} is no longer listening to any change events.");
        //        }
        //    }

        //    if (oldListener != null)
        //    {
        //        #if DEBUG
        //        bool wasRemoved = oldListener.Unsubscribe(dsSubscription);
        //        System.Diagnostics.Debug.Assert(wasListening == wasRemoved, "Was Listening does not agree with WasRemoved.");

        //        // Remove PropertyChanged or CollectionChanged event handlers, if any.
        //        oldListener.Reset(dsSubscription);
        //        #else
        //        // Remove all event handlers.
        //        oldListener.Reset(dsSubscription);
        //        #endif
        //    }

        //    if (newListener == null)
        //    {
        //        // It is the caller's responsibility to cleanup the dependent nodes.
        //        return ObservableSourceStatusEnum.NoType;
        //    }

        //    if (newListener.IsListeningForNewDC)
        //    {
        //        bool isListening = newListener.Subscribe(dsSubscription);
        //        System.Diagnostics.Debug.Assert(isListening, "dsSubscription was already present, but should not have been.");
        //        isListening = newListener.IsListeningForNewDC;
        //    }

        //    this[index] = newListener;

        //    #if DEBUG
        //    bool hasType = newListener.GetHasTypeAndHasData(out bool hasData);
        //    #endif

        //    // The new path listener is ready to start monitoring Property and/or Collection Changed events if 
        //    // it still has a reference to the object that will be monitored.
        //    return newListener.Status;
        //}
    }
}
