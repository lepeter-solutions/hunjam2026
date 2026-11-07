using System;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;

public class KnifeSelfCutDetector : MonoBehaviour
{
    [Header("Effects")]
    [Tooltip("Blood particle system prefab to spawn on cut.")]
    [SerializeField] private ParticleSystem bloodParticlePrefab;

    [Tooltip("Parent the spawned effect to the cut hand bone so it moves with the hand.")]
    [SerializeField] private bool attachEffectToHand = true;

    [Tooltip("Destroy the spawned particle instance after this many seconds (0 = do not destroy).")]
    [SerializeField] private float destroyEffectAfterSeconds = 3f;

    [Header("Detection Settings")]
    [Tooltip("Layer name assigned to the player's hand colliders.")]
    [SerializeField] private string playerHandsLayerName = "PlayerHands";

    [Tooltip("Minimum time between cuts to prevent spawning multiple effects across finger capsules in one slash.")]
    [SerializeField] private float cutCooldown = 0.35f;

    private HandGrabInteractable[] _handGrabInteractables;
    private DistanceHandGrabInteractable[] _distanceHandGrabInteractables;
    private int _playerHandsLayer;
    private float _lastCutTime = -999f;

    private void Awake()
    {
        // Search self, parents, and children so this script works on either the Knife root or a Blade child collider
        Transform root = transform.root != null ? transform.root : transform;
        _handGrabInteractables = GetComponentsInChildren<HandGrabInteractable>(true);
        if (_handGrabInteractables.Length == 0)
            _handGrabInteractables = GetComponentsInParent<HandGrabInteractable>(true);

        _distanceHandGrabInteractables = GetComponentsInChildren<DistanceHandGrabInteractable>(true);
        if (_distanceHandGrabInteractables.Length == 0)
            _distanceHandGrabInteractables = GetComponentsInParent<DistanceHandGrabInteractable>(true);

        _playerHandsLayer = LayerMask.NameToLayer(playerHandsLayerName);
        if (_playerHandsLayer == -1)
        {
            Debug.LogError($"[{nameof(KnifeSelfCutDetector)}] Layer '{playerHandsLayerName}' does not exist in Project Settings > Tags and Layers.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CanProcessHit(other, out Handedness hitHand)) return;

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitNormal = (transform.position - hitPoint).sqrMagnitude > 0.0001f
            ? (transform.position - hitPoint).normalized
            : -transform.forward;

        SpawnBloodEffect(hitPoint, hitNormal, other.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!CanProcessHit(collision.collider, out Handedness hitHand)) return;

        ContactPoint contact = collision.GetContact(0);
        SpawnBloodEffect(contact.point, contact.normal, collision.collider.transform);
    }

    private bool CanProcessHit(Collider hitCollider, out Handedness hitHandedness)
    {
        hitHandedness = default;

        if (hitCollider == null || bloodParticlePrefab == null) return false;
        if (Time.time - _lastCutTime < cutCooldown) return false;

        // 1. Verify the collider is on the PlayerHands layer
        if (hitCollider.gameObject.layer != _playerHandsLayer) return false;

        // 2. Check which hand(s) are currently holding the knife
        bool leftHolding = false;
        bool rightHolding = false;
        GetHoldingHands(ref leftHolding, ref rightHolding);

        // Must be held by at least one hand
        if (!leftHolding && !rightHolding) return false;

        // 3. Identify which hand was hit
        if (!TryGetHandedness(hitCollider, out hitHandedness)) return false;

        // 4. Only trigger if the hit hand is NOT holding the knife and the OTHER hand IS holding it
        bool isOtherHandOnly =
            (hitHandedness == Handedness.Left && !leftHolding && rightHolding) ||
            (hitHandedness == Handedness.Right && !rightHolding && leftHolding);

        return isOtherHandOnly;
    }

    private void GetHoldingHands(ref bool leftHolding, ref bool rightHolding)
    {
        if (_handGrabInteractables != null)
        {
            foreach (HandGrabInteractable interactable in _handGrabInteractables)
            {
                if (interactable == null) continue;
                foreach (HandGrabInteractor interactor in interactable.SelectingInteractors)
                {
                    if (TryGetHandedness(interactor, out Handedness handedness))
                    {
                        if (handedness == Handedness.Left) leftHolding = true;
                        else if (handedness == Handedness.Right) rightHolding = true;
                    }
                }
            }
        }

        if (_distanceHandGrabInteractables != null)
        {
            foreach (DistanceHandGrabInteractable interactable in _distanceHandGrabInteractables)
            {
                if (interactable == null) continue;
                foreach (DistanceHandGrabInteractor interactor in interactable.SelectingInteractors)
                {
                    if (TryGetHandedness(interactor, out Handedness handedness))
                    {
                        if (handedness == Handedness.Left) leftHolding = true;
                        else if (handedness == Handedness.Right) rightHolding = true;
                    }
                }
            }
        }
    }

    private static bool TryGetHandedness(Component component, out Handedness handedness)
    {
        handedness = default;
        if (component == null) return false;

        // 1. Check IHand (HandRef, Hand, SyntheticHand) in parent chain
        IHand hand = component.GetComponentInParent<IHand>(true);
        if (hand != null)
        {
            handedness = hand.Handedness;
            return true;
        }

        // 2. Fallback: Check OVRSkeleton in parent chain
        OVRSkeleton skeleton = component.GetComponentInParent<OVRSkeleton>(true);
        if (skeleton != null)
        {
            if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.HandLeft)
            {
                handedness = Handedness.Left;
                return true;
            }
            if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.HandRight)
            {
                handedness = Handedness.Right;
                return true;
            }
        }

        // 3. Fallback: Check hierarchy names (e.g., LeftHandCapsules / RightHandCapsules)
        Transform current = component.transform;
        while (current != null)
        {
            if (current.name.IndexOf("Left", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                handedness = Handedness.Left;
                return true;
            }
            if (current.name.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                handedness = Handedness.Right;
                return true;
            }
            current = current.parent;
        }

        return false;
    }

    private void SpawnBloodEffect(Vector3 position, Vector3 normal, Transform hitTransform)
    {
        _lastCutTime = Time.time;

       // Quaternion rotation = normal.sqrMagnitude > 0.0001f
        //    ? Quaternion.LookRotation(normal)
       //     : Quaternion.identity;

        Transform parent = attachEffectToHand ? hitTransform : null;
        ParticleSystem instance = Instantiate(bloodParticlePrefab, position, bloodParticlePrefab.transform.rotation, parent);
        instance.Play(true);

        if (destroyEffectAfterSeconds > 0f)
        {
            Destroy(instance.gameObject, destroyEffectAfterSeconds);
        }
    }
}