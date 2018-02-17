using DRM.PropBag;
using DRM.PropBag.ViewModelTools;
using DRM.PropBagControlsWPF;
using DRM.TypeSafePropertyBag;
using System;
using System.Windows;

namespace DRM.PropBagWPF
{
    public class SimplePropModelProvider : IProvidePropModels
    { 
        #region Private Fields

        private IPropBagTemplateProvider _propBagTemplateProvider;
        private IMapperRequestProvider _mapperRequestProvider;
        private IParsePropBagTemplates _pbtParser;
        private IPropFactoryFactory _propFactoryFactory;

        #endregion

        #region Constructor

        public SimplePropModelProvider
            (
            IPropBagTemplateProvider propBagTemplateProvider,
            IMapperRequestProvider mapperRequestProvider,
            IParsePropBagTemplates propBagTemplateParser,
            IPropFactoryFactory propFactoryFactory
            )
        {
            _propBagTemplateProvider = propBagTemplateProvider;
            _mapperRequestProvider = mapperRequestProvider;
            _pbtParser = propBagTemplateParser;
            _propFactoryFactory = propFactoryFactory ?? throw new ArgumentNullException(nameof(propFactoryFactory));
        }

        #endregion

        #region PropBagTemplate Locator Support

        public bool CanFindPropBagTemplateWithJustKey => _propBagTemplateProvider?.CanFindPropBagTemplateWithJustAKey != false;
        public bool HasPbtLookupResources => _propBagTemplateProvider != null;

        public IPropModel GetPropModel(string resourceKey)
        {
            try
            {
                if (CanFindPropBagTemplateWithJustKey)
                {
                    PropBagTemplate pbt = _propBagTemplateProvider.GetPropBagTemplate(resourceKey);
                    IPropModel pm = _pbtParser.ParsePropModel(pbt);
                    FixUpPropFactory(pm, _propFactoryFactory);
                    return pm;
                }
                else if (HasPbtLookupResources)
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelProvider was supplied with a PropBagTemplateProvider upon construction. " +
                        $"No class implementing: {nameof(IPropBagTemplateProvider)} was provided. " +
                        $"Please supply a PropBagTemplate object.");
                }
                else
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelProvider was supplied with the necessary resources upon construction. " +
                        $"A {_propBagTemplateProvider.GetType()} was provided, but it does not have the necessary resources. " +
                        $"Please supply a ResourceDictionary and ResourceKey or a ProbBagTemplate object.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException($"PropBagTemplate for ResourceKey = {resourceKey} was not found.", e);
            }
        }

        public IPropModel GetPropModel(ResourceDictionary rd, string resourceKey)
        {
            try
            {
                if (HasPbtLookupResources)
                {
                    PropBagTemplate pbt = _propBagTemplateProvider.GetPropBagTemplate(resourceKey);
                    IPropModel pm = _pbtParser.ParsePropModel(pbt);
                    FixUpPropFactory(pm, _propFactoryFactory);
                    return pm;
                }
                else
                {
                    throw new InvalidOperationException($"A call providing a ResourceDictionary and a ResouceKey can only be done, " +
                        $"if this PropModelProvider was supplied with a resource upon construction. " +
                        $"No class implementing: {nameof(IPropBagTemplateProvider)} was provided.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException("Resource was not found.", e);
            }
        }

        IPropModel FixUpPropFactory(IPropModel propModel, IPropFactoryFactory propFactoryGenerator/*, IPropFactory fallBackPropFactory*/)
        {
            // Include a reference to this PropModelProvider
            propModel.PropModelProvider = this;

            if (propModel.PropFactory == null)
            {
                if (propModel.PropFactoryType != null)
                {
                    // If the propModel does not specify a PropFactory, but it does specify a PropFactoryType,
                    // use the PropFactoryFactory given to us to create a PropFactory.
                    IPropFactory generated = propFactoryGenerator.BuildPropFactory(propModel.PropFactoryType);
                    propModel.PropFactory = generated;
                }
                else
                {
                    throw new InvalidOperationException("The PropModel does not have a value for PropFactory, nor does it have a value for PropFactoryType.");
                }
            }
            // If the propModel does not supply a PropFactory, use the one assigned to us upon construction.
            return propModel;
        }

        #endregion

        #region AutoMapperRequest Lookup Support

        public bool CanFindMapperRequestWithJustKey => _mapperRequestProvider?.CanFindMapperRequestWithJustAKey != false;
        public bool HasMrLookupResources => _mapperRequestProvider != null;

        public IMapperRequest GetMapperRequest(string resourceKey)
        {
            try
            {
                if (CanFindMapperRequestWithJustKey)
                {
                    MapperRequestTemplate mr = _mapperRequestProvider.GetMapperRequest(resourceKey);
                    IMapperRequest mrCooked = new MapperRequest(mr.SourceType, mr.DestinationPropModelKey, mr.ConfigPackageName);
                    return mrCooked;
                }
                else if (HasMrLookupResources)
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelProvider was supplied with a MapperRequestProvider upon construction. " +
                        $"No class implementing: {nameof(IMapperRequestProvider)} was provided. " +
                        $"Please supply a MapperRequest object.");
                }
                else
                {
                    throw new InvalidOperationException($"A call providing only a ResourceKey can only be done, " +
                        $"if this PropModelProvider was supplied with the necessary resources upon construction. " +
                        $"A {_mapperRequestProvider.GetType()} was provided, but it does not have the necessary resources. " +
                        $"Please supply a ResourceDictionary and ResourceKey or a MapperRequest object.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException($"MapperRequest for ResourceKey = {resourceKey} was not found.", e);
            }
        }

        public IMapperRequest GetMapperRequest(ResourceDictionary rd, string resourceKey)
        {
            try
            {
                if (HasMrLookupResources)
                {
                    MapperRequestTemplate mr = _mapperRequestProvider.GetMapperRequest(rd, resourceKey);

                    IMapperRequest mapperRequest = new MapperRequest(mr.SourceType, mr.DestinationPropModelKey, mr.ConfigPackageName);
                    return mapperRequest;
                }
                else
                {
                    throw new InvalidOperationException($"A call providing a ResourceDictionary and a ResouceKey can only be done, " +
                        $"if this PropModelProvider was supplied with a resource upon construction. " +
                        $"No class implementing: {nameof(IMapperRequestProvider)} was provided.");
                }
            }
            catch (System.Exception e)
            {
                throw new ApplicationException("Resource was not found.", e);
            }
        }

        #endregion
    }
}
