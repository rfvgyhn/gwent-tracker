using System;
using System.Collections.Generic;
using NGettext;
using Splat;

namespace GwentTracker.Localization
{
    public class Translate
    {
        private readonly IEnumerable<ICatalog> _catalogs;

        public Translate() : this(null) { }
        public Translate(IEnumerable<ICatalog> catalog)
        {
            _catalogs = catalog ?? Locator.Current.GetServices<ICatalog>();
            
            if (_catalogs == null)
                throw new ArgumentNullException(nameof(catalog), "Provide instance in constructor or register IEnumerable<ICatalog> with container.");
        }
        
        public string this[string key]
        {
            get
            {
                var parts = key.Split('|');

                if (parts.Length > 1)
                    return GetTranslation(key, c => c.GetParticularString(parts[0], parts[1]));

                return GetTranslation(key, c => c.GetString(key));
            }
        }

        private string GetTranslation(string fallback, Func<ICatalog, string> getString)
        {
            var value = fallback;
            foreach (var catalog in _catalogs)
            {
                value = getString(catalog);
                if (value != fallback)
                    break;
            }

            return value;
        }
    }
}