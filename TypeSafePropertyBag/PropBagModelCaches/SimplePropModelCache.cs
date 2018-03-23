using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;
    using PropModelCacheInterface = ICachePropModels<String>;
    using PropBagModelFamilyCollInterface = IPropModelFamilyCollection<String>;

    public class SimplePropModelCache : PropModelCacheInterface
    {
        public const long GEN_ZERO = 0;

        #region Private Properties

        private readonly Dictionary<string, PropBagModelFamilyCollInterface> _cache;
        private readonly List<IProvidePropModels> _propModelProviders;

        private object _syncLock = new object();

        #endregion

        #region Constructor

        public SimplePropModelCache(params IProvidePropModels[] propModelProviders)
        {
            _propModelProviders = new List<IProvidePropModels>(propModelProviders);

            _cache = new Dictionary<PropNameType, PropBagModelFamilyCollInterface>();
        }

        #endregion

        #region Public Methods

        public long Add(PropModelType propModel)
        {
            string fullClassName = propModel.FullClassName ?? throw new InvalidOperationException("The PropModel must have a non-null FullClassName.");

            if(propModel.PropModelCache != null)
            {
                throw new InvalidOperationException("The PropModel is already associated with a PropModel cache.");
            }

            long generationId;
            lock(_syncLock)
            {
                if (_cache.TryGetValue(fullClassName, out PropBagModelFamilyCollInterface familyCollection))
                {
                    generationId = familyCollection.Add(propModel);
                }
                else
                {
                    familyCollection = new SimplePropModelFamilyCollection();
                    _cache.Add(fullClassName, familyCollection);

                    generationId = familyCollection.Add(propModel);
                }

                AssignNewGenerationId(propModel, generationId);

                // Associate the propModel with this cache.
                propModel.PropModelCache = this;
            }

            return generationId;
        }

        private void AssignNewGenerationId(PropModelType propModel, long generationId)
        {
            if(propModel.IsFixed)
            {
                propModel.Open();
                propModel.GenerationId = generationId;
                TryFix(propModel);
            }
            else
            {
                propModel.GenerationId = generationId;
            }
        }


        public bool TryGetPropModel(string fullClassName, out PropModelType propModel)
        {
            bool result = TryGetPropModel(fullClassName, GEN_ZERO, out propModel);
            return result;
        }

        public bool TryGetPropModel(string fullClassName, long generationId, out PropModelType propModel)
        {
            lock(_syncLock)
            {
                if (_cache.TryGetValue(fullClassName, out PropBagModelFamilyCollInterface familyCollection))
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
                        //propModel.PropModelCache = this;
                        Add(propModel);
                        return true;
                    }
                }
            }

            propModel = null;
            return false;
        }

        public bool TryGetAllGenerations(string fullClassName, out IReadOnlyDictionary<long, PropModelType> familyCollection)
        {
            if (_cache.TryGetValue(fullClassName, out PropBagModelFamilyCollInterface list))
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

        public bool TryFind(PropModelType propModel, out long generationId)
        {
            string fullClassName = propModel.FullClassName ?? throw new ArgumentException("propModel.FullClassName cannot be null.");

            if (_cache.TryGetValue(propModel.FullClassName, out PropBagModelFamilyCollInterface list))
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

        public bool TryClone(PropModelType propModel, out PropModelType clonedCopy)
        {
            if(!ReferenceEquals(propModel.PropModelCache, this))
            {
                throw new InvalidOperationException("We are being asked to clone a PropModel that is assocated with some other PropModel cache.");
            }

            propModel.PropModelCache = null;
            clonedCopy = (PropModelType) propModel.CloneIt();
            Add(clonedCopy);
            return true;
        }

        public void Clear()
        {
            System.Diagnostics.Debug.WriteLine("The SimplePropModelCache is being cleared.");
        }

        #endregion

        #region Fix and Open

        public bool TryFix(PropModelType propModel)
        {
            CheckOwnerShip(propModel);

            if (propModel.IsFixed)
            {
                System.Diagnostics.Debug.WriteLine("Already Fixed.");
                return false;
            }
            else
            {
                propModel.Fix();
                return true;
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckOwnerShip(PropModelType propModel)
        {
            if (!TryFind(propModel, out long generationId))
            {
                throw new InvalidOperationException("We are being asked to fix a PropModel that is not in our cache.");
            }

        }

        // Make a copy within the same family, the copy is given the next available generationId.
        public PropModelType Open(PropModelType propModel, out long generationId)
        {
            PropModelType result = Open(propModel, null, out generationId);
            return result;
        }

        // Make a copy and if a new class name is given, start a new family of Type definitions.
        public PropModelType Open(PropModelType propModel, string fullClassName, out long generationId)
        {
            if (!propModel.IsFixed)
            {
                if (fullClassName == null)
                {
                    System.Diagnostics.Debug.WriteLine("Already Open.");
                    generationId = propModel.GenerationId;
                    return propModel;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot open the PropModel with FullClassName: {this}; it is already open.");
                }
            }
            else
            {
                //PropModelType result = (PropModelType)propModel.Clone();

                //if (fullClassName != null)
                //{
                //    string ns = TypeExtensions.GetNamespace(fullClassName, out string className);

                //    if (ns != null)
                //    {
                //        result.NamespaceName = ns;
                //    }

                //    result.ClassName = className;
                //}

                //generationId = Add(result);

                propModel.PropModelCache = null;
                PropModelType result = (PropModelType)propModel.CloneIt();
                result.Open();

                if (fullClassName != null)
                {
                    string ns = TypeExtensions.GetNamespace(fullClassName, out string className);

                    if (ns != null)
                    {
                        result.NamespaceName = ns;
                    }

                    result.ClassName = className;
                }

                generationId = Add(result);
                return result;
            }
        }

        #endregion

        #region Private Methods

        private bool TryFetchFromSourceProviders(string fullClassName, out PropModelType propModel)
        {
            //throw new NotSupportedException("The SimplePropModelCache cannot yet fetch PropModels from a list of providers using the fullClassName.");

            IDictionary<string, string> classNameToKeyMap;

            foreach (IProvidePropModels propModelProvider in _propModelProviders)
            {
                classNameToKeyMap = propModelProvider.GetTypeToKeyMap();

                if (classNameToKeyMap.TryGetValue(fullClassName, out string resourceKey))
                {
                    propModel = propModelProvider.GetPropModel(resourceKey);

                    //if(propModel.IsFixed)
                    //{
                    //    throw new InvalidOperationException("The PropModel just fetched from a source provider was fixed.");
                    //}

                    if (propModel.PropModelCache != null)
                    {
                        throw new InvalidOperationException("The PropModel just fetched is already associated with a PropModelCache.");
                    }

                    //// Make sure the propModel is not fixed. (So that we can set its GenerationId.
                    //if(propModel.IsFixed) propModel.Open();

                    return true;
                }
            }

            propModel = null;
            return false;
        }

        #endregion

        #region Get Mapper Request

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
                    // Ignore the exception.
                }

            }

            return result;
        }

        #endregion

        //#region OLD Style Methods

        //public PropModelType GetPropModel(string resourceKey)
        //{
        //    PropModelType result = null;
        //    foreach (IProvidePropModels propModelProvider in _propModelProviders)
        //    {
        //        try
        //        {
        //            result = propModelProvider.GetPropModel(resourceKey);
        //            result.PropModelCache = this;

        //            lock (_syncLock)
        //            {
        //                if (!TryGetPropModel(result.FullClassName, out PropModelType test))
        //                {
        //                    Add(result);
        //                }
        //            }
        //            break;
        //        }
        //        catch
        //        {
        //            // Ignore the exception and continue.
        //        }
        //    }

        //    return result;
        //}

        //#endregion
    }
}
