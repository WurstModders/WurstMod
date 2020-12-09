using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WurstModCodeGen.ResourceGenerators;

namespace WurstModCodeGen
{
    public static class WurstModCodeGen
    {
        // Hold a list of all the resource generators for a given guid
        private static readonly Dictionary<string, ResourceGenerator> _resourceGenerators = new()
        {
            // These are the GUIDs of the ScriptableObjects
            ["91c24846d2fa5a046b0ef4d2e3869c9e"] = new AssetEnumGenerator("FVRObjectAsset"),
            ["c66fb34a05334e547bd2e86bc689097b"] = new AssetEnumGenerator("MatDefAsset"),
            ["cc0242bada76dd34098a8c617562b266"] = new AssetEnumGenerator("PMatAsset")
            
        };

        // Declare some variables
        private static readonly Regex GuidLineRegex = new(@"^\s*m_Script: {fileID: \d+, guid: ([0-9a-f]+), type: \d+}$", RegexOptions.Compiled);
        private static int _scannedFiles;
        private static Stopwatch _stopwatch;

        public static string UnpackedResourcePath;
        
        public static void Main()
        {
            Console.WriteLine("This utility is used to generate the resource definitions from a copy of the game that has been unpacked using uTinyRipper. Please enter the location of the Assets/Resources folder from the unpacked game:");
            UnpackedResourcePath = Console.ReadLine();
            
            // If we can't find the game files, exit.
            if (!Directory.Exists(UnpackedResourcePath))
            {
                Console.WriteLine("The given directory does not exist. Exiting.");
                return;
            }

            // Start a stopwatch
            _stopwatch = Stopwatch.StartNew();

            // Scan for all related files
            ScanFiles();


            // Yes I know the quadruple brackets look silly but you have to escape them once here and again in a String.Format() call
            string resourceFileTemplate = $@"// ReSharper disable All
using System.Collections.Generic;

namespace {Constants.OutputNamespace}
{{{{
    public static class {Constants.OutputClassName} 
    {{{{
        {{0}}
    }}}}
}}}}
";
            _stopwatch.Restart();
            File.WriteAllText(Constants.DestinationPath, string.Format(resourceFileTemplate, string.Join("", _resourceGenerators.Values.Select(x => x.Generate()))));
            Console.WriteLine($"Done generating resource file. Duration: {_stopwatch.ElapsedMilliseconds / 1000d}s");
            Console.WriteLine("Complete!");
        }

        private static void ScanFiles()
        {
            // Iterate over every asset file in the game files.
            foreach (var file in Directory.EnumerateFiles(UnpackedResourcePath, "*.asset", SearchOption.AllDirectories))
            {
                // We want to know which ScriptableObject type this asset is.
                // Luckily for us it includes the GUID of it on line 8
                // This part allocates a lot of memory (Just under 1GB)
                // The using statement will release the used memory after we got what we need
                // but it still lingers for a bit until the garbage collector takes it.
                string line;
                using (var reader = new StreamReader(file))
                {
                    // Skip the first 7 lines
                    for (var i = 0; i < 7; i++)
                        reader.ReadLine();
                    line = reader.ReadLine();
                }

                // Next we want to take the line that says something like:
                // m_Script: {fileID: 11500000, guid: 91c24846d2fa5a046b0ef4d2e3869c9e, type: 3}
                // and just take that guid
                var guid = GuidLineRegex.Match(line ?? string.Empty).Groups[1].Value;

                // If we have a resource generator that cares about this guid pass it off
                if (!string.IsNullOrEmpty(guid) && _resourceGenerators.TryGetValue(guid, out var resourceGenerator))
                    resourceGenerator.Discovered(file);

                // Increment the scanned files variable
                _scannedFiles++;
            }

            // Output some stats to the console
            Console.WriteLine($"Done scanning files. Total scanned asset files: {_scannedFiles}, Duration: {_stopwatch.ElapsedMilliseconds / 1000d}s");
            foreach (var generator in _resourceGenerators.Values)
                Console.WriteLine($"{generator.Name}: {generator.ScannedFiles}");
        }
    }
}