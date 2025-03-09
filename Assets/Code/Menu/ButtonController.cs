using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public enum ButtonType { Play, Quit }
    public ButtonType buttonType;

    void OnMouseDown()
    {
        if (buttonType == ButtonType.Play)
        {
            // Replace "GameScene" with the name of your game scene.
            SceneManager.LoadScene("SampleScene");
        }
        else if (buttonType == ButtonType.Quit)
        {
            Application.Quit();
#if UNITY_EDITOR
            // For testing in the editor:
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
