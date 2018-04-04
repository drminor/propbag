using DRM.TypeSafePropertyBag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swhp.PropBagModelBuilder.Jason
{
    public class JasonPropModelBuilder : IPropModelBuilder
    {
        public JasonPropModelBuilder
            (
            IParsePropBagTemplates propBagTemplateParser,
            IPropFactoryFactory propFactoryFactory
            )
        {

        }

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
    }
}
