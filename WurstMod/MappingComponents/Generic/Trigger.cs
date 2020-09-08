using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace WurstMod.MappingComponents.Generic
{
    public enum TriggeredBy { Player, Sosig, Collider }
    public enum TriggerType { Any, Each }
    public enum TriggerShape { Cube, Sphere }

    public class Trigger : MonoBehaviour
    {
        // These triggers are going to be fairly expensive because we cannot use base Unity triggers.
        // I think this is due to some wonky raycast logic used by Anton, probably not fixable.

        // Inspectables
        [Header("What to Detect")]
        [Tooltip("What this is triggered by.")]
        public TriggeredBy triggeredBy = TriggeredBy.Collider;
        [Tooltip("Collider mode only: If there are elements in the whitelist, only objects with those names will be detected.")]
        public List<string> whitelist = new List<string>();
        [Tooltip("Collider mode only: Objects with names in the blacklist will be ignored.")]
        public List<string> blacklist = new List<string>();

        [Header("How to Detect")]
        [Tooltip("Any: Trigger Enter when number of valid objects in trigger goes from 0 to 1, and trigger Exit on 1 to 0\nEach: Trigger Enter when number of valid objects in trigger increases, and trigger Exit when it decreases.")]
        [HideInInspector] // TODO: Each has some duplicated object bugs that are going to be very amusing to fix. For now, I will ignore them. Any mode is plenty useful as is.
        public TriggerType triggerType = TriggerType.Any;

        [Header("Where to Detect")]
        [Tooltip("The shape of the trigger.")]
        public TriggerShape triggerShape = TriggerShape.Cube;
        [Tooltip("When in cube mode, this controls the size of the cube.")]
        public Vector3 cubeShape = Vector3.one;
        [Tooltip("When in sphere mode, this controls the size of the sphere.")]
        public float sphereRadius = 1f;

        [Header("What to Do")]
        [Tooltip("Actions to run when trigger is entered.")]
        public UnityEvent enterEvent;
        [Tooltip("Actions to run when trigger is exited.")]
        public UnityEvent exitEvent;

        // Internal for Controller
        private static Trigger controller;
        private static List<Trigger> allTriggers = new List<Trigger>();
        private static readonly int checkPerFrame = 10; // How many triggers we check each frame.
        private static bool checking = false; // Flag to control coroutine restarting.

        // Instance
        private HashSet<Collider> colliders = new HashSet<Collider>();

        private void OnDrawGizmos()
        {
            switch (triggerShape)
            {
                case TriggerShape.Cube:
                    Extensions.GenericGizmoCubeOutline(Color.green, Vector3.zero, cubeShape, transform);
                    break;
                case TriggerShape.Sphere:
                    Extensions.GenericGizmoSphereOutline(Color.green, Vector3.zero, sphereRadius, transform);
                    break;
            }
        }

        private void Awake()
        {
            // Set the singleton controller to the first Trigger to wake up.
            if (controller == null) controller = this;

            // Add ourself to the list of triggers.
            allTriggers.Add(this);
        }

        private void OnDestroy()
        {
            // Reload-safe logic.
            allTriggers.Remove(this);
            if (allTriggers.Count == 0)
            {
                controller = null;
            }
        }

        private void Update()
        {
            // The update logic is handled by the controller trigger only.
            if (controller == this)
            {
                if (!checking)
                {
                    StartCoroutine(CheckAllTriggers());
                }
            }
        }

        private static IEnumerator CheckAllTriggers()
        {
            checking = true;
            for (int ii = 0; ii < allTriggers.Count; ii++)
            {
                allTriggers[ii].CheckTrigger();

                // Skip a frame every {checkPerFrame} triggers.
                if (ii > 0 && ii % checkPerFrame == 0) yield return null;
            }
            checking = false;
        }
       
        private void CheckTrigger()
        {
            Collider[] collidersInOverlap = triggerShape == TriggerShape.Cube ? 
                Physics.OverlapBox(transform.position, cubeShape) :
                Physics.OverlapSphere(transform.position, sphereRadius);

            // Check enter
            foreach (Collider col in collidersInOverlap)
            {
                if (!colliders.Contains(col) && IsValidForTrigger(col))
                {
                    colliders.Add(col);
                    Debug.Log("ENTER: " + col.gameObject.name);

                    if (colliders.Count == 1 || triggerType == TriggerType.Each) enterEvent.Invoke();
                }
            }

            // Check exit
            Collider[] collidersInHashset = colliders.ToArray();
            foreach (Collider col in collidersInHashset)
            {
                if (!collidersInOverlap.Contains(col))
                {
                    colliders.Remove(col);
                    Debug.Log("EXIT: " + col.gameObject.name);

                    if (colliders.Count == 0 || triggerType == TriggerType.Each) exitEvent.Invoke();
                }
            }
        }

        private bool IsValidForTrigger(Collider col)
        {
            if (triggeredBy == TriggeredBy.Player && col.gameObject.name == "PlayerAIEntity") return true;
            if (triggeredBy == TriggeredBy.Sosig && col.gameObject.name == "AIEntity") return true;
            if (triggeredBy == TriggeredBy.Collider)
            {
                if (whitelist.Count > 0 && !whitelist.Contains(col.gameObject.name)) return false;
                if (blacklist.Count > 0 && blacklist.Contains(col.gameObject.name)) return false;
                return true;
            }
            return false;
        }
    }
}
