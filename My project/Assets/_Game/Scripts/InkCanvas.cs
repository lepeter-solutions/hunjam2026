using UnityEngine;

public class InkCanvas : MonoBehaviour
{
    public Camera inkCamera;

    void Start()
    {
        // 1. Clear the texture once to perfectly transparent
        inkCamera.clearFlags = CameraClearFlags.SolidColor;
        inkCamera.backgroundColor = new Color(0, 0, 0, 0);
        inkCamera.Render();

        // 2. Never clear it again, allowing the "ink" to build up permanently
        inkCamera.clearFlags = CameraClearFlags.Nothing;
    }
}