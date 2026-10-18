using ThreeMatches.Core;
using ThreeMatches.Shadow;
using UnityEngine;

namespace ThreeMatches.Characters
{
    /// <summary>
    /// Animates Shadow-Rafi (GAME_SPEC §11, §24): the same clips as Rafi, driven only by how the shadow moves.
    /// Walking along the wall blends idle, walk and jog by speed; moving up or down the wall plays the
    /// floating loop. Rafi is holding a lit match while the shadow exists, so the match pose is held.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class ShadowRig : MonoBehaviour
    {
        [SerializeField] private ShadowController shadow;

        [SerializeField, Min(0f)] private float speedSmoothTime = 0.08f;
        [SerializeField, Min(0.1f)] private float teleportDistance = 1.5f;
        [SerializeField, Min(1f)] private float turnDegreesPerSecond = 540f;
        [Tooltip("Vertical speed (m/s) above which the shadow floats instead of walking.")]
        [SerializeField, Min(0f)] private float verticalThreshold = 0.4f;
        [SerializeField, Min(0.01f)] private float matchBlendSeconds = 0.25f;

        private Animator animator;
        private LocomotionTracker tracker;
        private int matchLayer;
        private float yaw;
        private float matchWeight;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            tracker = new LocomotionTracker(speedSmoothTime, teleportDistance);
            matchLayer = animator.GetLayerIndex(CharacterParams.MatchLayerName);
        }

        // The controller builds its mover in its own Awake, so facing is read here, after every Awake.
        private void Start()
        {
            yaw = LocomotionMath.FacingYaw(shadow.FacingRight);
            transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Vector3 p = shadow.transform.position;
            tracker.Step(new Vector2(p.x, p.y), dt);

            bool floating = Mathf.Abs(tracker.VerticalSpeed) > verticalThreshold;
            animator.SetBool(CharacterParams.Vertical, floating);
            animator.SetFloat(CharacterParams.Speed, LocomotionMath.NormalizedSpeed(tracker.HorizontalSpeed, transform.lossyScale.y));

            yaw = LocomotionMath.TurnYaw(yaw, LocomotionMath.FacingYaw(shadow.FacingRight), turnDegreesPerSecond, dt);
            transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            matchWeight = Mathf.MoveTowards(matchWeight, shadow.IsActive ? 1f : 0f, dt / matchBlendSeconds);
            if (matchLayer >= 0) animator.SetLayerWeight(matchLayer, matchWeight);
        }
    }
}
