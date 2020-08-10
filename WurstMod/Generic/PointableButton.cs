using FistVR;
using UnityEngine;
using UnityEngine.UI;

namespace WurstMod.Generic
{
    [RequireComponent(typeof(BoxCollider))]
    public class PointableButton : ComponentProxy
    {
        // These are the colors for almost every button in the game
        public Color ColorUnselected = new Color32(0x29, 0x6E, 0xEA, 0xFF);
        public Color ColorSelected = new Color32(0xB1, 0xC9, 0xF4, 0xFF);

        [Tooltip("I think this is used for setting the color of a material? Not sure. Leave as-is.")]
        public string ColorName = "_Color";


        [Header("Component References")]
        public Button Button;

        public Image Image;
        public Text Text;
        public Renderer Rend;

        public override void InitializeComponent()
        {
            var proxied = gameObject.AddComponent<FVRPointableButton>();
            proxied.ColorUnselected = ColorUnselected;
            proxied.ColorSelected = ColorSelected;
            proxied.Button = Button;
            proxied.Image = Image;
            proxied.Text = Text;
            proxied.Rend = Rend;
            proxied.ColorName = ColorName;

            // Borrow the sprite
            proxied.Image.sprite = ObjectReferences.ButtonDonor.Image.sprite;

            // Force an update or something. Idk.
            proxied.ForceUpdate();

           Destroy(this);
        }

        public override void OnExport()
        {
            // Set the collider's size
            var collider = GetComponent<BoxCollider>();
            var size = GetComponent<RectTransform>().sizeDelta;
            collider.size = new Vector3(size.x, size.y, 1);
        }
    }
}
