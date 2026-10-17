using ThreeMatches.Core;
using UnityEngine;

namespace ThreeMatches.Memories
{
    /// <summary>Scene-side holder for the one <see cref="RescueState"/> so other components can reference it.</summary>
    public sealed class RescueStateService : MonoBehaviour
    {
        public RescueState State { get; } = new RescueState();
    }
}
