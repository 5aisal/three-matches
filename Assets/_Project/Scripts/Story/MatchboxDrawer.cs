using System.Collections;
using ThreeMatches.Core;
using ThreeMatches.Player;
using ThreeMatches.UI;
using UnityEngine;

namespace ThreeMatches.Story
{
    /// <summary>
    /// Noor's drawer: shows the note (L02), then Rafi's line (L03), and sets the HasMatches flag.
    /// Text comes from GAME_SPEC §2.
    /// </summary>
    public sealed class MatchboxDrawer : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private GameFlagsService flags;
        [SerializeField] private NoteCardUI noteCard;
        [SerializeField] private SubtitleSystem subtitles;
        [SerializeField] private RafiController rafi;
        [SerializeField, TextArea] private string noteText = "Papa, for when it gets dark.\n- Noor";
        [SerializeField] private string foundLine = "Three matches.";
        [SerializeField, Min(0f)] private float noteMaxSeconds = 8f;
        [SerializeField, Min(0f)] private float foundLineSeconds = 3f;

        private void OnEnable() => interactable.Interacted += OnInteracted;
        private void OnDisable() => interactable.Interacted -= OnInteracted;

        private void OnInteracted(Interactable _)
        {
            interactable.SetAvailable(false);
            StartCoroutine(Sequence());
        }

        private IEnumerator Sequence()
        {
            rafi.InputLocked = true;
            yield return noteCard.ShowRoutine(noteText, noteMaxSeconds);
            rafi.InputLocked = false;

            flags.Flags.Set(GameFlags.HasMatches);
            subtitles.Show(foundLine, foundLineSeconds);
        }
    }
}
