using UnityEngine;

namespace HunJam
{
    // Session state, independent of the current scene. ResetStory starts a new run.
    public sealed class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (!instance) new GameObject("Story GameManager").AddComponent<GameManager>();
                return instance;
            }
        }
        [SerializeField] private bool decision1, decision2, decision3, decision4;
        public string NextScene { get; internal set; }
        public string CompletedScene { get; internal set; }
        public int SavedStatus { get; internal set; }
        public bool InTransition { get; internal set; }
        private void Awake()
        {
            if (instance && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        public int GetStatusIndex() => (decision1 ? 1 : 0) | (decision2 ? 2 : 0) | (decision3 ? 4 : 0) | (decision4 ? 8 : 0);
        public void SetDecision(int number, bool value)
        {
            if (InTransition) { Debug.LogWarning("Story: decisions are locked during transition."); return; }
            switch (number)
            {
                case 1: decision1=value; break;
                case 2: decision2=value; break;
                case 3: decision3=value; break;
                case 4: decision4=value; break;
                default: Debug.LogError("Story: decision number must be 1..4."); break;
            }
        }
        public void ResetStory()
        {
            if (InTransition) { Debug.LogWarning("Story: wait until the transition finishes before resetting."); return; }
            decision1=decision2=decision3=decision4=false;
            SavedStatus=0;
            NextScene=CompletedScene=null;
        }
        private void OnDestroy() { if (instance == this) instance=null; }
    }
}
