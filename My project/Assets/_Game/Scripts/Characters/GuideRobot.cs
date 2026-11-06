using UnityEngine;

namespace HunJam
{
    public sealed class GuideRobot : DemoInteractable
    {
        [SerializeField] private DemoGame game;
        [SerializeField] private TextMesh dialogue;
        [SerializeField] private Transform head;
        public void Configure(DemoGame value) => game = value;
        public void SetPresentation(TextMesh text, Transform robotHead) { dialogue = text; head = robotHead; }
        public override void Interact()
        {
            if (game.Round.State != EnergyRound.Phase.Playing)
            {
                game.Begin();
                dialogue.text = "Hello, explorer!\nBring 3 orbs to the station.\nGrip to hold, release to deliver.";
            }
            else dialogue.text = $"You have delivered {game.Round.Count} / {game.Round.Target}.\nFind the glowing orbs\naround the courtyard!";
        }
        private void LateUpdate()
        {
            if (Camera.main == null) return;
            Vector3 direction = Camera.main.transform.position - head.position;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.01f)
                head.rotation = Quaternion.Slerp(head.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 3f);
        }
    }
}
