using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WurstMod;

namespace WurstMod.TNH
{
    /// <summary>
    /// This class is responsible for modifying the level selector panel to actually function.
    /// </summary>
    public static class TNH_LevelSelector
    {
        // Settings.
        private static readonly string levelDir = "CustomLevels/TakeAndHold";
        private static readonly string dataFile = "/leveldata";
        private static readonly string imageFile = "/thumb.png";
        private static readonly string infoFile = "/info.txt";

        // Referenced objects.
        public static Canvas levelSelectorCanvas;
        public static Image levelImage;
        public static Text titleText;
        public static Text levelNameText;
        public static Text levelDescriptionText;
        public static GameObject buttonToCopy;

        // LevelDatas
        private static List<LevelData> levels = new List<LevelData>();
        private static int currentLevelIndex = 0;

        /// <summary>
        /// Performs all actions required to setup the level selector in the TNH Lobby.
        /// </summary>
        public static void SetupLevelSelector(Scene loaded)
        {
            if (loaded.name == "TakeAndHold_Lobby_2")
            {
                Debug.Log("Initializing level selector...");
                currentLevelIndex = 0;
                GatherReferences();
                InitDirectories();
                FixLevelNameSize();
                SetupLevelDatas();

                SetupButtons();
            }
        }

        /// <summary>
        /// Grabs references to all of the objects we need to make this work.
        /// </summary>
        private static void GatherReferences()
        {
            Scene current = SceneManager.GetActiveScene();
            levelSelectorCanvas = current.GetAllGameObjectsInScene().Where(x => x.name == "MainMenuCanvas_Right").First().GetComponentInChildren<Canvas>();
            levelImage = levelSelectorCanvas.GetComponentsInChildren<Image>()[1];
            Text[] temp = levelSelectorCanvas.GetComponentsInChildren<Text>();
            titleText = temp[0];
            levelNameText = temp[1];
            levelDescriptionText = temp[2];
            buttonToCopy = current.GetAllGameObjectsInScene().Where(x => x.name == "Pick_0_Standard").First();

            // Oh hey, we can get rid of that "coming soon" :)
            titleText.text = "Select Environment";
        }

        /// <summary>
        /// Ensures proper folders exist.
        /// </summary>
        private static void InitDirectories()
        {
            if (!Directory.Exists(levelDir))
            {
                Directory.CreateDirectory(levelDir);
            }
        }

        private static void FixLevelNameSize()
        {
            RectTransform levelNameRect = levelNameText.transform as RectTransform;
            levelNameRect.sizeDelta = new Vector2(860, 80);
        }

        /// <summary>
        /// Create our own LevelData objects for the original level, and all loaded levels.
        /// </summary>
        private static void SetupLevelDatas()
        {
            levels.Add(new LevelData(levelImage.sprite, "Classic", "H3VR", levelDescriptionText.text, ""));
            //TODO TEST THIS Update information of original level for consistency with new format.
            levels[0].SetLevel();

            foreach(string ii in Directory.GetDirectories(levelDir))
            {
                string[] files = Directory.GetFiles(ii);
                if (!File.Exists(ii + dataFile))
                {
                    Debug.LogError($"Directory {ii} does not contain proper leveldata. The assetbundle must be named leveldata.");
                    continue;
                }
                Sprite image = null;
                if (File.Exists(ii + imageFile)) image = SpriteLoader.LoadNewSprite(ii + imageFile);

                string[] info = new string[0];
                string name;
                string author;
                string desc;
                if (File.Exists(ii + infoFile)) info = File.ReadAllLines(ii + infoFile);

                if (info.Length > 0) name = info[0];
                else name = "UNNAMED LEVEL";

                if (info.Length > 1) author = info[1];
                else author = "UNKNOWN AUTHOR";

                if (info.Length > 2) desc = string.Join("\n", info.Skip(2).ToArray());
                else desc = "NO DESCRIPTION";


                Debug.Log($"Found level: {name}");
                LevelData data = new LevelData(image, name, author, desc, ii + dataFile);
                levels.Add(data);
            }
        }

        /// <summary>
        /// Sets up the buttons you can press to switch between levels.
        /// </summary>
        private static void SetupButtons()
        {
            // Left button.
            GameObject buttonCopy = GameObject.Instantiate<GameObject>(buttonToCopy);
            buttonCopy.transform.SetParent(levelSelectorCanvas.transform, false);
            buttonCopy.transform.SetAsLastSibling();
            buttonCopy.transform.localEulerAngles = Vector3.zero;

            RectTransform xform = buttonCopy.transform as RectTransform;
            xform.sizeDelta = new Vector2(95, 433);
            xform.anchoredPosition = new Vector2(-385.5f, 188.9f);

            Text text = buttonCopy.GetComponent<Text>();
            text.text = "<";
            text.fontSize = 120;

            BoxCollider col = buttonCopy.GetComponent<BoxCollider>();
            col.size = new Vector3(75, 433, 5);

            Button button = buttonCopy.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                LeftButton();
            });

            // Copy for right button.
            GameObject buttonRight = GameObject.Instantiate<GameObject>(buttonCopy);
            buttonRight.transform.SetParent(levelSelectorCanvas.transform, false);
            buttonRight.transform.SetAsLastSibling();
            buttonRight.transform.localEulerAngles = Vector3.zero;

            xform = buttonRight.transform as RectTransform;
            xform.anchoredPosition = new Vector2(385.5f, 188.9f);

            text = buttonRight.GetComponent<Text>();
            text.text = ">";

            button = buttonRight.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                RightButton();
            });
        }

        /// <summary>
        /// Listener for LeftButton press.
        /// </summary>
        private static void LeftButton()
        {
            if (currentLevelIndex == 0) currentLevelIndex = levels.Count - 1;
            else currentLevelIndex--;

            levels[currentLevelIndex].SetLevel();
        }

        /// <summary>
        /// Listener for RightButton press.
        /// </summary>
        private static void RightButton()
        {
            if (currentLevelIndex == levels.Count - 1) currentLevelIndex = 0;
            else currentLevelIndex++;

            levels[currentLevelIndex].SetLevel();
        }
    }

    /// <summary>
    /// Class for neatly storing level information and updating the UI.
    /// </summary>
    class LevelData
    {
        public Sprite levelImage;
        public string levelName;
        public string levelAuthor;
        public string levelDescription;
        public string levelDataPath;

        public LevelData(Sprite image, string name, string author, string desc, string dataPath)
        {
            levelImage = image;
            levelName = name;
            levelAuthor = author;
            levelDescription = desc;
            levelDataPath = dataPath;
        }

        public void SetLevel()
        {
            TNH_LevelSelector.levelImage.sprite = levelImage;
            TNH_LevelSelector.levelNameText.text = levelName + "\nby " + levelAuthor;
            TNH_LevelSelector.levelDescriptionText.text = levelDescription;
            Loader.levelToLoad = levelDataPath;
        }
    }
}
