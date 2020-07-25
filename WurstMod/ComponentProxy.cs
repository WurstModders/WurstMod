using UnityEngine;

namespace WurstMod
{
    public abstract class ComponentProxy : MonoBehaviour
    {
        /// <summary>
        ///     Called when the loader is resolving the proxies.
        ///     Lets the proxy initialize the proxied component, then destroy itself
        ///     (Note that 'itself' means the component, not the game object)
        /// </summary>
        public void ResolveProxy()
        {
            InitializeComponent();
            Destroy(this);
        }

        /// <summary>
        ///     This is called at run-time to resolve the proxied component.
        /// </summary>
        protected virtual void InitializeComponent() { }

        /// <summary>
        ///     This is called before the map is exported. Put one-time calculations and component validations here.
        /// </summary>
        /// TODO: Have an object represent the validation result
        public virtual void OnExport() { }
    }
}
