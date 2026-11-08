using UnityEngine;
namespace HunJam
{
    public static class InteractionFeedback
    {
        private static AudioClip paper, tap;
        public static void Play(Vector3 position, bool tearing, float volume = 0.35f)
        {
            var clip = tearing ? paper : tap;
            if (clip == null)
            {
                int count = tearing ? 6615 : 2205;
                var data = new float[count]; var random = new System.Random(tearing ? 73 : 29);
                for (int i = 0; i < count; i++)
                {
                    float envelope = Mathf.Pow(1f - (float)i / count, 2);
                    data[i] = ((float)random.NextDouble() * 2 - 1) * envelope * (tearing ? 0.45f : 0.2f);
                }
                clip = AudioClip.Create(tearing ? "Paper rip" : "Soft contact", count, 1, 22050, false); clip.SetData(data, 0);
                if (tearing) paper = clip; else tap = clip;
            }
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
        public static void Pulse(Grabbable item, float strength, float duration)
        {
            item.Primary?.GetComponentInParent<DemoHand>()?.Pulse(strength, duration);
            item.Secondary?.GetComponentInParent<DemoHand>()?.Pulse(strength, duration);
        }
    }
}
