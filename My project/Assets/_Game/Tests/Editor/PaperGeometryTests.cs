using NUnit.Framework;
using UnityEngine;
namespace HunJam.Tests
{
    public sealed class PaperGeometryTests
    {
        private static float Area(Mesh mesh)
        {
            float result = 0; var vertices = mesh.vertices; var indices = mesh.triangles;
            for (int i = 0; i < indices.Length; i += 3)
                result += Vector3.Cross(vertices[indices[i + 1]] - vertices[indices[i]], vertices[indices[i + 2]] - vertices[indices[i]]).magnitude * 0.5f;
            return result;
        }
        [TestCase(0f, 0f, 1f, 0f)]
        [TestCase(0.05f, 0.02f, 1f, 0.8f)]
        [TestCase(-0.07f, -0.03f, 0.4f, 1f)]
        public void DynamicSplitConservesAreaAndOriginalUV(float x, float y, float nx, float ny)
        {
            var original = PaperMesh.Create(); var point = new Vector2(x, y); var normal = new Vector2(nx, ny).normalized;
            var a = PaperMesh.Clip(original, point, normal, false); var b = PaperMesh.Clip(original, point, normal, true);
            Assert.That(Area(a), Is.GreaterThan(0.001f)); Assert.That(Area(b), Is.GreaterThan(0.001f));
            Assert.That(Area(a) + Area(b), Is.EqualTo(Area(original)).Within(0.00001f));
            foreach (var mesh in new[] { a, b })
            {
                var vertices = mesh.vertices; var uv = mesh.uv;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Assert.That(uv[i].x, Is.EqualTo(vertices[i].x / 0.42f + 0.5f).Within(0.00001f));
                    Assert.That(uv[i].y, Is.EqualTo(vertices[i].y / 0.3f + 0.5f).Within(0.00001f));
                }
            }
            Object.DestroyImmediate(a); Object.DestroyImmediate(b); Object.DestroyImmediate(original);
        }
        [Test] public void GripPositionChangesFragmentGeometry()
        {
            var mesh = PaperMesh.Create();
            var a = PaperMesh.Clip(mesh, Vector2.zero, Vector2.right, true);
            var b = PaperMesh.Clip(mesh, Vector2.right * 0.07f, new Vector2(1, 0.4f), true);
            Assert.That(Mathf.Abs(Area(a) - Area(b)), Is.GreaterThan(0.01f));
            Object.DestroyImmediate(a); Object.DestroyImmediate(b); Object.DestroyImmediate(mesh);
        }
        [TestCase(0.03f, 0f)] [TestCase(2f, 1f)] [TestCase(-2f, -1f)]
        public void YokeDeadZoneAndLimits(float input, float expected)
        { Assert.That(FlightYoke.ApplyDeadZone(input, 0.06f), Is.EqualTo(expected).Within(0.0001f)); }
    }
}
