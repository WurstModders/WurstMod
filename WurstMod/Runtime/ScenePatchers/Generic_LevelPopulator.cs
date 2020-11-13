using System;
using System.Collections.Generic;
using System.Linq;
using FistVR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WurstMod.Shared;

namespace WurstMod.Runtime.ScenePatchers
{
    class Generic_LevelPopulator
    {
        // References
        private static GameObject labelBase;
        private static GameObject sceneScreenBase;
        private static GameObject changelogPanel;

        // Etc
        private static List<Vector3> screenPositions = new List<Vector3>();
        private static List<MainMenuScenePointable> screens = new List<MainMenuScenePointable>();
        private static List<string> levelPaths = new List<string>();

        public static void SetupLevelPopulator(Scene loaded)
        {
            if (loaded.name == "MainMenu3")
            {
                Debug.Log("Initializing level populator...");
                Reset();
                GatherReferences();
                CalculateScreenPositions();
                InitObjects();
                SetupLevelDefs();
                //SetupPanel(); TODO make this useful.
            }
        }

        private static void Reset()
        {
            Loader.LevelToLoad = null;
            screenPositions.Clear();
            screens.Clear();
            levelPaths.Clear();
        }

        private static void GatherReferences()
        {
            sceneScreenBase = GameObject.Find("SceneScreen_GDC");
            labelBase = GameObject.Find("Label_Description_1_Title (5)");
            changelogPanel = GameObject.Find("MainScreen1");
        }

        private static void CalculateScreenPositions()
        {
            // Get a circle.
            Func<float, float> CircleX = x => (14.13f * Mathf.Cos(Mathf.Deg2Rad * x)) - 0.39f;
            Func<float, float> CircleZ = z => (14.13f * Mathf.Sin(Mathf.Deg2Rad * z)) - 2.98f;
            for (int ii = 0; ii <= 360; ii += 13)
            {
                for (int jj = 0; jj < 4; jj++)
                {
                    screenPositions.Add(new Vector3(CircleX(ii), 0.5f + (jj * 2), CircleZ(ii)));
                }
            }

            // Trimming positions we don't want and order by -z.
            screenPositions = screenPositions.Where(x => x.z < -7f).ToList();
            screenPositions = screenPositions.OrderByDescending(x => -x.z).ThenBy(x => Mathf.Abs(x.y - 4.15f)).ToList();
        }

        private static void InitObjects()
        {
            // Modded Levels label.
            GameObject moddedScenesLabel = GameObject.Instantiate(labelBase, labelBase.transform.parent);
            moddedScenesLabel.transform.position = new Vector3(0f, 8.3f, -17.1f);
            moddedScenesLabel.transform.localEulerAngles = new Vector3(-180f, 0f, 180f);
            moddedScenesLabel.GetComponent<Text>().text = "Modded Scenes:";

            // Scene screens.
            for (int ii = 0; ii < screenPositions.Count; ii++)
            {
                // Create and position properly. Rename so patch can handle it properly.
                GameObject screen = GameObject.Instantiate(sceneScreenBase, sceneScreenBase.transform.parent);
                screen.transform.position = screenPositions[ii];
                //screen.transform.LookAt(Vector3.zero, Vector3.up);
                screen.transform.localEulerAngles = new Vector3(0, 180 - (Mathf.Rad2Deg * Mathf.Atan(-screen.transform.position.x / screen.transform.position.z)), 0);
                screen.transform.localScale = 0.5f * screen.transform.localScale;
                screen.name = "MODDEDSCREEN";

                // Make sure scaling is set correctly.
                MainMenuScenePointable screenComponent = screen.GetComponent<MainMenuScenePointable>();
                screenComponent.ReflectSet("m_startScale", screen.transform.localScale);

                // Add to list and disable until needed.
                screens.Add(screenComponent);
                screen.SetActive(false);
            }
        }

        private static void SetupLevelDefs()
        {
            foreach (var level in CustomLevelFinder.EnumerateLevelInfos().Where(x => x.Gamemode != Constants.GamemodeTakeAndHold))
            {
                var imageT = level.Thumbnail;
                Sprite image = null;
                if (imageT) image = SpriteLoader.ConvertTextureToSprite(imageT);

                // Create and apply scene def.
                MainMenuSceneDef moddedDef = ScriptableObject.CreateInstance<MainMenuSceneDef>();
                moddedDef.Name = level.SceneName + "\n" + level.Identifier;
                moddedDef.Type = level.Author;
                moddedDef.SceneName = "ProvingGround";
                moddedDef.Desciption = level.Description;
                moddedDef.Image = image;

                MainMenuScenePointable screen = screens.First(x => !x.gameObject.activeSelf);
                screen.Def = moddedDef;
                screen.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", imageT);

                // Enable the screen now that it has been set up.
                screen.gameObject.SetActive(true);
            }
        }

        private static void SetupPanel()
        {
            // Copy the changelog panel for my own purposes.
            GameObject panel = GameObject.Instantiate(changelogPanel, changelogPanel.transform.parent);
            panel.transform.position = new Vector3(-1.805f, 2.15f, -7.561f);
            panel.transform.localEulerAngles = new Vector3(0f, 13.91f, 0f);
            panel.name = "MODPANEL";

            // Two text fields, title and body.
            Text[] texts = panel.GetComponentsInChildren<Text>();
            Text title = texts[0];
            Text body = texts[1];

            //TODO Read from web source.
            title.text = "Welcome to WurstMod!";
            body.text = "This panel will be used to provide information and updates about WurstMod. An UPDATE button will appear below if there is an update released.";

            //TODO Update button.
        }
    }
}