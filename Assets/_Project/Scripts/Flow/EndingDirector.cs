using System;
using System.Collections;
using TMPro;
using ThreeMatches.Core;
using ThreeMatches.Memories;
using ThreeMatches.Strips;
using ThreeMatches.UI;
using UnityEngine;

namespace ThreeMatches.Flow
{
    /// <summary>
    /// Plays the three endings as simple scripted sequences across the strips (GAME_SPEC §16).
    /// Placeholder staging: coloured silhouettes, fades and captions on panels over the live strips.
    /// </summary>
    public sealed class EndingDirector : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField] private float captionSize = 34f;
        [SerializeField] private Vector2 adultSize = new Vector2(70f, 150f);
        [SerializeField] private Vector2 childSize = new Vector2(48f, 100f);
        [SerializeField] private Color fireBackdrop = new Color(0.28f, 0.09f, 0.02f, 0.9f);
        [SerializeField] private Color coldBackdrop = new Color(0.04f, 0.07f, 0.14f, 0.9f);
        [SerializeField] private Color morningBackdrop = new Color(0.32f, 0.32f, 0.3f, 0.9f);

        [Header("Timing (seconds)")]
        [SerializeField, Min(0.1f)] private float backdropFade = 0.9f;
        [SerializeField, Min(0.1f)] private float beat = 2.2f;
        [SerializeField, Min(0.1f)] private float walk = 2.4f;
        [SerializeField, Min(0.1f)] private float mergeSeconds = 0.5f;
        [SerializeField, Min(0.1f)] private float cFadeToBlack = 2.5f;
        [SerializeField, Min(0.1f)] private float cHold = 4.5f;

        private readonly StripPanel[] panels = new StripPanel[StripLayoutCalculator.StripCount];
        private StripLayoutController layout;
        private RescueStateService rescue;
        private MemoryWall wall;
        private SubtitleSystem subtitles;
        private Canvas panelCanvas;
        private bool following;

        internal void Build(TMP_FontAsset font, Canvas canvas, StripLayoutController stripLayout, RescueStateService rescueService,
            MemoryWall memoryWall, SubtitleSystem subtitleSystem)
        {
            layout = stripLayout;
            rescue = rescueService;
            wall = memoryWall;
            subtitles = subtitleSystem;
            panelCanvas = canvas;
            for (int i = 0; i < panels.Length; i++) panels[i] = new StripPanel(canvas.transform, font, "Panel_" + (StripId)i, captionSize);
        }

        private void OnDisable()
        {
            StopFollowing();
        }

        /// <summary>Plays the whole ending. Wait on the returned routine; it ends when the strips are ready to merge.</summary>
        public IEnumerator Play(EndingPlan plan)
        {
            StartFollowing();
            for (int i = 0; i < panels.Length; i++) panels[i].SetActive(true);
            FollowAll();

            switch (plan.Ending)
            {
                case EndingId.WalkIntoFire: yield return PlayA(plan); break;
                case EndingId.SaveHerForgetHer: yield return PlayB(plan); break;
                default: yield return PlayC(); break;
            }
        }

        /// <summary>Reverse crack: a flash, then the strips close into one screen (the Present strip).</summary>
        public IEnumerator MergeToOneScreen(Action flash)
        {
            flash?.Invoke();
            layout.SetStage(1);
            panels[(int)StripId.Present].FadeBackdrop(new Color(0f, 0f, 0f, 0f), mergeSeconds);
            yield return new WaitForSeconds(mergeSeconds);
            for (int i = 0; i < panels.Length; i++) panels[i].Clear();
            StopFollowing();
        }

        /// <summary>Drops every ending visual (used when "Choose again" is pressed).</summary>
        public void ClearAll()
        {
            for (int i = 0; i < panels.Length; i++) panels[i].Clear();
            StopFollowing();
        }

        // ---- A: WALK INTO THE FIRE (Past) ----------------------------------------------------

        private IEnumerator PlayA(EndingPlan plan)
        {
            StripPanel past = panels[(int)StripId.Past];
            StripPanel present = panels[(int)StripId.Present];
            StripPanel future = panels[(int)StripId.Future];

            past.FadeBackdrop(fireBackdrop, backdropFade);
            present.FadeBackdrop(coldBackdrop, backdropFade);
            future.FadeBackdrop(morningBackdrop, backdropFade);

            Actor flames = past.AddActor("flames", new Color(1f, 0.45f, 0.1f, 0.55f), new Vector2(360f, 200f), new Vector2(0.5f, 0.45f), 22f);
            Actor shadow = past.AddActor("Rafi's shadow", new Color(0f, 0f, 0f, 0.85f), adultSize, new Vector2(0.12f, 0.45f));
            Actor noor = past.AddActor("Noor", new Color(0.92f, 0.88f, 0.78f), childSize, new Vector2(0.75f, 0.42f));
            Actor rafi = present.AddActor("Rafi", new Color(0.12f, 0.14f, 0.2f), adultSize, new Vector2(0.5f, 0.45f));
            Actor photo = future.AddActor("his photo", new Color(0.7f, 0.68f, 0.6f), new Vector2(80f, 100f), new Vector2(0.5f, 0.62f), 22f);
            Actor noorAdult = future.AddActor("Noor, grown", new Color(0.85f, 0.8f, 0.72f), adultSize, new Vector2(0.28f, 0.4f));
            Actor candle = future.AddActor("candle", new Color(1f, 0.75f, 0.3f, 0f), new Vector2(20f, 44f), new Vector2(0.5f, 0.32f), 20f);
            yield return null;

            past.SetCaption("The shadow on the wall.");
            yield return new WaitForSeconds(beat);

            shadow.Color = new Color(0.04f, 0.04f, 0.06f, 1f);
            past.SetCaption("It becomes solid.");
            yield return new WaitForSeconds(beat * 0.6f);

            shadow.MoveTo(new Vector2(0.68f, 0.45f), walk);
            yield return new WaitForSeconds(walk);

            Vector2 exit = plan.ExitDone ? new Vector2(0.93f, 0.7f) : new Vector2(0.5f, 0.45f);
            past.SetCaption(plan.ExitDone ? "He carries her out through the window." : "He carries her out through the flames.");
            shadow.MoveTo(exit, walk);
            noor.MoveTo(exit, walk);
            rafi.FadeTo(0f, walk);
            present.SetCaption("The room is empty.");
            yield return new WaitForSeconds(walk);

            subtitles.Show(ScriptLines.L08);
            yield return new WaitForSeconds(beat * 1.2f);

            if (plan.SongBurnedBefore)
            {
                subtitles.Show(EndingRules.NoorSongLineA);
                yield return new WaitForSeconds(beat);
            }

            future.SetCaption("She grew up. She lights a candle under his photo.");
            candle.FadeTo(1f, 1f);
            noorAdult.MoveTo(new Vector2(0.42f, 0.4f), walk);
            yield return new WaitForSeconds(beat * 1.4f);
            photo.FadeTo(1f, 0.1f);
            flames.FadeTo(0.2f, 1f);
        }

        // ---- B: SAVE HER, FORGET HER (Present) ------------------------------------------------

        private IEnumerator PlayB(EndingPlan plan)
        {
            StripPanel past = panels[(int)StripId.Past];
            StripPanel present = panels[(int)StripId.Present];
            StripPanel future = panels[(int)StripId.Future];

            present.SetCaption(plan.BurnedNow.Count == 1 ? "A memory burns." : plan.BurnedNow.Count + " memories burn.");
            for (int i = 0; i < plan.BurnedNow.Count; i++)
            {
                MemoryId memory = plan.BurnedNow[i];
                bool done = false;
                rescue.State.Burn(memory);
                wall.PlayBurn(memory, () => done = true);
                while (!done) yield return null;
                yield return new WaitForSeconds(0.4f);
            }

            past.FadeBackdrop(fireBackdrop, backdropFade);
            future.FadeBackdrop(morningBackdrop, backdropFade);
            Actor noor = past.AddActor("Noor", new Color(0.92f, 0.88f, 0.78f), childSize, new Vector2(0.55f, 0.42f));
            Actor fireman = past.AddActor("fireman", new Color(0.08f, 0.08f, 0.1f), adultSize, new Vector2(0.9f, 0.18f));
            Actor oldRafi = future.AddActor("Rafi, old", new Color(0.12f, 0.14f, 0.2f), adultSize, new Vector2(0.2f, 0.4f));
            Actor noorAdult = future.AddActor("Noor, grown", new Color(0.85f, 0.8f, 0.72f), adultSize, new Vector2(1.05f, 0.4f));
            Actor son = future.AddActor("her son", new Color(0.8f, 0.76f, 0.7f), childSize, new Vector2(1.15f, 0.36f));
            yield return null;

            past.SetCaption("Noor climbs out of the window.");
            noor.MoveTo(new Vector2(0.9f, 0.7f), walk);
            yield return new WaitForSeconds(walk);

            past.SetCaption("Into a fireman's arms.");
            noor.MoveTo(new Vector2(0.9f, 0.28f), walk);
            yield return new WaitForSeconds(walk * 0.8f);

            if (plan.SongBurnedBefore)
            {
                past.SetCaption("She hums the song as she leaves.");
                yield return new WaitForSeconds(beat);
            }
            fireman.MoveTo(new Vector2(1.1f, 0.18f), walk);
            noor.MoveTo(new Vector2(1.1f, 0.28f), walk);

            future.SetCaption("She visits, with her son.");
            noorAdult.MoveTo(new Vector2(0.55f, 0.4f), walk);
            son.MoveTo(new Vector2(0.7f, 0.36f), walk);
            yield return new WaitForSeconds(walk);

            subtitles.Show(ScriptLines.L09);
            yield return new WaitForSeconds(beat);
            subtitles.Show(ScriptLines.L10);
            yield return new WaitForSeconds(beat * 1.4f);
            oldRafi.FadeTo(0.6f, 0.1f);
        }

        // ---- C: KEEP HER (Future) -------------------------------------------------------------

        private IEnumerator PlayC()
        {
            StripPanel past = panels[(int)StripId.Past];

            past.SetCaption("He blows out the match.");
            yield return new WaitForSeconds(beat * 0.7f);
            past.FadeBackdrop(Color.black, cFadeToBlack);
            yield return new WaitForSeconds(cFadeToBlack);

            subtitles.Show(ScriptLines.L11);
            yield return new WaitForSeconds(cHold);
        }

        // ---- panel plumbing --------------------------------------------------------------------

        private void StartFollowing()
        {
            if (following) return;
            following = true;
            layout.LayoutUpdated += FollowAll;
        }

        private void StopFollowing()
        {
            if (!following) return;
            following = false;
            layout.LayoutUpdated -= FollowAll;
        }

        private void FollowAll()
        {
            for (int i = 0; i < panels.Length; i++) panels[i].Follow(layout.GetRect((StripId)i));
        }
    }
}
