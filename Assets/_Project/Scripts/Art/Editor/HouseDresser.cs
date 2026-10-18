using ThreeMatches.Fire;
using UnityEditor;
using UnityEngine;

namespace ThreeMatches.Art.Editor
{
    /// <summary>
    /// Replaces the graybox house with real art (GAME_SPEC §5, §19). Walls, floors and slabs get PBR materials;
    /// graybox furniture cubes are switched off and a Kenney model is placed where each one stood. Every named
    /// object, anchor and component of the house is left in place. Safe to run again: each run rebuilds the
    /// "Art" child of every room from scratch.
    /// </summary>
    public static class HouseDresser
    {
        private const string FurnitureDir = "Assets/_Project/Art/Kenney/FurnitureKit/Models/";
        private const string BuildingDir = "Assets/_Project/Art/Kenney/BuildingKit/Models/FBX format/";

        /// <summary>Kenney models face +Z; the camera looks along +Z, so they are turned around by default.</summary>
        private const float FrontYaw = 180f;
        private const float WallFace = 4f;
        private const float TableHeight = 0.8f;

        private static readonly string[] RoomNames = { "Room_Entry", "Room_Living", "Room_Kitchen", "Room_Landing", "Room_Hallway", "Room_Bedroom" };

        private static readonly string[] HiddenGraybox =
        {
            "Prop_FrontDoor", "Prop_Rug", "Prop_Armchair", "Prop_Cabinet", "Decor_Counter", "Decor_Table",
            "Decor_Fridge", "Prop_ClosetDoor", "Prop_Bed", "Prop_Dresser", "Decor_Rail"
        };

        public static void Dress(string rootName, Era era)
        {
            GameObject houseObject = GameObject.Find(rootName);
            if (houseObject == null)
            {
                Debug.LogError("HouseDresser: no object named " + rootName);
                return;
            }

            Transform house = houseObject.transform;
            var art = new Transform[RoomNames.Length];
            for (int i = 0; i < RoomNames.Length; i++)
            {
                Transform room = house.Find(RoomNames[i]);
                if (room == null) { Debug.LogError("HouseDresser: " + rootName + " has no " + RoomNames[i]); return; }
                Transform old = room.Find("Art");
                if (old != null) Object.DestroyImmediate(old.gameObject);
                art[i] = new GameObject("Art").transform;
                art[i].SetParent(room, false);
            }

            DressShell(house, era);
            Entry(art[0], era);
            Living(art[1], era);
            Kitchen(art[2], era);
            Landing(house.Find("Room_Landing"), art[3], era);
            Hallway(art[4], era);
            Bedroom(house.Find("Room_Bedroom"), art[5], era);

            if (era == Era.E2036) SheetsFuture(house);
            if (era == Era.E1996) AddChar(house);

            EditorUtility.SetDirty(houseObject);
        }

        // ---- shell: walls, floors, slabs, small graybox pieces we keep ---------------------------------------

        private static void DressShell(Transform house, Era era)
        {
            foreach (MeshRenderer r in house.GetComponentsInChildren<MeshRenderer>(true))
            {
                string n = r.name;
                if (n == "ShadowWall") continue; // logic quad, intentionally not rendered

                Material material = null;
                bool metric = false;
                if (n == "Floor" || n.StartsWith("Step_")) { material = HouseArtAssets.Floor(era); metric = true; }
                else if (n == "BackWall" || n.StartsWith("OuterWall") || n.StartsWith("Divider")) { material = HouseArtAssets.Wall(era); metric = true; }
                else if (n.StartsWith("Roof_")) { material = HouseArtAssets.Slab; metric = true; }
                else if (n.StartsWith("Frame_")) material = HouseArtAssets.FrameWood;
                else if (n == "Prop_Shelf") material = HouseArtAssets.TrimWood;
                else if (n == "Prop_Curtains") material = HouseArtAssets.Curtain(era);
                else if (n == "Prop_Candle") material = HouseArtAssets.CandleWax;
                else if (n == "Prop_WallClock") material = HouseArtAssets.Brass;
                else if (n == "Prop_Window") material = HouseArtAssets.Glass;

                if (System.Array.IndexOf(HiddenGraybox, n) >= 0) r.enabled = false;
                if (material == null) continue;

                r.sharedMaterial = material;
                if (metric)
                {
                    MeshFilter filter = r.GetComponent<MeshFilter>();
                    filter.sharedMesh = HouseArtAssets.MetricMesh(filter.sharedMesh, r.transform.localScale);
                }
            }
        }

        // ---- rooms -------------------------------------------------------------------------------------------

        private static void Entry(Transform art, Era era)
        {
            Put(art, "doorwayFront", 0f, 0f, fit: new Vector3(1.1f, 2.2f, 0.3f), backZ: WallFace);
            if (era == Era.E1996)
            {
                Put(art, "coatRack", -2f, 0f, y: 1.5f, backZ: WallFace - 0.1f);
                Put(art, "sideTableDrawers", 1.9f, 0f, backZ: WallFace - 0.1f);
                Put(art, "lampRoundTable", 1.9f, 0f, y: TableHeight);
                Put(art, "pottedPlant", 2.55f, 0f, backZ: WallFace - 0.1f);
                Put(art, "rugRectangle", 0f, 1.5f, fit: new Vector3(2f, 0.03f, 1.2f));
                Toys(art, -0.9f, 1.3f, tidy: false);
                Put(art, "books", 0.7f, 1.7f, yaw: -20f);
                FamilyPhoto(art, 1.05f, 1.75f, 0.34f, 0.44f);
                FamilyPhoto(art, 1.65f, 1.95f, 0.30f, 0.38f);
            }
            else
            {
                Put(art, "sideTable", 1.9f, 0f, backZ: WallFace - 0.1f);
            }
        }

        private static void Living(Transform art, Era era)
        {
            Put(art, "loungeChair", 0.6f, 2.6f);
            Put(art, "cabinetTelevision", 2.4f, 0f, backZ: WallFace - 0.1f);
            if (era != Era.E1996) return;

            Put(art, "bookcaseClosedWide", -1.45f, 0f, backZ: WallFace - 0.1f);
            Put(art, "lampRoundFloor", -0.4f, 0f, backZ: WallFace - 0.1f);
            Put(art, "rugRectangle", 1.5f, 1.7f, fit: new Vector3(2.2f, 0.03f, 1.5f));
            Put(art, "tableCoffee", 1.5f, 1.7f);
            Put(art, "books", 1.4f, 1.7f, y: Height("tableCoffee"), yaw: 15f);
            Put(art, "pillow", 0.6f, 2.45f, y: 0.45f, yaw: 10f);
            FamilyPhoto(art, -0.95f, 2.3f, 0.34f, 0.44f);
        }

        private static void Kitchen(Transform art, Era era)
        {
            Put(art, "kitchenCabinetDrawer", -1.35f, 0f, backZ: WallFace - 0.1f);
            Put(art, "kitchenSink", -0.45f, 0f, backZ: WallFace - 0.1f);
            Put(art, "kitchenStove", 0.45f, 0f, backZ: WallFace - 0.1f);
            Put(art, "kitchenCabinet", 1.35f, 0f, backZ: WallFace - 0.1f);
            Put(art, "kitchenFridge", 2.4f, 0f, backZ: WallFace - 0.1f);
            Put(art, "table", -0.5f, 1.8f);

            if (era != Era.E1996) return;
            Put(art, "kitchenCabinetUpper", -1.35f, 0f, y: 1.5f, backZ: WallFace - 0.1f);
            Put(art, "kitchenCabinetUpper", 1.35f, 0f, y: 1.5f, backZ: WallFace - 0.1f);
            Put(art, "kitchenMicrowave", 1.35f, 3.6f, y: Height("kitchenCabinet"));
            Put(art, "kitchenBlender", -1.35f, 3.6f, y: Height("kitchenCabinetDrawer"));
            Put(art, "chair", -1.45f, 1.8f, yaw: 90f);
            Put(art, "chair", 0.45f, 1.8f, yaw: -90f);
            Put(art, "plantSmall2", -0.5f, 1.8f, y: Height("table"));
        }

        private static void Landing(Transform room, Transform art, Era era)
        {
            WindowArt(room, art);
            Banister(room, art);
            if (era != Era.E1996) return;

            Put(art, "sideTable", 0f, 0f, backZ: WallFace - 0.1f);
            Put(art, "plantSmall1", 0f, 3.7f, y: TableHeight);
            Put(art, "pottedPlant", -2.4f, 0f, backZ: WallFace - 0.1f);
            Put(art, "rugRectangle", -0.6f, 2.2f, fit: new Vector3(2f, 0.03f, 1.1f));
            Toys(art, 1.2f, 1.4f, tidy: false);
            FamilyPhoto(art, -1.5f, 1.9f, 0.34f, 0.44f);
        }

        private static void Hallway(Transform art, Era era)
        {
            Put(art, "bookcaseClosedDoors", 0f, 0f, fit: new Vector3(1.2f, 2.2f, 0.5f), backZ: WallFace - 0.03f);
            if (era != Era.E1996) return;

            Put(art, "sideTable", -1.9f, 0f, backZ: WallFace - 0.1f);
            Put(art, "lampRoundTable", -1.9f, 0f, y: TableHeight);
            Put(art, "pottedPlant", 2.2f, 0f, backZ: WallFace - 0.1f);
            Put(art, "rugRectangle", 0f, 1.6f, fit: new Vector3(3f, 0.03f, 1f));
            FamilyPhoto(art, -1.0f, 1.8f, 0.34f, 0.44f);
            FamilyPhoto(art, 1.0f, 1.75f, 0.30f, 0.38f);
            FamilyPhoto(art, 1.6f, 1.95f, 0.34f, 0.44f);
        }

        /// <summary>Noor's room. It is the same in every era: in 2026 and 2036 it is preserved like a shrine.</summary>
        private static void Bedroom(Transform room, Transform art, Era era)
        {
            WindowArt(room, art);
            Put(art, "bedSingle", 1.6f, 2.6f, yaw: 90f, fit: new Vector3(1.35f, 0.8f, 2.2f));
            Put(art, "pillow", 2.35f, 2.6f, y: 0.45f, yaw: 90f);
            Put(art, "cabinetBedDrawer", -0.5f, 0f, scale: 1.6f, backZ: WallFace - 0.1f);
            Put(art, "rugRounded", 0.2f, 1.8f, fit: new Vector3(1.8f, 0.03f, 1.3f));
            Put(art, "books", -1.0f, 1.5f, yaw: 30f);
            FamilyPhoto(art, -0.95f, 1.95f, 0.3f, 0.38f);
            Toys(art, 0.2f, 1.7f, tidy: era != Era.E1996);
            if (era == Era.E1996)
            {
                Put(art, "lampSquareTable", -0.5f, 0f, y: 0.88f, backZ: WallFace - 0.25f);
                Toys(art, -1.7f, 1.9f, tidy: false);
                Put(art, "books", 0.6f, 1.2f, yaw: -40f);
                FamilyPhoto(art, 2.2f, 2.0f, 0.3f, 0.38f);
            }
        }

        // ---- 2036 dust sheets --------------------------------------------------------------------------------

        private static void SheetsFuture(Transform house)
        {
            Mesh drape = HouseArtAssets.DrapeMesh();
            foreach (MeshFilter filter in house.GetComponentsInChildren<MeshFilter>(true))
                if (filter.name.StartsWith("Future_DustSheet_")) filter.sharedMesh = drape;

            Sheet(house.Find("Room_Living/Art"), "Armchair", 0.6f, 2.6f, new Vector3(1.05f, 1.0f, 1.0f));
            Sheet(house.Find("Room_Bedroom/Art"), "Bed", 1.6f, 2.6f, new Vector3(2.45f, 0.8f, 1.5f));
            Sheet(house.Find("Room_Bedroom/Art"), "Dresser", -0.5f, 3.5f, new Vector3(0.95f, 0.95f, 0.75f));
            Sheet(house.Find("Room_Entry/Art"), "SideTable", 1.9f, 3.8f, new Vector3(0.75f, 0.9f, 0.5f));
        }

        private static void Sheet(Transform art, string label, float x, float z, Vector3 size)
        {
            var go = new GameObject("Future_DustSheet_" + label);
            go.transform.SetParent(art, false);
            go.transform.localPosition = new Vector3(x, size.y * 0.5f, z);
            go.transform.localScale = size;
            go.AddComponent<MeshFilter>().sharedMesh = HouseArtAssets.DrapeMesh();
            go.AddComponent<MeshRenderer>().sharedMaterial = HouseArtAssets.DustSheet;
        }

        // ---- 1996 burn damage --------------------------------------------------------------------------------

        private static void AddChar(Transform house)
        {
            foreach (string roomName in new[] { "Room_Landing", "Room_Hallway", "Room_Bedroom" })
            {
                Transform room = house.Find(roomName);
                RoomChar roomChar = room.GetComponent<RoomChar>();
                if (roomChar == null) roomChar = room.gameObject.AddComponent<RoomChar>();
                var so = new SerializedObject(roomChar);
                so.FindProperty("fireRoom").objectReferenceValue = room.GetComponentInChildren<FireRoom>(true);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // ---- windows and photos ------------------------------------------------------------------------------

        private static void WindowArt(Transform room, Transform art)
        {
            Transform glass = room.Find("Prop_Window");
            if (glass == null) return;
            Vector3 p = glass.localPosition;
            Vector3 s = glass.localScale;

            Put(art, "doorwayOpen", p.x, 0f, y: p.y - s.y * 0.5f - 0.05f, fit: new Vector3(s.x + 0.1f, s.y + 0.1f, 0.3f), backZ: WallFace, name: "WindowFrame");

            var night = new GameObject("WindowNight");
            night.transform.SetParent(art, false);
            night.transform.localPosition = new Vector3(p.x, p.y, WallFace - 0.03f);
            night.transform.localScale = new Vector3(s.x, s.y, 1f);
            night.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            night.AddComponent<MeshRenderer>().sharedMaterial = HouseArtAssets.Night;
        }

        /// <summary>The landing banister: the old graybox slab becomes a handrail, a bottom rail and posts.</summary>
        private static void Banister(Transform room, Transform art)
        {
            Transform slab = room.Find("Decor_Rail");
            if (slab == null) return;
            Vector3 c = slab.localPosition;
            float length = slab.localScale.x;
            float top = c.y + slab.localScale.y * 0.5f;
            Material wood = HouseArtAssets.TrimWood;

            Prim(art, "Banister_Handrail", "Cube.fbx", new Vector3(c.x, top, c.z), new Vector3(length, 0.05f, 0.08f), wood);
            Prim(art, "Banister_BottomRail", "Cube.fbx", new Vector3(c.x, 0.12f, c.z), new Vector3(length, 0.04f, 0.05f), wood);
            const float postSpacing = 0.3f;
            int posts = Mathf.Max(2, Mathf.RoundToInt(length / postSpacing));
            for (int i = 0; i <= posts; i++)
            {
                float x = c.x - length * 0.5f + length * i / posts;
                Prim(art, "Banister_Post", "Cube.fbx", new Vector3(x, top * 0.5f, c.z), new Vector3(0.04f, top, 0.04f), wood);
            }
        }

        /// <summary>A few toy blocks and a ball: scattered in 1996, stacked neatly where the room is kept.</summary>
        private static void Toys(Transform art, float x, float z, bool tidy)
        {
            const float block = 0.12f;
            Material[] colours = { HouseArtAssets.ToyRed, HouseArtAssets.ToyBlue, HouseArtAssets.ToyYellow };
            for (int i = 0; i < 3; i++)
            {
                Vector3 pos = tidy
                    ? new Vector3(x + 0.07f * (i % 2), block * (i / 2 + 0.5f), z)
                    : new Vector3(x + 0.16f * i, block * 0.5f, z + 0.08f * (i % 2) - 0.04f);
                var go = new GameObject("Toy_Block");
                go.transform.SetParent(art, false);
                go.transform.localPosition = pos;
                go.transform.localRotation = Quaternion.Euler(0f, tidy ? 0f : 23f * (i + 1), 0f);
                go.transform.localScale = Vector3.one * block;
                go.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                go.AddComponent<MeshRenderer>().sharedMaterial = colours[i];
            }

            const float ballSize = 0.16f;
            var ball = new GameObject("Toy_Ball");
            ball.transform.SetParent(art, false);
            ball.transform.localPosition = new Vector3(x + (tidy ? -0.18f : 0.55f), ballSize * 0.5f, z + (tidy ? 0f : -0.12f));
            ball.transform.localScale = Vector3.one * ballSize;
            ball.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            ball.AddComponent<MeshRenderer>().sharedMaterial = HouseArtAssets.ToyRed;
        }

        private static void FamilyPhoto(Transform art, float x, float y, float w, float h)
        {
            Prim(art, "Decor_PhotoFrame", "Cube.fbx", new Vector3(x, y, WallFace - 0.015f), new Vector3(w + 0.08f, h + 0.08f, 0.03f), HouseArtAssets.FrameWood);
            Prim(art, "Decor_Photo", "Quad.fbx", new Vector3(x, y, WallFace - 0.04f), new Vector3(w, h, 1f), HouseArtAssets.Photo);
        }

        private static void Prim(Transform parent, string name, string builtin, Vector3 pos, Vector3 scale, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>(builtin);
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        // ---- placing Kenney models ---------------------------------------------------------------------------

        private static GameObject LoadModel(string model)
        {
            string path = (model.StartsWith("bk/") ? BuildingDir + model.Substring(3) : FurnitureDir + model) + ".fbx";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) Debug.LogError("HouseDresser: missing model " + path);
            return prefab;
        }

        /// <summary>Height of a model in metres (after the import scale), used to stack props on furniture.</summary>
        private static float Height(string model)
        {
            GameObject prefab = LoadModel(model);
            if (prefab == null) return 0f;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            float h = BoundsOf(instance).size.y;
            Object.DestroyImmediate(instance);
            return h;
        }

        private static Bounds BoundsOf(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        /// <summary>
        /// Puts a model in a room: footprint centred on (x, z), base at y, turned by yaw relative to facing the
        /// camera. <paramref name="fit"/> stretches it to a size in metres; <paramref name="backZ"/> pushes it
        /// back until its rear edge touches that z.
        /// </summary>
        private static Transform Put(Transform art, string model, float x, float z, float yaw = 0f, float y = 0f,
            float scale = 1f, Vector3? fit = null, float? backZ = null, string name = null)
        {
            GameObject prefab = LoadModel(model);
            if (prefab == null) return null;

            var holder = new GameObject(name ?? model).transform;
            holder.SetParent(art, false);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, holder);

            // The kit's meshes sit away from their pivot; recentre on the footprint with the base at zero.
            Bounds b = BoundsOf(instance);
            Vector3 anchor = holder.position;
            instance.transform.position += anchor - new Vector3(b.center.x, b.min.y, b.center.z);

            holder.localScale = fit.HasValue
                ? new Vector3(fit.Value.x / b.size.x, fit.Value.y / b.size.y, fit.Value.z / b.size.z)
                : Vector3.one * scale;
            holder.localRotation = Quaternion.Euler(0f, yaw + FrontYaw, 0f);
            holder.localPosition = new Vector3(x, y, z);

            if (backZ.HasValue)
            {
                Bounds placed = BoundsOf(holder.gameObject);
                float rearZ = art.position.z + backZ.Value;
                holder.position += new Vector3(0f, 0f, rearZ - placed.max.z);
            }
            return holder;
        }
    }
}
