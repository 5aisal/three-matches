using ThreeMatches.Lighting;
using UnityEngine;

namespace ThreeMatches.Vfx
{
    /// <summary>Match in Rafi's hand: spark burst and flame on strike, a smoke wisp when it goes out (GAME_SPEC §24).</summary>
    public sealed class MatchFlameVfx : MonoBehaviour
    {
        [SerializeField] private MatchFlameLight matchLight;
        [SerializeField] private ParticleSystem flame;
        [SerializeField] private ParticleSystem strikeSparks;
        [SerializeField] private ParticleSystem outSmoke;

        private void OnEnable()
        {
            matchLight.LitChanged += OnLitChanged;
        }

        private void OnDisable()
        {
            matchLight.LitChanged -= OnLitChanged;
        }

        private void OnLitChanged(bool lit)
        {
            if (lit)
            {
                strikeSparks.Play();
                flame.Play();
                return;
            }

            flame.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            outSmoke.Play();
        }
    }
}
