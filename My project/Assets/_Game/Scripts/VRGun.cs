using System;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Events;

namespace HunJam
{
    public class VRGun : MonoBehaviour
    {
        [Header("References")]
        public Transform muzzleTransform;
        public Collider playerCollider;
        public Transform enemyRoot;
        public Transform playerRig;
        public ParticleSystem muzzleFlash;
        [Header("Settings")]
        public string enemyTag = "enemy";
        public float maxDistance = 50f;
        public LayerMask hitLayers = ~0;
        [Header("Controller Trigger Input")]
        public bool useControllerTrigger = true;
        public OVRInput.RawButton triggerButton = OVRInput.RawButton.RIndexTrigger | OVRInput.RawButton.LIndexTrigger;
        public UnityEvent onFired;
        public UnityEvent onShotSelf;
        public UnityEvent onShotEnemy;
        public bool isGrabbed;
        private Oculus.Interaction.Grabbable grabbable;
        private HandGrabInteractable[] handGrabs;
        private bool resolved;

        private void Awake()
        {
            if (!muzzleTransform) muzzleTransform = transform;
            grabbable = GetComponent<Oculus.Interaction.Grabbable>();
            handGrabs = GetComponentsInChildren<HandGrabInteractable>(true);
        }

        private void Update()
        {
            if (grabbable) isGrabbed = grabbable.GrabPoints.Count > 0;
            if (!isGrabbed || !useControllerTrigger) return;
            // Only the hand holding the gun may fire it. Other-hand trigger presses are ignored.
            OVRInput.RawButton buttons = 0;
            foreach (var grab in handGrabs)
                foreach (var interactor in grab.SelectingInteractors)
                    if (interactor.Hand != null)
                        buttons |= interactor.Hand.Handedness == Handedness.Left
                            ? OVRInput.RawButton.LIndexTrigger : OVRInput.RawButton.RIndexTrigger;
            if (buttons == 0 && handGrabs.Length == 0) buttons = triggerButton;
            if (OVRInput.GetDown(buttons & triggerButton)) Fire();
        }

        public void Fire()
        {
            if (!isGrabbed || resolved || GameManager.Instance.InTransition) return;
            onFired?.Invoke();
            if (muzzleFlash)
            {
                var flash = Instantiate(muzzleFlash, muzzleTransform.position, muzzleTransform.rotation);
                flash.Play(true);
                Destroy(flash.gameObject, 5f);
            }
            Physics.SyncTransforms();
            Vector3 origin = muzzleTransform.position;
            // Physics.Raycast cannot report a collider when the ray starts inside it.
            if (playerCollider && playerCollider.enabled &&
                (playerCollider.ClosestPoint(origin) - origin).sqrMagnitude < 0.000001f)
            {
                Resolve(true);
                return;
            }
            var hits = Physics.RaycastAll(origin, muzzleTransform.forward, Mathf.Max(0, maxDistance),
                hitLayers, QueryTriggerInteraction.Collide);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                var target = hit.collider;
                if (target.transform.IsChildOf(transform)) continue;
                if (target == playerCollider) { Resolve(true); return; }
                if (playerRig && target.transform.IsChildOf(playerRig)) continue;
                bool enemy = enemyRoot ? target.transform.IsChildOf(enemyRoot) : target.CompareTag(enemyTag);
                if (enemy) { Resolve(false); return; }
                if (target.isTrigger) continue; // Meta grab volumes must not stop bullets.
                return; // The nearest solid wall/prop blocks the shot.
            }
        }

        private void Resolve(bool self)
        {
            resolved = true;
            if (self) onShotSelf?.Invoke();
            else onShotEnemy?.Invoke();
        }
        public void OnGrab() => isGrabbed = true;
        public void OnRelease() => isGrabbed = false;
        private void OnDrawGizmosSelected()
        {
            var origin = muzzleTransform ? muzzleTransform : transform;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(origin.position, origin.forward * 2f);
        }
    }
}
