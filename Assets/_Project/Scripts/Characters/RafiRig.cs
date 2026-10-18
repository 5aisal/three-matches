using ThreeMatches.Core;
using ThreeMatches.Memories;
using ThreeMatches.Player;
using UnityEngine;

namespace ThreeMatches.Characters
{
    /// <summary>
    /// Animates Rafi's body (GAME_SPEC §19, §24). It only reads the gameplay components and drives the
    /// Animator: seated at the start, stand-up, idle/walk blend by speed, a reach when he interacts, an
    /// arm-raised pose while a match is lit, a head turn toward the photo he hovers, and a permanent stoop.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class RafiRig : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RafiController rafi;
        [SerializeField] private InteractionDetector detector;
        [SerializeField] private GameInput input;
        [SerializeField] private MatchController matches;
        [SerializeField] private MemoryWall wall;

        [Header("Locomotion")]
        [SerializeField, Min(0f)] private float speedSmoothTime = 0.08f;
        [SerializeField, Min(0.1f)] private float teleportDistance = 1.5f;
        [SerializeField, Min(1f)] private float turnDegreesPerSecond = 540f;

        [Header("Posture")]
        [Tooltip("Forward lean of the spine in degrees, added after the animation each frame.")]
        [SerializeField, Range(0f, 30f)] private float stoopDegrees = 10f;
        [SerializeField, Min(0.01f)] private float stoopBlendSeconds = 0.6f;

        [Header("Match pose")]
        [SerializeField, Min(0.01f)] private float matchBlendSeconds = 0.25f;

        [Header("Look at hovered photo")]
        [SerializeField, Range(0f, 1f)] private float lookWeight = 0.6f;
        [SerializeField, Range(0f, 1f)] private float lookBodyWeight = 0.15f;
        [SerializeField, Min(0.01f)] private float lookBlendSeconds = 0.2f;

        private Animator animator;
        private LocomotionTracker tracker;
        private SeatedIntro intro;
        private Transform spine;
        private Transform chest;
        private int matchLayer;
        private float yaw;
        private float matchWeight;
        private float stoopAmount;
        private float lookAmount;
        private Vector3 lookPosition;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            tracker = new LocomotionTracker(speedSmoothTime, teleportDistance);
            intro = new SeatedIntro();
            matchLayer = animator.GetLayerIndex(CharacterParams.MatchLayerName);
            spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        }

        // The controller builds its model in its own Awake, so facing is read here, after every Awake.
        private void Start()
        {
            yaw = LocomotionMath.FacingYaw(rafi.FacingRight);
            transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Vector3 p = rafi.transform.position;
            tracker.Step(new Vector2(p.x, p.y), dt);
            float speed = tracker.Speed;
            animator.SetFloat(CharacterParams.Speed, LocomotionMath.NormalizedSpeed(speed, transform.lossyScale.y));

            // He stands as soon as he is free to move, or if a match is lit (a debug jump can skip the title).
            bool holdingMatch = matches != null && matches.Session.IsActive;
            if (intro.Step(rafi.InputLocked && !holdingMatch, speed)) animator.SetTrigger(CharacterParams.Stand);

            yaw = LocomotionMath.TurnYaw(yaw, LocomotionMath.FacingYaw(rafi.FacingRight), turnDegreesPerSecond, dt);
            transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            UpdateMatchPose(dt);
            UpdateInteract();

            float stoopTarget = intro.Seated ? 0f : 1f;
            stoopAmount = Mathf.MoveTowards(stoopAmount, stoopTarget, dt / stoopBlendSeconds);
        }

        private void UpdateMatchPose(float dt)
        {
            float target = !intro.Seated && matches != null && matches.Session.IsActive ? 1f : 0f;
            matchWeight = Mathf.MoveTowards(matchWeight, target, dt / matchBlendSeconds);
            if (matchLayer >= 0) animator.SetLayerWeight(matchLayer, matchWeight);
        }

        private void UpdateInteract()
        {
            Interactable focus = detector.Focus;
            if (focus != null && !focus.PromptOnly && input.Interact.WasPressedThisFrame())
            {
                animator.SetTrigger(CharacterParams.Interact);
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            MemoryFrame hovered = wall != null ? wall.HoveredFrame : null;
            float target = hovered != null && !intro.Seated ? 1f : 0f;
            if (hovered != null) lookPosition = hovered.transform.position;
            lookAmount = Mathf.MoveTowards(lookAmount, target, Time.deltaTime / lookBlendSeconds);

            animator.SetLookAtWeight(lookWeight * lookAmount, lookBodyWeight, 1f, 0f, 0.5f);
            animator.SetLookAtPosition(lookPosition);
        }

        private void LateUpdate()
        {
            if (spine == null || stoopAmount <= 0f) return;

            // After the Animator has posed the body: pitch the torso forward about the body's right axis.
            float lean = stoopDegrees * stoopAmount;
            Vector3 axis = transform.right;
            spine.rotation = Quaternion.AngleAxis(lean * 0.5f, axis) * spine.rotation;
            if (chest != null) chest.rotation = Quaternion.AngleAxis(lean * 0.5f, axis) * chest.rotation;
        }
    }
}
