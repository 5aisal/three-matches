using System;

namespace ThreeMatches.Core
{
    /// <summary>
    /// Movement rules of the shadow on the back walls (GAME_SPEC §11): a speed-limited walk inside a
    /// rectangle split into rooms. A room whose fire level is at or above the block level cannot be entered.
    /// </summary>
    public sealed class ShadowMover
    {
        const float EdgeEpsilon = 0.001f;

        readonly float _minX, _maxX, _minY, _maxY, _speed;
        readonly float[] _roomEdges;

        /// <param name="roomEdges">Ascending x of the borders between neighbouring rooms (rooms - 1 values).</param>
        public ShadowMover(float minX, float maxX, float minY, float maxY, float[] roomEdges, float speed, float startX, float startY)
        {
            _minX = minX;
            _maxX = Math.Max(minX, maxX);
            _minY = minY;
            _maxY = Math.Max(minY, maxY);
            _roomEdges = roomEdges;
            _speed = speed;
            X = Clamp(startX, _minX, _maxX);
            Y = Clamp(startY, _minY, _maxY);
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public bool FacingRight { get; private set; } = true;
        public int RoomCount => _roomEdges.Length + 1;

        public int RoomAt(float x)
        {
            int room = 0;
            while (room < _roomEdges.Length && x >= _roomEdges[room]) room++;
            return room;
        }

        /// <summary>Moves by input (-1..1 per axis, diagonal speed is not faster). Levels are indexed by room.</summary>
        public void Step(float h, float v, float dt, int[] roomLevels, int blockLevel)
        {
            float len = (float)Math.Sqrt(h * h + v * v);
            if (len > 1f)
            {
                h /= len;
                v /= len;
            }

            if (h > 0f) FacingRight = true;
            else if (h < 0f) FacingRight = false;

            float nextX = Clamp(X + h * _speed * dt, _minX, _maxX);
            int from = RoomAt(X);
            int to = RoomAt(nextX);
            if (to != from && roomLevels[to] >= blockLevel)
            {
                nextX = to > from ? _roomEdges[from] - EdgeEpsilon : _roomEdges[from - 1] + EdgeEpsilon;
                nextX = Clamp(nextX, _minX, _maxX);
            }

            X = nextX;
            Y = Clamp(Y + v * _speed * dt, _minY, _maxY);
        }

        static float Clamp(float value, float min, float max) => value < min ? min : (value > max ? max : value);
    }
}
