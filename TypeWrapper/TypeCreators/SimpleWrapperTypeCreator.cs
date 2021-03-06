﻿using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.TypeWrapper
{
    using static DRM.TypeSafePropertyBag.TypeExtensions.TypeExtensions;
    using PropModelType = IPropModel<String>;

    public class SimpleWrapperTypeCreator : ICreateWrapperTypes
    {
        #region Private Members

        private ICacheEmittedTypes _wrapperTypeCachingService { get; }
        ICacheTypeDescriptions _typeDescCachingService { get; }

        #endregion

        #region Constructor 

        public SimpleWrapperTypeCreator(
            ICacheEmittedTypes wrapperTypeCachingService,
            ICacheTypeDescriptions typeDescCachingService)
        {
            _wrapperTypeCachingService = wrapperTypeCachingService ?? throw new ArgumentNullException(nameof(wrapperTypeCachingService));
            _typeDescCachingService = typeDescCachingService ?? throw new ArgumentNullException(nameof(typeDescCachingService));
        }

        #endregion

        #region Public Methods

        public Type GetWrapperType(PropModelType propModel, Type typeToWrap)
        {
            if (!typeToWrap.IsPropBagBased())
            {
                throw new InvalidOperationException($"Type: {typeToWrap.Name} must derive from IPropBag.");
            }

            //TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, typeToCreate, null));
            //Type newWrapperType = _wrapperTypeCachingService.GetOrAdd(td);
            //return newWrapperType;

            Type result = GetWrapperType_Internal(propModel, typeToWrap, null);
            return result;
        }

        public Type GetWrapperType<BT>(PropModelType propModel) where BT : class, IPropBag
        {
            //TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, typeof(BT), null));
            //Type newWrapperType = _wrapperTypeCachingService.GetOrAdd(td);
            //return newWrapperType;

            Type result = GetWrapperType_Internal(propModel, typeof(BT), null);
            return result;
        }

        private Type GetWrapperType_Internal(PropModelType propModel, Type typeToWrap, string fullClassName)
        {
            TypeDescription td = _typeDescCachingService.GetOrAdd(new NewTypeRequest(propModel, typeToWrap, fullClassName));
            Type newWrapperType = _wrapperTypeCachingService.GetOrAdd(td);

            return newWrapperType;
        }

        // TODO: Note: This class hold two caches, however only one contributes to the result.
        public long ClearTypeCache()
        {
           long numTypeDescriptionsThatWereCached = _typeDescCachingService.ClearTypeCache();
           return _wrapperTypeCachingService.ClearTypeCache();
        }

        #endregion
    }
}
