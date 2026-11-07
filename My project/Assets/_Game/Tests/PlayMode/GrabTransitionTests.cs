using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HunJam.Tests
{
    public sealed class GrabTransitionTests
    {
        private GameObject item, first, second;
        private Grabbable grab;
        [SetUp] public void Setup()
        {
            item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.transform.position = Vector3.up * 5;
            item.transform.localScale = new Vector3(0.4f, 0.1f, 0.1f);
            item.AddComponent<Rigidbody>().useGravity = false;
            grab = item.AddComponent<Grabbable>();
            first = new GameObject("Left test anchor"); second = new GameObject("Right test anchor");
            first.transform.position = item.transform.position + Vector3.left * 0.15f;
            second.transform.position = item.transform.position + Vector3.right * 0.15f;
        }
        [TearDown] public void Cleanup()
        { Object.DestroyImmediate(item); Object.DestroyImmediate(first); Object.DestroyImmediate(second); }
        [UnityTest] public IEnumerator TransferBothDirectionsAndReleaseContinuity()
        {
            foreach (bool reverse in new[] { false, true })
            {
                var a = reverse ? second.transform : first.transform;
                var b = reverse ? first.transform : second.transform;
                Assert.IsTrue(grab.Grab(a));
                Assert.IsTrue(grab.Grab(b));
                Assert.AreEqual(Grabbable.GrabState.TransferPending, grab.State);
                var p = item.transform.position; var r = item.transform.rotation;
                grab.Release(a);
                Assert.AreEqual(b, grab.Primary);
                Assert.IsTrue(item.GetComponent<Rigidbody>().isKinematic);
                yield return new WaitForFixedUpdate();
                Assert.That(Vector3.Distance(p, item.transform.position), Is.LessThan(0.001f));
                Assert.That(Quaternion.Angle(r, item.transform.rotation), Is.LessThan(0.1f));
                grab.Release(b);
                Assert.IsFalse(grab.IsHeld);
                Assert.IsFalse(item.GetComponent<Rigidbody>().isKinematic);
            }
        }
        [UnityTest] public IEnumerator TwoHandsRotateAndSurviveSecondaryReleaseAndCoincidence()
        {
            grab.ConfigureGrip(Grabbable.GripMode.TwoHands);
            Assert.IsTrue(grab.Grab(first.transform)); Assert.IsTrue(grab.Grab(second.transform));
            var scale = item.transform.localScale;
            second.transform.position = first.transform.position + Vector3.forward * 0.3f;
            yield return new WaitForFixedUpdate(); yield return null;
            Assert.That(Quaternion.Angle(Quaternion.identity, item.transform.rotation), Is.GreaterThan(30));
            var position = item.transform.position; var rotation = item.transform.rotation;
            grab.Release(second.transform);
            yield return new WaitForFixedUpdate(); yield return null;
            Assert.That(Vector3.Distance(position, item.transform.position), Is.LessThan(0.001f));
            Assert.That(Quaternion.Angle(rotation, item.transform.rotation), Is.LessThan(0.1f));
            second.transform.position = first.transform.position;
            Assert.IsTrue(grab.Grab(second.transform));
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(float.IsNaN(item.transform.rotation.x)); Assert.AreEqual(scale, item.transform.localScale);
            grab.Release(first.transform); grab.Release(second.transform);
            Assert.IsFalse(grab.IsHeld);
        }
        [UnityTest] public IEnumerator OwnershipRangeDisableAndTrackingLoss()
        {
            Assert.IsTrue(grab.Grab(first.transform));
            second.transform.position += Vector3.right * 3;
            Assert.IsFalse(grab.Grab(second.transform));
            second.transform.position = item.transform.position;
            Assert.IsTrue(grab.Grab(second.transform));
            Assert.IsFalse(grab.Grab(second.transform));
            first.SetActive(false);
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(second.transform, grab.Primary);
            Assert.IsNull(Grabbable.HeldBy(first.transform));
            item.SetActive(false);
            Assert.IsNull(Grabbable.HeldBy(second.transform));
            item.SetActive(true); first.SetActive(true);
            Assert.IsTrue(grab.Grab(first.transform));
            Object.Destroy(first); yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsFalse(grab.IsHeld);
        }
        [UnityTest] public IEnumerator FinalReleaseUsesMotionAndNoCrossObjectOwnership()
        {
            var other = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            other.AddComponent<Rigidbody>(); var otherGrab = other.AddComponent<Grabbable>();
            Assert.IsTrue(grab.Grab(first.transform));
            Assert.IsFalse(otherGrab.Grab(first.transform));
            first.transform.position += Vector3.forward * 0.08f;
            yield return new WaitForFixedUpdate();
            grab.Release(first.transform);
            Assert.That(item.GetComponent<Rigidbody>().linearVelocity.z, Is.GreaterThan(0.1f));
            Assert.IsTrue(otherGrab.Grab(first.transform));
            Object.Destroy(other);
        }
    }
}
