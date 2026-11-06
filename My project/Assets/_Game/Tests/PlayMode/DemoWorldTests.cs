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
            anchor.position = new Vector3(0, 1, 3);
            Assert.IsTrue(cells[0].Grab(anchor));
            Assert.IsFalse(cells[0].Grab(player.transform));
            game.Deliver(cells[0]);
            Assert.AreEqual(0, game.Round.Count, "Held orbs must not score");
            yield return new WaitForFixedUpdate();
            cells[0].Release();
            foreach (var cell in cells)
            {
                cell.GetComponent<Rigidbody>().position = new Vector3(0, 1, 3);
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
        public IEnumerator TeleportRejectsObstacleAndAcceptsOpenFloor()
        {
            yield return SceneManager.LoadSceneAsync("DemoWorld");
            yield return null;
            var player = Object.FindFirstObjectByType<DemoPlayer>();
            Assert.IsTrue(Physics.Raycast(new Vector3(6, 4, -5), Vector3.down, out var floorHit, 10));
            Assert.IsTrue(player.CanTeleport(floorHit));
            Assert.IsTrue(Physics.Raycast(new Vector3(-3, 4, 2), Vector3.down, out var obstacleHit, 10));
            Assert.IsFalse(player.CanTeleport(obstacleHit));
            player.Teleport(floorHit.point);
            yield return new WaitForFixedUpdate();
            Assert.That(Vector3.Distance(new Vector3(player.View.transform.position.x, 0, player.View.transform.position.z), floorHit.point), Is.LessThan(0.1f));
            LogAssert.NoUnexpectedReceived();
        }
    }
}
