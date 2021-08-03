using System;
using UnityEngine;
using WurstMod.Runtime;
using WurstMod.Shared;

#if UNITY_EDITOR
using WurstMod.UnityEditor;
#else
using Valve.VR.InteractionSystem;
using FistVR;
#endif

namespace WurstMod.MappingComponents.Generic
{
    public class CustomScene : ComponentProxy
    {
        [HideInInspector] public string SceneName;
        [HideInInspector] public string Author;
        [HideInInspector] public string Gamemode;
        [HideInInspector] public string Description;
        [HideInInspector] public Material Skybox;

        [HideInInspector] public StringKeyValue[] ExtraData;

        [Header("Scene Settings")] public float MaxProjectileRange = 500f;
        [Header("AI Settings")] public int NumEntitiesToCheckPerFrame = 1;

        public int PlayerIFF = 0;

#if UNITY_EDITOR
        public override void OnExport(ExportErrors err)
        {
            Skybox = RenderSettings.skybox;
        }
#endif

        public void TeleportPlayer(Transform to)
        {
#if !UNITY_EDITOR
            // Proxy to the game's teleport method
            GM.CurrentMovementManager.TeleportToPoint(to.position, true, to.position + transform.forward);
#endif
        }
        public void KillPlayer()
        {
#if !UNITY_EDITOR
            // Damage the player directly, but then play the death noise.
            // This is required because otherwise the invincible power-up blocks the damage.
            GM.CurrentPlayerBody.RegisterPlayerHit(GM.CurrentPlayerBody.Health + 5, true);
            FVRPlayerHitbox hb = GM.CurrentPlayerBody.Hitboxes[0];
            hb.m_aud.PlayOneShot(hb.AudClip_Reset);
#endif
        }

        public override void InitializeComponent()
        {
#if !UNITY_EDITOR
            // This component is responsible for resolving many of the global/builtin things about a level.
            // Skybox
            if (Skybox != null)
            {
                RenderSettings.skybox = Skybox;
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
                    GameObject copiedTree = Instantiate(ii.terrainData.treePrototypes[jj.prototypeIndex].prefab,
                        ii.transform);
                    copiedTree.transform.localPosition = new Vector3(ii.terrainData.size.x * jj.position.x,
                        ii.terrainData.size.y * jj.position.y, ii.terrainData.size.z * jj.position.z);
                    copiedTree.transform.localScale = new Vector3(jj.widthScale, jj.heightScale, jj.widthScale);
                    copiedTree.transform.localEulerAngles = new Vector3(0f, jj.rotation, 0f);
                }

                ii.terrainData.treeInstances = new TreeInstance[0];
            }

            // Set the max range on the scene settings
            // TODO: There are probably a lot of settings we should carry over here.
            ObjectReferences.FVRSceneSettings.MaxProjectileRange = MaxProjectileRange;
            ObjectReferences.FVRSceneSettings.DefaultPlayerIFF = PlayerIFF;
            GM.CurrentPlayerBody.SetPlayerIFF(PlayerIFF);
#endif
        }

        [Serializable]
        public struct StringKeyValue
        {
            public string Key;
            public string Value;
        }
    }
}