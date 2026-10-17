# THREE MATCHES: Game Spec (source of truth)

Claude: read ONLY the sections the current prompt names. Section numbers never change.

---

## §1 Overview
- **Pitch:** An old man trades his memories of his daughter for the chance to save her.
- **Genre:** 2.5D story-driven horror, choice-based, 3 endings.
- **Engine:** Unity 6 (6000.x LTS), URP, Universal 3D template, new Input System.
- **Length:** about 5 minutes per playthrough. Judges must understand everything in ONE run.
- **Platform:** Windows build, 1920x1080, mouse + keyboard.
- **Themes (hackathon):** Fire, Shadow, Time. All three must be in gameplay, story and atmosphere.

## §2 Story
Rafi (70) lives alone in the house he rebuilt. In 1996 his daughter Noor (8) died in a fire
in that house at 11:47 PM. The living-room wall is covered with framed photos of her: the
Memory Wall.

Tonight, in Noor's old room, he finds a small matchbox with a note in a child's handwriting (font: Gloria Hallelujah):
"Papa, for when it gets dark. - Noor". It holds three matches.

When he strikes a match at the Memory Wall, he must choose one photo to burn. The photo
burns, the memory is gone from his mind forever, and the screen cracks: the PAST (1996)
appears beside his PRESENT (2026). In the past, Rafi is only a shadow on the walls. Through
his shadow he can open doors, push things and guide Noor, but only while the match burns.
The burned memory gives him the knowledge he needs (where she hides, the song that calms
her, how to open the stuck window).

With the second match the screen cracks again and the FUTURE (2036) appears, showing live
what his choices are creating. The last match decides how it ends: give up the rest of his
memories to save her, go into the fire himself, or blow it out and keep what's left of her.

Small twist (shown only in Ending A): the "shadow man" Noor saw on the wall that night was
always her father.

### Script lines (subtitles only; no voice acting)
| ID | When | Line |
|---|---|---|
| L01 | Game start (Rafi) | "Thirty years. I still hear her." |
| L02 | Matchbox found (note, Gloria Hallelujah font) | "Papa, for when it gets dark. - Noor" |
| L03 | Matchbox found (Rafi) | "Three matches." |
| L04 | Noor sees shadow, no song | "Go away! Go away!" |
| L05 | Noor hears humming | "...Papa? Is that you?" |
| L06 | Window opened | (sound only: wood cracking, cold air) |
| L07 | Final match | "One left." |
| L08 | Ending A (Noor) | "I always knew the shadow was you." |
| L09 | Ending B (grandson) | "Grandpa?" |
| L10 | Ending B (Rafi) | "I'm sorry... do I know you?" |
| L11 | Ending C (Rafi) | "I couldn't save you. But I never let you go." |

## §3 Themes map
| Theme | Gameplay | Story | Atmosphere |
|---|---|---|---|
| FIRE | Matches are your only light and your timer in the past. Every match burns a memory. | Noor died in a house fire. | Match light only; fire glow in the past. |
| SHADOW | In the past you exist only as a shadow; you move and act through it. Noor fears it unless you hum her song. | The shadow man she saw was her father. | Silhouettes, darkness, flicker. |
| TIME | Past, present, future on screen at once. The past only moves while a match burns. | 1996, 2026, 2036. | Each time has its own colour and sound. |

Trailer theme lines:
- FIRE: "Every match lights the past - and burns a memory."
- SHADOW: "In the past, you are only a shadow."
- TIME: "Three times. One house. One night to change."

## §4 Screen and strips
- One scene. Three copies of the house far apart in world space, each with its own camera:
  House_1996 (Past), House_2026 (Present), House_2036 (Future).
- Layout is horizontal strips stacked vertically, ordered by year: Past top, Present middle,
  Future bottom (only the strips that exist are shown).
- Stage 1: Present only (full screen).
- Stage 2 (after match 1 is struck): Past + Present.
- Stage 3 (after match 2 is struck): Past + Present + Future.
- The strip the player currently controls is larger: 2 strips = 62% / 38%;
  3 strips = 50% / 25% / 25%. Resize animates over 0.35 s (ease in-out).
- Each strip shows its year in the top-left corner (small serif text, 60% opacity).
- Stage change transition ("the crack"): white flash 2 frames, glass-crack overlay spreads
  from the new split line over 0.4 s, screen shake (small), deep bass hit, then new layout.
- Inactive past strip (match out): desaturated, darker, frozen.

## §5 The house (2.5D dollhouse cross-section)
- Side-on view, front wall removed. Two floors. Camera orthographic-looking perspective
  (FOV ~20, far back) with slight push-ins at key moments.
- Ground floor (left to right): Front door | Living room (Memory Wall, armchair, clock) |
  Kitchen (decor only).
- Upper floor (left to right): Landing with the STUCK WINDOW | Hallway with the CLOSET |
  Noor's bedroom (bed, candle on shelf by the curtains).
- Stairs connect living room and landing.
- Variants:
  - Past 1996: warm, cluttered, family life; upstairs fire scenario (§10). Stairs collapse at
    the start of the fire (the upper floor is cut off; the only exit is the stuck window).
  - Present 2026: rebuilt, tidy, empty, cold moonlight. Noor's room kept as it was.
  - Future 2036: dust and sheets over furniture by default; changes per §14.

## §6 Controls
| Input | Action |
|---|---|
| A / D | Move left/right (Rafi in present, shadow in past) |
| W / S | Use stairs (at stairs) / shadow up/down along wall (past) |
| E | Interact (hold where noted) |
| F | Strike a match (only at the Memory Wall) |
| H (hold) | Hum Noor's song (only after the SONG memory is burned) |
| Mouse | Choose a photo / choose a strip in the final choice |
| Esc | Pause |

Controls card shown for 6 s at game start and on pause.

## §7 Present: Rafi
- Walks slowly (2 m/s), no running, no jumping. Interact prompt appears above objects.
- Act 1 path: armchair -> Memory Wall (photos readable) -> stairs -> Noor's old room ->
  matchbox in the drawer -> back to the Memory Wall.
- While a match burns, Rafi stands at the wall holding it; the player controls the shadow.

## §8 Memory Wall and memories
Four framed photos. Each has a caption in the font Caveat (Rafi's handwriting), shown on hover/E.
| ID | Caption | What burning it gives | Rescue step |
|---|---|---|---|
| CLOSET | "Her hiding spot. The hallway closet. Always the closet." | The closet becomes visible/interactable for the shadow. | FIND |
| SONG | "Her song. The only thing that calmed her." | Hold H to hum: Noor follows your shadow instead of fleeing. | CALM |
| WINDOW | "The stuck window. I kept saying I'd fix it." | The shadow can force the stuck window open (hold E 2 s). | EXIT |
| CANDLE | "Her candle. She couldn't sleep in the dark." | Shows how the fire started (a 3 s vision: curtain drifts onto the candle). Fire spreads 30% slower for the rest of the game. | none |
- Burning a photo: edges curl, glow, ash particles, 2 s. A short sound from that memory plays
  muffled as it burns (closet: door creak + faint child giggle; song: music-box notes; window:
  window creak; candle: match strike). Afterwards the frame is empty
  and its caption reads "...".
- A burned memory can never be chosen again.

## §9 Matches
- 3 matches total, shown as 3 small icons bottom-right of the Present strip.
- Matches 1 and 2: rescue windows. Strike (F) -> choose a photo -> it burns -> crack ->
  40 s in the past. The flame shrinking at the bottom of the Present strip is the timer.
  Last 8 s: flame flickers, heartbeat sound.
- Match 3: final choice (§15). It never opens a rescue window.

## §10 Past timeline and fire
- Past time `t` (seconds) only advances while a match burns. Total possible: 80 s.
- Noor starts hidden inside the hallway closet. The stairs are gone.
- Fire schedule (multiply all times by 1.3 if CANDLE is burned):
| t | Event |
|---|---|
| 0 | Fire burning in Noor's bedroom (curtains). Smoke at hallway ceiling. |
| 20 | Bedroom fully ablaze. Hallway level 1 (thin flames at the bedroom door). |
| 45 | Hallway level 2 (flames along the floor near the closet). Noor coughs. |
| 65 | Hallway level 3. Landing level 1. |
| 80 | Upper floor engulfed (only reachable if the player stalls; normal play ends at 80). |
- Fire never kills Noor during the rescue windows (no game-over). It blocks the shadow:
  the shadow cannot pass through level 3 fire. Pressure comes from limited match time.

## §11 The shadow (past)
- A dark silhouette of Rafi on the back walls of the past house. Fake shadow (not a physics
  shadow): Rafi's animated model in pure black, flattened against the wall (§19), flickering
  with the fire light. Placeholder before B12B: a black sprite on a quad.
- Moves along the wall plane: A/D horizontal, W/S vertical within a room's wall area.
  Speed 3 m/s. Can move between the three upstairs rooms through doorways.
- Interacts with objects whose shapes touch the shadow: closet door (if CLOSET burned),
  stuck window (if WINDOW burned, hold E 2 s), curtains/props (cosmetic).
- Without the needed memory, the object shows no prompt; the shadow passes over it.

## §12 Noor (past)
State machine:
| State | Behaviour |
|---|---|
| Hidden | Inside the closet; quiet crying sound. |
| Out | After FIND: steps out, stands near the closet, scared. |
| Panicked | Shadow within 3 m and NOT humming: L04, runs back against the wall, frozen 3 s. |
| Following | Shadow within 4 m AND humming (H held): L05 once, follows the shadow at 1.5 m/s. Stops if humming stops for 1.5 s. |
| AtWindow | Reached the window while it is open: waits on the sill (does NOT escape yet; escape only happens in endings). |
- Noor moves only on the upper floor (landing, hallway, bedroom door area).

## §13 Rescue steps and state
- Steps: FIND (closet opened by shadow), CALM (Noor followed you at least once),
  EXIT (window forced open).
- A step only counts if it was actually performed in a window, not just unlocked.
- State persists between windows. Pure C# class `RescueState` holds: burned memories (ordered),
  completed steps, past time t, Noor state.
- With 2 rescue windows the player can complete at most 2 steps, so the final choice always
  matters.

## §14 Future strip (live preview)
Appears at Stage 3. Shows House_2036 (living room + Memory Wall). Updates live:
| Condition | What appears |
|---|---|
| Default | Dust sheets, empty armchair, old Rafi asleep in the chair. |
| FIND done | A small pair of shoes appears by the door (faint, fades in). |
| CALM done | A child's drawing appears on the fridge. |
| EXIT done | A faint adult woman's silhouette appears at the window. |
Each change: soft chime + 1 s glow on the Future strip.

## §15 Final choice (match 3)
- After window 2 ends: L07, prompt "Strike the last match (F)".
- On strike: time slows, the three strips resize to equal thirds and glow; hovering a strip shows
  its text; click to choose.
| Strip | Hover text | Ending |
|---|---|---|
| Past | "Go yourself. You won't come back." | A |
| Present | "Burn [N] more memory/memories to save her." (singular when N = 1) | B |
| Future | "Blow it out. Keep what's left." | C |
- Cost rule: N = number of missing steps (3 minus completed steps).
- Present is available only if N <= remaining (unburned) memories. Otherwise the strip shows
  "Not enough left to give." and cannot be clicked.
- Ending B burns N remaining memories, choosing in this order: the memories matching missing
  steps first, then any other remaining ones.

## §16 Endings
All endings: play across the strips, then the strips merge into ONE screen (reverse crack),
then the ending title card, then "Choose again" and "Credits" buttons.

**A: WALK INTO THE FIRE (Past)**
Rafi's shadow becomes solid. He carries Noor out (through the window if EXIT done, else
through the flames). Noor (L08). The future shows grown-up Noor lighting a candle under his
photo. Title: "She grew up. He didn't grow old."

Variant: if SONG was burned, add Noor: "You hummed my song."

**B: SAVE HER, FORGET HER (Present)**
N photos burn. The past shows Noor climbing out of the window into a fireman's arms. The
future shows grown-up Noor visiting with her son; the son: L09; Rafi: L10.
Title: "She lived. He forgot."

Variants: if a memory is still kept: "He kept one thing: [caption of kept memory]."
If SONG was burned before the final choice, Noor hums the song as she leaves.

**C: KEEP HER (Future)**
He blows out the match. The past fades to black; history stays the same. The present: Rafi
sits at the wall with the photos he kept (burned ones are empty frames). L11.
Title: "He couldn't save her. He never forgot her."

- "Choose again" restores the exact state before the final match (save a snapshot).

## §17 Flow and timing
| Part | Content | Target time |
|---|---|---|
| Title | "THREE MATCHES" + "Press any key" | - |
| Act 1 | Stage 1. Wake, L01, controls card, Memory Wall, upstairs, matchbox (L02, L03), back to wall | 1:10 |
| Match 1 | Choose photo, crack to Stage 2, 40 s window | 0:50 |
| Between | Past freezes, back to Rafi, strike again prompt | 0:10 |
| Match 2 | Choose photo, crack to Stage 3, 40 s window | 0:50 |
| Final | L07, final choice | 0:20 |
| Ending | Ending sequence + title card | 0:45 |

## §18 UI
- Title screen, pause menu (Resume / Controls / Quit), subtitles (bottom, white, serif,
  dark backing), interaction prompts (small key icon + verb), match icons, flame timer,
  year labels, final-choice hover text, ending title card, credits.
- Fonts: Cormorant Garamond (OFL) for all UI text. Memory Wall captions: Caveat. Matchbox
  note: Gloria Hallelujah. No other fonts.
- No HUD clutter. Nothing on screen that the player doesn't need right now.

## §19 Art direction
- Look: a dark 3D dollhouse lit only by flame and moonlight. Matte, warm, cinematic. Every
  frame should look like a still from a short film.
- Environment: Kenney Furniture Kit + Building Kit (CC0) for furniture/doors/windows; walls and
  floors are simple meshes with Poly Haven / ambientCG PBR textures (CC0) at 1K resolution.
- Characters: Quaternius Universal Base Characters (CC0), animated with the Quaternius
  Universal Animation Library (CC0) through a Humanoid Animator.
  - Rafi (present): Regular male, very dark desaturated materials, slight stoop (spine bone
    rotated in LateUpdate), walk animation at 0.75 speed. Lit only by moonlight/match, so he
    reads as a near-silhouette with a rim of light.
  - Shadow-Rafi (past): the same animated model, pure black unlit material, flattened against
    the wall (scale on the wall's normal axis ~0.02), alpha 0.85, scale/alpha flicker from
    the nearest fire light. It walks with real animations, like a true cast shadow.
  - Noor (past): Teen female base scaled to 0.72, long hairstyle, light nightgown colour.
  - Adult Noor, grandson (scaled Teen 0.62), fireman (Regular male, dark coat): endings only.
- Palettes: Past warm amber, then orange/red as the fire grows; Present cold blue moonlight;
  Future soft grey morning that warms with each completed step.
- Lighting: realtime point lights only where there is flame (match, candle, fire); moonlight
  is one directional light per house. Shadows on for flame lights.
- Post-processing per strip camera: bloom, vignette, film grain (low), colour adjustments per
  time, light chromatic aberration only during cracks.
- Handwriting fonts for captions and the note.

## §20 Audio
Music (Scott Buckley, CC-BY 4.0; credit format in §25):
| Moment | Track |
|---|---|
| Title + Act 1 | "She Moved Mountains" |
| Rescue windows (under the fire) | "Intervention" (No Piano Melody mix) |
| Final choice | "Shadows and Dust" |
| Endings A and B | "With These Hands" |
| Ending C + credits | "At The End Of All Things" |
Crossfade 2 s between tracks.
SFX source priority: Freesound CC0 > Sonniss > Kenney > synthesized.
| Sound | Source (priority order) |
|---|---|
| Match strike, match burn, match blown out | Freesound CC0 > Sonniss > Kenney > synthesized |
| Photo burning + muffled memory sound | Paper-burn SFX layered with the memory sound: closet = closet-door creak + faint child giggle; song = music-box notes; window = window creak; candle = match strike |
| Noor's song (H) | Synthesized music-box lullaby: 6 original notes, looped, soft low-pass so it feels like humming |
| Fire crackle loops (3 intensities), house fire roar | Sonniss > Freesound CC0 |
| Wood creaks, window forced open, cold air | Sonniss > Freesound CC0 |
| Room tones (past warm, present cold, future soft) | Sonniss > Freesound CC0 > synthesized |
| Bass hit, crack, risers, heartbeat | Synthesized (or Sonniss) |
| UI hover/click | Kenney Interface Sounds (CC0) |
| Voice lines | None. Subtitles only. |

## §21 Game feel
- Every action answers back with sound + visual: interact, strike, burn, crack, step done.
- Crack: hit-stop 80 ms, shake 0.15 s small, flash, chromatic aberration spike, bass.
- Step completed: 0.2 s slow-mo in the past strip, soft chime, Future strip glow.
- Camera eases (no snapping). All animated values use easing curves (LitMotion).
- Keep effects small. Nothing that makes the screen hard to read.
- Full motion details: §24.

## §22 Debug keys (editor + development builds only)
| Key | Action |
|---|---|
| F1 | Restart Act 1 |
| F2 | Jump to match 1 (Stage 2) with CLOSET burned |
| F3 | Jump to the start of window 2 (Stage 3) with CLOSET+SONG burned, FIND done |
| F4 | Jump to final choice (CLOSET+SONG burned, FIND+CALM done) |
| F6 | Sandbox: all 4 memories burned, unlimited match, Stage 3, t = 0 (testing only) |
| F5 | Toggle debug overlay (t, steps, burned, Noor state) |
| F9 | Trailer mode: hide all UI |

## §23 Out of scope (do NOT build)
Combat, enemy AI, inventory, saving to disk, settings menu beyond pause, multiple scenes
for gameplay, NavMesh, custom rigging or hand-made animation clips, procedural levels,
localization, WebGL build, Unity Asset Store assets.

## §24 Motion bible (every animation, with timing)
UI (LitMotion; Cormorant Garamond for UI, Caveat for captions, Gloria Hallelujah for the note):
| Element | Motion |
|---|---|
| Title screen | Black. Match strikes: 3-frame spark burst, flame grows 0.4 s (ease-out-back), light blooms. Title fades in 1.2 s with 8 px upward drift (ease-out-cubic). "Press any key" alpha breathes 0.4-0.9 every 2.4 s (sine). |
| Controls card | In: 24 px slide up + fade, 0.4 s ease-out. Out: 0.3 s fade. |
| Interaction prompt | In: scale 0.8 to 1 + fade, 0.18 s ease-out-back. Out: 0.12 s. |
| Subtitles | Fade + 6 px rise, 0.25 s. Hold 1.5 s minimum. Out 0.3 s. |
| Handwritten caption | Caption text reveals left to right with a 0.6 s mask, like ink appearing. |
| Match icons | A used match burns down 0.5 s, then turns ash-grey. |
| Flame timer | Shader flicker; last 8 s: stronger flicker + vignette pulses with the heartbeat. |
| Year labels | Appear digit by digit over 0.3 s after each crack. |
| Final-choice hover | Hovered strip +15% brightness, text fades in 0.2 s; other strips dim to 60%. |
| Ending title card | Letter spacing tightens from 0.3 em to 0.05 em over 1.5 s while fading in. |
| Credits | Scroll 25 px/s; text fades at top and bottom edges. |
Camera (Cinemachine):
| Element | Motion |
|---|---|
| Idle | Every strip camera breathes: Perlin drift +/-0.05 m and 0.1 degrees. |
| Interaction | Push-in FOV 20 to 18 over 1.2 s (ease-in-out), back after. |
| Crack | 80 ms hit-stop, 0.15 s small shake, chromatic aberration spike 0.2 s. |
| Strip resize | 0.35 s ease-in-out. Old crack lines stay as faint seams (20% alpha). |
| Final choice | timeScale 0.2; all cameras push in slowly. |
| Ending merge | Seams retract over 0.8 s ("the cracks heal"), strips cross-fade into one. |
Characters (Animator, Quaternius animations):
| Character | Animations |
|---|---|
| Rafi | Sitting idle at start, stand-up, idle (breathing), slow walk (0.75 speed), reach/interact, hold match (arm raised), head turns toward the photo he hovers (LookAt weight 0.6). |
| Shadow-Rafi | Same clips as Rafi, driven by the shadow's movement. Flicker with fire light. |
| Noor | Crouched scared idle (closet/panic), run (panic), walk (following, slower), cough (fire level 2+), sit on the window sill. |
| Endings | Carry (Ending A, or a hold pose), walk, idle, sit. |
Environment and VFX:
| Element | Motion |
|---|---|
| Match strike | Spark burst (Kenney sparks), light 0 to 3 to 1.5 intensity with 0.12 s overshoot, small smoke wisp when it goes out (Unity Labs CandleSmoke). |
| Photo burn (2 s) | Dissolve shader with a glowing burning edge (noise mask), emission orange to white to black, ash + embers rising, frame glass shimmer, muffled memory sound. |
| Room fire | Flipbook flames (CGHEVEN ground fire for rooms, Unity Labs Flame02/03 for small flames), embers rising, smoke layer under the ceiling (Unity Labs wispy smoke), lights flicker with Perlin noise. Fire level changes blend over 1.5 s. |
| Future changes | Object fades in from 0 alpha over 1 s with a soft bloom pulse. |
| Inactive past | Desaturate + darken over 0.6 s; fire particles pause (simulation speed 0). |

## §25 Asset map (folders and credits)
| Asset | Folder | Licence |
|---|---|---|
| Kenney Furniture Kit, Building Kit | Assets/_Project/Art/Kenney/ | CC0 |
| Kenney Particle Pack | Assets/_Project/Art/VFX/Kenney/ | CC0 |
| Poly Haven / ambientCG textures (1K) | Assets/_Project/Art/Textures/<name>/ | CC0 |
| Quaternius Universal Base Characters (only the used FBX + textures) | Assets/_Project/Art/Characters/Quaternius/ | CC0 |
| Quaternius Universal Animation Library (Standard) | Assets/_Project/Art/Animations/UAL/ | CC0 |
| Unity Labs VFX flipbooks | Assets/_Project/Art/VFX/UnityLabs/ | CC0 |
| CGHEVEN fire flipbooks | Assets/_Project/Art/VFX/CGHEVEN/ | CC0 |
| Scott Buckley music | Assets/_Project/Audio/Music/ | CC-BY 4.0 (credit required) |
| Sonniss GDC bundle sounds (only the files used) | Assets/_Project/Audio/SFX/Sonniss/ | Royalty-free, no attribution |
| Freesound CC0 sounds | Assets/_Project/Audio/SFX/Freesound/ | CC0 |
| Kenney Interface Sounds | Assets/_Project/Audio/SFX/Kenney/ | CC0 |
| Cormorant Garamond | Assets/_Project/Fonts/ | SIL OFL |
| Caveat (open-source font) | Assets/_Project/Fonts/ | SIL OFL |
| Gloria Hallelujah (open-source font) | Assets/_Project/Fonts/ | SIL OFL |
| LitMotion (tweening) | Package (git URL) | MIT |
Music credit format (Credits screen and README):
'[Title]' by Scott Buckley - released under CC-BY 4.0. www.scottbuckley.com.au
