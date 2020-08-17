using System.Linq;
using UnityEngine;
using WurstMod.Runtime;
using WurstMod.Shared;

namespace WurstMod.MappingComponents.TakeAndHold
{
    public class TNH_SupplyPoint : ComponentProxy
    {
        [Tooltip("A DISABLED mesh (usually a cube) that encompasses the entire Supply Point. No collider.")]
        public Transform Bounds;

        [Tooltip("PARENT. Parent for CoverPoints which are not a part of Barriers. Rotation does not matter for CoverPoints, and they should be placed on the floor.")]
        public Transform CoverPoints;

        [Tooltip("If the player spawns on this Supply Point, they will spawn here, facing Forward (+Z).")]
        public Transform SpawnPoint_PlayerSpawn;

        [Tooltip("PARENT. When this supply point spawns between waves, defenders will use the child positions of this object to spawn.")]
        public Transform SpawnPoints_Sosigs_Defense;

        [Tooltip("PARENT. I'm not sure these are used right now, but feel free to scatter them around.")]
        public Transform SpawnPoints_Turrets;

        [Tooltip("PARENT. Shop panels spawn at this object's children positions. Most Supply Points have 3-5, so I'd say at least 3. Place them on the ground with +Z facing where the player would stand to use the shop.")]
        public Transform SpawnPoints_Panels;

        [Tooltip("PARENT. Breakable white boxes will spawn at this object's children positions.")]
        public Transform SpawnPoints_Boxes;

        [Tooltip("PARENT. If the player spawns here, the child positions points are where tables will spawn with the initial items. Usually two tables. Place them on the ground.")]
        public Transform SpawnPoint_Tables;

        [Tooltip("Case position for when player spawns here. Place it about 0.8 units above the table for it to sit properly.")]
        public Transform SpawnPoint_CaseLarge;

        [Tooltip("Case position for when player spawns here. Place it about 0.8 units above the table for it to sit properly.")]
        public Transform SpawnPoint_CaseSmall;

        [Tooltip("Melee weapon position for when player spawns here. Place it about 1.1 units above the table.")]
        public Transform SpawnPoint_Melee;

        [Tooltip("PARENT. Small item(s) position for when player spawns here. Place 3 or more of them about 1 unit above the table.")]
        public Transform SpawnPoints_SmallItem;

        [Tooltip("Shield position for when player spawns here. Is this even implemented?")]
        public Transform SpawnPoint_Shield;




        void OnDrawGizmos()
        {
            if (Bounds != null) Extensions.GenericGizmoCubeOutline(Color.white, Vector3.zero, Vector3.one, Bounds);
            if (SpawnPoints_Panels != null) Extensions.GenericGizmoCube(new Color(0.4f, 0.4f, 0.9f, 0.5f), new Vector3(0f, 1.5f, 0.25f), new Vector3(2.3f, 1.2f, 0.5f), Vector3.forward, SpawnPoints_Panels.AsEnumerable().ToArray());
            if (SpawnPoint_CaseLarge != null) Extensions.GenericGizmoCube(new Color(1f, 0.4f, 0.0f, 0.5f), new Vector3(0f, 0.12f, 0f), new Vector3(1.4f, 0.24f, 0.35f), Vector3.forward, SpawnPoint_CaseLarge);
            if (SpawnPoint_CaseSmall != null) Extensions.GenericGizmoCube(new Color(1f, 0.4f, 0.0f, 0.5f), new Vector3(0f, 0.12f, 0f), new Vector3(0.6f, 0.24f, 0.35f), Vector3.forward, SpawnPoint_CaseSmall);
            if (SpawnPoint_Tables != null) Extensions.GenericGizmoCube(new Color(0.5f, 0.5f, 0.5f, 0.5f), new Vector3(0f, 0.4f, 0.1f), new Vector3(0.7f, 0.8f, 1.5f), Vector3.zero, SpawnPoint_Tables.AsEnumerable().ToArray());
            if (SpawnPoints_Boxes != null) Extensions.GenericGizmoCube(new Color(0.7f, 0.7f, 0.7f, 0.5f), Vector3.zero, 0.5f * Vector3.one, Vector3.zero, SpawnPoints_Boxes.AsEnumerable().ToArray());
            if (SpawnPoints_Sosigs_Defense != null) Extensions.GenericGizmoSphere(new Color(0.8f, 0f, 0f, 0.5f), Vector3.zero, 0.25f, SpawnPoints_Sosigs_Defense.AsEnumerable().ToArray());
            if (SpawnPoints_Turrets != null) Extensions.GenericGizmoSphere(new Color(0.8f, 0f, 0f, 0.1f), Vector3.zero, 0.25f, SpawnPoints_Turrets.AsEnumerable().ToArray());
            if (SpawnPoint_Melee != null) Extensions.GenericGizmoSphere(new Color(0.0f, 0.8f, 0f, 0.5f), Vector3.zero, 0.2f, SpawnPoint_Melee);
            if (SpawnPoints_SmallItem != null) Extensions.GenericGizmoSphere(new Color(0.0f, 0.8f, 0f, 0.5f), Vector3.zero, 0.1f, SpawnPoints_SmallItem.AsEnumerable().ToArray());
            if (SpawnPoint_PlayerSpawn != null) Extensions.GenericGizmoSphere(new Color(0.0f, 0.8f, 0.8f, 0.5f), Vector3.zero, 0.25f, SpawnPoint_PlayerSpawn);
            if (SpawnPoint_Shield != null) Extensions.GenericGizmoCube(new Color(0.0f, 0.8f, 0.0f, 0.1f), Vector3.zero, new Vector3(0.4f, 0.6f, 0.1f), Vector3.zero, SpawnPoint_Shield);
        }

        public override void InitializeComponent()
        {
            FistVR.TNH_SupplyPoint real = gameObject.AddComponent<FistVR.TNH_SupplyPoint>();

            real.M = ObjectReferences.ManagerDonor;
            real.Bounds = Bounds;
            real.CoverPoints = CoverPoints.AsEnumerable().Select(x => x.gameObject.GetComponent<global::AICoverPoint>()).ToList();
            real.SpawnPoint_PlayerSpawn = SpawnPoint_PlayerSpawn;
            real.SpawnPoints_Sosigs_Defense = SpawnPoints_Sosigs_Defense.AsEnumerable().ToList();
            real.SpawnPoints_Turrets = SpawnPoints_Turrets.AsEnumerable().ToList();
            real.SpawnPoints_Panels = SpawnPoints_Panels.AsEnumerable().ToList();
            real.SpawnPoints_Boxes = SpawnPoints_Boxes.AsEnumerable().ToList();
            real.SpawnPoint_Tables = SpawnPoint_Tables.AsEnumerable().ToList();
            real.SpawnPoint_CaseLarge = SpawnPoint_CaseLarge;
            real.SpawnPoint_CaseSmall = SpawnPoint_CaseSmall;
            real.SpawnPoint_Melee = SpawnPoint_Melee;
            real.SpawnPoints_SmallItem = SpawnPoints_SmallItem.AsEnumerable().ToList();
            real.SpawnPoint_Shield = SpawnPoint_Shield;

            Destroy(this);
        }
    }
}
