using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.TNH
{
    public class TNH_HoldPoint : MonoBehaviour
    {
        [Tooltip("A DISABLED mesh (usually a cube) that encompasses the entire Hold Point. No collider.")]
        public List<Transform> Bounds;

        [Tooltip("DISABLED parent object with DISABLED children. These become active during a Hold, and have colliders on the NavBlock layer to prevent the player from leaving the hold area. Place collider objects on the entrances to a hold point.")]
        public GameObject NavBlockers;

        [Tooltip("PARENT. Parent object for all barriers. Place children of this object on the floor.")]
        public Transform BarrierPoints;

        [Tooltip("PARENT. Parent object for cover points. Cover points not related to Barriers should be a child of this object.")]
        public Transform CoverPoints;

        [Tooltip("System Node spawns here. Place on floor.")]
        public Transform SpawnPoint_SystemNode;

        [Tooltip("PARENT. Encryption spawns in the positions of this object's children. Make a bunch, place in air.")]
        public Transform SpawnPoints_Targets;

        [Tooltip("PARENT. Unused?")]
        public Transform SpawnPoints_Turrets;

        [Tooltip("PARENT. Sosigs will choose from this object's AttackVector children when spawning in during a hold.")]
        public Transform AttackVectors;

        [Tooltip("PARENT. Sosigs will spawn in the positions of this object's children when this hold point becomes takeable. Place on floor.")]
        public Transform SpawnPoints_Sosigs_Defense;



        private void OnDrawGizmos()
        {
            if (Bounds != null) Extensions.GenericGizmoCubeOutline(Color.white, Vector3.zero, Vector3.one, Bounds.ToArray());
            if (SpawnPoint_SystemNode != null) Extensions.GenericGizmoSphere(new Color(0.0f, 0.8f, 0.8f), 1.5f * Vector3.up, 0.25f, SpawnPoint_SystemNode);
            if (SpawnPoints_Sosigs_Defense != null) Extensions.GenericGizmoSphere(new Color(0.8f, 0f, 0f, 0.5f), Vector3.zero, 0.25f, SpawnPoints_Sosigs_Defense.AsEnumerable().ToArray());
            if (SpawnPoints_Turrets != null) Extensions.GenericGizmoSphere(new Color(0.8f, 0f, 0f, 0.1f), Vector3.zero, 0.25f, SpawnPoints_Turrets.AsEnumerable().ToArray());
            if (SpawnPoints_Targets != null) Extensions.GenericGizmoCube(new Color(1f, 0.0f, 0.0f, 0.5f), Vector3.zero, new Vector3(0.1f, 0.5f, 0.1f), Vector3.zero, SpawnPoints_Targets.AsEnumerable().ToArray());
        }
    }
}
