using UnityEngine;

namespace HunJam
{
    public class ClearRender : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public RenderTexture renderTexture1;

        public void Awake()
        {
            ClearOutRenderTexture(renderTexture1);
        }

        public void ClearOutRenderTexture(RenderTexture renderTexture)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
