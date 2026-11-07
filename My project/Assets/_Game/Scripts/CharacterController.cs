using UnityEngine;

namespace HunJam
{
    public class CharacterController : MonoBehaviour
    {
        [Tooltip("Drag your OVRCameraRig or Player Controller here")]
        public Transform playerRig;

        [Tooltip("Drag the empty GameObject acting as your teleport destination here")]
        public Transform destination;

        void Update()
        {
            // OVRInput.RawButton.A specifically targets the A button on the right Meta Quest controller
            if (OVRInput.GetDown(OVRInput.RawButton.A))
            {
                if (playerRig != null && destination != null)
                {
                    // Move the player to the destination
                    playerRig.position = destination.position;

                    // Optional: Snap the player's rotation to match the destination's rotation
                    playerRig.rotation = destination.rotation;
                }
                else
                {
                    Debug.LogWarning("Teleport failed: Player Rig or Destination is missing in the Inspector.");
                }
            }
        }
    }
}
