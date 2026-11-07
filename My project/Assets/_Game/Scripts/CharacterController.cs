using UnityEngine;

namespace HunJam
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField]
        private Transform start_pivot;

        [SerializeField]
        private Camera sceneCamera;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
           
        }

        // Update is called once per frame
        void Update()
        {
            if (OVRInput.Get(OVRInput.RawButton.X)) ResetPosition();
        }

        private void ResetPosition()
        {
            sceneCamera.transform.position = start_pivot.position;
        }
    }
}
