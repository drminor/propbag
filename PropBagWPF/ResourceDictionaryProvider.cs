using ObjectSizeDiagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace DRM.PropBagWPF
{
    public class ResourceDictionaryProvider
    {
        static bool memTrackerEnabledState = false;

        MemConsumptionTracker _mct = 
            new MemConsumptionTracker("ResDictProv", "Starting Mem Tracker for Resource Dictionary Provider.", memTrackerEnabledState);

        /// <summary>
        /// Builds a set of MergedDictionaries from the list of filenames at the basePath
        /// using a new thread using a Single Threaded Apartment.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="filenames"></param>
        /// <returns>A ResourceDictionary whose MergedDictionaries property contains the set of merged resource dictionaries.</returns>
        public ResourceDictionary LoadUsingSTA(string basePath, string[] filenames)
        {
            _mct.CompactMeasureAndReport("Just Started LoadUsingSTA from a basePath and a list of filenames.");

            ResourceDictionary result = EmptySet;

            Thread thread = new Thread(() =>
            {
                result = Load(basePath, filenames);
            });

            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
            thread.Join();

            _mct.CompactMeasureAndReport("About to return from LoadUsingSTA.");

            return result;
        }

        /// <summary>
        /// Builds a set of MergedDictionaries from the list of filenames at the basePath
        /// using the caller's thread.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="filenames"></param>
        /// <returns>A ResourceDictionary whose MergedDictionaries property contains the set of merged resource dictionaries.</returns>
        public ResourceDictionary Load(string basePath, string[] filenames)
        {
            ResourceDictionary result = EmptySet;

            foreach (string fn in filenames)
            {
                string path = System.IO.Path.Combine(basePath, fn);
                _mct.CompactMeasureAndReport("Before Fetch Private");

                ResourceDictionary resourceDictionary = Fetch(path);
                _mct.CompactMeasureAndReport("After Fetch Private");

                result = Merge(resourceDictionary, result);
                _mct.CompactMeasureAndReport("After Merge Source Resource Dictionary.");
            }

            //object propBagTemplate = result["PersonVM"];

            //if(propBagTemplate is PropBagTemplate pbt)
            //{
            //    string fullClassName = pbt.FullClassName;
            //    Type propFactoryProviderType = pbt.PropFactoryProviderType; 

            //}

            return result;
        }

        /// <summary>
        /// Builds a set of MergedDictionaries from the list file paths.
        /// using a new thread using a Single Threaded Apartment.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="filenames"></param>
        /// <returns>A ResourceDictionary whose MergedDictionaries property contains the set of merged resource dictionaries.</returns>
        public ResourceDictionary LoadUsingSTA(string[] paths)
        {
            _mct.CompactMeasureAndReport("Just Started LoadUsingSTA from list of Paths.");

            ResourceDictionary result = EmptySet;

            Thread thread = new Thread(() =>
            {
                result = Load(paths);
            });

            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
            thread.Join();

            _mct.CompactMeasureAndReport("About to return from LoadUsingSTA");

            return result;
        }

        /// <summary>
        /// Builds a set of MergedDictionaries from the list file paths.
        /// using the caller's thread.
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="filenames"></param>
        /// <returns>A ResourceDictionary whose MergedDictionaries property contains the set of merged resource dictionaries.</returns>
        public ResourceDictionary Load(string[] paths)
        {
            ResourceDictionary result = EmptySet;

            foreach (string path in paths)
            {
                _mct.CompactMeasureAndReport("Before Fetch Private");

                ResourceDictionary resourceDictionary = Fetch(path);
                _mct.CompactMeasureAndReport("After Fetch Private");

                result = Merge(resourceDictionary, result);
                _mct.CompactMeasureAndReport("After Merge Source Resource Dictionary.");
            }

            return result;
        }

        /// <summary>
        /// Fetch the ResourceDictionary on a new thread using a Single Threaded Apartment
        /// and adds it to the target's Merged Dictionaries.
        /// </summary>
        /// <param name="pathToRDFile"></param>
        /// <param name="target">The Dictionary into which to merge the fectched ResourceDictionary</param>
        /// <returns>A reference to the target.</returns>
        public ResourceDictionary MergeUsingSTA(string pathToRDFile, ResourceDictionary target)
        {
            ResourceDictionary rdFetchedFromFile = FetchUsingSTA(pathToRDFile);

            target.MergedDictionaries.Add(rdFetchedFromFile);
            return target;
        }

        /// <summary>
        /// Fetch the ResourceDictionary on the current thread.
        /// and adds it to the target's Merged Dictionaries.
        /// </summary>
        /// <param name="pathToRDFile"></param>
        /// <param name="target">The Dictionary into which to merge the fectched ResourceDictionary</param>
        /// <returns>A reference to the target.</returns>
        public ResourceDictionary Merge(string pathToRDFile, ResourceDictionary target)
        {
            ResourceDictionary rdFetchedFromFile = Fetch(pathToRDFile);

            target.MergedDictionaries.Add(rdFetchedFromFile);
            return target;
        }

        /// <summary>
        /// Merges the source ResourceDictionary into the target's MergedDictionaries.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>A reference to the target.</returns>
        public ResourceDictionary Merge(ResourceDictionary source, ResourceDictionary target)
        {
            target.MergedDictionaries.Add(source);
            return target;
        }

        /// <summary>
        /// Creates a ResourceDictionary from the XAML file at the specified path
        /// using a new thread using a Single Threaded Apartment.
        /// </summary>
        /// <param name="pathToRDFile"></param>
        /// <returns>The new ResourceDictionary</returns>
        public ResourceDictionary FetchUsingSTA(string pathToRDFile)
        {
            ResourceDictionary result = EmptySet;

            Thread thread = new Thread(() =>
            {
                result = Fetch(pathToRDFile);
            });

            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
            thread.Join();

            return result;
        }

        /// <summary>
        /// Creates a ResourceDictionary from the XAML file at the specified path
        /// using the caller's thread.
        /// </summary>
        /// <param name="pathToRDFile"></param>
        /// <returns>The new ResourceDictionary</returns>
        public ResourceDictionary Fetch(string pathToRDFile)
        {
            using (XmlReader xmlReader = XmlReader.Create(pathToRDFile))
            {
                _mct.CompactMeasureAndReport("After Creating XML Reader.");

                ResourceDictionary resource = (ResourceDictionary)XamlReader.Load(xmlReader);
                _mct.CompactMeasureAndReport("After XAML Reader Load Operation.");

                return resource;
            }
        }

        /// <summary>
        /// Returns an empty ResourceDictionary.
        /// </summary>
        public ResourceDictionary EmptySet
        {
            get
            {
                return new ResourceDictionary();
            }
        }

    }
}
