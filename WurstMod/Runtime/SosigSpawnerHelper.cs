using System;
using System.Collections.Generic;
using FistVR;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace WurstMod.Runtime
{
    public static class SosigSpawnerHelper
    {
        public static  void SpawnSosigWithTemplate(SosigEnemyTemplate template, SpawnOptions spawnOptions, Vector3 position,
            Vector3 forward)
        {
            var sosigPrefab = template.SosigPrefabs[Random.Range(0, template.SosigPrefabs.Count)];
            var configTemplate = template.ConfigTemplates[Random.Range(0, template.ConfigTemplates.Count)];
            var w1 = template.OutfitConfig[Random.Range(0, template.OutfitConfig.Count)];
            var key = SpawnSosigAndConfigureSosig(sosigPrefab.GetGameObject(), position,
                Quaternion.LookRotation(forward, Vector3.up), configTemplate, w1);
            key.InitHands();
            key.Inventory.Init();
            if (template.WeaponOptions.Count > 0)
            {
                var w2 = SpawnWeapon(template.WeaponOptions);
                w2.SetAutoDestroy(true);
                key.ForceEquip(w2);
                if (w2.Type == SosigWeapon.SosigWeaponType.Gun && spawnOptions.SpawnWithFullAmmo)
                    key.Inventory.FillAmmoWithType(w2.AmmoType);
            }

            var spawnWithSecondaryWeapon = spawnOptions.EquipmentMode == 0 || spawnOptions.EquipmentMode == 2 ||
                                           spawnOptions.EquipmentMode == 3 &&
                                           Random.Range(0.0f, 1f) >= template.SecondaryChance;
            if (template.WeaponOptions_Secondary.Count > 0 && spawnWithSecondaryWeapon)
            {
                var w2 = SpawnWeapon(template.WeaponOptions_Secondary);
                w2.SetAutoDestroy(true);
                key.ForceEquip(w2);
                if (w2.Type == SosigWeapon.SosigWeaponType.Gun && spawnOptions.SpawnWithFullAmmo)
                    key.Inventory.FillAmmoWithType(w2.AmmoType);
            }

            var spawnWithTertiaryWeapon = spawnOptions.EquipmentMode == 0 ||
                                          spawnOptions.EquipmentMode == 3 &&
                                          Random.Range(0.0f, 1f) >= template.TertiaryChance;
            if (template.WeaponOptions_Tertiary.Count > 0 && spawnWithTertiaryWeapon)
            {
                var w2 = SpawnWeapon(template.WeaponOptions_Tertiary);
                w2.SetAutoDestroy(true);
                key.ForceEquip(w2);
                if (w2.Type == SosigWeapon.SosigWeaponType.Gun && spawnOptions.SpawnWithFullAmmo)
                    key.Inventory.FillAmmoWithType(w2.AmmoType);
            }

            var sosigIFF = spawnOptions.IFF;
            if (sosigIFF >= 5)
                sosigIFF = Random.Range(6, 10000);
            key.E.IFFCode = sosigIFF;
            key.CurrentOrder = Sosig.SosigOrder.Disabled;
            switch (spawnOptions.SpawnState)
            {
                case 0:
                    key.FallbackOrder = Sosig.SosigOrder.Disabled;
                    break;
                case 1:
                    key.FallbackOrder = Sosig.SosigOrder.GuardPoint;
                    break;
                case 2:
                    key.FallbackOrder = Sosig.SosigOrder.Wander;
                    break;
                case 3:
                    key.FallbackOrder = Sosig.SosigOrder.Assault;
                    break;
            }

            var targetPos = spawnOptions.SosigTransformTarget
                ? spawnOptions.SosigTransformTarget.position
                : spawnOptions.SosigTargetPosition;
            var targetRot = spawnOptions.SosigTransformTarget
                ? spawnOptions.SosigTransformTarget.eulerAngles
                : spawnOptions.SosigTargetRotation;
            key.UpdateGuardPoint(targetPos);
            key.SetDominantGuardDirection(targetRot);
            key.UpdateAssaultPoint(targetPos);
            if (!spawnOptions.SpawnActivated)
                return;
            key.SetCurrentOrder(key.FallbackOrder);
        }

        private static SosigWeapon SpawnWeapon(List<FVRObject> o) => Object
            .Instantiate(o[Random.Range(0, o.Count)].GetGameObject(), new Vector3(0.0f, 30f, 0.0f), Quaternion.identity)
            .GetComponent<SosigWeapon>();

        private static Sosig SpawnSosigAndConfigureSosig(GameObject prefab, Vector3 pos, Quaternion rot, SosigConfigTemplate t,
            SosigOutfitConfig w)
        {
            var componentInChildren = Object.Instantiate(prefab, pos, rot).GetComponentInChildren<Sosig>();
            if (Random.Range(0.0f, 1f) < w.Chance_Headwear)
                SpawnAccessoryToLink(w.Headwear, componentInChildren.Links[0]);
            if (Random.Range(0.0f, 1f) < w.Chance_Facewear)
                SpawnAccessoryToLink(w.Facewear, componentInChildren.Links[0]);
            if (Random.Range(0.0f, 1f) < w.Chance_Eyewear)
                SpawnAccessoryToLink(w.Eyewear, componentInChildren.Links[0]);
            if (Random.Range(0.0f, 1f) < w.Chance_Torsowear)
                SpawnAccessoryToLink(w.Torsowear, componentInChildren.Links[1]);
            if (Random.Range(0.0f, 1f) < w.Chance_Pantswear)
                SpawnAccessoryToLink(w.Pantswear, componentInChildren.Links[2]);
            if (Random.Range(0.0f, 1f) < w.Chance_Pantswear_Lower)
                SpawnAccessoryToLink(w.Pantswear_Lower, componentInChildren.Links[3]);
            if (Random.Range(0.0f, 1f) < w.Chance_Backpacks)
                SpawnAccessoryToLink(w.Backpacks, componentInChildren.Links[1]);
            if (t.UsesLinkSpawns)
            {
                for (var index = 0; index < componentInChildren.Links.Count; ++index)
                {
                    if (Random.Range(0.0f, 1f) < t.LinkSpawnChance[index])
                        componentInChildren.Links[index].RegisterSpawnOnDestroy(t.LinkSpawns[index]);
                }
            }

            componentInChildren.Configure(t);
            return componentInChildren;
        }

        private static void SpawnAccessoryToLink(List<FVRObject> gs, SosigLink l)
        {
            if (gs.Count < 1) return;
            var accessory = Object.Instantiate(gs[Random.Range(0, gs.Count)].GetGameObject(), l.transform.position,
                l.transform.rotation);
            accessory.transform.SetParent(l.transform);
            accessory.GetComponent<SosigWearable>().RegisterWearable(l);
        }

        [Serializable]
        public struct SpawnOptions
        {
            [Tooltip("Whether to spawn the Sosig activated or not")]
            public bool SpawnActivated;

            [Tooltip("Sets the Sosig's IFF (Team). Values 5 and above get randomized")]
            public int IFF;

            [Tooltip("Spawns the Sosig with full ammo")]
            public bool SpawnWithFullAmmo;

            [Tooltip("Not sure what this does. Recommended to just leave at 0")]
            public int EquipmentMode;

            [Tooltip("The state to spawn the Sosig in. 0 = Disabled, 1 = Guard, 2 = Wander, 3 = Assault")]
            public int SpawnState;

            [Tooltip("The position of the Sosig's attack / guard position")]
            public Vector3 SosigTargetPosition;

            public Vector3 SosigTargetRotation;

            [Tooltip("Set this a transform to make the Sosigs spawn with it's position and rotation as it's target.")]
            public Transform SosigTransformTarget;
        }
    }
}