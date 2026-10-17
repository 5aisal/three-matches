namespace ThreeMatches.Core
{
    /// <summary>Hold-to-confirm timer (e.g. forcing the window: hold E for 2 s). Releasing resets it.</summary>
    public sealed class HoldTimer
    {
        readonly float _duration;
        float _elapsed;

        public HoldTimer(float duration)
        {
            _duration = duration;
        }

        /// <summary>0..1 fill of the progress ring.</summary>
        public float Progress => _duration <= 0f ? 0f : (_elapsed >= _duration ? 1f : _elapsed / _duration);

        /// <summary>Advances while held. Returns true on the tick the hold completes.</summary>
        public bool Tick(bool held, float dt)
        {
            if (!held)
            {
                _elapsed = 0f;
                return false;
            }

            if (_elapsed >= _duration) return false;
            _elapsed += dt;
            return _elapsed >= _duration;
        }

        public void Reset() => _elapsed = 0f;
    }
}
