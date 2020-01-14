#nullable enable
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GwentTracker.Localization
{
    public class TranslateStringConverter : IYamlTypeConverter
    {
        private readonly Translate _translate;

        public TranslateStringConverter()
        {
            _translate = new Translate();
        }
        public bool Accepts(Type type)
        {
            return type == typeof(string);
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            var value = parser.Consume<Scalar>().Value;
            return _translate[value];
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}