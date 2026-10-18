using ThreeMatches.Core;
using ThreeMatches.Fire;
using ThreeMatches.Noor;
using UnityEngine;

namespace ThreeMatches.Characters
{
    /// <summary>
    /// Animates Noor's body (GAME_SPEC §12, §24) from her gameplay state: crouched and scared by the closet
    /// and after a panic, a run when she bolts, a slower walk while following, coughing in heavy smoke and
    /// sitting on the window sill. It only reads <see cref="NoorController"/> and drives the Animator.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class NoorRig : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NoorController noor;
        [SerializeField] private PastFireController fire;
        [Tooltip("Meshes shown while she is out of the closet.")]
        [SerializeField] private Renderer[] renderers;

        [Header("Locomotion")]
        [SerializeField, Min(0f)] private float speedSmoothTime = 0.08f;
        [SerializeField, Min(0.1f)] private float teleportDistance = 1.5f;
        [SerializeField, Min(1f)] private float turnDegreesPerSecond = 540f;
        [Tooltip("Speed (m/s) above which she counts as travelling.")]
        [SerializeField, Min(0f)] private float movingThreshold = 0.15f;

        [Header("Window sill")]
        [Tooltip("Yaw (degrees) she faces while sitting: 180 looks at the camera.")]
        [SerializeField] private float sitYaw = 180f;
        [Tooltip("How far she is lifted onto the sill (metres, world).")]
        [SerializeField, Min(0f)] private float sillLift = 0.8f;
        [Tooltip("How far toward the wall she moves to sit on the sill (metres).")]
        [SerializeField] private float sillDepth = 0.25f;
        [SerializeField, Min(0.01f)] private float sillBlendSeconds = 0.9f;

        [Header("Cough")]
        [SerializeField, Min(0.5f)] private float minCoughInterval = 2.5f;
        [SerializeField, Min(0.5f)] private float maxCoughInterval = 5f;

        private Animator animator;
        private LocomotionTracker tracker;
        private CoughTimer cough;
        private Vector3 restPosition;
        private float yaw;
        private float sillAmount;
        private bool shown = true;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            tracker = new LocomotionTracker(speedSmoothTime, teleportDistance);
            var random = new System.Random();
            cough = new CoughTimer(() => (float)random.NextDouble(), minCoughInterval, maxCoughInterval);
            restPosition = transform.localPosition;
            SetShown(false);
        }

        // The controller builds its model in its own Awake, so facing is read here, after every Awake.
        private void Start()
        {
            yaw = LocomotionMath.FacingYaw(noor.FacingRight);
            transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Vector3 p = noor.transform.position;
            tracker.Step(new Vector2(p.x, p.y), dt);
            float speed = tracker.Speed;
            bool moving = speed > movingThreshold;

            NoorPose pose = NoorPoseRules.Choose(noor.State, moving);
            SetShown(pose != NoorPose.Hidden);

            animator.SetFloat(CharacterParams.Speed, LocomotionMath.NormalizedSpeed(speed, transform.lossyScale.y));
            animator.SetBool(CharacterParams.Scared, pose == NoorPose.Scared);
            animator.SetBool(CharacterParams.Sit, pose == NoorPose.Sitting);

            bool canCough = pose == NoorPose.Scared || (pose == NoorPose.Standing && !moving);
            if (cough.Step(dt, canCough, FireLevelAt(p.x))) animator.SetTrigger(CharacterParams.Cough);

            float targetYaw = pose == NoorPose.Sitting ? sitYaw : LocomotionMath.FacingYaw(noor.FacingRight);
            yaw = LocomotionMath.TurnYaw(yaw, targetYaw, turnDegreesPerSecond, dt);

            sillAmount = Mathf.MoveTowards(sillAmount, pose == NoorPose.Sitting ? 1f : 0f, dt / sillBlendSeconds);
            transform.localPosition = restPosition + new Vector3(0f, sillLift * sillAmount, sillDepth * sillAmount);
            transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private int FireLevelAt(float x)
        {
            FireRoom[] rooms = fire.Rooms;
            FireRoom nearest = null;
            float best = float.MaxValue;
            for (int i = 0; i < rooms.Length; i++)
            {
                float d = Mathf.Abs(rooms[i].transform.position.x - x);
                if (d >= best) continue;
                best = d;
                nearest = rooms[i];
            }
            return nearest != null ? nearest.TargetLevel : 0;
        }

        private void SetShown(bool value)
        {
            if (shown == value) return;
            shown = value;
            for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = value;
        }
    }
}
