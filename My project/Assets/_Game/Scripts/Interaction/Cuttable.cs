using UnityEngine;
namespace HunJam
{
    [RequireComponent(typeof(BoxCollider), typeof(MeshRenderer))]
    public sealed class Cuttable : MonoBehaviour
    {
        private readonly GameObject[] pieces = new GameObject[2];
        public bool IsCut { get; private set; }
        public void Cut(Vector3 velocity)
        {
            if (IsCut || velocity.magnitude < 1f) return;
            IsCut = true;
            GetComponent<Collider>().enabled = false;
            GetComponent<Renderer>().enabled = false;
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1 : 1;
                var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                piece.name = name + " half " + i;
                piece.transform.SetPositionAndRotation(transform.TransformPoint(new Vector3(side * 0.26f, 0, 0)), transform.rotation);
                piece.transform.localScale = Vector3.Scale(transform.lossyScale, new Vector3(0.48f, 1, 1));
                piece.GetComponent<Renderer>().sharedMaterial = GetComponent<Renderer>().sharedMaterial;
                var body = piece.AddComponent<Rigidbody>();
                body.mass = 0.15f;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.linearVelocity = Vector3.ClampMagnitude(velocity, 2) + transform.right * side * 0.3f;
                piece.AddComponent<Grabbable>();
                pieces[i] = piece;
            }
        }
        public void ResetTarget()
        {
            foreach (var piece in pieces) if (piece != null) Destroy(piece);
            IsCut = false;
            GetComponent<Collider>().enabled = true;
            GetComponent<Renderer>().enabled = true;
        }
        private void OnDestroy()
        { foreach (var piece in pieces) if (piece != null) Destroy(piece); }
    }
}
