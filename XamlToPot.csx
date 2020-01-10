#r "System.Xml"
#r "System.Xml.Linq"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

var AttributesToTranslate = new[] { "Title", "Content", "Text", "Header", "Tooltip", "Watermark"};
var NodesToTranslate = new[] { "TextBlock" };
var xamlFiles = Args;
var messages = new Dictionary<string, List<string>>();

foreach (var file in xamlFiles)
{
    if (string.IsNullOrEmpty(file) || !File.Exists(file)) continue;
    
    var xmlDoc = XDocument.Load(file, LoadOptions.SetLineInfo);
    foreach (var element in xmlDoc.Descendants())
    {
        var lineNumber = -1;
        if (((IXmlLineInfo)element).HasLineInfo())
            lineNumber = ((IXmlLineInfo)element).LineNumber;

        var nodeName = string.Empty;
        foreach (var attr in element.Attributes())
        {
            if (string.Equals("Name", attr.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                nodeName = attr.Value;
        }
        if (string.IsNullOrEmpty(nodeName))
            nodeName = element.Name.LocalName;

        if (NodesToTranslate.Any(tn => string.Equals(tn, element.Name.LocalName, StringComparison.InvariantCultureIgnoreCase)))
            AddMessage(element.Value, messages, file, lineNumber, nodeName);

        foreach (var attr in element.Attributes())
        {
            var elementMatches = AttributesToTranslate.Any(translate => string.Equals(translate, attr.Name.LocalName, StringComparison.InvariantCultureIgnoreCase));
            if (elementMatches && (!attr.Value.StartsWith("{") || attr.Value.Contains("StringFormat") || attr.Value.Contains("Translate")))
            {
                AddMessage(attr.Value, messages, file, lineNumber, nodeName, $".{attr.Name}");
            }
        }
    }
}

Console.Write(WritePot(messages));
    
private static void AddMessage(string messageId, Dictionary<string, List<string>> msgIds, string fileName, int lineNumber, string nodeName, string suffix = "")
{
    if (string.IsNullOrEmpty(messageId))
        return;
    
    var pattern = @"[{'](?!Binding)(?:i18n:Translate\s)?(.+?[^\d])[}']";
    var match = Regex.Match(messageId, pattern);
    if (match.Success)
        messageId = match.Groups[1].Value;
    var comment = $"#: {fileName}:{lineNumber} -> {nodeName}{suffix}";
    
    if (msgIds.ContainsKey(messageId))
    {
        var index = msgIds[messageId].Count - 2;
        msgIds[messageId].Insert(index, comment);
    }
    else
    {
        msgIds[messageId] = new List<string>
        {
            comment,
            $"msgid \"{messageId}\"",
            "msgstr \"\""
        };
    }
}

private static string WritePot(Dictionary<string, List<string>> messages)
{
    var now = DateTime.Now;
    var utcOffset = TimeZoneInfo.Local.GetUtcOffset(now);
    var symbol = utcOffset.ToString().StartsWith("-") ? "-" : "+";
    var date = $"{now:yyyy-MM-dd HH:mm}{symbol}{utcOffset.Hours:00}{utcOffset.Minutes:00}";
    var header = @$"
msgid """"
msgstr """"
""POT-Creation-Date: {date}\n""
""PO-Revision-Date: {date}\n""
""Plural-Forms: nplurals=2; plural=n != 1;\n""
""X-Poedit-KeywordsList: GetString;GetPluralString:1,2;GetParticularString:1c,2;GetParticularPluralString:1c,2,3;_;_n:1,2;_p:1c,2;_pn:1c,2,3\n""
";
    var output = new StringBuilder();
    output.AppendLine(header);
    output.AppendLine();
    messages.Aggregate(output, (builder, pair) =>
    {
        foreach (var line in pair.Value)
            builder.AppendLine(line);

        builder.AppendLine("");

        return builder;
    });

    return output.ToString();
}