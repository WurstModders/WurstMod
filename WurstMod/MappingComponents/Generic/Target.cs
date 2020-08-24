using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WurstMod.MappingComponents.Generic
{
    /// <summary>
    /// A target like the ones in the friendly 45 range.
    /// You can assign a sound that plays on hit and define a range for random volume, pitch, and play speed.
    /// Additionally, you can include a Unity event for some advanced behaviour.
    /// </summary>
    public class Target : ComponentProxy
    {
        [Tooltip("Clip to be played when shot.")]
        public List<AudioClip> clips;

        [Tooltip("Range of volume for clip, chosen from randomly each time the target is shot.")]
        public Vector2 volumeRange = new Vector2(0.9f, 1.1f);

        [Tooltip("Range of pitch for clip, chosen from randomly each time the target is shot.")]
        public Vector2 pitchRange = new Vector2(0.97f, 1.03f);

        [Tooltip("Range of speed for clip, chosen from randomly each time the target is shot.")]
        public Vector2 speedRange = new Vector2(1f, 1f);

        [Tooltip("A Unity event, useful for triggering events that exist in base Unity code.")]
        public UnityEvent shotEvent;

        public override void InitializeComponent()
        {
            FistVR.ReactiveSteelTarget baseTarget = gameObject.AddComponent<FistVR.ReactiveSteelTarget>();
            baseTarget.HitEvent = new FistVR.AudioEvent();
            baseTarget.HitEvent.Clips = clips;
            baseTarget.HitEvent.VolumeRange = volumeRange;
            baseTarget.HitEvent.PitchRange = pitchRange;
            baseTarget.HitEvent.ClipLengthRange = speedRange;

            if (baseTarget.HitEvent.Clips.Count == 0)
            {
                baseTarget.HitEvent.Clips = new List<AudioClip>();
                baseTarget.HitEvent.Clips.Add(new AudioClip());
            }

            baseTarget.BulletHolePrefabs = new GameObject[0];
        }
    }
}
