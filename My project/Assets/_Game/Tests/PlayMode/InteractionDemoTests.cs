using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace HunJam.Tests
{
    public sealed class InteractionDemoTests
    {
        private static Transform Anchor(string name, Vector3 position)
        { var obj = new GameObject(name); obj.transform.position = position; return obj.transform; }
        [UnityTest] public IEnumerator PaperRejectsWeakPullThenSplitsIntoOwnedFragmentsAndResets()
        {
            yield return SceneManager.LoadSceneAsync("VRInteractionTest"); yield return null;
            var paper = Object.FindFirstObjectByType<Tearable>(); var grab = paper.GetComponent<Grabbable>();
            var body = paper.GetComponent<Rigidbody>(); body.interpolation = RigidbodyInterpolation.None;
            paper.transform.SetPositionAndRotation(Vector3.up * 3, Quaternion.identity);
            body.position = Vector3.up * 3; body.rotation = Quaternion.identity;
            Physics.SyncTransforms();
            var a = Anchor("Paper left", body.position + Vector3.left * 0.16f);
            var b = Anchor("Paper right", body.position + Vector3.right * 0.16f);
            Assert.IsTrue(grab.Grab(a));
            a.position += Vector3.left * 0.08f; yield return new WaitForFixedUpdate();
            Assert.IsFalse(paper.IsTorn, "One hand cannot tear");
            b.position = body.position + Vector3.right * 0.16f;
            Assert.IsTrue(grab.Grab(b));
            yield return new WaitForFixedUpdate();
            a.position += Vector3.left * 0.015f; b.position += Vector3.right * 0.015f;
            yield return new WaitForFixedUpdate(); Assert.IsFalse(paper.IsTorn, "Weak pull cannot tear");
            for (int i = 0; i < 8 && !paper.IsTorn; i++)
            { a.position += Vector3.left * 0.015f; b.position += Vector3.right * 0.015f; yield return new WaitForFixedUpdate(); }
            Assert.IsTrue(paper.IsTorn, $"Tension {paper.Tension}, force {paper.EstimatedForce}, state {grab.State}, contacts {grab.PrimaryContact} / {grab.SecondaryContact}");
            var left = Grabbable.HeldBy(a); var right = Grabbable.HeldBy(b);
            Assert.IsNotNull(left); Assert.IsNotNull(right); Assert.AreNotEqual(left, right);
            Assert.IsTrue(left.GetComponent<Rigidbody>().isKinematic); Assert.IsTrue(right.GetComponent<Rigidbody>().isKinematic);
            Assert.IsNull(left.GetComponent<Tearable>()); Assert.IsNull(right.GetComponent<Tearable>());
            Assert.IsFalse(grab.enabled); Assert.IsFalse(paper.GetComponent<Collider>().enabled);
            left.Release(a); Assert.IsFalse(left.GetComponent<Rigidbody>().isKinematic); Assert.IsTrue(right.IsHeld);
            //paper.ResetPaper(); yield return null;
            Assert.IsFalse(paper.IsTorn); Assert.IsTrue(grab.enabled); Assert.IsNull(Grabbable.HeldBy(b));
            Object.Destroy(a.gameObject); Object.Destroy(b.gameObject);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator WrongPaperRegionsAndNormalObjectsDoNotTear()
        {
            yield return SceneManager.LoadSceneAsync("VRInteractionTest"); yield return null;
            var paper = Object.FindFirstObjectByType<Tearable>(); var grab = paper.GetComponent<Grabbable>();
            var body = paper.GetComponent<Rigidbody>(); body.interpolation = RigidbodyInterpolation.None;
            paper.transform.SetPositionAndRotation(Vector3.up * 3, Quaternion.identity);
            body.position = Vector3.up * 3; body.rotation = Quaternion.identity; Physics.SyncTransforms();
            var a = Anchor("Same side A", body.position + new Vector3(0.1f, -0.09f, 0));
            var b = Anchor("Same side B", body.position + new Vector3(0.1f, 0.09f, 0));
            Assert.IsTrue(grab.Grab(a)); Assert.IsTrue(grab.Grab(b));
            for (int i = 0; i < 6; i++) { a.position += Vector3.down * 0.025f; b.position += Vector3.up * 0.025f; yield return new WaitForFixedUpdate(); }
            Assert.IsFalse(paper.IsTorn);
            var references = Object.FindObjectsByType<Grabbable>(FindObjectsSortMode.None);
            Assert.That(System.Array.FindAll(references, item => item.GetComponent<Tearable>() == null).Length, Is.GreaterThanOrEqualTo(4));
            Object.Destroy(a.gameObject); Object.Destroy(b.gameObject);
        }
        [UnityTest] public IEnumerator StabDirectionDepthWithdrawalAndReset()
        {
            yield return SceneManager.LoadSceneAsync("VRInteractionTest"); yield return null;
            var tool = Object.FindFirstObjectByType<StabbingTool>(); var target = Object.FindFirstObjectByType<StabTarget>();
            target.transform.position = new Vector3(0, 3, 0.4f);
            var body = tool.GetComponent<Rigidbody>(); body.interpolation = RigidbodyInterpolation.None;
            tool.transform.SetPositionAndRotation(new Vector3(0, 3, 0), Quaternion.identity);
            body.position = new Vector3(0, 3, 0); body.rotation = Quaternion.identity; Physics.SyncTransforms();
            var hand = Anchor("Stab hand", body.position); Assert.IsTrue(tool.Grab(hand));
            yield return new WaitForSeconds(0.2f);
            Assert.IsFalse(tool.TryInsert(target, target.transform.position, Vector3.back, Vector3.right * 2));
            Assert.IsFalse(tool.TryInsert(target, target.transform.position, Vector3.back, Vector3.forward * 0.1f));
            Assert.IsFalse(tool.TryInsert(null, target.transform.position, Vector3.back, Vector3.forward * 2));
            for (int i = 0; i < 8 && !tool.IsInserted; i++)
            { hand.position += Vector3.forward * 0.04f; yield return new WaitForFixedUpdate(); yield return null; }
            Assert.IsTrue(tool.IsInserted, "Real tip sweep must enter the designated foam");
            for (int i = 0; i < 6; i++) { hand.position += Vector3.forward * 0.04f; yield return new WaitForFixedUpdate(); }
            Assert.That(tool.Depth, Is.InRange(0.001f, target.MaximumDepth));
            for (int i = 0; i < 12 && tool.IsInserted; i++) { hand.position -= Vector3.forward * 0.04f; yield return new WaitForFixedUpdate(); }
            Assert.IsFalse(tool.IsInserted);
            Object.FindFirstObjectByType<WorkshopReset>().Interact(); Assert.IsFalse(tool.IsHeld);
            Object.Destroy(hand.gameObject); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator FlightActuallyMovesYokeIsBoundedAndTransfersWithoutJump()
        {
            yield return SceneManager.LoadSceneAsync("FlightYoke"); yield return null;
            var vehicle = Object.FindFirstObjectByType<FlightVehicle>(); var yoke = Object.FindFirstObjectByType<FlightYoke>();
            var player = Object.FindFirstObjectByType<DemoPlayer>();
            Assert.AreEqual(vehicle.transform, player.transform.parent);
            Assert.That(Vector3.Distance(player.View.transform.position, vehicle.transform.TransformPoint(new Vector3(0, 1.2f, 0))), Is.LessThan(0.1f));
            var start = vehicle.transform.position; yield return new WaitForSeconds(0.3f);
            Assert.That(Vector3.Distance(start, vehicle.transform.position), Is.GreaterThan(2), $"Speed {vehicle.Speed}, resets {vehicle.ResetCount}, position {vehicle.transform.position}, kinematic {vehicle.GetComponent<Rigidbody>().isKinematic}");
            var a = Anchor("Yoke right", yoke.transform.TransformPoint(new Vector3(0.21f, 0.04f, 0)));
            a.SetParent(vehicle.transform, true);
            Assert.IsTrue(yoke.Grab(a));
            for (int i = 0; i < 8; i++) { a.localPosition += new Vector3(0, -0.025f, -0.018f); yield return new WaitForFixedUpdate(); }
            yield return new WaitForSeconds(0.2f);
            Assert.That(yoke.Roll, Is.GreaterThan(0.2f)); Assert.That(yoke.Pitch, Is.GreaterThan(0.2f));
            Assert.That(Mathf.Abs(yoke.Angle), Is.LessThanOrEqualTo(40.01f)); Assert.That(Mathf.Abs(yoke.Travel), Is.LessThanOrEqualTo(0.0901f));
            var b = Anchor("Yoke left", yoke.transform.TransformPoint(new Vector3(-0.21f, 0.04f, 0))); b.SetParent(vehicle.transform, true);
            Assert.IsTrue(yoke.Grab(b)); float before = yoke.Angle; yoke.Release(a); yield return new WaitForFixedUpdate();
            Assert.That(Mathf.Abs(before - yoke.Angle), Is.LessThan(1));
            yoke.Release(b); Assert.IsTrue(yoke.GetComponent<Rigidbody>().isKinematic);
            yield return new WaitForSeconds(1); Assert.That(Mathf.Abs(yoke.Roll), Is.LessThan(0.05f));
            Assert.That(vehicle.transform.forward.y, Is.GreaterThan(0.005f), "Pull must command nose up");
            vehicle.ResetFlight(); Assert.That(vehicle.ResetCount, Is.GreaterThan(0)); Assert.IsFalse(yoke.IsHeld);
            Object.Destroy(a.gameObject); Object.Destroy(b.gameObject); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator LeftHandlePushCommandsDownAndBankLeft()
        {
            yield return SceneManager.LoadSceneAsync("FlightYoke"); yield return null;
            var vehicle = Object.FindFirstObjectByType<FlightVehicle>(); var yoke = Object.FindFirstObjectByType<FlightYoke>();
            var hand = Anchor("Yoke left push", yoke.transform.TransformPoint(new Vector3(-0.21f, 0.04f, 0)));
            hand.SetParent(vehicle.transform, true); Assert.IsTrue(yoke.Grab(hand));
            for (int i = 0; i < 8; i++) { hand.localPosition += new Vector3(0, -0.025f, 0.018f); yield return new WaitForFixedUpdate(); }
            yield return new WaitForSeconds(0.6f);
            Assert.That(yoke.Roll, Is.LessThan(-0.2f)); Assert.That(yoke.Pitch, Is.LessThan(-0.2f));
            Assert.That(vehicle.transform.forward.y, Is.LessThan(-0.005f));
            Assert.That(Mathf.DeltaAngle(0, vehicle.transform.eulerAngles.z), Is.GreaterThan(0.1f));
            Assert.AreEqual(0, vehicle.ResetCount, "The cockpit must not collide with itself");
            yoke.Release(hand); Object.Destroy(hand.gameObject); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator CruiseRemainsStableAndLowAltitudeResets()
        {
            yield return SceneManager.LoadSceneAsync("FlightYoke"); yield return null;
            var vehicle = Object.FindFirstObjectByType<FlightVehicle>();
            float oldScale = Time.timeScale;
            try
            {
                Time.timeScale = 10;
                yield return new WaitForSeconds(30);
                Assert.AreEqual(0, vehicle.ResetCount);
                Assert.That(vehicle.Speed, Is.InRange(13f, 19f));
                Assert.That(vehicle.transform.position.y, Is.InRange(100f, 150f));
                Assert.That(vehicle.transform.position.z, Is.GreaterThan(400));
                vehicle.GetComponent<Rigidbody>().position = new Vector3(0, 5, 0);
                yield return new WaitForFixedUpdate(); yield return null;
                Assert.That(vehicle.ResetCount, Is.GreaterThan(0));
                Assert.That(vehicle.transform.position.y, Is.GreaterThan(100));
            }
            finally { Time.timeScale = oldScale; }
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator SceneButtonsRoundTrip()
        {
            yield return SceneManager.LoadSceneAsync("DemoWorld"); yield return null;
            var button = System.Array.Find(Object.FindObjectsByType<DemoSceneButton>(FindObjectsSortMode.None), b => b.name == "FLIGHT YOKE");
            Assert.IsNotNull(button); button.Interact(); yield return null; yield return null;
            Assert.AreEqual("FlightYoke", SceneManager.GetActiveScene().name);
            button = System.Array.Find(Object.FindObjectsByType<DemoSceneButton>(FindObjectsSortMode.None), b => b.name == "DEMO WORLD");
            Assert.IsNotNull(button); button.Interact(); yield return null; yield return null;
            Assert.AreEqual("DemoWorld", SceneManager.GetActiveScene().name);
            button = System.Array.Find(Object.FindObjectsByType<DemoSceneButton>(FindObjectsSortMode.None), b => b.name == "INTERACTION LAB");
            Assert.IsNotNull(button); button.Interact(); yield return null; yield return null;
            Assert.AreEqual("VRInteractionTest", SceneManager.GetActiveScene().name);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
