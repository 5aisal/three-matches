using System.Collections.Generic;
using ThreeMatches.Fire;
using ThreeMatches.Memories;
using ThreeMatches.Noor;
using ThreeMatches.Player;
using ThreeMatches.Shadow;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Garment = ThreeMatches.Characters.Editor.CharacterAssets.Garment;

namespace ThreeMatches.Characters.Editor
{
    /// <summary>
    /// Builds the character prefabs (GAME_SPEC §19) and swaps the placeholder sprites in the scene for them.
    /// Run "1 Build Assets" first, then "2 Wire Scene". Both are safe to run again.
    /// </summary>
    public static class CharacterBuilder
    {
        private const string PrefabDir = "Assets/_Project/Prefabs/Characters/";
        private const float NoorScale = 0.72f;
        private const float GrandsonScale = 0.62f;

        private sealed class Spec
        {
            public string Name;
            public string Fbx;
            public float Scale = 1f;
            public Material Skin;
            public Material Brow;
            public readonly List<(Garment garment, Material material)> Garments = new List<(Garment, Material)>();
            public readonly List<(string hair, Material material)> Hair = new List<(string, Material)>();
            public AnimatorController Controller;
            public bool Silhouette;
        }

        [MenuItem("Three Matches/Characters/1 Build Assets")]
        public static void BuildAssets()
        {
            AssetDatabase.Refresh();
            CharacterControllers.ResetClipCache();
            CharacterAssets.EnsureFolder(PrefabDir);

            CharacterControllers.Rafi();
            CharacterControllers.ShadowRafi();
            CharacterControllers.Noor();
            CharacterControllers.EndingActor();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AnimatorController rafiCtrl = CharacterControllers.Load("Rafi");
            AnimatorController shadowCtrl = CharacterControllers.Load("ShadowRafi");
            AnimatorController noorCtrl = CharacterControllers.Load("Noor");
            AnimatorController endingCtrl = CharacterControllers.Load("EndingActor");

            // Rafi: dark, desaturated, close to a silhouette in moonlight.
            Material rafiSkin = CharacterAssets.Skin("Char_Rafi_Skin", CharacterAssets.MaleFbx, "T_Superhero_Male_Dark.png", new Color(0.40f, 0.38f, 0.36f));
            Material rafiBrow = CharacterAssets.HairMaterial("Char_Rafi_Hair", "Hair_SimpleParted", new Color(0.09f, 0.08f, 0.07f));
            Material rafiShirt = CharacterAssets.Cloth("Char_Rafi_Shirt", new Color(0.09f, 0.09f, 0.10f));
            Material rafiTrousers = CharacterAssets.Cloth("Char_Rafi_Trousers", new Color(0.06f, 0.06f, 0.07f));

            // Noor: light nightgown, long dark hair.
            Material noorSkin = CharacterAssets.Skin("Char_Noor_Skin", CharacterAssets.FemaleFbx, "T_Superhero_Female_Light_BaseColor.png", new Color(0.92f, 0.86f, 0.82f));
            Material noorHair = CharacterAssets.HairMaterial("Char_Noor_Hair", "Hair_Long", new Color(0.26f, 0.17f, 0.12f));
            Material noorBrow = CharacterAssets.HairMaterial("Char_Noor_Brow", "Hair_Long", new Color(0.20f, 0.13f, 0.09f));
            Material gown = CharacterAssets.Cloth("Char_Noor_Nightgown", new Color(0.93f, 0.89f, 0.80f));

            // Ending actors.
            Material adultDress = CharacterAssets.Cloth("Char_NoorAdult_Dress", new Color(0.42f, 0.28f, 0.30f));
            Material grandsonShirt = CharacterAssets.Cloth("Char_Grandson_Shirt", new Color(0.28f, 0.38f, 0.52f));
            Material grandsonTrousers = CharacterAssets.Cloth("Char_Grandson_Trousers", new Color(0.14f, 0.14f, 0.18f));
            Material grandsonHair = CharacterAssets.HairMaterial("Char_Grandson_Hair", "Hair_BuzzedFemale", new Color(0.22f, 0.15f, 0.10f));
            Material firemanSkin = CharacterAssets.Skin("Char_Fireman_Skin", CharacterAssets.MaleFbx, "T_Superhero_Male_Dark.png", new Color(0.7f, 0.65f, 0.6f));
            Material firemanCoat = CharacterAssets.Cloth("Char_Fireman_Coat", new Color(0.04f, 0.04f, 0.05f));
            Material firemanTrousers = CharacterAssets.Cloth("Char_Fireman_Trousers", new Color(0.03f, 0.03f, 0.04f));
            Material firemanHair = CharacterAssets.HairMaterial("Char_Fireman_Hair", "Hair_Buzzed", new Color(0.10f, 0.08f, 0.07f));

            // Shadow-Rafi: flat black, drawn once per pixel.
            Material silhouette = SilhouetteMaterial();

            var rafi = new Spec { Name = "Rafi", Fbx = CharacterAssets.MaleFbx, Skin = rafiSkin, Brow = rafiBrow, Controller = rafiCtrl };
            rafi.Garments.Add((Garment.Shirt, rafiShirt));
            rafi.Garments.Add((Garment.Trousers, rafiTrousers));
            rafi.Hair.Add(("Hair_SimpleParted", rafiBrow));
            rafi.Hair.Add(("Hair_Beard", rafiBrow));

            var shadow = new Spec { Name = "ShadowRafi", Fbx = CharacterAssets.MaleFbx, Skin = silhouette, Brow = silhouette, Controller = shadowCtrl, Silhouette = true };
            shadow.Hair.Add(("Hair_SimpleParted", silhouette));
            shadow.Hair.Add(("Hair_Beard", silhouette));

            var noor = new Spec { Name = "Noor", Fbx = CharacterAssets.FemaleFbx, Scale = NoorScale, Skin = noorSkin, Brow = noorBrow, Controller = noorCtrl };
            noor.Garments.Add((Garment.Nightgown, gown));
            noor.Hair.Add(("Hair_Long", noorHair));

            var adult = new Spec { Name = "NoorAdult", Fbx = CharacterAssets.FemaleFbx, Skin = noorSkin, Brow = noorBrow, Controller = endingCtrl };
            adult.Garments.Add((Garment.Nightgown, adultDress));
            adult.Hair.Add(("Hair_Long", noorHair));

            var grandson = new Spec { Name = "Grandson", Fbx = CharacterAssets.FemaleFbx, Scale = GrandsonScale, Skin = noorSkin, Brow = grandsonHair, Controller = endingCtrl };
            grandson.Garments.Add((Garment.Shirt, grandsonShirt));
            grandson.Garments.Add((Garment.Trousers, grandsonTrousers));
            grandson.Hair.Add(("Hair_BuzzedFemale", grandsonHair));

            var fireman = new Spec { Name = "Fireman", Fbx = CharacterAssets.MaleFbx, Skin = firemanSkin, Brow = firemanHair, Controller = endingCtrl };
            fireman.Garments.Add((Garment.Coat, firemanCoat));
            fireman.Garments.Add((Garment.Trousers, firemanTrousers));
            fireman.Hair.Add(("Hair_Buzzed", firemanHair));

            foreach (Spec spec in new[] { rafi, shadow, noor, adult, grandson, fireman }) BuildPrefab(spec);

            // The controllers were deleted and recreated, so reimport the prefabs to drop stale references.
            foreach (Spec spec in new[] { rafi, shadow, noor, adult, grandson, fireman })
                AssetDatabase.ImportAsset(PrefabDir + spec.Name + ".prefab", ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            Debug.Log("CharacterBuilder: built 4 controllers and 6 character prefabs.");
        }

        private static Material SilhouetteMaterial()
        {
            CharacterAssets.EnsureFolder(CharacterAssets.MaterialDir);
            string path = CharacterAssets.MaterialDir + "Char_ShadowSilhouette.mat";
            Shader shader = Shader.Find("ThreeMatches/ShadowSilhouette");
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = shader;
            m.SetColor("_BaseColor", new Color(0f, 0f, 0f, 0.85f));
            EditorUtility.SetDirty(m);
            return m;
        }

        private static void BuildPrefab(Spec spec)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(spec.Fbx);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = spec.Name;

            SkinnedMeshRenderer body = CharacterAssets.BodyOf(instance);
            body.sharedMaterial = spec.Skin;
            foreach (SkinnedMeshRenderer smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.name == "Eyebrows") smr.sharedMaterial = spec.Brow;
                else if (spec.Silhouette && smr != body) smr.sharedMaterial = spec.Skin;
            }

            foreach (var (garment, material) in spec.Garments)
            {
                Mesh mesh = CharacterAssets.BuildGarment(spec.Name + "_" + garment, body, garment);
                CharacterAssets.AddGarment(instance.transform, body, mesh, material, garment.ToString());
            }
            foreach (var (hair, material) in spec.Hair) CharacterAssets.AddHair(instance.transform, hair, material, hair);

            var animator = instance.GetComponent<Animator>();
            animator.runtimeAnimatorController = spec.Controller;
            animator.applyRootMotion = false;
            instance.transform.localScale = Vector3.one * spec.Scale;

            switch (spec.Name)
            {
                case "Rafi": instance.AddComponent<RafiRig>(); break;
                case "ShadowRafi": instance.AddComponent<ShadowRig>(); break;
                case "Noor":
                    var rig = instance.AddComponent<NoorRig>();
                    SetObjects(rig, "renderers", instance.GetComponentsInChildren<SkinnedMeshRenderer>(true));
                    break;
            }

            PrefabUtility.SaveAsPrefabAsset(instance, PrefabDir + spec.Name + ".prefab");
            Object.DestroyImmediate(instance);
        }

        // ---- scene -----------------------------------------------------------------------------------------

        [MenuItem("Three Matches/Characters/2 Wire Scene")]
        public static void WireScene()
        {
            WireRafi();
            WireShadow();
            WireNoor();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("CharacterBuilder: scene wired.");
        }

        private static GameObject Instantiate(string prefab, Transform parent)
        {
            // Remove any model already placed here, whatever it is called (renames do not survive a prefab reimport).
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.GetComponent<Animator>() != null) Object.DestroyImmediate(child.gameObject);
            }
            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + prefab + ".prefab"), parent);
            model.name = "Model";
            model.transform.localPosition = Vector3.zero;
            return model;
        }

        private static void WireRafi()
        {
            GameObject rafiObject = Object.FindAnyObjectByType<RafiController>().gameObject;
            Transform sprite = rafiObject.transform.Find("Sprite");
            if (sprite != null) Object.DestroyImmediate(sprite.gameObject);

            GameObject model = Instantiate("Rafi", rafiObject.transform);
            var rig = model.GetComponent<RafiRig>();
            SetObject(rig, "rafi", rafiObject.GetComponent<RafiController>());
            SetObject(rig, "detector", rafiObject.GetComponent<InteractionDetector>());
            SetObject(rig, "input", Object.FindAnyObjectByType<GameInput>());
            SetObject(rig, "matches", Object.FindAnyObjectByType<MatchController>());
            SetObject(rig, "wall", Object.FindAnyObjectByType<MemoryWall>());
        }

        private static void WireShadow()
        {
            GameObject shadowObject = Object.FindAnyObjectByType<ShadowController>().gameObject;
            Transform body = shadowObject.transform.Find("Body");
            foreach (string old in new[] { "Halo", "Main", "Flatten" })
            {
                Transform t = body.Find(old);
                if (t != null) Object.DestroyImmediate(t.gameObject);
            }

            GameObject model = Instantiate("ShadowRafi", body);
            SetObject(model.GetComponent<ShadowRig>(), "shadow", shadowObject.GetComponent<ShadowController>());

            var visual = shadowObject.GetComponent<ShadowVisual>();
            SetObject(visual, "body", body);
            SetObjects(visual, "renderers", model.GetComponentsInChildren<SkinnedMeshRenderer>(true));
        }

        private static void WireNoor()
        {
            var controller = Object.FindAnyObjectByType<NoorController>();
            var placeholder = controller.GetComponent<SpriteRenderer>();
            if (placeholder != null) Object.DestroyImmediate(placeholder);

            GameObject model = Instantiate("Noor", controller.transform);
            var rig = model.GetComponent<NoorRig>();
            SetObject(rig, "noor", controller);
            SetObject(rig, "fire", Object.FindAnyObjectByType<PastFireController>());
        }

        // ---- serialized field helpers ----------------------------------------------------------------------

        private static void SetObject(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogError("CharacterBuilder: no field " + field + " on " + target.GetType().Name); return; }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjects(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogError("CharacterBuilder: no field " + field + " on " + target.GetType().Name); return; }
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
