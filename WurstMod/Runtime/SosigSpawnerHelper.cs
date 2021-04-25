#if !UNITY_EDITOR
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
            var outfitConfig = template.OutfitConfig[Random.Range(0, template.OutfitConfig.Count)];
            var sosig = SpawnSosigAndConfigureSosig(sosigPrefab.GetGameObject(), position,
                Quaternion.LookRotation(forward, Vector3.up), configTemplate, outfitConfig);
            sosig.InitHands();
            sosig.Inventory.Init();
            if (template.WeaponOptions.Count > 0)
            {
                var weapon = SpawnWeapon(template.WeaponOptions);
                weapon.SetAutoDestroy(true);
                sosig.ForceEquip(weapon);
                if (weapon.Type == SosigWeapon.SosigWeaponType.Gun && spawnOptions.SpawnWithFullAmmo)
                    sosig.Inventory.FillAmmoWithType(weapon.AmmoType);
            }

            var spawnWithSecondaryWeapon = spawnOptions.EquipmentMode == 0 || spawnOptions.EquipmentMode == 2 ||
                                           spawnOptions.EquipmentMode == 3 &&
                                           Random.Range(0.0f, 1f) >= template.SecondaryChance;
            if (template.WeaponOptions_Secondary.Count > 0 && spawnWithSecondaryWeapon)
            {
                var weapon = SpawnWeapon(template.WeaponOptions_Secondary);
                weapon.SetAutoDestroy(true);
                sosig.ForceEquip(weapon);
                if (weapon.Type == SosigWeapon.SosigWeaponType.Gun && spawnOptions.SpawnWithFullAmmo)
                    sosig.Inventory.FillAmmoWithType(weapon.AmmoType);
            }

            var spawnWithTertiaryWeapon = spawnOptions.EquipmentMode == 0 ||
                                          spawnOptions.EquipmentMode == 3 &&
                                          Random.Range(0.0f, 1f) >= template.TertiaryChance;
            if (template.WeaponOptions_Tertiary.Count > 0 && spawnWithTertiaryWeapon)
            {
                var w2 = SpawnWeapon(template.WeaponOptions_Tertiary);
                w2.SetAutoDestroy(true);
                sosig.ForceEquip(w2);
                if (w2.Type == SosigWeapon.SosigWeaponType.Gun && spawnOptions.SpawnWithFullAmmo)
                    sosig.Inventory.FillAmmoWithType(w2.AmmoType);
            }

            var sosigIFF = spawnOptions.IFF;
            if (sosigIFF >= 5)
                sosigIFF = Random.Range(6, 10000);
            sosig.E.IFFCode = sosigIFF;
            sosig.CurrentOrder = Sosig.SosigOrder.Disabled;
            switch (spawnOptions.SpawnState)
            {
                case 0:
                    sosig.FallbackOrder = Sosig.SosigOrder.Disabled;
                    break;
                case 1:
                    sosig.FallbackOrder = Sosig.SosigOrder.GuardPoint;
                    break;
                case 2:
                    sosig.FallbackOrder = Sosig.SosigOrder.Wander;
                    break;
                case 3:
                    sosig.FallbackOrder = Sosig.SosigOrder.Assault;
                    break;
            }

            var targetPos = spawnOptions.SosigTargetPosition;
            var targetRot = spawnOptions.SosigTargetRotation;
            sosig.UpdateGuardPoint(targetPos);
            sosig.SetDominantGuardDirection(targetRot);
            sosig.UpdateAssaultPoint(targetPos);
            if (!spawnOptions.SpawnActivated)
                return;
            sosig.SetCurrentOrder(sosig.FallbackOrder);
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
        
        public struct SpawnOptions
        {
            public bool SpawnActivated;
            public int IFF;
            public bool SpawnWithFullAmmo;
            public int EquipmentMode;
            public int SpawnState;
            public Vector3 SosigTargetPosition;
            public Vector3 SosigTargetRotation;
        }
    }
}
#endif