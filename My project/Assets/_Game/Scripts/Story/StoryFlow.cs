using System;
using UnityEngine;

namespace HunJam
{
    [CreateAssetMenu(menuName = "HunJam/Story flow")]
    public sealed class StoryFlow : ScriptableObject
    {
        [Serializable] public sealed class Stage
        {
            public string scene;
            [Range(1,4)] public int decisionNumber = 1;
            public Texture2D falseImage, trueImage;
            public Texture2D Map(int status) => (status & (1 << (decisionNumber-1))) != 0 ? trueImage : falseImage;
        }

        public string transitionScene = "VR_Transition";
        public string endScene = "EndScene";
        [Min(0)] public float fadeIn = .8f, hold = 5f, fadeOut = .8f;
        public bool debugLogging;
        public Stage[] stages;
        [Tooltip("EndScene only. Indexed by all four decisions (0..15).")]
        public Texture2D[] endings = new Texture2D[16];
        public Texture2D Ending(int index) => endings != null && index >= 0 && index < endings.Length ? endings[index] : null;
        public int IndexOf(string scene) => stages == null ? -1 : Array.FindIndex(stages, s => s != null && s.scene == scene);
        public string Next(int i) => i + 1 < stages.Length ? stages[i+1]?.scene : endScene;
    }
}
