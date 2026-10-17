using System;

namespace ThreeMatches.Core
{
    /// <summary>Distances, speeds and timings of Noor's behaviour (GAME_SPEC §12).</summary>
    public struct NoorTuning
    {
        public float PanicRadius;
        public float FollowRadius;
        public float FollowSpeed;
        public float FollowGap;
        public float HumStopSeconds;
        public float PanicRunDistance;
        public float PanicRunSpeed;
        public float PanicFreezeSeconds;
        public float WindowReach;

        public static NoorTuning Default => new NoorTuning
        {
            PanicRadius = 3f,
            FollowRadius = 4f,
            FollowSpeed = 1.5f,
            FollowGap = 0.6f,
            HumStopSeconds = 1.5f,
            PanicRunDistance = 1.5f,
            PanicRunSpeed = 3f,
            PanicFreezeSeconds = 3f,
            WindowReach = 1f
        };
    }

    [Flags]
    public enum NoorEvents
    {
        None = 0,
        Panicked = 1,
        StartedFollowing = 2,
        FirstFollow = 4,
        ReachedWindow = 8
    }

    /// <summary>
    /// Noor's state machine on the upper floor (GAME_SPEC §12), along one axis (x). The panic trigger is
    /// edge-based: it fires when the shadow comes within the panic radius, not while it stays there.
    /// </summary>
    public sealed class NoorModel
    {
        readonly NoorTuning _t;
        readonly float _minX, _maxX, _windowX;

        float _runTarget;
        float _freezeLeft;
        float _noHumTime;
        bool _arrived;
        bool _primed;
        bool _wasInsidePanic;
        bool _hasFollowed;

        public NoorModel(NoorTuning tuning, float minX, float maxX, float windowX)
        {
            _t = tuning;
            _minX = minX;
            _maxX = Math.Max(minX, maxX);
            _windowX = windowX;
        }

        public NoorState State { get; private set; } = NoorState.Hidden;
        public float X { get; private set; }
        public bool FacingRight { get; private set; } = true;
        public bool IsMoving { get; private set; }

        /// <summary>Hidden -> Out at the given x (FIND). Does nothing in any other state.</summary>
        public void Reveal(float x)
        {
            if (State != NoorState.Hidden) return;
            State = NoorState.Out;
            X = Clamp(x, _minX, _maxX);
        }

        /// <summary>
        /// Puts Noor into a stored state at the start of a window. Panicked and Following do not carry over
        /// between windows; she is simply Out.
        /// </summary>
        public void Restore(NoorState state, float x)
        {
            State = state == NoorState.Panicked || state == NoorState.Following ? NoorState.Out : state;
            X = Clamp(x, _minX, _maxX);
            _noHumTime = 0f;
            _freezeLeft = 0f;
            _primed = false;
            IsMoving = false;
        }

        public NoorEvents Step(float dt, float shadowX, bool humming, bool windowOpen)
        {
            float dist = Math.Abs(shadowX - X);
            bool insidePanic = dist <= _t.PanicRadius;
            bool entered = _primed && insidePanic && !_wasInsidePanic;
            _wasInsidePanic = insidePanic;
            _primed = true;
            IsMoving = false;

            switch (State)
            {
                case NoorState.Out: return StepOut(shadowX, dist, humming, entered);
                case NoorState.Panicked: StepPanicked(dt); return NoorEvents.None;
                case NoorState.Following: return StepFollowing(dt, shadowX, dist, humming, windowOpen);
                default: return NoorEvents.None;
            }
        }

        NoorEvents StepOut(float shadowX, float dist, bool humming, bool entered)
        {
            if (humming && dist <= _t.FollowRadius)
            {
                State = NoorState.Following;
                _noHumTime = 0f;
                NoorEvents e = NoorEvents.StartedFollowing;
                if (!_hasFollowed) e |= NoorEvents.FirstFollow;
                _hasFollowed = true;
                return e;
            }

            if (entered && !humming)
            {
                float dir = X >= shadowX ? 1f : -1f;
                _runTarget = Clamp(X + dir * _t.PanicRunDistance, _minX, _maxX);
                _arrived = false;
                _freezeLeft = _t.PanicFreezeSeconds;
                State = NoorState.Panicked;
                return NoorEvents.Panicked;
            }

            return NoorEvents.None;
        }

        void StepPanicked(float dt)
        {
            if (!_arrived)
            {
                MoveToward(_runTarget, _t.PanicRunSpeed * dt);
                _arrived = Math.Abs(_runTarget - X) < 0.0001f;
                return;
            }

            _freezeLeft -= dt;
            if (_freezeLeft <= 0f) State = NoorState.Out;
        }

        NoorEvents StepFollowing(float dt, float shadowX, float dist, bool humming, bool windowOpen)
        {
            if (humming) _noHumTime = 0f;
            else _noHumTime += dt;

            if (_noHumTime >= _t.HumStopSeconds)
            {
                State = NoorState.Out;
                return NoorEvents.None;
            }

            float room = dist - _t.FollowGap;
            if (room > 0f)
            {
                float step = Math.Min(room, _t.FollowSpeed * dt);
                MoveToward(shadowX, step);
            }

            if (windowOpen && Math.Abs(_windowX - X) <= _t.WindowReach)
            {
                X = Clamp(_windowX, _minX, _maxX);
                State = NoorState.AtWindow;
                return NoorEvents.ReachedWindow;
            }

            return NoorEvents.None;
        }

        void MoveToward(float target, float maxStep)
        {
            float delta = target - X;
            if (Math.Abs(delta) < 0.0001f) return;
            FacingRight = delta > 0f;
            float move = Math.Min(Math.Abs(delta), maxStep);
            X = Clamp(X + Math.Sign(delta) * move, _minX, _maxX);
            IsMoving = move > 0f;
        }

        static float Clamp(float v, float min, float max) => v < min ? min : (v > max ? max : v);
    }
}
