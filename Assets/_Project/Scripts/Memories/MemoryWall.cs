using System;
using LitMotion;
using TMPro;
using ThreeMatches.Core;
using ThreeMatches.Player;
using ThreeMatches.Strips;
using UnityEngine;

namespace ThreeMatches.Memories
{
    /// <summary>
    /// The four framed photos (GAME_SPEC §8). Shows captions on hover / E, and while a match is lit
    /// lets the player choose one frame (mouse click, or A/D + E). The wall never decides what a pick
    /// means; <see cref="MatchController"/> does.
    /// </summary>
    public sealed class MemoryWall : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameInput input;
        [SerializeField] private StripLayoutController layout;
        [SerializeField] private RescueStateService rescue;
        [SerializeField] private Interactable strikePrompt;
        [SerializeField] private MemoryFrame[] frames;
        [SerializeField] private TMP_Text captionLabel;
        [SerializeField] private ParticleSystem ashParticles;

        [Header("Timing")]
        [SerializeField, Min(0.1f)] private float burnSeconds = 2f;
        [SerializeField, Min(0f)] private float captionHoldSeconds = 4f;
        [SerializeField, Min(0.01f)] private float captionFade = 0.2f;
        [SerializeField, Range(0.1f, 1f)] private float axisThreshold = 0.5f;

        private MotionHandle captionHandle;
        private MemoryFrame hovered;
        private MemoryFrame selected;
        private MemoryFrame captionOwner;
        private MemoryFrame heldCaption;
        private float heldUntil;
        private bool choosing;
        private bool captionShown;
        private int lastAxisDir;

        /// <summary>Raised when the player confirms an unburned frame while choosing.</summary>
        public event Action<MemoryId> Picked;

        public bool IsChoosing => choosing;

        /// <summary>True when Rafi stands close enough to the wall to strike a match.</summary>
        public bool InReach(Vector3 position) => strikePrompt.InReach(position.x, position.y);

        private void Awake()
        {
            captionLabel.alpha = 0f;
        }

        private void OnEnable()
        {
            for (int i = 0; i < frames.Length; i++) frames[i].Interactable.Interacted += OnFrameInteracted;
        }

        private void OnDisable()
        {
            for (int i = 0; i < frames.Length; i++) frames[i].Interactable.Interacted -= OnFrameInteracted;
        }

        private void OnDestroy()
        {
            if (captionHandle.IsActive()) captionHandle.Cancel();
        }

        private void Update()
        {
            hovered = FindHovered();

            if (choosing) UpdateChoosing();

            MemoryFrame wanted = hovered;
            if (wanted == null && heldCaption != null && Time.time < heldUntil) wanted = heldCaption;
            if (wanted == null && choosing) wanted = selected;
            ShowCaption(wanted);
        }

        /// <summary>Updates the strike prompt and which frames can be read with E (after any state change).</summary>
        public void Refresh(bool canStrike)
        {
            RescueState state = rescue.State;
            for (int i = 0; i < frames.Length; i++)
            {
                MemoryFrame f = frames[i];
                f.Interactable.SetAvailable(!canStrike && !choosing && !state.IsBurned(f.Memory));
            }
            strikePrompt.SetAvailable(canStrike && !choosing);
        }

        /// <summary>Snaps every frame to the look the rescue state says (used after debug jumps).</summary>
        public void SyncFrames()
        {
            RescueState state = rescue.State;
            for (int i = 0; i < frames.Length; i++) frames[i].SetBurned(state.IsBurned(frames[i].Memory));
            heldCaption = null;
            ShowCaption(null);
        }

        public void BeginChoosing()
        {
            choosing = true;
            strikePrompt.SetAvailable(false);
            for (int i = 0; i < frames.Length; i++) frames[i].Interactable.SetAvailable(false);
            lastAxisDir = 0;
            SetSelected(FirstUnburned());
        }

        public void EndChoosing()
        {
            choosing = false;
            SetSelected(null);
        }

        /// <summary>Feedback for a refused pick (a burned frame).</summary>
        public void Refuse(MemoryId memory)
        {
            MemoryFrame f = Find(memory);
            if (f != null) f.PlayRefuse();
        }

        public void PlayBurn(MemoryId memory, Action finished)
        {
            MemoryFrame f = Find(memory);
            choosing = false;
            SetSelected(null);
            if (f == null)
            {
                finished?.Invoke();
                return;
            }

            ashParticles.transform.position = f.transform.position;
            ashParticles.Play();
            f.PlayBurn(burnSeconds, finished);
        }

        /// <summary>Drops any pick or burn in progress without touching the rescue state.</summary>
        public void CancelAll()
        {
            choosing = false;
            SetSelected(null);
            ashParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void UpdateChoosing()
        {
            if (hovered != null && !hovered.IsBurned && hovered != selected) SetSelected(hovered);

            int dir = ReadAxisDirection();
            if (dir != 0 && dir != lastAxisDir) StepSelection(dir);
            lastAxisDir = dir;

            if (input.Click.WasPressedThisFrame() && hovered != null) TryPick(hovered);
            else if (input.Interact.WasPressedThisFrame() && selected != null) TryPick(selected);
        }

        private void TryPick(MemoryFrame frame)
        {
            if (frame.IsBurned || rescue.State.IsBurned(frame.Memory))
            {
                frame.PlayRefuse();
                return;
            }
            Picked?.Invoke(frame.Memory);
        }

        private int ReadAxisDirection()
        {
            float axis = input.Move.ReadValue<float>();
            if (axis > axisThreshold) return 1;
            if (axis < -axisThreshold) return -1;
            return 0;
        }

        private void StepSelection(int dir)
        {
            int start = Array.IndexOf(frames, selected);
            for (int step = 1; step <= frames.Length; step++)
            {
                int index = ((start + dir * step) % frames.Length + frames.Length) % frames.Length;
                if (frames[index].IsBurned) continue;
                SetSelected(frames[index]);
                return;
            }
        }

        private MemoryFrame FirstUnburned()
        {
            for (int i = 0; i < frames.Length; i++) if (!frames[i].IsBurned) return frames[i];
            return null;
        }

        private void SetSelected(MemoryFrame frame)
        {
            if (frame == selected) return;
            if (selected != null) selected.SetSelected(false);
            selected = frame;
            if (selected != null) selected.SetSelected(true);
        }

        private MemoryFrame FindHovered()
        {
            Camera cam = layout.GetCamera(StripId.Present);
            if (cam == null || !cam.enabled || input.Point == null) return null;

            Vector2 mouse = input.Point.ReadValue<Vector2>();
            if (!cam.pixelRect.Contains(mouse)) return null;

            Ray ray = cam.ScreenPointToRay(mouse);
            MemoryFrame best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < frames.Length; i++)
            {
                float d = frames[i].Raycast(ray);
                if (d >= 0f && d < bestDistance)
                {
                    bestDistance = d;
                    best = frames[i];
                }
            }
            return best;
        }

        private void OnFrameInteracted(Interactable who)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i].Interactable != who) continue;
                heldCaption = frames[i];
                heldUntil = Time.time + captionHoldSeconds;
                return;
            }
        }

        private void ShowCaption(MemoryFrame frame)
        {
            if (frame != null && frame == captionOwner && captionShown && captionLabel.text == frame.CurrentCaption) return;
            if (frame == null && !captionShown) return;

            if (frame != null)
            {
                captionOwner = frame;
                captionLabel.text = frame.CurrentCaption;
            }
            captionShown = frame != null;
            if (captionHandle.IsActive()) captionHandle.Cancel();
            captionHandle = LMotion.Create(captionLabel.alpha, captionShown ? 1f : 0f, captionFade)
                .WithEase(Ease.InOutSine)
                .Bind(captionLabel, static (a, label) => label.alpha = a);
        }

        private MemoryFrame Find(MemoryId memory)
        {
            for (int i = 0; i < frames.Length; i++) if (frames[i].Memory == memory) return frames[i];
            return null;
        }
    }
}
