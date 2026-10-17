# Three Matches: rules for Claude Code

## Project
2.5D horror game, Unity 6 (URP, Universal 3D template), built in a 48-hour hackathon.
Design source of truth: docs/GAME_SPEC.md. Read ONLY the sections the current prompt names.
Trailer work lives in TRAILER.md (ignore it unless the prompt says trailer).

## Golden rules
- IMPORTANT: Do only the task in the current prompt. Don't build ahead. Don't refactor or
  restyle code the task doesn't need.
- No voice acting, no team recordings, no scanned handwriting. Dialogue is subtitles only.
- Never leave the project with compile errors. Stability beats features.
- After every change, read the Unity Console through the Unity MCP and fix every error and
  warning you caused before saying you're done.
- If the spec is unclear, contradicts itself, or a step is impossible: stop and ask me.
- Finish every task with evidence: console status, test results, files changed, and (if the
  MCP can capture it) a Game view screenshot you have looked at.
- End every task with a manual test checklist for me (max 6 steps).

## Unity conventions
- New Input System only. Never use UnityEngine.Input.
- Code: Assets/_Project/Scripts/<System>/, namespace ThreeMatches.<System>.
- One responsibility per MonoBehaviour. Game rules live in plain C# classes (no
  MonoBehaviour) so EditMode tests can cover them.
- Data: ScriptableObjects in Assets/_Project/Data/.
- Use Unity MCP tools to create/edit GameObjects, prefabs, materials and scenes.
  Save the scene after edits. Main scene: Assets/_Project/Scenes/Main.unity.
- Animation tools: LitMotion for all tweens/UI motion, Cinemachine for cameras, Animator for
  characters. No other tween or animation libraries.
- Cache references; no Find/GetComponent in Update; no per-frame allocations.
- All tunable numbers (speeds, timings, sizes) are serialized fields or ScriptableObject
  values, not magic numbers.
- Don't edit Library/, Temp/, Logs/, UserSettings/. Change packages only via the Package
  Manager / manifest when a prompt asks.
- Every external asset must be listed in docs/CREDITS.md (name, author, licence, link).
  Only use assets from GAME_SPEC §25 folders. Never add Unity Asset Store assets (the repo is
  public and their licence doesn't allow redistribution).

## Testing
- EditMode tests: Assets/_Project/Tests/EditMode (assembly ThreeMatches.Tests.EditMode).
  Run them via the Unity MCP test runner and report pass/fail counts.
- Debug keys from GAME_SPEC §22 must keep working after every task.

## Git
- Commit at the end of every task that passes: "feat(<system>): <summary>" or
  "fix(<system>): <summary>". Never commit with compile errors or failing tests.
