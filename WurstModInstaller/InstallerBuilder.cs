using Microsoft.Win32;
using System;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using WixSharp;
using WixSharp.Controls;

namespace WurstModInstaller
{
    class InstallerBuilder
    {
        static void Main(string[] args)
        {
            Console.WriteLine(FindH3Directory());
            CreateInstaller();
            //Project_BeforeInstall(null);

            Console.ReadLine();
        }



        private static string FindH3Directory()
        {
            // Find base steam dir from registry.
            string h3manifest = "appmanifest_450540.acf";
            string steamDir = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", "") as string;
            if (steamDir == "") steamDir = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", "") as string;
            if (steamDir == "") return "";

            // Check steamapps for h3 manifest.
            if (System.IO.File.Exists(steamDir + @"\steamapps\" + h3manifest)) return steamDir + @"\steamapps\common\H3VR";

            // We didn't find it, look at other library folders by lazily parsing libraryfolders.
            List<string> lines = System.IO.File.ReadAllLines(steamDir + "/steamapps/libraryfolders.vdf").Skip(4).Where(x => x.Length != 0 && x[0] != '}').ToList();
            lines = lines.Select(x => x.Split('\t')[3].Trim('"').Replace(@"\\", @"\")).ToList();
            foreach (string ii in lines)
            {
                if (System.IO.File.Exists(ii + @"\steamapps\" + h3manifest)) return ii + @"\steamapps\common\H3VR" + @"\DBGINSTALL";
            }

            return "";
        }

        private static void CreateInstaller()
        {
            var project = new ManagedProject()
            {
                Name = "WurstMod",
                OutFileName = "WurstMod",
                UI = WUI.WixUI_ProgressOnly,
                GUID = new Guid("803c488b-b6e4-4ca6-a436-94512dbb304b"),

                Dirs = new[]
                {
                    new Dir(new Id("H3DIR"), "root1", new File(@"..\..\..\WurstMod\bin\debug\WurstMod.dll"))
                }
            };

            project.Load += Project_Load;
            project.BeforeInstall += Project_BeforeInstall;

            project.BuildMsi();
        }



        private static void Project_Load(SetupEventArgs e)
        {
            string h3Dir = FindH3Directory();
            e.Session["H3DIR"] = h3Dir + @"\BepInEx\plugins";
        }

        private static void Project_BeforeInstall(SetupEventArgs e)
        {
            // Download and install BepInEx.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var client = new WebClient())
            {
                client.DownloadFile("https://github.com/BepInEx/BepInEx/releases/download/v5.3/BepInEx_x64_5.3.0.0.zip", "BepInEx.zip");
                ZipFile.ExtractToDirectory("BepInEx.zip", FindH3Directory());
            }
        }
    }
}
