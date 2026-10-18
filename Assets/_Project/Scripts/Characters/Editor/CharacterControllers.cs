using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ThreeMatches.Characters.Editor
{
    /// <summary>
    /// Builds the Animator Controllers (GAME_SPEC §24 Characters table) from the Quaternius Universal
    /// Animation Library, retargeted through Humanoid. Every locomotion blend tree is driven by one
    /// "Speed" parameter in body-scale-normalised metres per second.
    /// </summary>
    public static class CharacterControllers
    {
        public const string ControllerDir = "Assets/_Project/Animation/Characters/";

        // Ground speed (m/s at scale 1) of the stance foot in each clip once retargeted to the base characters,
        // measured in the editor with the body held still. Each blend point plays its clip at the pace that
        // keeps planted feet from sliding.
        private const float WalkSpeed = 0.82f;
        private const float JogSpeed = 5.4f;
        private const float SlowWalkScale = 0.5f;
        private const float BriskWalkScale = 1.8f;

        private static Dictionary<string, AnimationClip> clips;

        public static AnimationClip Clip(string name)
        {
            if (clips == null || clips.Count == 0)
            {
                clips = new Dictionary<string, AnimationClip>();
                foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(CharacterAssets.UalFbx))
                {
                    var c = o as AnimationClip;
                    if (c == null || c.name.StartsWith("__preview")) continue;
                    clips[c.name.Substring(c.name.IndexOf('|') + 1)] = c;
                }
            }

            if (!clips.TryGetValue(name, out AnimationClip clip)) throw new KeyNotFoundException("UAL clip not found: " + name);
            return clip;
        }

        public static void ResetClipCache() => clips = null;

        /// <summary>Loads a built controller from disk (assets that were deleted and recreated are stale in memory).</summary>
        public static AnimatorController Load(string name)
        {
            var c = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerDir + name + ".controller");
            if (c == null) throw new KeyNotFoundException("Controller not built: " + name);
            return c;
        }

        // ---- masks -----------------------------------------------------------------------------------------

        public static AvatarMask ArmsMask()
        {
            CharacterAssets.EnsureFolder(ControllerDir);
            string path = ControllerDir + "Mask_Arms.mask";
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(path);
            if (mask == null)
            {
                mask = new AvatarMask();
                AssetDatabase.CreateAsset(mask, path);
            }

            foreach (AvatarMaskBodyPart part in System.Enum.GetValues(typeof(AvatarMaskBodyPart)))
            {
                if (part == AvatarMaskBodyPart.LastBodyPart) continue;
                bool arm = part == AvatarMaskBodyPart.LeftArm || part == AvatarMaskBodyPart.RightArm
                    || part == AvatarMaskBodyPart.LeftFingers || part == AvatarMaskBodyPart.RightFingers;
                mask.SetHumanoidBodyPartActive(part, arm);
            }
            EditorUtility.SetDirty(mask);
            return mask;
        }

        // ---- the four controllers --------------------------------------------------------------------------

        /// <summary>Rafi: seated start, stand-up, idle/walk blend, reach, and the arm-raised match pose.</summary>
        public static AnimatorController Rafi()
        {
            AnimatorController c = New("Rafi");
            AddFloat(c, CharacterParams.SpeedName);
            AddTrigger(c, CharacterParams.StandName);
            AddTrigger(c, CharacterParams.InteractName);

            AnimatorStateMachine sm = c.layers[0].stateMachine;
            AnimatorState sit = sm.AddState("Sit");
            sit.motion = Clip("Sitting_Idle_Loop");
            sm.defaultState = sit;
            AnimatorState stand = sm.AddState("StandUp");
            stand.motion = Clip("Sitting_Exit");
            AnimatorState loco = Locomotion(c, 0, "Locomotion");

            Trigger(sit, stand, CharacterParams.StandName, 0.1f);
            Timed(stand, loco, 0.9f, 0.2f);
            Interrupt(stand, loco, 0.3f, 0.2f);

            MatchLayer(c);
            InteractLayer(c);
            SetBaseIk(c);
            return Save(c);
        }

        /// <summary>Shadow-Rafi: Rafi's clips driven by the shadow's movement, plus a float for moving up the wall.</summary>
        public static AnimatorController ShadowRafi()
        {
            AnimatorController c = New("ShadowRafi");
            AddFloat(c, CharacterParams.SpeedName);
            AddBool(c, CharacterParams.VerticalName);

            AnimatorStateMachine sm = c.layers[0].stateMachine;
            AnimatorState loco = Locomotion(c, 0, "Locomotion");
            AnimatorState floating = sm.AddState("Float");
            floating.motion = Clip("Jump_Loop");
            Condition(loco, floating, CharacterParams.VerticalName, true, 0.15f);
            Condition(floating, loco, CharacterParams.VerticalName, false, 0.15f);

            MatchLayer(c);
            return Save(c);
        }

        /// <summary>Noor: crouched scared idle, run/walk blend, cough, sit on the sill.</summary>
        public static AnimatorController Noor()
        {
            AnimatorController c = New("Noor");
            AddFloat(c, CharacterParams.SpeedName);
            AddBool(c, CharacterParams.ScaredName);
            AddBool(c, CharacterParams.SitName);
            AddTrigger(c, CharacterParams.CoughName);

            AnimatorStateMachine sm = c.layers[0].stateMachine;
            AnimatorState loco = Locomotion(c, 0, "Locomotion");
            AnimatorState crouch = sm.AddState("CrouchedScared");
            crouch.motion = Clip("Crouch_Idle_Loop");
            AnimatorState coughState = sm.AddState("Cough");
            coughState.motion = Clip("Hit_Chest");
            coughState.speed = 0.55f;
            AnimatorState sitEnter = sm.AddState("SitEnter");
            sitEnter.motion = Clip("Sitting_Enter");
            AnimatorState sitIdle = sm.AddState("SitOnSill");
            sitIdle.motion = Clip("Sitting_Idle_Loop");

            // Scared and standing still: crouch. Moving or calm: the locomotion blend.
            AnimatorStateTransition toCrouch = loco.AddTransition(crouch);
            Setup(toCrouch, 0.2f);
            toCrouch.AddCondition(AnimatorConditionMode.If, 0, CharacterParams.ScaredName);
            toCrouch.AddCondition(AnimatorConditionMode.Less, 0.2f, CharacterParams.SpeedName);

            AnimatorStateTransition calm = crouch.AddTransition(loco);
            Setup(calm, 0.2f);
            calm.AddCondition(AnimatorConditionMode.IfNot, 0, CharacterParams.ScaredName);
            Interrupt(crouch, loco, 0.2f, 0.15f);

            // Cough from any standing or crouched rest, then back to wherever she was.
            foreach (AnimatorState from in new[] { loco, crouch })
            {
                AnimatorStateTransition t = from.AddTransition(coughState);
                Setup(t, 0.1f);
                t.AddCondition(AnimatorConditionMode.If, 0, CharacterParams.CoughName);
                t.AddCondition(AnimatorConditionMode.Less, 0.2f, CharacterParams.SpeedName);
            }
            AnimatorStateTransition coughToCrouch = coughState.AddTransition(crouch);
            SetupTimed(coughToCrouch, 0.9f, 0.15f);
            coughToCrouch.AddCondition(AnimatorConditionMode.If, 0, CharacterParams.ScaredName);
            AnimatorStateTransition coughToLoco = coughState.AddTransition(loco);
            SetupTimed(coughToLoco, 0.9f, 0.15f);
            coughToLoco.AddCondition(AnimatorConditionMode.IfNot, 0, CharacterParams.ScaredName);

            // Window sill: climb on, then sit.
            AnimatorStateTransition toSit = sm.AddAnyStateTransition(sitEnter);
            Setup(toSit, 0.2f);
            toSit.canTransitionToSelf = false;
            toSit.AddCondition(AnimatorConditionMode.If, 0, CharacterParams.SitName);
            Timed(sitEnter, sitIdle, 0.95f, 0.15f);
            Condition(sitIdle, loco, CharacterParams.SitName, false, 0.3f);
            Condition(sitEnter, loco, CharacterParams.SitName, false, 0.3f);

            sm.defaultState = loco;
            return Save(c);
        }

        /// <summary>Ending actors (adult Noor, grandson, fireman): idle/walk, sit, and a held carry pose.</summary>
        public static AnimatorController EndingActor()
        {
            AnimatorController c = New("EndingActor");
            AddFloat(c, CharacterParams.SpeedName);
            AddBool(c, CharacterParams.SitName);
            AddBool(c, CharacterParams.CarryName);

            AnimatorStateMachine sm = c.layers[0].stateMachine;
            AnimatorState loco = Locomotion(c, 0, "Locomotion");
            AnimatorState sitEnter = sm.AddState("SitEnter");
            sitEnter.motion = Clip("Sitting_Enter");
            AnimatorState sit = sm.AddState("Sit");
            sit.motion = Clip("Sitting_Idle_Loop");
            AnimatorState carry = sm.AddState("Carry");
            carry.motion = Clip("PickUp_Table");
            carry.speed = 0f;
            carry.cycleOffset = 0.75f;

            Condition(loco, sitEnter, CharacterParams.SitName, true, 0.2f);
            Timed(sitEnter, sit, 0.95f, 0.15f);
            Condition(sit, loco, CharacterParams.SitName, false, 0.3f);
            Condition(loco, carry, CharacterParams.CarryName, true, 0.3f);
            Condition(carry, loco, CharacterParams.CarryName, false, 0.3f);

            sm.defaultState = loco;
            return Save(c);
        }

        // ---- shared parts ----------------------------------------------------------------------------------

        /// <summary>Idle, then slow, normal and brisk walk, then jog: every point keeps the feet on the ground.</summary>
        private static AnimatorState Locomotion(AnimatorController c, int layer, string name)
        {
            BlendTree tree;
            AnimatorState state = c.CreateBlendTreeInController(name, out tree, layer);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = CharacterParams.SpeedName;
            tree.useAutomaticThresholds = false;

            AnimationClip idle = Clip("Idle_Loop");
            AnimationClip walk = Clip("Walk_Loop");
            AnimationClip jog = Clip("Jog_Fwd_Loop");
            AddChild(tree, idle, 0f, 1f);
            AddChild(tree, walk, WalkSpeed * SlowWalkScale, SlowWalkScale);
            AddChild(tree, walk, WalkSpeed, 1f);
            AddChild(tree, walk, WalkSpeed * BriskWalkScale, BriskWalkScale);
            AddChild(tree, jog, JogSpeed, 1f);
            return state;
        }

        private static void AddChild(BlendTree tree, Motion motion, float threshold, float timeScale)
        {
            tree.AddChild(motion, threshold);
            ChildMotion[] children = tree.children;
            children[children.Length - 1].threshold = threshold;
            children[children.Length - 1].timeScale = timeScale;
            tree.children = children;
        }

        private static void MatchLayer(AnimatorController c)
        {
            AnimatorControllerLayer layer = AddArmsLayer(c, CharacterParams.MatchLayerName, 0f);
            AnimatorState torch = layer.stateMachine.AddState("HoldMatch");
            torch.motion = Clip("Idle_Torch_Loop");
            layer.stateMachine.defaultState = torch;
        }

        private static void InteractLayer(AnimatorController c)
        {
            AnimatorControllerLayer layer = AddArmsLayer(c, CharacterParams.InteractLayerName, 1f);
            AnimatorState empty = layer.stateMachine.AddState("Empty");
            layer.stateMachine.defaultState = empty;
            AnimatorState reach = layer.stateMachine.AddState("Reach");
            reach.motion = Clip("Interact");
            reach.speed = 1.6f;

            Trigger(empty, reach, CharacterParams.InteractName, 0.1f);
            Timed(reach, empty, 0.85f, 0.2f);
        }

        private static AnimatorControllerLayer AddArmsLayer(AnimatorController c, string name, float weight)
        {
            c.AddLayer(name);
            AnimatorControllerLayer[] layers = c.layers;
            AnimatorControllerLayer layer = layers[layers.Length - 1];
            layer.avatarMask = ArmsMask();
            layer.defaultWeight = weight;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;
            layers[layers.Length - 1] = layer;
            c.layers = layers;
            return c.layers[c.layers.Length - 1];
        }

        private static void SetBaseIk(AnimatorController c)
        {
            AnimatorControllerLayer[] layers = c.layers;
            layers[0].iKPass = true;
            c.layers = layers;
        }

        // ---- transitions -----------------------------------------------------------------------------------

        private static void Setup(AnimatorStateTransition t, float duration)
        {
            t.hasExitTime = false;
            t.hasFixedDuration = true;
            t.duration = duration;
        }

        private static void SetupTimed(AnimatorStateTransition t, float exitTime, float duration)
        {
            t.hasExitTime = true;
            t.exitTime = exitTime;
            t.hasFixedDuration = true;
            t.duration = duration;
        }

        private static void Trigger(AnimatorState from, AnimatorState to, string trigger, float duration)
        {
            AnimatorStateTransition t = from.AddTransition(to);
            Setup(t, duration);
            t.AddCondition(AnimatorConditionMode.If, 0, trigger);
        }

        private static void Condition(AnimatorState from, AnimatorState to, string boolName, bool value, float duration)
        {
            AnimatorStateTransition t = from.AddTransition(to);
            Setup(t, duration);
            t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, boolName);
        }

        private static void Timed(AnimatorState from, AnimatorState to, float exitTime, float duration)
        {
            SetupTimed(from.AddTransition(to), exitTime, duration);
        }

        private static void Interrupt(AnimatorState from, AnimatorState to, float speedAbove, float duration)
        {
            AnimatorStateTransition t = from.AddTransition(to);
            Setup(t, duration);
            t.AddCondition(AnimatorConditionMode.Greater, speedAbove, CharacterParams.SpeedName);
        }

        // ---- controller plumbing ---------------------------------------------------------------------------

        private static AnimatorController New(string name)
        {
            CharacterAssets.EnsureFolder(ControllerDir);
            string path = ControllerDir + name + ".controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null) AssetDatabase.DeleteAsset(path);
            return AnimatorController.CreateAnimatorControllerAtPath(path);
        }

        private static AnimatorController Save(AnimatorController c)
        {
            EditorUtility.SetDirty(c);
            AssetDatabase.SaveAssets();
            return c;
        }

        private static void AddFloat(AnimatorController c, string n) => c.AddParameter(n, AnimatorControllerParameterType.Float);
        private static void AddBool(AnimatorController c, string n) => c.AddParameter(n, AnimatorControllerParameterType.Bool);
        private static void AddTrigger(AnimatorController c, string n) => c.AddParameter(n, AnimatorControllerParameterType.Trigger);
    }
}
