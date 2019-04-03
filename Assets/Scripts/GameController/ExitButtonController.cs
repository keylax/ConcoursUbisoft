using UnityEngine;

public class ExitButtonController : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUILayout.Button("Exit Game", GUILayout.Height(40)))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
