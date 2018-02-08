using DRM.TypeSafePropertyBag.Fundamentals;

namespace DRM.TypeSafePropertyBag
{
    public class PropTemplateCache : IProvidePropTemplates
    {
        LockingConcurrentDictionary<IPropTemplate, IPropTemplate> _cache;
        //HashSet<IPropTemplate> _cache;

        object _sync = new object();

        public PropTemplateCache()
        {
            //_cache = new HashSet<IPropTemplate>(new PropTemplateGenComparer());
            _cache = new LockingConcurrentDictionary<IPropTemplate, IPropTemplate>(Factory, new PropTemplateGenComparer());
        }

        public int Count
        {
            get
            {
                return _cache.Count;
            }
        }

        public IPropTemplate GetOrAdd(IPropTemplate propTemplate)
        {
            //lock(_sync)
            //{
            //    if(!_cache.Contains(propTemplate))
            //    {
            //        System.Diagnostics.Debug.WriteLine($"Adding a new PropTemplate for type = {propTemplate.Type}; kind = {propTemplate.PropKind}.");
            //        _cache.Add(propTemplate);
            //        return propTemplate;
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine($"Using existing PropTemplate for type = {propTemplate.Type}; kind = {propTemplate.PropKind}.");
            //        // TODO: Does this perform a linear search, or does it indeed use the hash values to locate the proper bucket(s)?
            //        IPropTemplate existingEntry = _cache.First(x => x.Equals(propTemplate));
            //        return existingEntry;
            //    }
            //}

            IPropTemplate result = _cache.GetOrAdd(propTemplate);
            return result;
        }

        private IPropTemplate Factory(IPropTemplate propTemplate)
        {
            return propTemplate;
        }


    }
}
