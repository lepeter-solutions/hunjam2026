using UnityEngine;
namespace HunJam
{
    // Own only runtime meshes, never destroy shared project assets.
    public sealed class PaperFragment : MonoBehaviour
    {
        public Mesh Surface { get; set; }
        public Mesh Hull { get; set; }
        private void OnDestroy() { if (Surface != null) Destroy(Surface); if (Hull != null) Destroy(Hull); }
    }
}
