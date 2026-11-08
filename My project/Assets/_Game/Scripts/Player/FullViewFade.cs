using UnityEngine;
using UnityEngine.Rendering;

namespace HunJam
{
    // Clip-space coverage is independent of Canvas size, eye separation and camera FOV.
    public sealed class FullViewFade : MonoBehaviour
    {
        private Mesh mesh;
        private Material material;
        private MeshRenderer overlayRenderer;

        private void Awake()
        {
            var shader = Resources.Load<Shader>("FullViewFade");
            if (shader == null)
            {
                Debug.LogError("Resources/FullViewFade shader is missing.", this);
                return;
            }
            material = new Material(shader);
            mesh = new Mesh { name = "Full View Fade Quad" };
            mesh.vertices = new[] { new Vector3(-1, -1, 0), new Vector3(1, -1, 0),
                new Vector3(-1, 1, 0), new Vector3(1, 1, 0) };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            // Shader writes clip coordinates; keep CPU frustum culling from rejecting it.
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            overlayRenderer = gameObject.AddComponent<MeshRenderer>();
            overlayRenderer.sharedMaterial = material;
            overlayRenderer.shadowCastingMode = ShadowCastingMode.Off;
            overlayRenderer.receiveShadows = false;
            overlayRenderer.allowOcclusionWhenDynamic = false;
            SetAlpha(1f);
        }

        public void ConfigureWhite()
        {
            if (material == null) return;
            material.SetColor("_FadeColor", Color.white);
            material.renderQueue = 5000;
            overlayRenderer.sortingOrder = short.MaxValue;
        }

        public void SetAlpha(float alpha)
        {
            if (material == null) return;
            float opacity = Mathf.Clamp01(alpha);
            material.SetFloat("_Alpha", opacity);
            overlayRenderer.enabled = opacity > 0f;
        }

        private void OnDestroy()
        {
            if (material != null) Destroy(material);
            if (mesh != null) Destroy(mesh);
        }
    }
}
