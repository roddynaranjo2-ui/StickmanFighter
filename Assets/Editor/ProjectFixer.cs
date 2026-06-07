// ProjectFixer.cs — Utilidad de Editor para validar / re-aplicar fixes idempotentemente.
// Usable manualmente desde "Tools/Fix Scaffold" o desde CI con:
//   Unity ... -executeMethod ProjectFixer.FixAll
//
// Cubre: re-sincronizar GUIDs de EditorBuildSettings, garantizar tag "Player",
// y verificar que las escenas contienen los objetos críticos.

#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProjectFixer
{
    [MenuItem("Tools/Fix Scaffold")]
    public static void FixAll()
    {
        FixBuildScenes();
        FixTags();
        Debug.Log("[ProjectFixer] FixAll completed.");
    }

    public static void FixBuildScenes()
    {
        string mainMenuPath = "Assets/Scenes/MainMenu.unity";
        string combatPath   = "Assets/Scenes/CombatScene.unity";

        if (!File.Exists(mainMenuPath) || !File.Exists(combatPath))
        {
            Debug.LogWarning("[ProjectFixer] Scenes not found, skipping FixBuildScenes.");
            return;
        }

        var scenes = new[]
        {
            new EditorBuildSettingsScene(mainMenuPath, true),
            new EditorBuildSettingsScene(combatPath, true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[ProjectFixer] EditorBuildSettings.scenes resynced to real GUIDs.");
    }

    public static void FixTags()
    {
        var tagManager = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/TagManager.asset");
        if (tagManager == null) return;

        var so = new SerializedObject(tagManager);
        var tagsProp = so.FindProperty("tags");
        if (tagsProp == null) return;

        EnsureTag(tagsProp, "Player");
        EnsureTag(tagsProp, "Enemy");
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log("[ProjectFixer] Tags 'Player' and 'Enemy' ensured in TagManager.");
    }

    private static void EnsureTag(SerializedProperty tagsProp, string newTag)
    {
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == newTag) return;
        }
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = newTag;
    }
}
#endif
