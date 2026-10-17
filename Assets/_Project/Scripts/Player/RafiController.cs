using ThreeMatches.Core;
using UnityEngine;

namespace ThreeMatches.Player
{
    /// <summary>
    /// Drives present-day Rafi along the floors and stairs of a house. Rules live in
    /// <see cref="WalkerModel"/>; this component only reads input and places the transform.
    /// Layout numbers are house-local (relative to <c>houseOrigin</c>).
    /// </summary>
    public sealed class RafiController : MonoBehaviour
    {
        [SerializeField] private GameInput input;
        [SerializeField] private Transform houseOrigin;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField, Min(0f)] private float walkSpeed = 2f;
        [SerializeField] private float startX = 0.6f;
        [Tooltip("Depth offset from the house origin; negative is toward the camera.")]
        [SerializeField] private float walkDepth = -0.15f;
        [SerializeField] private float minX = -8.65f;
        [SerializeField] private float maxX = 8.65f;
        [SerializeField] private float groundY;
        [SerializeField] private float upperY = 3.1f;
        [SerializeField] private float stairBottomX = -0.05f;
        [SerializeField] private float stairTopX = -2.85f;
        [SerializeField, Min(0f)] private float stairEntryRadius = 0.35f;

        private WalkerModel model;

        public WalkerMode Mode => model.Mode;
        public bool InputLocked { get; set; }

        private void Awake()
        {
            var layout = new WalkerLayout(minX, maxX, groundY, upperY, stairBottomX, stairTopX, stairEntryRadius);
            model = new WalkerModel(layout, walkSpeed, startX);
            Apply();
        }

        private void Update()
        {
            float h = 0f;
            float v = 0f;
            if (!InputLocked)
            {
                h = input.Move.ReadValue<float>();
                v = input.Vertical.ReadValue<float>();
            }

            model.Step(h, v, Time.deltaTime);
            Apply();
        }

        private void Apply()
        {
            Vector3 origin = houseOrigin.position;
            transform.position = new Vector3(origin.x + model.X, origin.y + model.Y, origin.z + walkDepth);
            visual.flipX = !model.FacingRight;
        }
    }
}
