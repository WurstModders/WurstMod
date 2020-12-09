using System;
using System.Collections.Generic;
using System.IO;

namespace WurstModCodeGen.ResourceGenerators
{
    public class AssetEnumGenerator : ResourceGenerator
    {
        private Dictionary<string, long> _assets = new();
        public override string Name { get; }

        public AssetEnumGenerator(string enumName)
        {
            Name = enumName;
        }

        protected override void Discovered(FileInfo file)
        {
        }

        public override string Generate()
        {
            return @"";
        }
    }
}