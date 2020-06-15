# WurstMod

WurstMod is a mod for H3VR currently focusing on making it easy to create and import custom maps. Custom maps are currently limited to the Take and Hold gamemode.

## Installation

Download the most recent release and unzip into your H3VR install folder. You can easily access this folder by right clicking H3VR in your steam library, going to Properties, the Local Files tab, Browse Local Files.

If your folder looks like the image below, you're good to go. Just run the game as you normally would and load into Take And Hold. There will be arrows on the map image panel, pressing those will switch between installed maps.

![Correct folder structure](https://i.imgur.com/pmIefmr.png)

## Getting More Maps

Maps consist of a folder with three files: *leveldata*, *info.txt*, and *thumb.png*. When you download a map someone has made, put its folder into H3VR/CustomLevels/TakeAndHold and it will be selectable next time you start the game.

## Making Maps

1. You need a **specific version of Unity, Unity 5.6.3**. You can find that [Here](https://unity3d.com/get-unity/download/archive)
1. Download my template project, which includes the source for The Blandville Debug Arena, [Here](TODO)
1. TODO
1. TODO Generate Navmesh data.
1. TODO Generate Occlusion data.
1. Run H3VR/Export TNH from the menu bar to export the level. It will be placed in AssetBundles/\<scene name\> in your Unity project folder.
1. Install the level by moving leveldata and info.txt to a new folder in H3VR/CustomLevels/TakeAndHold.
1. Add a thumb.png (roughly 16:9 aspect should be fine) to the folder for a level image.

## Contributing

TODO
