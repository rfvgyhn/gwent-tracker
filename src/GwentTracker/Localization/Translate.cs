using System;
using NGettext;
using Splat;

namespace GwentTracker.Localization
{
    public class Translate
    {
        private readonly ICatalog _catalog;

        public Translate() : this(null) { }
        public Translate(ICatalog catalog)
        {
            _catalog = catalog ?? Locator.Current.GetService<ICatalog>();
            
            if (_catalog == null)
                throw new ArgumentNullException(nameof(catalog), "Provide instance in constructor or register ICatalog with container.");
        }
        
        public string this[string key]
        {
            get
            {
                var parts = key.Split('|');
                
                if (parts.Length > 1)
                    return _catalog.GetParticularString(parts[0], parts[1]);

                return _catalog.GetString(key);
            }
        }
    }
}