# THREE MATCHES: Build prompts (paste one at a time)

How to run every prompt:
1. Type /clear first (fresh context).
2. Set the mode shown (Shift+Tab cycles: plan mode / normal). PLAN = review the plan, then approve.
3. Paste the prompt. Answer questions if it asks.
4. Do the "You check" list yourself in Unity. If it fails, describe exactly what you saw.
5. If it's still wrong after 2 corrections: Esc Esc (rewind) or /clear, then re-run the
   prompt with what you learned added at the end.

---

## B01 Packages and project settings  (mode: normal)
```
Read CLAUDE.md. Task: configure this fresh Unity 6 URP project. Nothing else.
1. Using the Package Manager (via the Unity MCP or Packages/manifest.json), make sure these are
   installed: Input System, Cinemachine, Unity Test Framework, TextMeshPro (import TMP
   Essentials), and LitMotion from git URL
   https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion
   (if that URL fails, use:
   https://github.com/yn01-dev/LitMotion.git?path=src/LitMotion/Assets/LitMotion).
   Set Active Input Handling to "Input System Package (New)".
2. Player settings: company "Shadowedarts", product "Three Matches", default resolution
   1920x1080, fullscreen window, colour space Linear.
3. Create the folders: Assets/_Project/{Scenes,Scripts,Data,Prefabs,Art,Audio,Materials,
   Fonts,Tests/EditMode}, plus docs/ at the repo root if missing.
4. Create Assets/_Project/Scenes/Main.unity, add it as the only scene in Build Settings.
5. Create docs/CREDITS.md with a table header (Asset | Author | Licence | Link).
Done when: project compiles, console has 0 errors, Main scene opens. Show me the final
manifest.json dependencies and the console status. Then commit "chore: project setup".
```
You check: Play mode runs (empty scene). Edit > Project Settings > Player shows the values.

## B02 Core rules + tests  (mode: PLAN)
```
Read CLAUDE.md and docs/GAME_SPEC.md sections §8, §9, §10, §13, §15, §16.
Task: write ONLY the pure C# game rules (no MonoBehaviours, no scene work) in
Assets/_Project/Scripts/Core, plus EditMode tests.
- enums: MemoryId {Closet, Song, Window, Candle}, RescueStep {Find, Calm, Exit},
  EndingId {WalkIntoFire, SaveHerForgetHer, KeepHer}, NoorState (§12).
- RescueState: burned memories (ordered), completed steps, pastTime, matchesUsed,
  noorState, CanBurn(id), Burn(id), CompleteStep(step) (only if its memory is burned),
  snapshot/restore for "Choose again".
- FireTimeline: given pastTime and whether Candle is burned, returns fire level (0-3) per
  room (Bedroom, Hallway, Landing) using the §10 table.
- FinalChoiceRules: MissingSteps, Cost N, IsPresentAvailable, MemoriesBurnedByEndingB (order
  rule from §15), and kept memories after each ending.
- Create the test assembly ThreeMatches.Tests.EditMode and tests covering: every row of the
  §10 table (with and without Candle); cost and availability for these burn paths:
  Closet+Song with Find+Calm done; Closet+Window with Find+Exit; Song+Window with Exit;
  Candle+Song with nothing done; CompleteStep refusing a step whose memory isn't burned;
  snapshot/restore.
First show me the plan (files, class APIs, test list). After I approve: implement, run the
tests through the Unity MCP, show the pass/fail counts, fix until all pass, commit
"feat(core): rescue rules and tests".
```
You check: Window > General > Test Runner > EditMode: all green.

## B03 Graybox house prefab  (mode: PLAN)
```
Read CLAUDE.md and docs/GAME_SPEC.md §5 and §19 (only the layout parts).
Task: build the house as a graybox prefab with Unity primitives only (no imported art yet).
- Prefab Assets/_Project/Prefabs/House.prefab: two-floor cross-section, front wall removed.
  Ground: front door | living room (Memory Wall area with 4 empty frame quads, armchair box,
  wall clock) | kitchen. Upper: landing with window | hallway with closet door | Noor's
  bedroom (bed, shelf with candle, curtains). Stairs from living room to landing.
- Every interactable gets an empty child named exactly: MemoryWall, Frame_Closet,
  Frame_Song, Frame_Window, Frame_Candle, Stairs, Drawer_Matchbox, ClosetDoor, StuckWindow,
  Candle, Curtains, FrontDoor, and a "ShadowWall" quad per upstairs room (back wall area
  where the shadow can move).
- Room sizes: each room 6 m wide, 3 m tall, 4 m deep.
- In Main.unity place three instances: House_1996 at (0,0,0), House_2026 at (100,0,0),
  House_2036 at (200,0,0). Add one camera per house (Cam_Past, Cam_Present, Cam_Future):
  perspective FOV 20, side-on, framing the whole house.
Done when: scene saved, 0 console errors. Capture a Game view screenshot from Cam_Present,
look at it, and confirm both floors are fully visible and readable. Commit
"feat(house): graybox house and cameras".
```
You check: each camera shows the full dollhouse; names match exactly.

## B04 Split-screen strips  (mode: PLAN)
```
Read CLAUDE.md and docs/GAME_SPEC.md §4.
Task: the strip layout system only.
- StripLayoutController (Scripts/Strips): manages Cam_Past/Cam_Present/Cam_Future viewport
  rects. Stage 1/2/3 and "active strip" per §4, animated resize 0.35 s ease in-out.
- Year labels per strip (TextMeshPro, top-left).
- Crack transition (placeholder version): white flash 2 frames + simple line overlay +
  camera shake; expose an event OnCrackFinished. Real VFX comes later.
- Inactive-past look: a method that desaturates/darkens the Past strip (URP Volume per camera
  or a simple overlay).
- Temporary debug keys: 1/2/3 = stage, Tab = cycle active strip.
Done when: in Play mode the keys show stages 1-3 with correct sizes and order (Past top,
Present middle, Future bottom). Capture screenshots of each stage and check them. 0 console
errors. Commit "feat(strips): split-screen layout".
```
You check: press 1, 2, 3, Tab in Play mode; sizes and order are right; no flicker.

## B05 Rafi in the present  (mode: normal)
```
Read CLAUDE.md and docs/GAME_SPEC.md §6 and §7.
Task: present-day player only.
- Input actions asset (new Input System) with Move, Vertical, Interact, Strike, Hum, Pause,
  Point/Click, matching §6.
- RafiController: walks along the floor in House_2026 at 2 m/s, uses Stairs with W/S, faces
  move direction (flip sprite). Placeholder silhouette: a dark capsule-shaped sprite quad.
- Interactable component + prompt UI (key icon + verb above the object).
- Controls card UI shown 6 s at start (§6 table) and on Esc.
- Drawer_Matchbox interaction: shows note text L02, then L03 subtitle; sets a flag
  "HasMatches". Subtitle system: bottom-centre, serif, dark backing.
Done when: I can walk the whole present house, go up/down stairs, open the drawer and see the
note and subtitle. 0 console errors. Commit "feat(player): rafi movement and interaction".
```
You check: walk everywhere; no getting stuck; prompts appear only near objects.

## B06 Memory Wall + matches  (mode: PLAN)
```
Read CLAUDE.md and docs/GAME_SPEC.md §8, §9 and §22. Use RescueState from Scripts/Core.
Task: Memory Wall and match striking (no past gameplay yet).
- Frames show placeholder photos (coloured quads) and captions on hover/E.
- At the MemoryWall with HasMatches: F strikes a match (only if matches remain). Then the
  player picks an unburned photo (mouse click or A/D + E). Picked photo plays a 2 s burn
  (placeholder: tint to black + scale pulse + ash particles), RescueState.Burn(id), caption
  becomes "...".
- Match icons (3) bottom-right of the Present strip; flame timer at the bottom of the
  Present strip, 40 s, flicker in the last 8 s. Raise events: OnMatchLit, OnMatchOut.
- After the burn finishes on match 1: call StripLayoutController to crack to Stage 2; on
  match 2 to Stage 3. Match 3 only raises OnFinalMatchLit (no window).
- Create Scripts/Debug/DebugJumps (editor + development builds only) with the §22 jump keys
  F2, F3, F4, F6, implemented as far as the game exists now (set RescueState, matches used,
  stage, and light a match where needed). Later prompts extend it; keep it working.
Done when: I can strike 2 matches, burn 2 different photos (a burned one can't be picked
again), see the stage cracks, and the timers run out. 0 errors, B02 tests still pass.
Commit "feat(memories): memory wall and matches".
```
You check: strike, pick, burn, crack, timer ends; try picking a burned frame (must refuse).

## B07 Past timeline + fire  (mode: normal)
```
Read CLAUDE.md and docs/GAME_SPEC.md §10. Use FireTimeline from Scripts/Core.
Task: the past fire visuals driven by past time.
- PastClock: advances RescueState.pastTime only between OnMatchLit and OnMatchOut.
- FireRoom component for Bedroom, Hallway, Landing in House_1996: levels 0-3 with simple
  particle fire + point light (flicker) + crackle audio placeholder per level.
- Stairs in House_1996 shown collapsed from the start.
- If CANDLE is burned: play a 3 s "vision" in the Past strip (curtain drifting onto the
  candle, then flame) the first time a window opens, and the 1.3x slower schedule applies.
- When the match is out the Past strip uses the inactive look from B04 and fire visuals
  freeze.
- Debug overlay (F5) showing pastTime and fire levels.
Done when: over two 40 s windows the fire follows the §10 table (check with F5), freezes
between windows, and resumes. 0 errors. Commit "feat(fire): past timeline and fire".
```
You check: with F5, compare times to the §10 table; freeze between matches.

## B08 The shadow  (mode: PLAN)
```
Read CLAUDE.md and docs/GAME_SPEC.md §11 and §13.
Task: the player's shadow in the past.
- ShadowController: active only while a match burns (input switches from Rafi to the shadow
  on OnMatchLit and back on OnMatchOut). Moves on the ShadowWall quads of House_1996 at
  3 m/s, A/D and W/S, can move between the three upstairs rooms, blocked by fire level 3.
- Visual: Rafi silhouette sprite tinted black, soft edge, scale and alpha flicker driven by
  the nearest fire light.
- Interactions: ClosetDoor (only if Closet burned) completes FIND; StuckWindow (only if
  Window burned, hold E 2 s with progress ring) completes EXIT. Without the memory there is
  no prompt. Use RescueState.CompleteStep.
- Step completed feedback (placeholder): soft chime + 0.2 s slow-mo in the Past strip.
Done when: with F2 (Closet burned) I can open the closet but get no window prompt; with F6
I can force the window; without the memories nothing happens. 0 errors, tests pass.
Commit "feat(shadow): shadow movement and interactions".
```
You check: try every interaction with and without the memory burned.

## B09 Noor  (mode: PLAN)
```
Read CLAUDE.md and docs/GAME_SPEC.md §12 and §13.
Task: Noor's behaviour only.
- NoorController in House_1996 with the §12 state machine. Placeholder silhouette sprite
  (child, long hair). Moves only on the upper floor, along the floor line.
- Hidden until FIND. Panicked if the shadow comes within 3 m without humming (L04 subtitle,
  3 s freeze against the wall). Following if the shadow is within 4 m and H is held (only if
  Song burned): L05 once, follows at 1.5 m/s, stops 1.5 s after humming stops. First time
  she follows: complete CALM. AtWindow if she reaches the open window.
- Humming: while H is held and Song is burned, show a subtle musical-note particle on the
  shadow and play the humming loop placeholder.
- Her state is stored in RescueState and restored when the next window opens.
Done when: with F3 she follows me to the window when I hum; without humming she panics.
0 errors, tests pass. Commit "feat(noor): noor behaviour".
```
You check: approach without H (panic), with H (follow), stop humming (she stops).

## B10 Future strip  (mode: normal)
```
Read CLAUDE.md and docs/GAME_SPEC.md §14.
Task: the live Future preview in House_2036.
- Default dressing: dust sheets, empty armchair, old Rafi asleep (placeholder silhouette).
- FutureTableau component listens to RescueState step changes and fades in: shoes by the door
  (FIND), drawing on the fridge (CALM), adult woman silhouette at the window (EXIT). 1 s
  fade, soft chime, 1 s glow on the Future strip.
- If steps were completed before Stage 3 appeared, show them immediately when it appears.
Done when: with F3 the Future strip appears with the shoes already shown; humming until
Noor follows adds the drawing; with F6, forcing the window adds the woman with the chime.
0 errors. Commit "feat(future): live future preview".
```
You check: each step triggers its change once, never twice.

## B11 Game flow, final choice, endings  (mode: PLAN)
```
Read CLAUDE.md and docs/GAME_SPEC.md §2 (script lines), §15, §16, §17, §22.
Task: wire the full flow from title to credits. Use FinalChoiceRules and RescueState.
- GameFlowDirector: Title -> Act 1 -> match 1 -> between -> match 2 -> final -> ending,
  following §17. L01 at start, L07 before the final match.
- Final choice: on OnFinalMatchLit slow time to 0.2, all strips glow, hover text per §15
  (with the real N), Present greyed out with "Not enough left to give." when unavailable,
  click to choose.
- Endings A, B, C as simple scripted sequences (placeholder animations are fine: fades,
  moving silhouettes, text) with the §16 variants. Then reverse crack to one screen, ending
  title card, buttons "Choose again" (restore the snapshot taken right before the final
  match) and "Credits" (credits screen reads docs/CREDITS.md content copied into a
  TextAsset).
- Finish all debug keys per §22 (F1-F6, plus F9 trailer mode hiding all UI).
Done when: I can play start to finish and reach all three endings (and the greyed-out case
via a Candle+Song path). Write an EditMode test that checks ending selection + variants
logic. 0 errors, all tests pass. Commit "feat(flow): full game loop and endings".
```
You check: play 4 full runs: A, B, C, and a run where Present is unavailable.

## >>> MILESTONE: the whole game is playable in graybox. Tag it: ask Claude
"create git tag v0.1-graybox and push". Everything below is polish. <<<

## B12 Art: house, textures, furniture  (mode: PLAN)
Before running: the Asset Kit (guide Part 6) is downloaded into the §25 folders.
```
Read CLAUDE.md and docs/GAME_SPEC.md §5, §19 and §25.
Task: replace the graybox house with real art. Keep every named child object, collider and
interaction working.
- Furniture, doors and windows: Kenney Furniture Kit / Building Kit in Assets/_Project/Art/Kenney/.
  Fix import scale so a door is ~2.1 m tall. Convert materials to URP Lit.
- Walls, floors, ceilings: simple meshes with PBR materials made from the textures in
  Assets/_Project/Art/Textures/ (wood floor, plaster/wallpaper walls). Texture import max
  size 1024. Materials in Assets/_Project/Materials/.
- Three prefab variants: House_1996 (warm, cluttered, family photos, toys), House_2026 (tidy,
  empty, Noor's room preserved like a shrine), House_2036 (dust sheets over furniture).
  Burn damage in House_1996's upstairs is a darkened, charred material variant that blends in
  with fire level (expose a 0-1 "char" value per room).
- Add all used assets to docs/CREDITS.md.
Done when: Game-view screenshots of all three strips look like a furnished dollhouse at
night; every debug jump F1-F6 still works. 0 errors, tests pass. Commit "feat(art): house".
```
You check: replay the full loop once; nothing broke; nothing pink (missing material).

## B12B Characters and animation  (mode: PLAN)
```
Read CLAUDE.md and docs/GAME_SPEC.md §11, §12, §19, §24 (Characters table), §25.
Task: replace placeholder characters with animated 3D characters.
- Models: Quaternius Universal Base Characters in Assets/_Project/Art/Characters/Quaternius/.
  Import as Humanoid. Rafi = Regular male; Noor = Teen female scaled 0.72 with long hair;
  adult Noor = Teen female 1.0; grandson = Teen scaled 0.62; fireman = Regular male, dark coat.
- Animations: Quaternius Universal Animation Library in Assets/_Project/Art/Animations/UAL/,
  retargeted via Humanoid. Pick the closest clips for every row of the §24 Characters table
  and list your choices. Build one Animator Controller per character with blend by speed.
- Rafi: dark desaturated materials, stoop (spine rotation in LateUpdate), walk at 0.75 speed,
  LookAt toward the hovered photo (weight 0.6), hold-match pose while a match is lit.
- Shadow-Rafi: duplicate of Rafi's rig with a pure black unlit material, flattened against the
  ShadowWall plane (~0.02 scale on the wall normal), alpha 0.85, flicker from the nearest
  fire light, animations driven by ShadowController movement.
- Noor: states from §12 mapped to clips (crouched scared idle, run, walk, cough, sit on sill).
- Keep gameplay logic unchanged; controllers only drive the Animator.
Done when: screenshots show every character in the right place; walking/idle blend smoothly;
the shadow reads as a cast shadow on the wall. 0 errors, tests pass.
Commit "feat(characters): animated characters and shadow".
```
You check: watch Rafi, the shadow and Noor in motion. Any sliding feet or T-poses? Report them.

## B13 Lighting and atmosphere  (mode: normal)
```
Read CLAUDE.md and docs/GAME_SPEC.md §19 and §4 (inactive past look).
Task: lighting and post-processing only.
- Present: one cold blue directional moonlight through the windows, very dark corners; the
  match is a warm point light in Rafi's hand.
- Past: warm amber room light at t=0, then fire lights take over (orange/red).
- Future: soft grey morning light that warms with each completed step.
- Realtime shadows only on flame lights. One URP Volume per strip camera (use layers):
  bloom, vignette, film grain (low), colour adjustments per time.
- Keep 60 fps at 1080p on this laptop; report frame time.
Done when: screenshots of the three strips show three clearly different moods and all text is
readable. 0 errors. Commit "feat(lighting): atmosphere per timeline".
```
You check: is everything readable? Does the fire glow on the walls behind the shadow?

## B13B Fire and VFX  (mode: PLAN)
```
Read CLAUDE.md and docs/GAME_SPEC.md §10, §24 (Environment and VFX table) and §25.
Task: all particle effects. Use the flipbooks in Assets/_Project/Art/VFX/ (UnityLabs,
CGHEVEN, Kenney).
- Room fire per level (0-3): CGHEVEN ground-fire flipbook for room fires, Unity Labs
  Flame02/Flame03 for small flames, embers rising (Kenney spark textures), smoke layer under
  the ceiling (Unity Labs wispy smoke). Additive/alpha-blended URP particle materials with
  texture-sheet animation set to the correct grid size. Level changes blend over 1.5 s.
- Candle flame (Unity Labs SmallFlame) in Noor's room; match flame in Rafi's hand; spark burst
  on strike; smoke wisp when a match goes out.
- Photo burn: dissolve shader (Shader Graph) with glowing noise edge, emission ramp, ash and
  ember particles, 2 s.
- Particles pause (simulation speed 0) when the past is inactive.
Done when: each effect looks right in a screenshot and costs < 2 ms GPU in the profiler.
0 errors. Commit "feat(vfx): fire, smoke, match and burn effects".
```
You check: does the fire look real and scary from the default camera distance?

## B14 Audio and music  (mode: normal)
```
Read CLAUDE.md and docs/GAME_SPEC.md §20 and §25.
Task: audio system and hookup.
- AudioManager + AudioMixer (Master, Music, SFX, Voice, Ambience) with snapshots: normal,
  final-choice (low-pass + quieter), ending.
- Music per §20 from Assets/_Project/Audio/Music/ (Scott Buckley tracks), 2 s crossfades,
  ducking under voice. Add the CC-BY credit lines from §25 to docs/CREDITS.md.
- Hook every SFX in §20 to its event, using Team/ first, then Sonniss/, Freesound/, Kenney/.
  For anything still missing, synthesize a placeholder with a Python script (numpy/soundfile)
  you write and run, and list it.
- Fire crackle volume follows fire level near the shadow; humming loop only while H is held.
Done when: every event in §20 plays its sound; no clipping (report peaks). 0 errors.
Commit "feat(audio): music and sound".
```
You check: play with headphones; nothing too loud; the music changes at the right moments.

## B15 Motion and game feel  (mode: PLAN)
First, in a normal terminal (not inside Claude), run:
`npx skills add https://github.com/gamedev-skills/awesome-gamedev-agent-skills --skill game-feel`
```
Read CLAUDE.md and docs/GAME_SPEC.md §4, §21 and §24. Use the game-feel skill.
Task: implement EVERY row of the §24 UI and Camera tables with LitMotion and Cinemachine, plus
the §21 feel rules. No new mechanics.
- Real crack: glass-crack overlay (generate the crack texture procedurally if none exists)
  spreading from the split line, 80 ms hit-stop, small shake, flash, chromatic aberration
  spike, bass hit; seams stay at 20% alpha; reverse "healing" for endings.
- Camera breathing per strip, interaction push-ins, final-choice slow push.
- All timings/strengths in one MotionSettings ScriptableObject so we can tune without code.
- Make a checklist of every §24 row with done/not done and show it to me.
Done when: every §24 UI/Camera row is done, each feedback fires once per event, the screen
stays readable. 0 errors. Commit "feat(motion): motion and feel pass".
```
You check: does every action "answer back"? Anything too strong or nauseating? Tone it down.

## B16 Handwriting, fonts and title  (mode: normal)
```
Read CLAUDE.md and docs/GAME_SPEC.md §8, §18, §2 and §24 (UI rows).
Task: real text assets.
- Scanned handwriting PNGs in Assets/_Project/Art/Handwriting/ (captions_closet.png,
  captions_song.png, captions_window.png, captions_candle.png, note_matchbox.png): make the
  paper background transparent if needed (Python script), then use them for the captions and
  the note with the ink-reveal animation from §24. Keep TMP text as a fallback.
- Cormorant Garamond from Assets/_Project/Fonts/ (or download it from github.com/google/fonts):
  TMP font assets, used for ALL UI text.
- Title screen exactly per §24. Credits from docs/CREDITS.md + team names: [TEAM NAMES HERE].
Done when: no default fonts remain anywhere (search the project and report). 0 errors.
Commit "feat(ui): handwriting, font and title".
```
You check: read every piece of text in the game once.

## B17 Review and bug hunt  (mode: normal)
```
Read CLAUDE.md and docs/GAME_SPEC.md (all sections).
Task: find and fix bugs, no new features.
1. Use a subagent to review the whole project against GAME_SPEC.md. Report only gaps that
   break the spec or can crash/soft-lock the game. No style opinions.
2. Check these cases in Play mode or by code reading: spamming F/E/H; striking a match while
   another is lit; Esc during a crack or the final choice; clicking a greyed-out strip;
   "Choose again" 3 times in a row; Noor reaching the window before it is open; alt-tab.
3. Fix every real problem found, one at a time, re-running the EditMode tests after each fix.
Report: list of issues found, fixed, and anything left. Commit "fix: bug hunt pass".
```
You check: a teammate who hasn't played plays one full run without help. Watch silently.

## B18 Build  (mode: normal)
```
Read CLAUDE.md. Task: release build.
- Disable debug keys and the debug overlay in non-development builds (keep F9 trailer mode).
- Build Windows 64-bit to Builds/ThreeMatches/ (Builds/ is git-ignored). Use a release
  (non-development) build.
- Write README.md at the repo root: game summary (3 lines), controls, how to run, team,
  credits link, themes and how they are used (one line each).
- Verify the build folder contains ThreeMatches.exe and its _Data folder; report its size.
- Commit "chore: release build config and README", tag v1.0, push to GitHub.
```
You check: copy the build folder to ANOTHER laptop, play start to finish twice.

---

## Recovery prompts
**Compile errors after a task**
```
The Unity console shows these errors: [paste]. Find the root cause and fix it. Don't
suppress or comment out code to hide errors. Show me the console after.
```
**Something works wrong**
```
Bug: [what I did] -> [what happened] -> [what should happen per GAME_SPEC §X].
Find the root cause, fix it, and tell me how you verified it.
```
**Claude hit the usage limit mid-task**
Run /login with the next account, then:
```
Read CLAUDE.md. We were in the middle of prompt [B__]. Check git status and the console,
tell me what's done and what's left of that prompt, then finish only what's left.
```
**MCP not responding**
```
Check whether the Unity MCP is connected (list its tools). If not, tell me exactly what to
click in Unity (Window > MCP for Unity) to reconnect.
```
