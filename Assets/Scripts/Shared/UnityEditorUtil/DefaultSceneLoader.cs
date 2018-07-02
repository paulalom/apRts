#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Assets.Scripts.Shared.UnityEditorUtil
{
    [InitializeOnLoad]
    static class DefaultSceneLoader
    {
        static DefaultSceneLoader()
        {
            EditorApplication.playModeStateChanged += LoadDefaultScene;
        }

        static void LoadDefaultScene(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorSceneManager.LoadScene(0);
            }
        }
    }
}
#endif