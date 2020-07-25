using FistVR;
using UnityEngine;
using UnityEngine.UI;

namespace WurstMod.Generic
{
    [RequireComponent(typeof(BoxCollider))]
    public class PointableButton : ComponentProxy
    {
        // These are the colors for almost every button in the game
        public Color ColorUnselected = new Color(0.107f, 0.286f, 0.609f, 0.628f);
        public Color ColorSelected = new Color(0.750f, 0.793f, 0.872f, 0.847f);

        [Tooltip("I think this is used for setting the color of a material? Not sure. Leave as-is.")]
        public string ColorName = "_Color";


        [Header("Component References")]
        public Button Button;

        public Image Image;
        public Text Text;
        public Renderer Rend;

        protected override void InitializeComponent()
        {
            var proxied = gameObject.AddComponent<FVRPointableButton>();
            proxied.ColorUnselected = ColorUnselected;
            proxied.ColorSelected = ColorSelected;
            proxied.Button = Button;
            proxied.Image = Image;
            proxied.Text = Text;
            proxied.Rend = Rend;
            proxied.ColorName = ColorName;

            // 'Borrow' the sprite and font from the donor
            proxied.Image.sprite = ObjectReferences.ButtonDonor.Image.sprite;
            // TODO: This produces a NullReferenceException. Investigate.
            //proxied.Text.font = ObjectReferences.ButtonDonor.Text.font;

            // Force an update or something. Idk.
            proxied.ForceUpdate();
        }
    }
}
