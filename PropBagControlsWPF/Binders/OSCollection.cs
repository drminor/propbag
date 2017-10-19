using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.PropBag.ControlsWPF.Binders
{

    public class OSCollection : List<ObservableSource>
    {
        public OSCollection(string rootElementName, string[] pathElements, Type sourceType, string binderName)
        {
            Add(new ObservableSource(rootElementName, binderName));

            int compCount = pathElements.Length;
            // Add the intervening nodes if any.
            for (int nPtr = 0; nPtr < compCount - 1; nPtr++)
            {
                Add(new ObservableSource(pathElements[nPtr], binderName));
            }

            if (compCount == 0)
            {
                // Path is empty, bind to the datasource.
                Add(new ObservableSource(string.Empty, sourceType, PathConnectorTypeEnum.Dot, binderName));
            }
            else
            {
                // Add the terminal node.
                Add(new ObservableSource(pathElements[compCount - 1], sourceType, PathConnectorTypeEnum.Dot, binderName));
            }
        }

        public new void Add(ObservableSource os)
        {
            if (os == null) throw new InvalidOperationException("Items of an OSCollection cannot be null.");
            base.Add(os);
        }

        public new ObservableSource this[int index]
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
                    if (result != null) result += ".";
                    result += this[nPtr].PathElement;
                }

                return result ?? ".";
            }
        }

        public string NewPath
        {
            get
            {
                string result = null;

                for (int nPtr = 1; nPtr < Count; nPtr++)
                {
                    result += GetConnectedPathElement(result == null, this[nPtr].PathConnector, this[nPtr].NewPathElement);
                }

                return result ?? ".";
            }
        }

        public string Path2 => string.Join(null, this.Skip(1).Select((x, idx) => GetConnectedPathElement(idx == 0, x.PathConnector, x.PathElement)));

        public string NewPath2 => string.Join(null, this.Skip(1).Select((x, idx) => GetConnectedPathElement(idx == 0, x.PathConnector, x.NewPathElement)));

        private string GetConnectedPathElement(bool isFirst, PathConnectorTypeEnum po, string pathElement)
        {
            if (isFirst)
            {
                switch (po)
                {
                    case PathConnectorTypeEnum.Dot: return pathElement ?? ".";
                    case PathConnectorTypeEnum.Slash: return pathElement ?? ".";
                    case PathConnectorTypeEnum.DotIndexer: return $"[{pathElement}]";
                    case PathConnectorTypeEnum.SlashIndexer: return $"[{pathElement}]";
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

                pathElement = pathElement ?? "Not Yet Determined.";

                switch (po)
                {
                    case PathConnectorTypeEnum.Dot: return $".{pathElement}";
                    case PathConnectorTypeEnum.Slash: return $"/{pathElement}";
                    case PathConnectorTypeEnum.DotIndexer: return $".[{pathElement}]";
                    case PathConnectorTypeEnum.SlashIndexer: return $"/[{pathElement}]";
                    default:
                        {
                            throw new InvalidOperationException($"The value {po} is not a supported or recognized PathOperatorEnum.");
                        }
                }
            }
        }

        public void ResetListeners(int startIndex, DataSourceChangedEventHandler subscriber)
        {
            // Note: Resetting the TerminalNode is not supported, and never needed.
            for (int nPtr = startIndex; nPtr < Count - 1; nPtr++)
            {
                this[nPtr].Reset(subscriber);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathListeners"></param>
        /// <param name="index"></param>
        /// <param name="newListenerProvider"></param>
        /// <param name="subscriber"></param>
        /// <param name="newListener"></param>
        /// <returns>True if the path listener is ready to start listening to property and/or collection change events.</returns>
        public ObservableSourceStatusEnum ReplaceListener(int index, ref ObservableSourceProvider newListenerProvider,
            DataSourceChangedEventHandler subscriber, out ObservableSource newListener)
        {
            if (index == Count - 1)
            {
                System.Diagnostics.Debug.Assert(index != Count - 1,
                    "We are replacing the terminal listener. This is not good.");
            }

            ObservableSource oldListener = this[index];

            if (newListenerProvider != null)
            {
                newListener = newListenerProvider.CreateObservableSource(); newListenerProvider = null;
            }
            else
            {
                newListener = null;
            }

            bool wasListening = oldListener?.IsListeningForNewDC == true;

            if (newListener?.IsListeningForNewDC != true)
            {
                // The new ObservableSource is not responding to any change events.
                if (index == 0)
                {
                    System.Diagnostics.Debug.WriteLine("The Source Root is not listening.");
                }
                else if (wasListening)
                {
                    System.Diagnostics.Debug.WriteLine($"The ObservableSource at node {index} is no longer listening to any change events.");
                }
            }

            if (oldListener != null)
            {
#if DEBUG
                bool wasRemoved = oldListener.Unsubscribe(subscriber);
                System.Diagnostics.Debug.Assert(wasListening == wasRemoved, "Was Listening does not agree with WasRemoved.");

                // Remove PropertyChanged or CollectionChanged event handlers, if any.
                oldListener.Reset();
#else
                // Remove all event handlers.
                oldListener.Reset(subscriber);
#endif
            }

            if (newListener == null)
            {
                // It is the caller's responsibility to cleanup the dependent nodes.
                return ObservableSourceStatusEnum.NoType;
            }

            if (newListener.IsListeningForNewDC)
            {
                bool isListening = newListener.Subscribe(subscriber);
                System.Diagnostics.Debug.Assert(isListening, "Subscriber was already present, but should not have been.");
                isListening = newListener.IsListeningForNewDC;
            }

            this[index] = newListener;

#if DEBUG
            bool hasType = newListener.GetHasTypeAndHasData(out bool hasData);
#endif

            // The new path listener is ready to start monitoring Property and/or Collection Changed events if 
            // it still has a reference to the object that will be monitored.
            return newListener.Status;
        }
    }
}
