using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WurstModCodeGen
{
    /// <summary>
    /// This class generates a bunch of Enums! These enums should never break backwards compatibility as new objects are
    /// added to the game, because they are indexed by a stable hash. This is necessary because if an enum value changes,
    /// Unity will remember the index, not the name. Suddenly, an AKM is a can of tuna. This is bad.
    /// </summary>
    class Generator
    {
        
        static readonly string UnpackedPath = @"../../../uTinyExport";
        static readonly string ClassName = "ResourceDefs";
        static readonly string DestinationPath = $@"../../../WurstMod/Shared/{ClassName}.cs";

        static string[] allFiles;

        static void Main(string[] args)
        {
            if (!Directory.Exists(UnpackedPath))
            {
                Console.WriteLine(
@"This utility is used to generate certain enums from the unpacked source of the game, 
which will be automatically updated in the main WurstMod project.

Using this utility requires an unpacked version of H3VR to be placed in a uTinyExport 
folder at the root of the repository.");
                Console.ReadLine();
            }
            else
            {
                GetAllFiles();
                string result = Generate();
                result = result.Replace("\r", "");
                result = result.Replace("\n", "\r\n");
                File.WriteAllText(DestinationPath, result);
            }
        }

        static void GetAllFiles()
        {
            allFiles = Directory.EnumerateFiles(UnpackedPath, "*.*", SearchOption.AllDirectories).ToArray();
        }

        static string Generate()
        {
            string sosigBodyRegion = GenerateRegion("osigBody", "[SZ]osigBody_", "SosigBody", "objectids");
            string sosigAccessoryRegion = GenerateRegion("accessory", "[sS]osig[aA]ccessory_", "SosigAccessory", "objectids");
            string sosigMeleeRegion = GenerateRegion("SosigMelee", "SosigMelee_", "SosigMelee", "objectids");
            string sosigGunRegion = GenerateRegion("Sosiggun", "Sosiggun_", "SosigGun", "objectids");
            string weaponStuffRegion = GenerateRegion("weaponry_", @".*\\", "WeaponStuff", "objectids");
            string pMatRegion = GenerateRegion("PMat", "PMat_", "PMat", "pmaterialdefinitions");
            string matDefRegion = GenerateRegion("", @".*\\", "MatDef", "matdefs");

            string output =
$@"using System.Collections.Generic;

namespace WurstMod.Shared
{{
    public static class {ClassName}
    {{
        {sosigBodyRegion}
        {sosigAccessoryRegion}
        {sosigMeleeRegion}
        {sosigGunRegion}
        {weaponStuffRegion}
        {pMatRegion}
        {matDefRegion}
    }}
}}
";

            return output;
        }

        static string[] GenericEnumDictifier(string fileFilter, string filePattern, string enumName, string folder)
        {
            // Helper: Remove unwanted characters and ensure format is valid for C# enum declarations.
            string FormatEnum(string str)
            {
                char[] toRemove = { ' ', '-', '+' };
                foreach (string ii in toRemove.Select(x => x.ToString()))
                {
                    str = str.Replace(ii, "_");
                }
                if (char.IsDigit(str[0])) str = "_" + str;
                return str;
            }

            // Helper: Concatenate a string's hashcode onto the end of it like so:
            // {STRING} = {HASHCODE}
            // This provides a convenient way to make enums that are very unlikely
            // to be shifted around or broken as new elements are added.
            string[] OrderEnum(string[] values)
            {
                int[] orders = values.Select(x => x.GetStableHashCode()).ToArray();
                if (orders.Count() != orders.Distinct().Count())
                {
                    Console.WriteLine("Hash overlap happened. Backwards compatibility may be nuked.");
                    Console.ReadLine();
                }
                string[] results = new string[values.Length];
                for (int ii = 0; ii < values.Length; ii++) results[ii] = $"{values[ii]} = {orders[ii]}";

                return results;
            }



            string[] relevantFiles = allFiles.Where(x => x.ToLower().Contains(fileFilter.ToLower()) && Path.GetExtension(x) == ".asset").ToArray();

            Regex fileRegex = new Regex($@".*?({folder}.*?{filePattern}(.*?))\.asset");
            Match[] relevantMatches = relevantFiles.Select(x => fileRegex.Match(x)).ToArray();
            relevantMatches = relevantMatches.Where(x => x.Groups[1].Success && x.Groups[2].Success).ToArray();

            string[] enumValues = relevantMatches.Select(x => FormatEnum(x.Groups[2].ToString())).ToArray();
            string[] enumValuesWithOrders = OrderEnum(enumValues);
            string[] pathValues = relevantMatches.Select(x => x.Groups[1].ToString()).ToArray();

            string enumFinal = string.Join(",\n            ", enumValuesWithOrders.OrderBy(x => x));
            string pathFinal = string.Join(",\n            ", enumValues.Zip(pathValues, (e, p) => new { EnumVal = e, PathVal = p }).OrderBy(x => x.EnumVal).Select(x => "{ " + enumName + "." + x.EnumVal + ", @\"" + x.PathVal + "\" }").ToArray());

            string[] output = new string[] { enumFinal, pathFinal };

            return output;
        }

        static string GenerateRegion(string fileFilter, string filePattern, string enumName, string folder)
        {
            string[] enumDict = GenericEnumDictifier(fileFilter, filePattern, enumName, folder);

            string output =
$@"
        #region {enumName}
        public enum {enumName}
        {{
            {enumDict[0]}
        }}

        public static readonly Dictionary<{enumName}, string> {enumName}Resources = new Dictionary<{enumName}, string>()
        {{
            {enumDict[1]}
        }};
        #endregion";

            return output;
        }
    }
}
