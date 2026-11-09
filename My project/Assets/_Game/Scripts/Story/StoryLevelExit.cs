using UnityEngine;

namespace HunJam
{
    // UnityEvent adapter: decisions and level completion can be wired independently.
    public sealed class StoryLevelExit : MonoBehaviour
    {
        [Range(1,4)] public int decisionNumber=1;
        public int waitTimeOnTrue = 2;
        public int waitTimeOnFalse = 0;

        private bool descisonValue;

        private bool decisionRecorded;

        public void SetDecision(bool value)
        {
            if (GameManager.Instance.InTransition || !MatchesStage()) return;
            GameManager.Instance.SetDecision(decisionNumber, value);
            descisonValue = value;
            decisionRecorded = true;
        }

        public void CompleteLevel()
        {
            if (GameManager.Instance.InTransition || !MatchesStage()) return;
            if (!decisionRecorded)
            {
                Debug.LogError("Story: call SetDecision before CompleteLevel, or use CompleteWithDecision(bool).", this);
                return;
            }
            TransitionManager.StartTransition(null, descisonValue ? waitTimeOnTrue : waitTimeOnFalse);
        }

        public void CompleteWithDecision(bool value)
        {
            SetDecision(value);
            CompleteLevel();
        }

        private bool MatchesStage()
        {
            var flow = Resources.Load<StoryFlow>("StoryFlow");
            int index = flow ? flow.IndexOf(gameObject.scene.name) : -1;
            if (index >= 0 && flow.stages[index].decisionNumber == decisionNumber) return true;
            Debug.LogError($"Story: exit in '{gameObject.scene.name}' has decision {decisionNumber}; it must match its StoryFlow stage.", this);
            return false;
        }

        public void ResetStory()
        {
            if (GameManager.Instance.InTransition) return;
            GameManager.Instance.ResetStory();
            decisionRecorded = false;
        }
    }
}
