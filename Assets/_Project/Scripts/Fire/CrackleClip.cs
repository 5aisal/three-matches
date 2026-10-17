using UnityEngine;

namespace ThreeMatches.Fire
{
    /// <summary>
    /// Placeholder fire crackle: a short looping clip of random pops, generated once.
    /// Replace with a real sample from GAME_SPEC §20 later.
    /// </summary>
    internal static class CrackleClip
    {
        private const int SampleRate = 22050;
        private const float Seconds = 2f;
        private const int Seed = 1996;
        private const float PopChance = 0.0012f;
        private const float PopDecay = 0.985f;
        private const float HissLevel = 0.03f;

        private static AudioClip clip;

        public static AudioClip Get()
        {
            if (clip != null) return clip;

            int count = (int)(SampleRate * Seconds);
            var data = new float[count];
            var rng = new System.Random(Seed);
            float pop = 0f;
            for (int i = 0; i < count; i++)
            {
                if (rng.NextDouble() < PopChance) pop = (float)(0.4 + rng.NextDouble() * 0.6);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                data[i] = noise * (HissLevel + pop);
                pop *= PopDecay;
            }

            clip = AudioClip.Create("FireCrackle (placeholder)", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
