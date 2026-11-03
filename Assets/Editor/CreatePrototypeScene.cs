using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreatePrototypeScene
{
    [MenuItem("Tools/Scan With Your Friends/Create Prototype Scene")]
    public static void Create()
    {
        const string scenePath = "Assets/Scenes/Prototype.unity";
        Directory.CreateDirectory("Assets/Scenes");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject game = new GameObject("Call Center Game");
        game.AddComponent<CallCenterPrototype>();

        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        AssetDatabase.Refresh();
        Debug.Log("Prototype scene created and added to Build Profiles: " + scenePath);
    }
}
