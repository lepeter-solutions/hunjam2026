using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace HunJam
{
    [DisallowMultipleComponent]
    public sealed class VRFadeManager : MonoBehaviour
    {
        public static VRFadeManager Instance { get; private set; }
        private FullViewFade overlay;
        private float opacity;
        public int RenderedScene { get; private set; } = -1;
        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(this); return; }
            Instance=this;
            DontDestroyOnLoad(transform.root.gameObject);
            var go=new GameObject("White full-view overlay");
            go.transform.SetParent(transform,false);
            overlay=go.AddComponent<FullViewFade>();
            overlay.ConfigureWhite();
            SetOpacity(0);
            RenderPipelineManager.beginCameraRendering+=BeforeCamera;
            RenderPipelineManager.endCameraRendering+=AfterCamera;
        }
        private bool IsView(Camera c) => c.cameraType==CameraType.Game && c.targetTexture==null &&
            (!UnityEngine.XR.XRSettings.isDeviceActive || c.stereoTargetEye!=StereoTargetEyeMask.None);
        private void BeforeCamera(ScriptableRenderContext context, Camera camera)
        {
            if (opacity<=0 || !IsView(camera)) return;
            overlay.transform.position=camera.transform.position;
            int mask=camera.cullingMask;
            for(int i=0;i<32;i++) if ((mask & (1<<i))!=0) { overlay.gameObject.layer=i; break; }
        }
        private void AfterCamera(ScriptableRenderContext context, Camera camera)
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (IsView(camera) && (camera.gameObject.scene==scene || camera==Camera.main))
                RenderedScene=scene.handle;
        }
        public void SetOpacity(float value) { opacity=Mathf.Clamp01(value); overlay.SetAlpha(opacity); }
        public IEnumerator FadeTo(float target,float seconds)
        {
            float start=opacity;
            for(float t=0;t<seconds;t+=Time.unscaledDeltaTime)
            { SetOpacity(Mathf.Lerp(start,target,t/seconds)); yield return null; }
            SetOpacity(target);
        }
        private void OnDestroy()
        {
            if(Instance!=this) return;
            RenderPipelineManager.beginCameraRendering-=BeforeCamera;
            RenderPipelineManager.endCameraRendering-=AfterCamera;
            Instance=null;
        }
    }
}
