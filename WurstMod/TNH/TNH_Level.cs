using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

namespace WurstMod.TNH
{
    public class TNH_Level : ComponentProxy
    {
        // Used by exporter to generate an info file I guess?
        public string levelName;
        public string levelAuthor;

        [TextArea(15, 20)] public string levelDescription;

        public Material skybox;

        public override void OnExport()
        {
            skybox = RenderSettings.skybox;
        }

        public override void InitializeComponent()
        {
            // This component is responsible for resolving many of the global/builtin things about a level.
            // Skybox
            if (skybox != null)
            {
                RenderSettings.skybox = skybox;
                RenderSettings.skybox.RefreshShader();
                DynamicGI.UpdateEnvironment();
            }

            // Shaders
            foreach (MeshRenderer ii in GetComponentsInChildren<MeshRenderer>(true))
            {
                foreach (Material jj in ii.materials)
                {
                    jj.RefreshShader();
                }
            }

            // Particle Shaders
            foreach (ParticleSystemRenderer ii in GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                ii.materials.ForEach(x => x.RefreshShader());
            }

            // Terrain
            foreach (Terrain ii in GetComponentsInChildren<Terrain>(true))
            {
                ii.materialTemplate.RefreshShader();
                ii.terrainData.treePrototypes.ForEach(x => x.prefab.layer = LayerMask.NameToLayer("Environment"));
                foreach (TreePrototype jj in ii.terrainData.treePrototypes)
                {
                    jj.prefab.layer = LayerMask.NameToLayer("Environment");
                    MeshRenderer[] mrs = jj.prefab.GetComponentsInChildren<MeshRenderer>();
                    mrs.ForEach(x => x.material.RefreshShader());
                }

                foreach (TreeInstance jj in ii.terrainData.treeInstances)
                {
                    GameObject copiedTree = Instantiate(ii.terrainData.treePrototypes[jj.prototypeIndex].prefab, ii.transform);
                    copiedTree.transform.localPosition = new Vector3(ii.terrainData.size.x * jj.position.x, ii.terrainData.size.y * jj.position.y, ii.terrainData.size.z * jj.position.z);
                    copiedTree.transform.localScale = new Vector3(jj.widthScale, jj.heightScale, jj.widthScale);
                    copiedTree.transform.localEulerAngles = new Vector3(0f, jj.rotation, 0f);
                }

                ii.terrainData.treeInstances = new TreeInstance[0];
            }

            // Copy font data to all text components using the default font.
            Font font = ObjectReferences.ButtonDonor.GetComponentInChildren<Text>().font;
            foreach (Text text in GetComponentsInChildren<Text>(true))
            {
                if (text.font == null || text.font.name == "Arial") text.font = font;
            }
        }
    }
}