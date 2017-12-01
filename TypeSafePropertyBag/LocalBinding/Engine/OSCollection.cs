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

        public new void Clear()
        {
            foreach(ObservableSource<T> listener in this)
            {
                listener.Dispose();
            }

            base.Clear();
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




    }
}
