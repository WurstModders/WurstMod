using UnityEngine;

namespace WurstMod
{
    public abstract class ComponentProxy : MonoBehaviour
    {
        /// <summary>
        /// Certain components MUST be initialized before others. Override this
        /// to ensure ordering. HIGH values run before lower values, default importance is 1.
        public virtual int GetImportance() { return 1; }

        /// <summary>
        ///     Called when the loader is resolving the proxies.
        ///     Lets the proxy initialize the proxied component, then destroy itself
        ///     (Note that 'itself' means the component, not the game object)
        /// </summary>
        public void ResolveProxy()
        {
            if (InitializeComponent())
            {
                Destroy(this);
            }
        }

        /// <summary>
        ///     This is called at run-time to resolve the proxied component.
        ///     Returns true if the unproxied component should be deleted afterwards.
        /// </summary>
        protected virtual bool InitializeComponent() { return true; }

        /// <summary>
        ///     This is called before the map is exported. Put one-time calculations and component validations here.
        /// </summary>
        /// TODO: Have an object represent the validation result
        public virtual void OnExport() { }
    }
}
