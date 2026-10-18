using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThreeMatches.Art.Editor
{
    /// <summary>Menu entry points for building the house art. Run in order: materials, dress, save prefabs.</summary>
    public static class HouseArtBuilder
    {
        private const string PrefabDir = "Assets/_Project/Prefabs/";

        [MenuItem("Three Matches/Art/1 Build Materials")]
        public static void BuildMaterials() => HouseArtAssets.BuildTexturesAndMaterials();

        [MenuItem("Three Matches/Art/2 Dress Houses")]
        public static void DressHouses()
        {
            HouseDresser.Dress("House_1996", Era.E1996);
            HouseDresser.Dress("House_2026", Era.E2026);
            HouseDresser.Dress("House_2036", Era.E2036);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Three Matches/Art/3 Save House Prefab Variants")]
        public static void SaveHousePrefabs()
        {
            foreach (string year in new[] { "1996", "2026", "2036" })
            {
                GameObject house = GameObject.Find("House_" + year);
                if (house == null) { Debug.LogError("No House_" + year + " in the scene"); continue; }
                PrefabUtility.SaveAsPrefabAssetAndConnect(house, PrefabDir + "House_" + year + ".prefab", InteractionMode.AutomatedAction);
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
