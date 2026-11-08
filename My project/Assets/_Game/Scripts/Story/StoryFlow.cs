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
            [Tooltip("One full panorama per status, in binary index order (0..15).")]
            public Texture2D[] maps = new Texture2D[16];
            public Texture2D Map(int index) => maps != null && index >= 0 && index < maps.Length ? maps[index] : null;
        }

        public string transitionScene = "VR_Transition";
        public string endScene = "EndScene";
        [Min(0)] public float fadeIn = .8f, hold = 5f, fadeOut = .8f;
        public bool debugLogging;
        public Stage[] stages;
        public int IndexOf(string scene) => stages == null ? -1 : Array.FindIndex(stages, s => s != null && s.scene == scene);
        public string Next(int i) => i + 1 < stages.Length ? stages[i+1]?.scene : endScene;
    }
}
