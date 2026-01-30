using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public static class SceneManager
{
    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("SceneManager.LoadScene called with an empty scene name.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning($"Scene '{sceneName}' is not in build settings.");
            return;
        }

        UnitySceneManager.LoadScene(sceneName);
    }

    public static void LoadMenu() => LoadScene("Menu");
    public static void Load3DEditor() => LoadScene("3D Editor");
}
