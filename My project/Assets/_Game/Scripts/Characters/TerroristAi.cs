using UnityEngine;
using UnityEngine.UI;

namespace HunJam
{
    [RequireComponent(typeof(Rigidbody))]
    public class TerroristAi : MonoBehaviour
    {
        public StoryLevelExit sle;

        [SerializeField]
        private float speed = 2;

        [SerializeField]
        private float restartDelay = 3;

        [SerializeField]
        private Image fade;

        [SerializeField]
        private AudioSource playerAudioSource;

        [SerializeField]
        private AudioSource musicAudioSource;

        [SerializeField]
        private AudioClip deathAudioClip;

        [SerializeField] private FlightVehicle playerAircraft;
        [Tooltip("Optional epilogue scene name from the build scene list.")]
        [SerializeField] private string winSceneName;
        private Rigidbody body;
        private bool gameOver;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (playerAircraft == null) playerAircraft = FindFirstObjectByType<FlightVehicle>();
            PrepareBuildingColliders();
            EnsureCollisionShape();
            // Kinematic vs static does not produce the collision callbacks used here.
            body.isKinematic = false;
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void PrepareBuildingColliders()
        {
            // Imported building roots have no MeshFilter. Put each static collision
            // mesh on the child that owns its geometry so transforms remain correct.
            foreach (var empty in FindObjectsByType<MeshCollider>(FindObjectsSortMode.None))
            {
                if (empty.gameObject.scene != gameObject.scene || !empty.enabled || empty.isTrigger
                    || empty.sharedMesh != null || empty.attachedRigidbody != null) continue;
                bool found = false;
                foreach (var filter in empty.GetComponentsInChildren<MeshFilter>())
                {
                    if (filter.sharedMesh == null || filter.GetComponentInParent<Rigidbody>() != null) continue;
                    var existing = filter.GetComponent<Collider>();
                    if (existing != null && existing != empty && existing.enabled && !existing.isTrigger
                        && (!(existing is MeshCollider existingMesh) || existingMesh.sharedMesh != null))
                    { found = true; continue; }
                    var shape = filter.GetComponent<MeshCollider>();
                    if (shape == null) shape = filter.gameObject.AddComponent<MeshCollider>();
                    if (shape.sharedMesh == null)
                    {
                        shape.sharedMesh = filter.sharedMesh;
                        shape.sharedMaterial = empty.sharedMaterial;
                        shape.convex = false;
                        shape.isTrigger = false;
                        shape.enabled = true;
                    }
                    found = true;
                }
                if (found && empty.sharedMesh == null) empty.enabled = false;
                if (!found) Debug.LogWarning("Empty building MeshCollider has no static child meshes: " + empty.name, empty);
            }
        }

        private void EnsureCollisionShape()
        {
            bool hasShape = false;
            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                if (collider.attachedRigidbody != body || !collider.enabled || collider.isTrigger) continue;
                if (collider is MeshCollider mesh && (mesh.sharedMesh == null || !mesh.convex))
                {
                    collider.enabled = false;
                    continue;
                }
                hasShape = true;
            }
            if (hasShape) return;

            // The imported model's root has an empty MeshCollider. Fit a primitive
            // to its visible mesh bounds instead of relying on that empty component.
            Bounds bounds = default;
            bool found = false;
            foreach (var filter in GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null) continue;
                Bounds meshBounds = filter.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = meshBounds.center + Vector3.Scale(meshBounds.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 point = transform.InverseTransformPoint(filter.transform.TransformPoint(corner));
                    if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; }
                    else bounds.Encapsulate(point);
                }
            }
            if (!found)
            {
                Debug.LogError("Enemy aircraft needs a non-trigger primitive or convex collider.", this);
                return;
            }
            var box = gameObject.AddComponent<BoxCollider>();
            box.center = bounds.center;
            box.size = Vector3.Max(bounds.size, Vector3.one * 0.01f);
        }

        private void FixedUpdate()
        {
            if (!gameOver) body.linearVelocity = transform.forward * speed;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.GetComponentInParent<FlightVehicle>() != null)
            {
                BeginGameOver(true);
            }
            else
            {
                // Building colliders can live on model children, not the named root.
                for (Transform hit = collision.collider.transform; hit != null; hit = hit.parent)
                    if (hit.name == "Twins") { BeginGameOver(false); break; }
            }
        }

        public void BeginGameOver(bool win) => GameOver(win);

        public void GameOver(bool win)
        {
            if (gameOver || GameManager.Instance.InTransition) return;
            if (sle == null)
            {
                Debug.LogError("Story: assign Manhattan's decision-4 StoryLevelExit to TerroristAi.", this);
                return;
            }

            sle.CompleteWithDecision(win);
            if (!GameManager.Instance.InTransition) return;
            gameOver = true;
            body.linearVelocity = body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            if (playerAircraft != null) playerAircraft.StopForStoryTransition();
            if (musicAudioSource != null) musicAudioSource.Stop();
            if (playerAudioSource != null)
            {
                playerAudioSource.Stop();
                playerAudioSource.pitch = 1;
                playerAudioSource.clip = deathAudioClip;
                playerAudioSource.loop = false;
                if (deathAudioClip != null) playerAudioSource.Play();
            }
        }
    }
}
