using System.Collections;
using System.Linq;
using FistVR;
using UnityEngine;
using WurstMod.Runtime;
using WurstMod.Shared;
using WurstMod.UnityEditor;
using Random = UnityEngine.Random;

namespace WurstMod.MappingComponents.Generic
{
    public class SosigSpawner : ComponentProxy
    {
        [Tooltip("A list of Sosig types to spawn from")]
        public Enums.P_SosigEnemyID[] SosigTypes;

        [Tooltip("The delay between this spawner becoming active and when it starts spawning")]
        public float SpawnDelay = 10f;

        [Tooltip("The delay between the spawning of each Sosig")]
        public float SpawnInterval = 5f;

        [Tooltip("The number of Sosigs to spawn before automatically deactivating (0 for infinite)")]
        public int SpawnCount = 5;

        [Tooltip("Whether or not this spawner is active by default")]
        public bool Active = true;

        [Header("Spawn Options")]
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
        
        // This needs to be a ScriptableObject because otherwise Unity throws a fit
        private ScriptableObject[] _enemyTemplates;
        private IEnumerator _coroutine;

        private void Awake()
        {
            // Convert the sosig types to the templates
            _enemyTemplates = SosigTypes.Select(x =>
                ManagerSingleton<IM>.Instance.odicSosigObjsByID[(SosigEnemyID) x] as ScriptableObject).ToArray();
        }

        private void Start()
        {
            // If we want to start active, do that.
            SetActive(Active);
        }

        /// <summary>
        ///     Sets this spawner active or inactive. Setting an active spawner to inactive will instantly
        ///     cancel any further spawning and setting an active spawner to active will reset the spawner state.
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            // Update the variable
            Active = active;

            // We stop the coroutine either way
            if (_coroutine != null) StopCoroutine(_coroutine);

            // But we only restart it if we're activating
            if (!Active) return;

            // Reset the coroutine
            _coroutine = SpawnCoroutine();
            StartCoroutine(_coroutine);
        }

        /// <summary>
        ///     Spawns a single Sosig
        /// </summary>
        public void Spawn()
        {
            // Pick a random template and spawn the Sosig
            var template = _enemyTemplates[Random.Range(0, _enemyTemplates.Length)] as SosigEnemyTemplate;
            SosigSpawnerHelper.SpawnSosigWithTemplate(template, new SosigSpawnerHelper.SpawnOptions
            {
                SpawnActivated = SpawnActivated,
                IFF = IFF,
                SpawnWithFullAmmo = SpawnWithFullAmmo,
                EquipmentMode = EquipmentMode,
                SpawnState = SpawnState,
                SosigTargetPosition = SosigTransformTarget ? SosigTransformTarget.position : SosigTargetPosition,
                SosigTargetRotation = SosigTransformTarget ? SosigTransformTarget.rotation.eulerAngles : SosigTargetRotation
            }, transform.position, transform.forward);
        }

        /// <summary>
        ///     Coroutine to handle spawning logic
        /// </summary>
        private IEnumerator SpawnCoroutine()
        {
            // Wait for the activation delay
            yield return new WaitForSeconds(SpawnDelay);

            // Repeat as many times as needed
            var counter = 0u;
            while (counter < SpawnCount || SpawnCount == 0)
            {
                // Spawn a sosig
                Spawn();

                // Then wait for the interval delay to expire
                yield return new WaitForSeconds(SpawnInterval);
                
                // Increment the counter. This *might* break if it's set to infinite spawning and we exceed the
                // max 32-bit uint value but I don't think we need to worry about that.
                counter++;
            }
        }

        public override void OnExport(ExportErrors err)
        {
            if (SosigTypes.Length == 0) err.AddError("Sosig Spawner has no types to spawn!", this);
            if (SpawnDelay < 0) err.AddError("Sosig Spawner cannot have a spawn delay of less than zero", this);
            if (SpawnInterval < 0) err.AddError("Sosig Spawner cannot have a spawn interval of less than zero", this);
            if (SpawnCount < 0) err.AddError("Sosig Spawner cannot have a spawn count of less than zero", this);
        }

        private void OnDrawGizmos()
        {
            Extensions.GenericGizmoSphere(new Color(0.8f, 0f, 0f, 0.5f), Vector3.zero, 0.25f, transform);
        }
    }
}