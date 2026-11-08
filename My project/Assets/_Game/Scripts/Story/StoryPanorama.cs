using UnityEngine;
using UnityEngine.Rendering;

namespace HunJam
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public sealed class StoryPanorama : MonoBehaviour
    {
        [Min(.5f)] public float radius=5, height=3;
        public float centreHeight=1.6f, yaw;
        [Range(12,256)] public int segments=96;
        [Tooltip("Used only when opening this scene directly in the Editor.")]
        public string previewStage;
        [Range(0,15)] public int previewStatus;
        private Mesh mesh;
        private OVRCameraRig rig;

        private void Start()
        {
            rig=FindFirstObjectByType<OVRCameraRig>();
            if(rig) rig.UpdatedAnchors+=CentreAtFirstPose;
        }

        private void CentreAtFirstPose(OVRCameraRig source)
        {
            if(UnityEngine.XR.XRSettings.isDeviceActive)
            {
                var head=UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);
                if(!head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked,out bool tracked) || !tracked) return;
            }
            var eye=source.centerEyeAnchor.position;
            transform.position=new Vector3(eye.x,transform.position.y,eye.z);
            source.UpdatedAnchors-=CentreAtFirstPose;
        }

        private void Awake()
        {
            var flow=Resources.Load<StoryFlow>("StoryFlow");
            var state=GameManager.Instance;
            int stage=flow ? flow.IndexOf(state.CompletedScene ?? previewStage) : -1;
            int status=state.CompletedScene!=null ? state.SavedStatus : previewStatus;
            Texture2D image=stage>=0 ? flow.stages[stage].Map(status) : null;
            var surface=GetComponent<MeshRenderer>();
            if(!image || !surface.sharedMaterial)
            {
                surface.enabled=false;
                Debug.LogError($"Story: missing panorama/material for '{state.CompletedScene ?? previewStage}', status {status:D2}; showing white.",this);
                return;
            }
            var properties=new MaterialPropertyBlock();
            properties.SetTexture("_BaseMap",image);
            surface.SetPropertyBlock(properties);
            surface.shadowCastingMode=ShadowCastingMode.Off;
            surface.receiveShadows=false;
            int n=Mathf.Clamp(segments,12,256);
            var vertices=new Vector3[(n+1)*2]; var uv=new Vector2[vertices.Length]; var faces=new int[n*6];
            for(int i=0;i<=n;i++)
            {
                float u=i/(float)n, angle=((u-.5f)*360+yaw)*Mathf.Deg2Rad;
                Vector3 mid=new Vector3(Mathf.Sin(angle)*radius,centreHeight,Mathf.Cos(angle)*radius);
                int k=i*2;
                vertices[k]=mid-Vector3.up*height*.5f; vertices[k+1]=mid+Vector3.up*height*.5f;
                uv[k]=new Vector2(u,0); uv[k+1]=new Vector2(u,1);
                if(i==n) continue;
                int j=i*6; faces[j]=k;faces[j+1]=k+1;faces[j+2]=k+2;
                faces[j+3]=k+2;faces[j+4]=k+1;faces[j+5]=k+3;
            }
            mesh=new Mesh {name="Inside panorama",vertices=vertices,uv=uv,triangles=faces};
            mesh.RecalculateBounds(); GetComponent<MeshFilter>().sharedMesh=mesh;
        }
        private void OnDestroy()
        {
            if(rig) rig.UpdatedAnchors-=CentreAtFirstPose;
            if(mesh) Destroy(mesh);
        }
    }
}
