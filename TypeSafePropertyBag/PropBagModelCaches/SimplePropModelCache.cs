using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;
    using PropModelCacheInterface = ICachePropModels<String>;
    using PropBagModelCachesCollInterface = IPropModelFamilyCollection<String>;

    public class SimplePropModelCache : PropModelCacheInterface
    {
        public const long GEN_ZERO = 0;

        private readonly Dictionary<string, PropBagModelCachesCollInterface> _cache;
        private readonly List<IProvidePropModels> _propModelProviders;

        private object _syncLock = new object();

        public SimplePropModelCache(params IProvidePropModels[] propModelProviders)
        {
            _propModelProviders = new List<IProvidePropModels>(propModelProviders);

            _cache = new Dictionary<PropNameType, PropBagModelCachesCollInterface>();
        }

        public void Fix(PropModelType propModel)
        {
            if(propModel.IsFixed)
            {
                System.Diagnostics.Debug.WriteLine("Already Fixed.");
                return;
            }
            else
            {
                //long generationId = Add(propModel);
                //propModel.GenerationId = generationId;
                propModel.Fix();
            }
        }

        public PropModelType Open(PropModelType propModel, out long generationId)
        {
            if(!propModel.IsFixed)
            {
                System.Diagnostics.Debug.WriteLine("Already Open.");
                generationId = propModel.GenerationId;
                return propModel;
            }
            else
            {
                PropModelType result = (PropModelType)propModel.Clone();
                generationId = Add(result);
                return result;
            }
        }

        public long Add(PropModelType propModel)
        {
            string fullClassName = propModel.FullClassName ?? throw new InvalidOperationException("The PropModel must have a non-null FullClassName.");

            long generationId;
            lock(_syncLock)
            {
                if (_cache.TryGetValue(fullClassName, out PropBagModelCachesCollInterface familyCollection))
                {
                    generationId = familyCollection.Add(propModel);
                }
                else
                {
                    familyCollection = new SimplePropModelFamilyCollection();
                    generationId = familyCollection.Add(propModel);
                    propModel.GenerationId = generationId;
                }
            }

            return generationId;
        }

        public bool TryGetAllGenerations(string fullClassName, out IReadOnlyDictionary<long, PropModelType> familyCollection)
        {
            if(_cache.TryGetValue(fullClassName, out PropBagModelCachesCollInterface list))
            {
                familyCollection = list.GetAll();
                return true;
            }
            else
            {
                familyCollection = null;
                return false;
            }
        }

        public bool TryGetValue(string fullClassName, out PropModelType propModel)
        {
            bool result = TryGetValue(fullClassName, GEN_ZERO, out propModel);
            return result;
        }

        public bool TryGetValue(string fullClassName, long generationId, out PropModelType propModel)
        {
            lock(_syncLock)
            {
                if (_cache.TryGetValue(fullClassName, out PropBagModelCachesCollInterface familyCollection))
                {
                    if (familyCollection.TryGetPropModel(generationId, out propModel))
                    {
                        return true;
                    }
                }

                if (generationId == GEN_ZERO)
                {
                    if (TryFetchFromSourceProviders(fullClassName, out propModel))
                    {
                        Add(propModel);
                        return true;
                    }
                }
            }

            propModel = null;
            return false;
        }

        private bool TryFetchFromSourceProviders(string fullClassName, out PropModelType propModel)
        {
            propModel = null;
            return false;
        }

        public bool TryFind(PropModelType propModel, out long generationId)
        {
            string fullClassName = propModel.FullClassName ?? throw new ArgumentException("propModel.FullClassName cannot be null.");

            if (_cache.TryGetValue(propModel.FullClassName, out PropBagModelCachesCollInterface list))
            {
                generationId = list.Find(propModel);
                return true;
            }
            else
            {
                generationId = -1;
                return false;
            }
        }

        public PropModelType GetPropModel(string resourceKey)
        {
            PropModelType result = null;
            foreach (IProvidePropModels propModelProvider in _propModelProviders)
            {
                try
                {
                    result = propModelProvider.GetPropModel(resourceKey);
                    result.PropModelCache = this;
                    break;
                }
                catch
                {
                    // ignore the exception.
                }

            }

            return result;
        }

        public IMapperRequest GetMapperRequest(string resourceKey)
        {
            IMapperRequest result = null;
            foreach (IProvidePropModels propModelProvider in _propModelProviders)
            {
                try
                {
                    result = propModelProvider.GetMapperRequest(resourceKey);
                    break;
                }
                catch
                {
                    // ignore the exception.
                }

            }

            return result;
        }
    }
}
