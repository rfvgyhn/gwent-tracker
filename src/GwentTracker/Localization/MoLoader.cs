using System.IO;
using NGettext.Loaders;
using NGettext.Plural;

namespace GwentTracker.Localization
{
    public class MoLoader : NGettext.Loaders.MoLoader
    {
        protected override string GetFileName(string localeDir, string domain, string locale)
        {
            return Path.Combine(localeDir, $"{domain}.{locale.ToLowerInvariant()}.mo");
        }

        public MoLoader(string domain, string localeDir, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser) : base(domain, localeDir, pluralRuleGenerator, parser)
        {
        }

        public MoLoader(string filePath, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser) : base(filePath, pluralRuleGenerator, parser)
        {
        }

        public MoLoader(Stream moStream, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser) : base(moStream, pluralRuleGenerator, parser)
        {
        }

        public MoLoader(string domain, string localeDir, IPluralRuleGenerator pluralRuleGenerator) : base(domain, localeDir, pluralRuleGenerator)
        {
        }

        public MoLoader(string domain, string localeDir, MoFileParser parser) : base(domain, localeDir, parser)
        {
        }

        public MoLoader(string domain, string localeDir) : base(domain, localeDir)
        {
        }

        public MoLoader(string filePath, IPluralRuleGenerator pluralRuleGenerator) : base(filePath, pluralRuleGenerator)
        {
        }

        public MoLoader(string filePath, MoFileParser parser) : base(filePath, parser)
        {
        }

        public MoLoader(string filePath) : base(filePath)
        {
        }

        public MoLoader(Stream moStream, IPluralRuleGenerator pluralRuleGenerator) : base(moStream, pluralRuleGenerator)
        {
        }

        public MoLoader(Stream moStream, MoFileParser parser) : base(moStream, parser)
        {
        }

        public MoLoader(Stream moStream) : base(moStream)
        {
        }
    }
}