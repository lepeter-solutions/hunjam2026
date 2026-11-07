using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Oculus.Interaction;
using Oculus.Interaction.Locomotion;

namespace HunJam.Tests
{
    public sealed class DemoWorldTests
    {
        [SetUp]
        public void SetSimulationRate() => Time.captureDeltaTime = 1f / 90f;

        [TearDown]
        public void RestoreSimulationRate() => Time.captureDeltaTime = 0;

        [UnityTest]
        public IEnumerator RobotGrabPhysicsDeliveryAndReplay()
        {
            yield return SceneManager.LoadSceneAsync("DemoWorld");
            yield return null;
            var game = Object.FindFirstObjectByType<DemoGame>();
            var robot = Object.FindFirstObjectByType<GuideRobot>();
            var player = Object.FindFirstObjectByType<OVRCameraRig>();
            var cells = Object.FindObjectsByType<EnergyCell>(FindObjectsSortMode.None);
            Assert.AreEqual(EnergyRound.Phase.Waiting, game.Round.State);
            Assert.AreEqual(3, cells.Length);
            foreach (var text in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
                Assert.IsNotNull(text.font, "World instructions need a font");
            robot.Interact();
            Assert.AreEqual(EnergyRound.Phase.Playing, game.Round.State);
            var anchor = new GameObject("Test hand anchor").transform;
            anchor.position = new Vector3(-0.9f, 1.1f, 0.65f);
            Select(cells[0], anchor);
            Assert.IsTrue(cells[0].IsHeld);
            game.Deliver(cells[0]);
            Assert.AreEqual(0, game.Round.Count, "Held orbs must not score");
            yield return new WaitForFixedUpdate();
            Release(cells[0], anchor);
            foreach (var cell in cells)
            {
                cell.GetComponent<Rigidbody>().position = new Vector3(-0.9f, 1.1f, 0.65f);
                cell.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                yield return new WaitForSeconds(0.15f);
            }
            Assert.AreEqual(3, game.Round.Count, "Physics receiver must accept each released orb");
            Assert.AreEqual(EnergyRound.Phase.Complete, game.Round.State);
            robot.Interact();
            yield return null;
            Assert.AreEqual(EnergyRound.Phase.Playing, game.Round.State);
            Assert.AreEqual(0, game.Round.Count);
            foreach (var cell in cells)
            {
                Assert.IsTrue(cell.gameObject.activeSelf);
                Assert.IsFalse(cell.IsHeld);
                Assert.IsFalse(cell.GetComponent<Rigidbody>().isKinematic);
            }
            Object.Destroy(anchor.gameObject);
        }

        [UnityTest]
        public IEnumerator PhysicalOnlyRigAndNearInteraction()
        {
            yield return SceneManager.LoadSceneAsync("DemoWorld");
            yield return null;
            var player = Object.FindFirstObjectByType<OVRCameraRig>();
            Vector3 position = player.transform.position;
            Quaternion rotation = player.transform.rotation;
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(position, player.transform.position);
            Assert.AreEqual(rotation, player.transform.rotation);
            var origin = player.GetComponent<OVRManager>().trackingOriginType;
            Assert.IsTrue(origin == OVRManager.TrackingOrigin.FloorLevel || origin == OVRManager.TrackingOrigin.Stage);
            Assert.That(player.GetComponentsInChildren<GrabInteractor>(true).Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(player.GetComponentsInChildren<MetaUseAction>(true).Length, Is.EqualTo(2));
            foreach (var locomotor in player.GetComponentsInChildren<PlayerLocomotor>(true))
                Assert.IsFalse(locomotor.enabled, "Physical movement only");
            foreach (var item in Object.FindObjectsByType<GameItem>(FindObjectsSortMode.None))
            {
                Assert.IsNotNull(item.GetComponent<Oculus.Interaction.Grabbable>());
                Assert.IsNotNull(item.GetComponent<GrabInteractable>());
            }
        }

        [UnityTest]
        public IEnumerator HeldToolsUseAndWorkshopReset()
        {
            yield return SceneManager.LoadSceneAsync("DemoWorld");
            yield return null;
            var tools = Object.FindObjectsByType<Tool>(FindObjectsSortMode.None);
            Assert.AreEqual(3, tools.Length);
            var lamp = System.Array.Find(tools, t => t.Kind == Tool.ToolKind.Lamp);
            lamp.Use();
            Assert.IsFalse(lamp.IsOn);
            var anchor = new GameObject("Test hand").transform;
            anchor.position = lamp.transform.position;
            Select(lamp, anchor);
            Assert.IsTrue(lamp.GetComponent<Rigidbody>().detectCollisions);
            lamp.Use();
            Assert.IsTrue(lamp.IsOn);
            lamp.Use();
            Assert.IsFalse(lamp.IsOn);
            var cuttable = Object.FindFirstObjectByType<Cuttable>();
            cuttable.Cut(Vector3.zero);
            Assert.IsFalse(cuttable.IsCut);
            cuttable.Cut(Vector3.down * 2);
            Assert.IsTrue(cuttable.IsCut);
            Assert.IsFalse(cuttable.GetComponent<Collider>().enabled);
            var target = Object.FindFirstObjectByType<ImpactTarget>();
            target.Strike(0.1f);
            Assert.AreEqual(0, target.Hits);
            target.Strike(2);
            target.Strike(2);
            Assert.AreEqual(1, target.Hits);
            Object.FindFirstObjectByType<WorkshopReset>().Interact();
            yield return null;
            Assert.IsFalse(cuttable.IsCut);
            Assert.IsTrue(cuttable.GetComponent<Collider>().enabled);
            Assert.AreEqual(0, target.Hits);
            Assert.IsFalse(lamp.IsHeld);
            Object.Destroy(anchor.gameObject);
        }

        [UnityTest]
        public IEnumerator HammerSwingViaMetaSdk()
        {
            yield return SceneManager.LoadSceneAsync("DemoWorld");
            yield return null;
            var hammer = System.Array.Find(Object.FindObjectsByType<Tool>(FindObjectsSortMode.None), t => t.Kind == Tool.ToolKind.Hammer);
            var target = Object.FindFirstObjectByType<ImpactTarget>();
            target.transform.position = new Vector3(0, 3, 0);
            var body = hammer.GetComponent<Rigidbody>();
            hammer.transform.SetPositionAndRotation(new Vector3(0, 3, -0.5f), Quaternion.identity);
            var anchor = new GameObject("Hammer hand").transform;
            anchor.position = hammer.transform.position;
            Physics.SyncTransforms();
            Select(hammer, anchor);
            yield return new WaitForSeconds(0.25f);
            Assert.AreEqual(0, target.Hits);
            for (int i = 0; i < 10 && target.Hits == 0; i++)
            {
                anchor.position += Vector3.forward * 0.06f;
                Move(hammer, anchor);
                Assert.That(Vector3.Distance(hammer.transform.position, anchor.position), Is.LessThan(0.001f), "SDK transformer follows its grab point");
                yield return new WaitForFixedUpdate();
                yield return null;
            }
            Assert.AreEqual(1, target.Hits);
            Release(hammer, anchor);
            Assert.IsFalse(hammer.IsHeld);
            Assert.IsFalse(body.isKinematic);
            Object.Destroy(anchor.gameObject);
        }

        [UnityTest]
        public IEnumerator MovingBladeCutsButStationaryBladeDoesNot()
        {
            yield return SceneManager.LoadSceneAsync("DemoWorld");
            yield return null;
            var blade = System.Array.Find(Object.FindObjectsByType<Tool>(FindObjectsSortMode.None), t => t.Kind == Tool.ToolKind.Blade);
            var block = Object.FindFirstObjectByType<Cuttable>();
            var body = blade.GetComponent<Rigidbody>();
            // Isolate the sweep from the bench and put the blade tip just before the target.
            block.transform.position = new Vector3(0, 3, 0);
            blade.transform.SetPositionAndRotation(new Vector3(0, 3, -0.6f), Quaternion.identity);
            var anchor = new GameObject("Swing hand").transform;
            anchor.position = blade.transform.position;
            Physics.SyncTransforms();
            Select(blade, anchor);
            yield return new WaitForSeconds(0.25f);
            Assert.IsFalse(block.IsCut);
            for (int i = 0; i < 10 && !block.IsCut; i++)
            {
                anchor.position += Vector3.forward * 0.06f;
                Move(blade, anchor);
                yield return new WaitForFixedUpdate();
                yield return null;
            }
            Assert.IsTrue(block.IsCut, "A held moving blade must cut through the real contact sweep");
            Object.Destroy(anchor.gameObject);
        }
        private static void Select(GameItem item, Transform anchor)
        {
            var sdk = item.GetComponent<Oculus.Interaction.Grabbable>();
            var pose = new Pose(anchor.position, anchor.rotation);
            sdk.ProcessPointerEvent(new PointerEvent(123, PointerEventType.Hover, pose));
            sdk.ProcessPointerEvent(new PointerEvent(123, PointerEventType.Select, pose));
        }
        private static void Move(GameItem item, Transform anchor)
        {
            item.GetComponent<Oculus.Interaction.Grabbable>().ProcessPointerEvent(
                new PointerEvent(123, PointerEventType.Move, new Pose(anchor.position, anchor.rotation)));
        }
        private static void Release(GameItem item, Transform anchor)
        {
            item.GetComponent<Oculus.Interaction.Grabbable>().ProcessPointerEvent(
                new PointerEvent(123, PointerEventType.Unselect, new Pose(anchor.position, anchor.rotation)));
        }
    }
}
