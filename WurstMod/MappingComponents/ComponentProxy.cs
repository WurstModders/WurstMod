using UnityEngine;

#if UNITY_EDITOR
using WurstMod.UnityEditor;
#endif

namespace WurstMod.MappingComponents
{
    public abstract class ComponentProxy : MonoBehaviour
    {
        /// <summary>
        /// Certain components MUST be initialized before others. Override this
        /// to ensure ordering. HIGH values run before lower values, default importance is 1.
        /// </summary>
        public virtual int ResolveOrder => 1;
        
        /// <summary>
        ///     This is called at run-time to resolve the proxied component.
        ///     Returns true if the unproxied component should be deleted afterwards.
        /// </summary>
        public virtual void InitializeComponent() { }

#if UNITY_EDITOR
        /// <summary>
        ///     This is called before the map is exported. Put one-time calculations and component validations here.
        /// </summary>
        public virtual void OnExport(ExportErrors err) { }
#endif
    }
}
