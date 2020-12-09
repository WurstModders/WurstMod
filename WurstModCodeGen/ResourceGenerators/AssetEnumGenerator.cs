using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WurstModCodeGen.ResourceGenerators
{
    public class AssetEnumGenerator : ResourceGenerator
    {
        // Some format strings for this generator
        private const string EnumLine = "            {0} = {1}";
        private const string DictLine = "            [{0}.{1}] = @\"{2}\"";
        private const string Template = @"
        #region {0}
        public enum {0}
        {{
{1}
        }}

        public static readonly Dictionary<{0}, string> {0}Resources = new()
        {{
{2}
        }};
        #endregion
";

        // List of assets we've discovered
        private readonly List<(string resourcePath, string enumName, long enumValue)> _assets = new();
        public override string Name { get; }

        public AssetEnumGenerator(string enumName)
        {
            Name = enumName;
        }

        protected override void Discovered(FileInfo file)
        {
            // If the file isn't in the resources folder, we won't be able to load it anyway so skip.
            if (!file.FullName.Contains("Resources")) return;

            // Just add it to the assets list for now
            var resourcePathLength = WurstModCodeGen.UnpackedResourcePath.Length + 1;
            var resourcePath = file.FullName.Substring(resourcePathLength, file.FullName.Length - (6 + resourcePathLength));
            var enumName = string.Join("_", resourcePath.Split('\\').Skip(1).Select(x => EscapeEnumName(x).Replace("_", "")));
            var hashCode = file.FullName.GetStableHashCode();
            _assets.Add((resourcePath, enumName, hashCode));
        }

        public override string Generate()
        {
            var enumLines = string.Join(",\n", _assets.Select(x => string.Format(EnumLine, x.enumName, x.enumValue)));
            var dictLines = string.Join(",\n", _assets.Select(x => string.Format(DictLine, Name, x.enumName, x.resourcePath)));
            return string.Format(Template, Name, enumLines, dictLines);
        }

        // Removes unwanted characters from file names to be valid in an enum
        private static readonly char[] UnwantedCharacters = {' ', '-', '+'};

        private static string EscapeEnumName(string str)
        {
            return UnwantedCharacters.Aggregate(str, (aggregate, c) => aggregate.Replace(c, '_'));
        }
    }
}