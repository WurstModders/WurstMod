using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WurstModCodeGen.ResourceGenerators
{
    public class AssetEnumGenerator : ResourceGenerator
    {
        private readonly List<(string resourcePath, string enumName, long enumValue)> _assets = new();
        public override string Name { get; }

        private static readonly int ResourcePathLength = Constants.UnpackedResourcePath.Length;

        public AssetEnumGenerator(string enumName)
        {
            Name = enumName;
        }

        protected override void Discovered(FileInfo file)
        {
            // If the file isn't in the resources folder, we won't be able to load it anyway so skip.
            if (!file.FullName.Contains("Resources")) return;
            
            // Just add it to the assets list for now
            var resourcePath = file.FullName.Substring(ResourcePathLength, file.FullName.Length - (6 + ResourcePathLength));
            var enumName = string.Join("_", resourcePath.Split('\\').Skip(1).Select(x => EscapeEnumName(x).Replace("_", "")));
            var hashCode = file.FullName.GetStableHashCode();
            _assets.Add((resourcePath, enumName, hashCode));
        }

        public override string Generate()
        {
            const string EnumLine = "            {0} = {1}";
            const string DictLine = "            [{0}] = @\"{1}\"";
            var template = $@"
        #region {Name}
        public enum {Name}
        {{{{
{{0}}
        }}}}

        public static readonly Dictionary<{Name}, string> {Name}Resources = new()
        {{{{
{{1}}
        }}}}
        #endregion
";

            var enumLines = string.Join(",\n", _assets.Select(x => string.Format(EnumLine, x.enumName, x.enumValue)));
            var dictLines = string.Join(",\n", _assets.Select(x => string.Format(DictLine, x.enumName, x.resourcePath)));
            return string.Format(template,enumLines, dictLines);
        }

        // Removes unwanted characters from file names to be valid in an enum
        private static readonly char[] UnwantedCharacters = {' ', '-', '+'};
        private string EscapeEnumName(string str)
        {
            return UnwantedCharacters.Aggregate(str, (aggregate, c) => aggregate.Replace(c, '_'));
        }
    }
}