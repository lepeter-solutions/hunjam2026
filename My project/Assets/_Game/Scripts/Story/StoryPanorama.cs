using UnityEngine;
using UnityEngine.Rendering;

namespace HunJam
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public sealed class StoryPanorama : MonoBehaviour
    {
        [Min(.5f)] public float radius=5, height=3;
        public float centreHeight=1.6f, yaw;
        [Tooltip("Limit the map's horizontal field of view. Wide images shrink uniformly to fit.")]
        [Range(20,150)] public float maximumArcDegrees=100;
        [Tooltip("Place the map at eye height, in the initial viewing direction, then leave it fixed in the room.")]
        public bool alignToInitialView=true;
        [Min(0)] public float trackedPoseSettleTime=.2f;
        [Range(12,256)] public int segments=96;
        [Tooltip("Used only when opening this scene directly in the Editor.")]
        public string previewStage;
        [Range(0,15)] public int previewStatus;
        private Mesh mesh;
        private OVRCameraRig rig;
        private MeshRenderer surface;
        private float trackedSince=-1;
        public bool IsReady { get; private set; }

        private void Start()
        {
            if(IsReady) return;
            if(!alignToInitialView) { ShowMap(); return; }
            rig=FindFirstObjectByType<OVRCameraRig>();
            if(rig) rig.UpdatedAnchors+=CentreAtFirstPose;
            else
            {
                if(Camera.main) CentreAtView(Camera.main.transform);
                ShowMap();
            }
        }

        private void CentreAtFirstPose(OVRCameraRig source)
        {
            if(UnityEngine.XR.XRSettings.isDeviceActive)
            {
                var head=UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);
                if(!head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked,out bool tracked) || !tracked)
                { trackedSince=-1; return; }
                if(trackedSince<0) trackedSince=Time.unscaledTime;
                if(Time.unscaledTime-trackedSince<Mathf.Max(0,trackedPoseSettleTime)) return;
            }
            if(!source.centerEyeAnchor) return;
            CentreAtView(source.centerEyeAnchor);
            source.UpdatedAnchors-=CentreAtFirstPose;
            ShowMap();
        }

        private void CentreAtView(Transform eye)
        {
            Vector3 forward=Vector3.ProjectOnPlane(eye.forward,Vector3.up);
            if(forward.sqrMagnitude>.0001f)
                transform.rotation=Quaternion.LookRotation(forward,Vector3.up);
            transform.position=eye.position-Vector3.up*centreHeight;
        }

        private void ShowMap()
        {
            surface.enabled=mesh!=null;
            IsReady=true;
        }

        private void Awake()
        {
            var flow=Resources.Load<StoryFlow>("StoryFlow");
            var state=GameManager.Instance;
            int stage=flow ? flow.IndexOf(state.CompletedScene ?? previewStage) : -1;
            int status=state.CompletedScene!=null ? state.SavedStatus : previewStatus;
            // EndScene holds the last chosen branch map instead of replacing it
            // with a separately configured (previously placeholder) ending image.
            Texture2D image=stage>=0 ? flow.stages[stage].Map(status) : null;
            surface=GetComponent<MeshRenderer>();
            surface.enabled=false;
            if(!image || !surface.sharedMaterial)
            {
                IsReady=true; // Allow the fade to reveal the logged white fallback.
                Debug.LogError($"Story: missing panorama/material for '{state.CompletedScene ?? previewStage}', status {status:D2}; showing white.",this);
                return;
            }
            var properties=new MaterialPropertyBlock();
            properties.SetTexture("_BaseMap",image);
            surface.SetPropertyBlock(properties);
            surface.shadowCastingMode=ShadowCastingMode.Off;
            surface.receiveShadows=false;
            float safeRadius=Mathf.Max(.5f,radius);
            float aspect=image.width/(float)Mathf.Max(1,image.height);
            float arcLimit=Mathf.Clamp(maximumArcDegrees,20,150)*Mathf.Deg2Rad;
            float displayHeight=Mathf.Min(Mathf.Max(.5f,height),safeRadius*arcLimit/aspect);
            // Arc length / height equals the source texture's aspect ratio.
            // The complete 0..1 image fits once; no cropping, tiling or 360-degree stretch.
            float arc=displayHeight*aspect/safeRadius;
            int n=Mathf.Clamp(segments,12,256);
            var vertices=new Vector3[(n+1)*2]; var uv=new Vector2[vertices.Length]; var faces=new int[n*6];
            for(int i=0;i<=n;i++)
            {
                float u=i/(float)n, angle=(u-.5f)*arc+yaw*Mathf.Deg2Rad;
                Vector3 mid=new Vector3(Mathf.Sin(angle)*safeRadius,centreHeight,Mathf.Cos(angle)*safeRadius);
                int k=i*2;
                vertices[k]=mid-Vector3.up*displayHeight*.5f; vertices[k+1]=mid+Vector3.up*displayHeight*.5f;
                uv[k]=new Vector2(u,0); uv[k+1]=new Vector2(u,1);
                if(i==n) continue;
                int j=i*6; faces[j]=k;faces[j+1]=k+1;faces[j+2]=k+2;
                faces[j+3]=k+2;faces[j+4]=k+1;faces[j+5]=k+3;
            }
            mesh=new Mesh {name="Aspect-preserved story map",vertices=vertices,uv=uv,triangles=faces};
            mesh.RecalculateBounds(); GetComponent<MeshFilter>().sharedMesh=mesh;
        }
        private void OnDestroy()
        {
            if(rig) rig.UpdatedAnchors-=CentreAtFirstPose;
            if(mesh) Destroy(mesh);
        }
    }
}
