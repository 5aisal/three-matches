using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreeMatches.Art.Editor
{
    public enum Era { E1996, E2026, E2036 }

    /// <summary>Materials, texture import settings and generated meshes for the house art (GAME_SPEC §19).</summary>
    public static class HouseArtAssets
    {
        public const string TextureDir = "Assets/_Project/Art/Textures/";
        public const string MaterialDir = "Assets/_Project/Materials/";
        public const string MeshDir = "Assets/_Project/Art/Generated/";
        private const int MaxTextureSize = 1024;
        private const float WallBoost = 1.9f;

        private static Shader LitShader => Shader.Find("Universal Render Pipeline/Lit");

        // ---- lookups used by the dresser -------------------------------------------------------------------

        public static Material Wall(Era e) => Load("Wall_Plaster_" + Tag(e));
        public static Material Floor(Era e) => Load("Floor_Laminate_" + Tag(e));
        public static Material Curtain(Era e) => Load(e == Era.E1996 ? "Curtain_Fabric" : "Curtain_Fabric_2026");
        public static Material Slab => Load("Slab_Concrete");
        public static Material FrameWood => Load("Frame_Wood");
        public static Material TrimWood => Load("Trim_Wood");
        public static Material CandleWax => Load("Candle_Wax");
        public static Material Brass => Load("Clock_Brass");
        public static Material Glass => Load("Window_Glass");
        public static Material Night => Load("Night_Backdrop");
        public static Material Photo => Load("Photo_Placeholder");
        public static Material ToyRed => Load("Toy_Red");
        public static Material ToyBlue => Load("Toy_Blue");
        public static Material ToyYellow => Load("Toy_Yellow");
        public static Material DustSheet => Load("Future_Dressing");

        private static string Tag(Era e) => e == Era.E1996 ? "1996" : e == Era.E2026 ? "2026" : "2036";

        private static Material Load(string name) => AssetDatabase.LoadAssetAtPath<Material>(MaterialDir + name + ".mat");

        // ---- textures and materials ------------------------------------------------------------------------

        public static void BuildTexturesAndMaterials()
        {
            ImportTextures();

            // The plaster albedo is a dark grey, so wall tints are pushed above 1 to land at a readable mid-tone.
            Pbr("Wall_Plaster_1996", "plastered_wall_05", new Color(1f, 0.80f, 0.58f) * WallBoost, 0.12f, 0.6f);
            Pbr("Wall_Plaster_2026", "plastered_wall_05", new Color(0.62f, 0.74f, 1f) * WallBoost, 0.12f, 0.6f);
            Pbr("Wall_Plaster_2036", "plastered_wall_05", new Color(0.88f, 0.86f, 0.82f) * WallBoost, 0.08f, 0.6f);
            Pbr("Floor_Laminate_1996", "laminate_floor_02", new Color(1f, 0.82f, 0.62f), 0.35f, 0.8f);
            Pbr("Floor_Laminate_2026", "laminate_floor_02", new Color(0.62f, 0.70f, 0.85f), 0.35f, 0.8f);
            Pbr("Floor_Laminate_2036", "laminate_floor_02", new Color(0.72f, 0.70f, 0.66f), 0.2f, 0.8f);

            Material slab = Create("Slab_Concrete");
            slab.SetTexture("_BaseMap", Tex("Concrete034/Concrete034_1K-JPG_Color.jpg"));
            slab.SetColor("_BaseColor", new Color(0.55f, 0.55f, 0.58f));
            slab.SetTexture("_BumpMap", Tex("Concrete034/Concrete034_1K-JPG_NormalGL.jpg"));
            slab.EnableKeyword("_NORMALMAP");
            slab.SetFloat("_Smoothness", 0.1f);
            Tile(slab, 0.5f);
            EditorUtility.SetDirty(slab);

            Flat("Frame_Wood", new Color(0.45f, 0.29f, 0.16f), 0.3f);
            Flat("Toy_Red", new Color(0.75f, 0.12f, 0.10f), 0.3f);
            Flat("Toy_Blue", new Color(0.15f, 0.30f, 0.70f), 0.3f);
            Flat("Toy_Yellow", new Color(0.85f, 0.70f, 0.12f), 0.3f);
            Flat("Trim_Wood", new Color(0.30f, 0.18f, 0.10f), 0.3f);
            Flat("Curtain_Fabric", new Color(0.55f, 0.22f, 0.20f), 0.05f);
            Flat("Curtain_Fabric_2026", new Color(0.25f, 0.30f, 0.42f), 0.05f);
            Flat("Candle_Wax", new Color(0.92f, 0.86f, 0.70f), 0.3f);
            Flat("Clock_Brass", new Color(0.55f, 0.42f, 0.18f), 0.5f);

            Material night = Flat("Night_Backdrop", new Color(0.02f, 0.04f, 0.09f), 0f);
            night.EnableKeyword("_EMISSION");
            night.SetColor("_EmissionColor", new Color(0.04f, 0.07f, 0.16f));
            night.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

            Material glass = Flat("Window_Glass", new Color(0.55f, 0.7f, 0.9f, 0.18f), 0.9f);
            glass.SetFloat("_Surface", 1f);
            glass.SetFloat("_Blend", 0f);
            glass.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            glass.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            glass.SetFloat("_ZWrite", 0f);
            glass.SetOverrideTag("RenderType", "Transparent");
            glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            glass.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            glass.SetShaderPassEnabled("ShadowCaster", false);

            AssetDatabase.SaveAssets();
        }

        private static void ImportTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TextureDir.TrimEnd('/') });
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;
                    string lower = path.ToLowerInvariant();
                    importer.maxTextureSize = MaxTextureSize;
                    if (lower.Contains("nor_gl") || lower.Contains("normalgl"))
                    {
                        importer.textureType = TextureImporterType.NormalMap;
                    }
                    else
                    {
                        importer.textureType = TextureImporterType.Default;
                        importer.sRGBTexture = lower.Contains("diff") || lower.Contains("color");
                    }
                    importer.SaveAndReimport();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static Texture2D Tex(string relative) => AssetDatabase.LoadAssetAtPath<Texture2D>(TextureDir + relative);

        private static Material Create(string name)
        {
            string path = MaterialDir + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(LitShader);
                AssetDatabase.CreateAsset(m, path);
            }
            else
            {
                m.shader = LitShader;
            }
            return m;
        }

        private static void Tile(Material m, float tiling)
        {
            var v = new Vector2(tiling, tiling);
            m.SetTextureScale("_BaseMap", v);
            m.SetTextureScale("_BumpMap", v);
            m.SetTextureScale("_OcclusionMap", v);
        }

        private static Material Pbr(string name, string set, Color tint, float smoothness, float tiling)
        {
            Material m = Create(name);
            string dir = set + "/" + set;
            m.SetTexture("_BaseMap", Tex(dir + "_diff_1k.jpg"));
            m.SetColor("_BaseColor", tint);
            m.SetTexture("_BumpMap", Tex(dir + "_nor_gl_1k.exr"));
            m.EnableKeyword("_NORMALMAP");
            m.SetTexture("_OcclusionMap", Tex(dir + "_ao_1k.jpg"));
            m.EnableKeyword("_OCCLUSIONMAP");
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", 0f);
            Tile(m, tiling);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static Material Flat(string name, Color colour, float smoothness)
        {
            Material m = Create(name);
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", 0f);
            EditorUtility.SetDirty(m);
            return m;
        }

        // ---- generated meshes ------------------------------------------------------------------------------

        private static readonly Dictionary<string, Mesh> MeshCache = new Dictionary<string, Mesh>();

        /// <summary>
        /// A copy of the builtin cube/quad whose UVs are in metres for the given object scale, so a tiling
        /// texture keeps one size on every wall instead of stretching with the object.
        /// </summary>
        public static Mesh MetricMesh(Mesh source, Vector3 scale)
        {
            bool quad = source.name.Contains("Quad");
            string key = (quad ? "Quad_" : "Cube_") + scale.x.ToString("0.###") + "x" + scale.y.ToString("0.###") + "x" + scale.z.ToString("0.###");
            if (MeshCache.TryGetValue(key, out Mesh cached) && cached != null) return cached;

            string path = MeshDir + "Metric_" + key + ".asset";
            Mesh asset = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (asset != null)
            {
                MeshCache[key] = asset;
                return asset;
            }

            Mesh basis = Resources.GetBuiltinResource<Mesh>(quad ? "Quad.fbx" : "Cube.fbx");
            Mesh mesh = new Mesh { name = "Metric_" + key };
            Vector3[] verts = basis.vertices;
            Vector3[] normals = basis.normals;
            var uvs = new Vector2[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 p = Vector3.Scale(verts[i], scale);
                Vector3 n = new Vector3(Mathf.Abs(normals[i].x), Mathf.Abs(normals[i].y), Mathf.Abs(normals[i].z));
                if (n.x >= n.y && n.x >= n.z) uvs[i] = new Vector2(p.z, p.y);
                else if (n.y >= n.z) uvs[i] = new Vector2(p.x, p.z);
                else uvs[i] = new Vector2(p.x, p.y);
            }
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = basis.triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            Save(mesh, path);
            MeshCache[key] = mesh;
            return mesh;
        }

        /// <summary>A unit-height box that tapers, flares at the hem and ripples, for dust sheets.</summary>
        public static Mesh DrapeMesh()
        {
            const string path = MeshDir + "Drape.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;

            const int n = 10;
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            void Face(Vector3 origin, Vector3 u, Vector3 v, Vector3 outward)
            {
                int start = verts.Count;
                for (int i = 0; i <= n; i++)
                {
                    for (int j = 0; j <= n; j++)
                    {
                        Vector3 p = origin + u * (i / (float)n) + v * (j / (float)n);
                        verts.Add(Deform(p, outward));
                        uvs.Add(new Vector2(i / (float)n, j / (float)n));
                    }
                }
                bool flip = Vector3.Dot(Vector3.Cross(verts[start + n + 1] - verts[start], verts[start + 1] - verts[start]), outward) < 0f;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        int a = start + i * (n + 1) + j, b = a + n + 1, c = a + 1, d = b + 1;
                        if (flip) { tris.Add(a); tris.Add(c); tris.Add(b); tris.Add(b); tris.Add(c); tris.Add(d); }
                        else { tris.Add(a); tris.Add(b); tris.Add(c); tris.Add(b); tris.Add(d); tris.Add(c); }
                    }
                }
            }

            Vector3 h = Vector3.one * 0.5f;
            Face(new Vector3(-h.x, -h.y, -h.z), Vector3.right, Vector3.up, Vector3.back);
            Face(new Vector3(h.x, -h.y, h.z), Vector3.left, Vector3.up, Vector3.forward);
            Face(new Vector3(-h.x, -h.y, h.z), Vector3.back, Vector3.up, Vector3.left);
            Face(new Vector3(h.x, -h.y, -h.z), Vector3.forward, Vector3.up, Vector3.right);
            Face(new Vector3(-h.x, h.y, -h.z), Vector3.right, Vector3.forward, Vector3.up);

            Mesh mesh = new Mesh { name = "Drape" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            Save(mesh, path);
            return mesh;
        }

        private static Vector3 Deform(Vector3 p, Vector3 outward)
        {
            float t = p.y + 0.5f;
            float taper = 1f - 0.14f * t;
            float hem = t < 0.15f ? 0.06f * (1f - t / 0.15f) : 0f;
            p.x *= taper + hem;
            p.z *= taper + hem;

            float edge = Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.z)) * 2f;
            p.y -= 0.07f * edge * edge * edge * Mathf.SmoothStep(0f, 1f, (t - 0.7f) / 0.3f);

            if (Mathf.Abs(outward.y) < 0.5f)
            {
                float ripple = 0.02f * Mathf.Sin(p.y * 9f + (p.x + p.z) * 11f);
                p += outward * ripple;
            }
            return p;
        }

        private static void Save(Mesh mesh, string path)
        {
            if (!AssetDatabase.IsValidFolder(MeshDir.TrimEnd('/'))) AssetDatabase.CreateFolder("Assets/_Project/Art", "Generated");
            AssetDatabase.CreateAsset(mesh, path);
        }
    }
}
