using FistVR;
using UnityEngine;
using WurstMod.Runtime;

namespace WurstMod.MappingComponents.Sandbox
{
    public class GroundPanel : ComponentProxy
    {
        // For some reason, the SosigTestingPanels aren't centered at the base.
        private static readonly Vector3 PositionOffset = new Vector3(0f, 0.768f, 0f);
        private static readonly Vector3 RotationOffset = new Vector3(-60f, 0f, 0f);
        
        public Canvas Canvas;
        
        public override void InitializeComponent()
        {
            // Make a copy of the SosigTestingPanel
            var panel = Instantiate(ObjectReferences.GroundPanel, transform);

            // Correct it's position and rotation
            var correctedPos = transform.position + PositionOffset;
            var correctedRot = Quaternion.Euler(transform.rotation.eulerAngles + RotationOffset);
            panel.transform.SetPositionAndRotation(correctedPos, correctedRot);
            panel.transform.localScale = Vector3.one;
            
            // Remove the existing canvas and components
            var originalCanvas = panel.GetComponentInChildren<Canvas>();
            var originalPos = originalCanvas.transform.position;
            var originalRot = originalCanvas.transform.rotation;
            Destroy(panel.GetComponent<SosigTestingPanel1>());
            Destroy(originalCanvas.gameObject);
            
            // Replace it with the new canvas
            var parent = panel.transform.Find("Selections");
            Canvas.transform.parent = parent;
            Canvas.transform.SetPositionAndRotation(originalPos, originalRot);
        }

        public void OnDrawGizmos()
        {
            // TODO: Find out what shape to draw this
        }
    }
}
