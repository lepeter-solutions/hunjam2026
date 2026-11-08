using System;
using UnityEngine;

namespace HunJam
{
    [CreateAssetMenu(menuName = "HunJam/Story flow")]
    public sealed class StoryFlow : ScriptableObject
    {
        public enum Picture { A, B }
        [Serializable] public struct Branch
        {
            public Picture ifFalse, ifTrue;
        }
        [Serializable] public sealed class Stage
        {
            public string scene;
            [Range(1,4)] public int decisionNumber = 1;
            public Texture2D imageA, imageB;
            [Tooltip("One node per previous decision history. In each node choose A or B for the current decision.")]
            public Branch[] branches;
            public Texture2D Map(int status)
            {
                if (decisionNumber < 1 || decisionNumber > 4) return null;
                int bit = 1 << (decisionNumber - 1);
                int node = status & (bit - 1);
                if (branches == null || node >= branches.Length)
                {
                    Debug.LogError($"Story: missing branch {node} in '{scene}'. Configure {bit} nodes.");
                    return null;
                }
                var branch = branches[node];
                var picture = (status & bit) == 0 ? branch.ifFalse : branch.ifTrue;
                return picture == Picture.A ? imageA : imageB;
            }
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
