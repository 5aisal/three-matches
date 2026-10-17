# THREE MATCHES: TRAILER SPEC (use only after the game is finished)

You (Claude Code) produce the full competition trailer end to end. The team will only play
the game when asked and watch the final cut once to approve it.

## Hard rules (from the event handbook)
- Runtime 2:58 (never over 3:00).
- At least 2:00 of ACTUAL GAMEPLAY, directly screen-recorded. Target 2:05.
  Gameplay segments: cuts only. No speed changes, no reversing, no re-rendering strips
  separately, no post colour grading, no overlays except light film grain.
- Teaser/stylised material (cards, cinematic shots, slow motion) max ~0:55, spread through.
- Must clearly show HOW each theme (Fire, Shadow, Time) is used in the game.
- Do NOT show any ending. Show the final-choice screen at most.
- Only free licensed assets (CC0 / CC-BY / OFL / team-made, see GAME_SPEC §25). No film
  soundtracks. Log sources in Trailer/CREDITS.txt.

## The game (context)
Rafi, an old man, strikes matches at a wall of photos of his daughter Noor, who died in a
house fire in 1996. Each match burns one memory and cracks the screen open into the past,
where he exists only as a shadow and has 40 seconds to guide her. The second match opens the
future, which changes live with his choices. The last match decides how it ends.

## Folder layout
Trailer/raw, Trailer/teaser, Trailer/audio, Trailer/gfx, Trailer/build, Trailer/final,
Trailer/inputs, Trailer/CREDITS.txt  (Trailer/ is at the repo root, git-ignored except
CREDITS.txt and scripts).

## Beat sheet (G = gameplay, T = teaser)
| Time | Type | Shot | Sound / text |
|---|---|---|---|
| 0:00-0:06 | T | Macro: a match strikes in darkness (in-engine, or teaser/live_match.mp4 if provided) | Scrape, hiss. Text: "You've watched 89 trailers today." |
| 0:06-0:10 | T | Black, a small flame | Text: "This one starts with three matches." |
| 0:10-0:32 | G | Present: Rafi in the cold house, the Memory Wall captions, upstairs, the matchbox note | Cold room tone, footsteps, L01 subtitle |
| 0:32-0:37 | G | Strike, choose a photo, it burns, the screen cracks to two strips | Strike, burn, bass hit |
| 0:37-0:42 | T | Card | "FIRE" / "Every match lights the past - and burns a memory." |
| 0:42-1:17 | G | Past: the shadow on the walls, the closet, Noor panics, humming, she follows, fire spreading | Crackle rising, humming |
| 1:17-1:22 | T | Card | "SHADOW" / "In the past, you are only a shadow." |
| 1:22-1:52 | G | Second match, crack to three strips, the future changing (shoes, drawing), forcing the window | Chimes on future changes |
| 1:52-1:57 | T | Card | "TIME" / "Three times. One house. One night to change." |
| 1:57-2:30 | G | Fast montage of gameplay; ends on the last match lit, three strips glowing, hover texts | Music peaks, then drops |
| 2:30-2:38 | T | Slow-motion trailer-mode shots: a photo curling, Noor's silhouette, the flame | Low drone |
| 2:38-2:48 | T | Black | Noor (whisper): "...Papa? Is that you?" Text: "What would you burn to save her?" |
| 2:48-2:58 | T | The match on a table, still burning | Title THREE MATCHES. Replay the 0:00 strike sound. |
Gameplay 2:05, teaser 0:53, runtime 2:58. Snap cuts to music beats (within 150 ms) unless
that breaks the gameplay minimum.

## Look and sound
- Cards: Cormorant Garamond, white on black, max 7 words, 12-frame fade in, hard cut out.
- Crack transitions in gameplay are the game's own; don't add extra ones on gameplay.
- Teaser shots only: letterbox 2.39:1, light grain, vignette.
- Music bed: 'Filaments' by Scott Buckley (CC-BY 4.0), in Assets/_Project/Audio/Music/. It builds
  to a big finish: line its peak up with 1:57-2:30. Credit it in Trailer/CREDITS.txt, at the end
  card, and in the Google Drive description: 'Filaments' by Scott Buckley - released under
  CC-BY 4.0. www.scottbuckley.com.au. Add synthesized bass hits and risers on top.
- Duck music -8 dB under gameplay audio. Master -14 LUFS integrated, -1 dBTP.

## Quality checks
- Duration <= 2:58; gameplay seconds (from the EDL) >= 125.
- ffmpeg blackdetect: no unintended black > 0.5 s.
- Loudness within spec. Text on screen >= 1.5 s. No gameplay shot used twice.
- Contact sheet (1 frame / 2 s) reviewed; fix anything unreadable or repetitive.
- Report: duration, gameplay seconds, sources.

## Prompts (run after the game is built; /clear between each)
T0 Tools:
Read TRAILER.md. Install ffmpeg (winget), Python 3.11 venv in Trailer/.venv with moviepy
numpy scipy librosa soundfile pillow opencv-python. Download Cormorant Garamond into
Trailer/gfx/fonts. Create the folder layout. Verify every tool runs and report.

T1 Unity trailer tools:
Read TRAILER.md and CLAUDE.md. Add: (1) F7 records my inputs with timestamps to
Trailer/inputs/<run>.json and a deterministic replayer (fixed seed/timestep); (2) Unity
Recorder (com.unity.recorder) with menu "Trailer > Record Gameplay Take" that replays a
chosen input file and records the Game view to Trailer/raw (1920x1080, 60 fps, audio);
(3) Cinemachine teaser rigs + menu "Trailer > Record Teaser Shots" to Trailer/teaser: match
striking macro, match burning on a table (10 s loop), a photo curling in slow motion, Noor
silhouette in firelight, slow push-ins on the three strips. F9 trailer mode hides UI.
Fix all console errors and tell me exactly what to play.

T2 Capture (after I record one clean run with F7):
Read TRAILER.md. Record the gameplay take(s) and teaser shots. Check each file plays, isn't
black (blackdetect) and has audio. Make contact sheets and look at them. Re-record broken ones.

T3 Sound and graphics:
Read TRAILER.md. Make all cards, synthesized hits/risers/drones, and the music bed edit.
Show me one image of each card.

T4 Edit:
Read TRAILER.md. Detect beats, choose the best gameplay moments per block (no repeats),
write an EDL (JSON) that follows the beat sheet exactly, render a 720p preview, review a
contact sheet like a trailer editor, fix pacing/readability, 2 improvement passes. Report
gameplay seconds and runtime.

T5 Final:
Read TRAILER.md. Render 1920x1080 H.264 to Trailer/final/ThreeMatches_Trailer.mp4, master
audio, run every quality check, report results and sources.
