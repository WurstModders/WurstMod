using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace WurstMod.Any
{
    /// <summary>
    /// A target like the ones in the friendly 45 range.
    /// You can assign a sound that plays on hit and define a range for random volume, pitch, and play speed.
    /// Additionally, you can include a Unity event for some advanced behaviour.
    /// </summary>
    class Target : MonoBehaviour
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
    }
}
