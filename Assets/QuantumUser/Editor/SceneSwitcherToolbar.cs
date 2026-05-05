using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Adds a dropdown to Unity's main toolbar for quick scene switching.
/// Place this script in an "Editor" folder.
/// </summary>
public static class SceneSwitcherToolbar
{
    private const string ElementPath = "QuickSceneSwitcher/SceneDropdown";
    private const string Tooltip = "Switch between scenes in your project";

    [MainToolbarElement(ElementPath, defaultDockPosition = MainToolbarDockPosition.Left)]
    private static MainToolbarElement CreateSceneSwitcher()
    {
        // Update scene name when a scene is opened
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneOpened += OnSceneOpened;

        var icon = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
        var currentSceneName = SceneManager.GetActiveScene().name;
        var content = new MainToolbarContent(currentSceneName, icon, Tooltip);

        return new MainToolbarDropdown(content, ShowSceneDropdown);
    }

    private static void ShowSceneDropdown(Rect dropdownRect)
    {
        // Find all scenes in the project (excluding Packages)
        var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/" });
        var menu = new GenericMenu();
        var activeScenePath = SceneManager.GetActiveScene().path;

        foreach (var guid in guids)
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(guid);
            var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            var isActive = scenePath == activeScenePath;

            menu.AddItem(new GUIContent(sceneName), isActive, () => SwitchToScene(scenePath));
        }

        menu.DropDown(dropdownRect);
    }

    private static void SwitchToScene(string scenePath)
    {
        // Ask user to save changes if needed
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.OpenScene(scenePath);
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        // Refresh the toolbar button text
        MainToolbar.Refresh(ElementPath);
    }
}