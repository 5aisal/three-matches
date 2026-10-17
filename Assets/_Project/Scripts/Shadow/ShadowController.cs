using System;
using ThreeMatches.Core;
using ThreeMatches.Fire;
using ThreeMatches.Memories;
using ThreeMatches.Player;
using UnityEngine;

namespace ThreeMatches.Shadow
{
    /// <summary>
    /// Rafi's shadow on the back walls of the past house (GAME_SPEC §11). It is controlled only while a
    /// rescue window is open (Rafi is locked then); the walking rules live in <see cref="ShadowMover"/>.
    /// The walkable area is built from the ShadowWall quads, ordered left to right.
    /// </summary>
    public sealed class ShadowController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameInput input;
        [SerializeField] private MatchController matches;
        [SerializeField] private PastFireController fire;
        [SerializeField] private SpriteRenderer[] visuals;

        [Header("Walls (left to right)")]
        [SerializeField] private Transform[] wallQuads;
        [SerializeField] private RoomId[] roomOrder = { RoomId.Landing, RoomId.Hallway, RoomId.Bedroom };
        [SerializeField, Min(0)] private int startRoom;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float speed = 3f;
        [SerializeField, Range(0, 3)] private int blockedAtFireLevel = 3;
        [Tooltip("Space kept free above the feet so the silhouette stays inside the wall.")]
        [SerializeField, Min(0f)] private float headroom = 1f;
        [Tooltip("Height of the silhouette's centre above its feet (interaction reach is measured there).")]
        [SerializeField, Min(0f)] private float bodyCenterHeight = 0.8f;
        [Tooltip("World z the silhouette sits at (just in front of the ShadowWall quads).")]
        [SerializeField] private float depth = 3.9f;

        private ShadowMover mover;
        private int[] levels;
        private FireRoom[] roomsByOrder;
        private bool active;

        public bool IsActive => active;
        public Vector3 BodyCenter => transform.position + Vector3.up * bodyCenterHeight;

        /// <summary>Raised when control passes to the shadow (true) or back to Rafi (false).</summary>
        public event Action<bool> ActiveChanged;

        private void Awake()
        {
            BuildMover();
            levels = new int[roomOrder.Length];
            roomsByOrder = new FireRoom[roomOrder.Length];
            SetVisible(false);
            Apply();
        }

        private void Start()
        {
            FireRoom[] rooms = fire.Rooms;
            for (int i = 0; i < roomOrder.Length; i++)
            {
                for (int j = 0; j < rooms.Length; j++)
                {
                    if (rooms[j].Room == roomOrder[i]) roomsByOrder[i] = rooms[j];
                }
            }
        }

        private void Update()
        {
            bool open = matches.Session.IsWindowOpen;
            if (open != active)
            {
                active = open;
                SetVisible(open);
                ActiveChanged?.Invoke(open);
            }

            if (!active) return;

            for (int i = 0; i < levels.Length; i++)
            {
                levels[i] = roomsByOrder[i] != null ? roomsByOrder[i].TargetLevel : 0;
            }

            float h = input.Move.ReadValue<float>();
            float v = input.Vertical.ReadValue<float>();
            mover.Step(h, v, Time.deltaTime, levels, blockedAtFireLevel);
            Apply();
        }

        private void BuildMover()
        {
            int n = wallQuads.Length;
            var edges = new float[n - 1];
            float minY = float.MinValue;
            float maxY = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                Vector3 p = wallQuads[i].position;
                Vector3 s = wallQuads[i].lossyScale;
                minY = Mathf.Max(minY, p.y - s.y * 0.5f);
                maxY = Mathf.Min(maxY, p.y + s.y * 0.5f);
                if (i < n - 1)
                {
                    float right = p.x + s.x * 0.5f;
                    Vector3 nextP = wallQuads[i + 1].position;
                    float nextLeft = nextP.x - wallQuads[i + 1].lossyScale.x * 0.5f;
                    edges[i] = (right + nextLeft) * 0.5f;
                }
            }

            Vector3 first = wallQuads[0].position;
            Vector3 last = wallQuads[n - 1].position;
            float minX = first.x - wallQuads[0].lossyScale.x * 0.5f;
            float maxX = last.x + wallQuads[n - 1].lossyScale.x * 0.5f;
            float startX = wallQuads[Mathf.Clamp(startRoom, 0, n - 1)].position.x;
            mover = new ShadowMover(minX, maxX, minY, maxY - headroom, edges, speed, startX, minY);
        }

        private void Apply()
        {
            transform.position = new Vector3(mover.X, mover.Y, depth);
            for (int i = 0; i < visuals.Length; i++) visuals[i].flipX = !mover.FacingRight;
        }

        private void SetVisible(bool value)
        {
            for (int i = 0; i < visuals.Length; i++) visuals[i].enabled = value;
        }
    }
}
