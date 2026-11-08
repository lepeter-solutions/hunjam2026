using UnityEngine;

namespace HunJam
{
    // UnityEvent adapter: decisions and level completion can be wired independently.
    public sealed class StoryLevelExit : MonoBehaviour
    {
        [Range(1,4)] public int decisionNumber=1;
        public void SetDecision(bool value) => GameManager.Instance.SetDecision(decisionNumber,value);
        public void CompleteLevel() { Debug.Log("triggered");  TransitionManager.StartTransition(); }
        public void ResetStory() => GameManager.Instance.ResetStory();
    }
}
