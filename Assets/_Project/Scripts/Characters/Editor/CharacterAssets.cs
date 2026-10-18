using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreeMatches.Characters.Editor
{
    /// <summary>Paths, materials, clothing meshes and hair for the Quaternius characters (GAME_SPEC §19, §25).</summary>
    public static class CharacterAssets
    {
        public const string PackRoot = "Assets/_Project/Art/Characters/Quaternius/Universal Base Characters[Standard]/Universal Base Characters[Standard]/";
        public const string MaleFbx = PackRoot + "Base Characters/Unity/Superhero_Male_FullBody.fbx";
        public const string FemaleFbx = PackRoot + "Base Characters/Unity/Superhero_Female_FullBody.fbx";
        public const string HairDir = PackRoot + "Hairstyles/Rigged to Head Bone/FBX (Unity)/";
        public const string TextureDir = PackRoot + "Base Characters/Textures/";

        public const string GeneratedDir = "Assets/_Project/Art/Characters/Generated/";
        public const string MaterialDir = "Assets/_Project/Materials/Characters/";

        public const string UalFbx = "Assets/_Project/Art/Animations/UAL/Universal Animation Library[Standard]/Universal Animation Library[Standard]/Unity/UAL1_Standard.fbx";

        private static readonly string[] Torso = { "spine_01", "spine_02", "spine_03", "clavicle_l", "clavicle_r" };
        private static readonly string[] Arms = { "upperarm_l", "upperarm_r", "lowerarm_l", "lowerarm_r" };
        private static readonly string[] ShortSleeves = { "upperarm_l", "upperarm_r" };
        private static readonly string[] Hips = { "pelvis" };
        private static readonly string[] Thighs = { "thigh_l", "thigh_r" };
        private static readonly string[] Calves = { "calf_l", "calf_r" };

        public enum Garment { Shirt, Trousers, Nightgown, Coat }

        // ---- folders ---------------------------------------------------------------------------------------

        public static void EnsureFolder(string path)
        {
            path = path.TrimEnd('/');
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }

        /// <summary>The body mesh of a base character (the renderer that is not the eyes or eyebrows).</summary>
        public static SkinnedMeshRenderer BodyOf(GameObject character)
        {
            foreach (SkinnedMeshRenderer smr in character.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (smr.name.ToLowerInvariant().Contains("superhero")) return smr;
            throw new InvalidOperationException("No body mesh found under " + character.name);
        }

        // ---- materials -------------------------------------------------------------------------------------

        public static Material Skin(string name, string sourceFbx, string textureFile, Color tint)
        {
            Material source = BodyOf(AssetDatabase.LoadAssetAtPath<GameObject>(sourceFbx)).sharedMaterial;
            Material m = Copy(name, source);
            if (!string.IsNullOrEmpty(textureFile))
                m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(TextureDir + textureFile));
            m.SetColor("_BaseColor", tint);
            m.SetFloat("_Smoothness", 0.2f);
            EditorUtility.SetDirty(m);
            return m;
        }

        public static Material HairMaterial(string name, string hairFbx, Color tint)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(HairDir + hairFbx + ".fbx");
            Material m = Copy(name, go.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial);
            m.SetColor("_BaseColor", tint);
            m.SetFloat("_Smoothness", 0.15f);
            EditorUtility.SetDirty(m);
            return m;
        }

        public static Material Cloth(string name, Color colour)
        {
            Material m = Copy(name, null);
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", 0.08f);
            m.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static Material Copy(string name, Material source)
        {
            EnsureFolder(MaterialDir);
            string path = MaterialDir + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = source != null ? new Material(source) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, path);
            }
            else if (source != null)
            {
                m.CopyPropertiesFromMaterial(source);
            }
            return m;
        }

        public static Material Load(string name) => AssetDatabase.LoadAssetAtPath<Material>(MaterialDir + name + ".mat");

        // ---- clothing --------------------------------------------------------------------------------------

        /// <summary>
        /// A garment shell: the part of the body mesh whose vertices are mostly driven by the chosen bones,
        /// pushed out along its normals. It shares the body's bones and weights, so it animates with it.
        /// </summary>
        public static Mesh BuildGarment(string meshName, SkinnedMeshRenderer body, Garment garment)
        {
            string[] bones;
            float offset;
            switch (garment)
            {
                case Garment.Shirt: bones = Join(Torso, Arms); offset = 0.012f; break;
                case Garment.Trousers: bones = Join(Hips, Thighs, Calves); offset = 0.012f; break;
                case Garment.Nightgown: bones = Join(Torso, ShortSleeves, Hips, Thighs); offset = 0.014f; break;
                default: bones = Join(Torso, Arms, Hips, Thighs); offset = 0.03f; break;
            }

            Mesh src = body.sharedMesh;
            Transform[] boneTransforms = body.bones;
            var wanted = new HashSet<string>(bones);
            BoneWeight[] weights = src.boneWeights;
            Vector3[] vertices = src.vertices;
            Vector3[] normals = src.normals;
            Vector2[] uv = src.uv;
            int[] triangles = src.triangles;

            var selected = new bool[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) selected[i] = wanted.Contains(boneTransforms[weights[i].boneIndex0].name);

            var remap = new int[vertices.Length];
            for (int i = 0; i < remap.Length; i++) remap[i] = -1;
            var newVerts = new List<Vector3>();
            var newNormals = new List<Vector3>();
            var newUv = new List<Vector2>();
            var newWeights = new List<BoneWeight>();
            var newTris = new List<int>();

            for (int t = 0; t < triangles.Length; t += 3)
            {
                int a = triangles[t], b = triangles[t + 1], c = triangles[t + 2];
                if (!selected[a] || !selected[b] || !selected[c]) continue;
                foreach (int v in new[] { a, b, c })
                {
                    if (remap[v] < 0)
                    {
                        remap[v] = newVerts.Count;
                        newVerts.Add(vertices[v] + normals[v] * offset);
                        newNormals.Add(normals[v]);
                        newUv.Add(uv.Length > v ? uv[v] : Vector2.zero);
                        newWeights.Add(weights[v]);
                    }
                    newTris.Add(remap[v]);
                }
            }

            var mesh = new Mesh { name = meshName };
            mesh.SetVertices(newVerts);
            mesh.SetNormals(newNormals);
            mesh.SetUVs(0, newUv);
            mesh.boneWeights = newWeights.ToArray();
            mesh.bindposes = src.bindposes;
            mesh.SetTriangles(newTris, 0);
            mesh.RecalculateBounds();

            EnsureFolder(GeneratedDir);
            string path = GeneratedDir + meshName + ".asset";
            if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static string[] Join(params string[][] sets)
        {
            var all = new List<string>();
            foreach (string[] s in sets) all.AddRange(s);
            return all.ToArray();
        }

        // ---- skinned parts ---------------------------------------------------------------------------------

        /// <summary>Adds a garment mesh as a skinned renderer on the same skeleton as the body.</summary>
        public static SkinnedMeshRenderer AddGarment(Transform root, SkinnedMeshRenderer body, Mesh mesh, Material material, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh;
            smr.bones = body.bones;
            smr.rootBone = body.rootBone;
            smr.sharedMaterial = material;
            smr.localBounds = body.localBounds;
            smr.updateWhenOffscreen = false;
            return smr;
        }

        /// <summary>Adds a hairstyle (rigged to the head bone on the same skeleton) by matching bones by name.</summary>
        public static SkinnedMeshRenderer AddHair(Transform root, string hairFbx, Material material, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HairDir + hairFbx + ".fbx");
            SkinnedMeshRenderer source = prefab.GetComponentInChildren<SkinnedMeshRenderer>();

            var byName = new Dictionary<string, Transform>();
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) byName[t.name] = t;

            var bones = new Transform[source.bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                if (!byName.TryGetValue(source.bones[i].name, out bones[i]))
                    Debug.LogWarning("CharacterAssets: bone " + source.bones[i].name + " missing for " + hairFbx);
            }

            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = source.sharedMesh;
            smr.bones = bones;
            smr.rootBone = byName.TryGetValue(source.rootBone.name, out Transform rb) ? rb : root;
            smr.sharedMaterial = material;
            smr.localBounds = source.localBounds;
            return smr;
        }
    }
}
