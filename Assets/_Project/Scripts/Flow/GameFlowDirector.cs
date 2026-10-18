using System.Collections;
using LitMotion;
using TMPro;
using ThreeMatches.Core;
using ThreeMatches.Memories;
using ThreeMatches.Player;
using ThreeMatches.Strips;
using ThreeMatches.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThreeMatches.Flow
{
    /// <summary>
    /// Title -> Act 1 -> match 1 -> between -> match 2 -> final choice -> ending -> credits (GAME_SPEC §15-§17).
    /// The phase rules live in <see cref="GameFlowModel"/>; this component wires them to the scene.
    /// </summary>
    public sealed class GameFlowDirector : MonoBehaviour
    {
        // Set by F1 so the reloaded scene skips the title and starts Act 1 directly.
        private static bool skipTitleOnce;

        [Header("References")]
        [SerializeField] private GameInput input;
        [SerializeField] private RescueStateService rescue;
        [SerializeField] private MatchController matches;
        [SerializeField] private MemoryWall wall;
        [SerializeField] private StripLayoutController layout;
        [SerializeField] private RafiController rafi;
        [SerializeField] private SubtitleSystem subtitles;
        [SerializeField] private ControlsCardUI controlsCard;

        [Header("Content")]
        [SerializeField] private TMP_FontAsset uiFont;
        [SerializeField] private TextAsset creditsText;

        [Header("Final choice")]
        [SerializeField, Range(0.05f, 1f)] private float finalTimeScale = 0.2f;
        [SerializeField, Min(0.05f)] private float timeScaleSeconds = 0.6f;
        [SerializeField, Min(0f)] private float flashPeakAlpha = 0.85f;
        [SerializeField, Min(0.05f)] private float flashSeconds = 0.35f;

        [Header("Canvas sorting")]
        [SerializeField] private int panelsSortingOrder = 5;
        [SerializeField] private int topSortingOrder = 40;

        private GameFlowModel model;
        private TitleScreen title;
        private FinalChoiceUI finalChoice;
        private EndingDirector endings;
        private EndCardUI endCard;
        private Image flash;
        private MotionHandle timeHandle, flashHandle;
        private RescueSnapshot beforeFinalMatch;
        private EndingPlan currentPlan;
        private string defaultStrikeVerb;
        private bool syncPending;
        private bool restoringSnapshot;

        public GamePhase Phase => model.Phase;

        /// <summary>True while a screen owns the player (title, final choice, ending, credits); debug jumps wait.</summary>
        public bool IsBusy
        {
            get
            {
                switch (model.Phase)
                {
                    case GamePhase.Title:
                    case GamePhase.FinalChoice:
                    case GamePhase.Ending:
                    case GamePhase.EndCard:
                    case GamePhase.Credits:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private void Awake()
        {
            model = new GameFlowModel();
            defaultStrikeVerb = wall.StrikeVerb;
            BuildScreens();
        }

        private void OnEnable()
        {
            matches.OnMatchLit += OnMatchLit;
            matches.OnMatchOut += OnMatchOut;
            matches.OnFinalMatchLit += OnFinalMatchLit;
            rescue.State.Restored += OnStateRestored;
            model.PhaseChanged += OnPhaseChanged;
            endCard.ChooseAgainClicked += OnChooseAgain;
            endCard.CreditsClicked += OnCreditsClicked;
            endCard.BackClicked += OnCreditsBack;
        }

        private void OnDisable()
        {
            matches.OnMatchLit -= OnMatchLit;
            matches.OnMatchOut -= OnMatchOut;
            matches.OnFinalMatchLit -= OnFinalMatchLit;
            rescue.State.Restored -= OnStateRestored;
            model.PhaseChanged -= OnPhaseChanged;
            endCard.ChooseAgainClicked -= OnChooseAgain;
            endCard.CreditsClicked -= OnCreditsClicked;
            endCard.BackClicked -= OnCreditsBack;
        }

        private void OnDestroy()
        {
            if (timeHandle.IsActive()) timeHandle.Cancel();
            if (flashHandle.IsActive()) flashHandle.Cancel();
            Time.timeScale = 1f;
        }

        private void Start()
        {
            if (skipTitleOnce)
            {
                skipTitleOnce = false;
                model.DismissTitle();
                BeginAct1();
                return;
            }

            rafi.InputLocked = true;
            title.Show(OnTitleDismissed);
        }

        private void Update()
        {
            if (!syncPending) return;
            syncPending = false;
            model.SyncToState(rescue.State.MatchesUsed, matches.Session.IsWindowOpen);
        }

        /// <summary>F1: reloads the scene and starts Act 1 without the title.</summary>
        public void RestartAct1()
        {
            skipTitleOnce = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // ---- build ---------------------------------------------------------------------------

        private void BuildScreens()
        {
            Canvas panelCanvas = UiKit.CreateCanvas("Flow_Panels", transform, panelsSortingOrder);
            panelCanvas.gameObject.AddComponent<TrailerKeep>();
            Canvas topCanvas = UiKit.CreateCanvas("Flow_Screens", transform, topSortingOrder);

            endings = CreateChild<EndingDirector>("Endings");
            endings.Build(uiFont, panelCanvas, layout, rescue, wall, subtitles);

            finalChoice = CreateChild<FinalChoiceUI>("FinalChoice");
            finalChoice.Build(uiFont, topCanvas, input, layout);

            endCard = CreateChild<EndCardUI>("EndCard");
            endCard.Build(uiFont, topCanvas, input, creditsText != null ? creditsText.text : "");

            // Flash used by the reverse crack. Built after the cards so it draws on top of them.
            flash = UiKit.CreateImage("Flash", topCanvas.transform, new Color(1f, 1f, 1f, 0f));
            UiKit.Stretch(flash.rectTransform);

            title = CreateChild<TitleScreen>("Title");
            title.Build(uiFont, topCanvas);
        }

        private T CreateChild<T>(string childName) where T : Component
        {
            var go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }

        // ---- Act 1 and the rescue windows ------------------------------------------------------

        private void OnTitleDismissed()
        {
            model.DismissTitle();
            BeginAct1();
        }

        private void BeginAct1()
        {
            rafi.InputLocked = false;
            subtitles.Show(ScriptLines.L01);
            controlsCard.ShowIntro();
        }

        private void OnMatchLit(int number) => model.MatchLit(number);

        private void OnMatchOut(int number)
        {
            if (!model.MatchOut(number)) return;
            if (number == 2) subtitles.Show(ScriptLines.L07);
        }

        private void OnStateRestored()
        {
            // Debug jumps replace the state first and open the window afterwards, so look at it next frame.
            if (!restoringSnapshot) syncPending = true;
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            wall.SetStrikeVerb(phase == GamePhase.Final ? ScriptLines.StrikeLastMatchVerb : defaultStrikeVerb);
        }

        // ---- final choice ---------------------------------------------------------------------

        private void OnFinalMatchLit()
        {
            // A debug jump may not have been synced yet.
            if (model.Phase != GamePhase.Final) model.SyncToState(MatchSession.FinalMatchNumber - 1, false);
            if (!model.FinalMatchLit()) return;

            RescueSnapshot now = rescue.State.TakeSnapshot();
            // The snapshot is the state right before this strike: two matches used.
            beforeFinalMatch = new RescueSnapshot(now.BurnedMemories, now.CompletedSteps, now.PastTime,
                MatchSession.FinalMatchNumber - 1, now.NoorState);

            rafi.InputLocked = true;
            TweenTimeScale(finalTimeScale);
            layout.ShowEqualThirds();
            finalChoice.Show(rescue.State, OnEndingChosen);
        }

        private void OnEndingChosen(EndingId ending)
        {
            currentPlan = EndingRules.Build(rescue.State, ending);
            if (currentPlan == null || !model.ChooseEnding(ending))
            {
                finalChoice.Show(rescue.State, OnEndingChosen);
                return;
            }

            finalChoice.Hide();
            TweenTimeScale(1f);
            StartCoroutine(RunEnding(currentPlan));
        }

        private IEnumerator RunEnding(EndingPlan plan)
        {
            yield return endings.Play(plan);
            yield return endings.MergeToOneScreen(PlayFlash);

            model.EndingFinished();
            endCard.ShowTitleCard(plan.Title, SublineFor(plan));
        }

        private string SublineFor(EndingPlan plan)
        {
            MemoryId? kept = plan.Ending == EndingId.SaveHerForgetHer ? plan.KeptMemory : null;
            if (!kept.HasValue) return "";
            string caption = wall.CaptionOf(kept.Value).Trim().TrimEnd('.');
            return "He kept one thing: " + caption + ".";
        }

        // ---- end card ------------------------------------------------------------------------

        private void OnChooseAgain()
        {
            if (!model.ChooseAgain()) return;
            StartCoroutine(RestoreBeforeFinalMatch());
        }

        private IEnumerator RestoreBeforeFinalMatch()
        {
            endCard.HideTitleCard();
            endings.ClearAll();

            restoringSnapshot = true;
            rescue.State.Restore(beforeFinalMatch);
            restoringSnapshot = false;
            matches.DebugClearSession(); // drops the finished session, unlocks Rafi, re-syncs the photo frames

            layout.SetStage(3);
            while (layout.IsBusy) yield return null;
            layout.RestoreLayout();
        }

        private void OnCreditsClicked()
        {
            if (!model.ShowCredits()) return;
            endCard.SetCardVisible(false);
            endCard.ShowCredits();
        }

        private void OnCreditsBack()
        {
            if (!model.BackToEndCard()) return;
            endCard.HideCredits();
            endCard.SetCardVisible(true);
        }

        // ---- effects -------------------------------------------------------------------------

        private void TweenTimeScale(float target)
        {
            if (timeHandle.IsActive()) timeHandle.Cancel();
            timeHandle = LMotion.Create(Time.timeScale, target, timeScaleSeconds)
                .WithEase(Ease.InOutSine)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(static t => Time.timeScale = t);
        }

        private void PlayFlash()
        {
            if (flashHandle.IsActive()) flashHandle.Cancel();
            flashHandle = LMotion.Create(flashPeakAlpha, 0f, flashSeconds)
                .WithEase(Ease.OutCubic)
                .Bind(flash, static (a, image) => image.color = new Color(1f, 1f, 1f, a));
        }
    }
}
