using System;
using UnityEngine;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;

public class KnifeSelfCutDetector : MonoBehaviour
{
    [Header("Effects")]
    [Tooltip("Blood particle system prefab to spawn on cut.")]
    [SerializeField] private ParticleSystem bloodParticlePrefab;

    [Tooltip("Follow the cut hand's position without inheriting its rotation.")]
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
        if (!CanProcessHit(other, out _)) return;

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitNormal = (transform.position - hitPoint).sqrMagnitude > 0.0001f
            ? (transform.position - hitPoint).normalized
            : Vector3.down;

        SpawnBloodEffect(hitPoint, hitNormal, other.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!CanProcessHit(collision.collider, out _)) return;

        ContactPoint contact = collision.GetContact(0);
        SpawnBloodEffect(contact.point, contact.normal, collision.collider.transform);
    }

    private bool CanProcessHit(Collider hitCollider, out Handedness hitHandedness)
    {
        hitHandedness = default;

        if (hitCollider == null || bloodParticlePrefab == null) return false;
        if (Time.time - _lastCutTime < cutCooldown) return false;
        if (hitCollider.gameObject.layer != _playerHandsLayer) return false;

        bool leftHolding = false;
        bool rightHolding = false;
        GetHoldingHands(ref leftHolding, ref rightHolding);

        if (!leftHolding && !rightHolding) return false;
        if (!TryGetHandedness(hitCollider, out hitHandedness)) return false;

        return (hitHandedness == Handedness.Left && !leftHolding && rightHolding) ||
               (hitHandedness == Handedness.Right && !rightHolding && leftHolding);
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

        IHand hand = component.GetComponentInParent<IHand>(true);
        if (hand != null)
        {
            handedness = hand.Handedness;
            return true;
        }

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

        Quaternion rotation = normal.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(normal)
            : Quaternion.identity;

        // Spawn unparented so hand rotation never rotates the emitter
        ParticleSystem instance = Instantiate(bloodParticlePrefab, position, rotation);

        // Force World simulation space so already-fallen particles never move or rotate with the hand
        foreach (ParticleSystem ps in instance.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        }

        // Follow the cut spot's position without inheriting the hand's rotation
        if (attachEffectToHand && hitTransform != null)
        {
            instance.gameObject.AddComponent<FollowPositionIgnoreRotation>().Initialize(hitTransform, position);
        }

        instance.Play(true);

        if (destroyEffectAfterSeconds > 0f)
        {
            Destroy(instance.gameObject, destroyEffectAfterSeconds);
        }
    }
}

public class FollowPositionIgnoreRotation : MonoBehaviour
{
    private Transform _target;
    private Vector3 _localOffset;

    public void Initialize(Transform target, Vector3 worldHitPosition)
    {
        _target = target;
        _localOffset = target.InverseTransformPoint(worldHitPosition);
    }

    private void LateUpdate()
    {
        if (_target == null) return;
        transform.position = _target.TransformPoint(_localOffset);
    }
}