using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swhp.PropBagModelBuilder.Jason
{
    using PropModelType = IPropModel<String>;

    public class JasonPropModelBuilder : IPropModelBuilder
    {
        #region Private Fields

        private IParsePropBagTemplates _pbtParser;
        private IPropFactoryFactory _propFactoryFactory;

        private Dictionary<string, PropModelType> _propModelCache;
        private Dictionary<string, IMapperRequest> _mapperRequestCache;

        #endregion

        #region Constructor

        public JasonPropModelBuilder
            (
            IParsePropBagTemplates propBagTemplateParser,
            IPropFactoryFactory propFactoryFactory
            )
        {
            _pbtParser = propBagTemplateParser;
            _propFactoryFactory = propFactoryFactory ?? throw new ArgumentNullException(nameof(propFactoryFactory));

            _propModelCache = new Dictionary<string, PropModelType>();
            _mapperRequestCache = new Dictionary<string, IMapperRequest>();
        }

        #endregion

        #region Working on this code



        #endregion

        #region Public Methods

        public IMapperRequest GetMapperRequest(string resourceKey)
        {
            throw new NotImplementedException();
        }

        public IPropModel<string> GetPropModel(string resourceKey)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, string> GetTypeToKeyMap()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
