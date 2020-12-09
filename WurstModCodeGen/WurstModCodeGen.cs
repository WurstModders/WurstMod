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
            ["91c24846d2fa5a046b0ef4d2e3869c9e"] = new AssetEnumGenerator("FVRObjectAssets")
        };

        // Declare some variables
        private static readonly Regex GuidLineRegex = new(@"^\s*m_Script: {fileID: \d+, guid: ([0-9a-f]+), type: \d+}$", RegexOptions.Compiled);
        private static int _scannedFiles;
        private static Stopwatch _stopwatch;

        public static void Main()
        {
            // If we can't find the game files, exit.
            if (!Directory.Exists(Constants.UnpackedResourcePath))
            {
                Console.WriteLine("This utility is used to generate certain enums from the unpacked source of the game, \nwhich will be automatically updated in the main WurstMod project.\n\nUsing this utility requires an unpacked version of H3VR to be placed in a uTinyExport \nfolder at the root of the repository.");
                return;
            }

            // Start a stopwatch
            _stopwatch = Stopwatch.StartNew();

            // Scan for all related files
            ScanFiles();


            // Yes I know the quadruple brackets look silly but you have to escape them once here and again in a String.Format() call
            string resourceFileTemplate = $@"using System.Collections.Generic;

namespace {Constants.OutputNamespace}
{{{{
    public static class {Constants.OutputClassName} 
    {{{{
        {{0}}
    }}}}
}}}}
";
            File.WriteAllText(Constants.DestinationPath, string.Format(resourceFileTemplate, string.Join("\n\n", _resourceGenerators.Values.Select(x => x.Generate()))));
        }

        private static void ScanFiles()
        {
            // Iterate over every asset file in the game files.
            foreach (var file in Directory.EnumerateFiles(Constants.UnpackedResourcePath, "*.asset", SearchOption.AllDirectories))
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
            }

            // Output some stats to the console
            Console.WriteLine($"Done scanning files. Files: {_scannedFiles}, Elapsed: {_stopwatch.ElapsedMilliseconds / 1000d}s");
            foreach (var generator in _resourceGenerators.Values)
                Console.WriteLine($"{generator.Name}: {generator.ScannedFiles}");
        }
    }
}