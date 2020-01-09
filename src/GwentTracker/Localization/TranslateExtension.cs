namespace GwentTracker.Localization
{
    public class TranslateExtension
    {
        public string Key { get; set; }
        public string Context { get; set; }
        
        public TranslateExtension(string key)
        {
            Key = key;
        }
        
        public string ProvideValue()
        {
            var key = Key;
            if (!string.IsNullOrWhiteSpace(Context))
            {
                key = $"{Context}|{Key}";
            }

            return new Translate()[key];
        }
    }
}