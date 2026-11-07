using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HunJam.Tests
{
    public sealed class DemoWorldTests
    {
        [UnityTest]
        public IEnumerator RobotGrabPhysicsDeliveryAndReplay()
        {
            yield return SceneManager.LoadSceneAsync("DemoWorld");
            yield return null;
            var game = Object.FindFirstObjectByType<DemoGame>();
            var robot = Object.FindFirstObjectByType<GuideRobot>();
            var player = Object.FindFirstObjectByType<DemoPlayer>();
            var cells = Object.FindObjectsByType<EnergyCell>(FindObjectsSortMode.None);
            Assert.AreEqual(EnergyRound.Phase.Waiting, game.Round.State);
            Assert.AreEqual(3, cells.Length);
            foreach (var text in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
                Assert.IsNotNull(text.font, "World instructions need a font");
            robot.Interact();
            Assert.AreEqual(EnergyRound.Phase.Playing, game.Round.State);
            var anchor = new GameObject("Test hand anchor").transform;
            anchor.position = new Vector3(-0.9f, 1.1f, 0.65f);
            Assert.IsTrue(cells[0].Grab(anchor));
            Assert.IsFalse(cells[0].Grab(player.transform));
            game.Deliver(cells[0]);
            Assert.AreEqual(0, game.Round.Count, "Held orbs must not score");
            yield return new WaitForFixedUpdate();
            cells[0].Release();
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
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PhysicalOnlyRigAndNearInteraction()
        {
            yield return SceneManager.LoadSceneAsync("DemoWorld");
            yield return null;
            var player = Object.FindFirstObjectByType<DemoPlayer>();
            Vector3 position = player.transform.position;
            Quaternion rotation = player.transform.rotation;
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(position, player.transform.position);
            Assert.AreEqual(rotation, player.transform.rotation);
            Assert.IsNull(player.GetComponent<CharacterController>());
            Assert.IsNull(typeof(DemoPlayer).GetMethod("Teleport"));
            var hand = Object.FindFirstObjectByType<DemoHand>();
            var cell = Object.FindFirstObjectByType<EnergyCell>();
            hand.transform.position = cell.transform.position + Vector3.up * 0.18f;
            Physics.SyncTransforms();
            Assert.AreEqual(cell, hand.FindNearby<Grabbable>());
            hand.transform.position = cell.transform.position + Vector3.up * 0.5f;
            Physics.SyncTransforms();
            Assert.IsNull(hand.FindNearby<Grabbable>());
            LogAssert.NoUnexpectedReceived();
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
            Assert.IsTrue(lamp.Grab(anchor));
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
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator HammerSwingAndTrackingJump()
        {
            yield return SceneManager.LoadSceneAsync("DemoWorld");
            yield return null;
            var hammer = System.Array.Find(Object.FindObjectsByType<Tool>(FindObjectsSortMode.None), t => t.Kind == Tool.ToolKind.Hammer);
            var target = Object.FindFirstObjectByType<ImpactTarget>();
            target.transform.position = new Vector3(0, 3, 0);
            var body = hammer.GetComponent<Rigidbody>();
            body.position = new Vector3(0, 3, -0.5f);
            body.rotation = Quaternion.identity;
            var anchor = new GameObject("Hammer hand").transform;
            anchor.position = body.position;
            Physics.SyncTransforms();
            Assert.IsTrue(hammer.Grab(anchor));
            yield return new WaitForSeconds(0.25f);
            Assert.AreEqual(0, target.Hits);
            for (int i = 0; i < 10 && target.Hits == 0; i++)
            {
                anchor.position += Vector3.forward * 0.06f;
                yield return new WaitForFixedUpdate();
                yield return null;
            }
            Assert.AreEqual(1, target.Hits);
            anchor.position += Vector3.right * 2;
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.IsFalse(hammer.IsHeld, "Tracking jumps must release instead of causing an impact");
            Assert.AreEqual(1, target.Hits);
            Assert.That(body.linearVelocity.magnitude, Is.LessThan(1));
            Object.Destroy(anchor.gameObject);
            LogAssert.NoUnexpectedReceived();
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
            body.position = new Vector3(0, 3, -0.6f);
            body.rotation = Quaternion.identity;
            var anchor = new GameObject("Swing hand").transform;
            anchor.position = body.position;
            Physics.SyncTransforms();
            Assert.IsTrue(blade.Grab(anchor));
            yield return new WaitForSeconds(0.25f);
            Assert.IsFalse(block.IsCut);
            for (int i = 0; i < 10 && !block.IsCut; i++)
            {
                anchor.position += Vector3.forward * 0.06f;
                yield return new WaitForFixedUpdate();
                yield return null;
            }
            Assert.IsTrue(block.IsCut, "A held moving blade must cut through the real contact sweep");
            Object.Destroy(anchor.gameObject);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
